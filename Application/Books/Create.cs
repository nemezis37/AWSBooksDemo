using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS;
using Amazon.SQS.Model;
using Domain;
using Infrastructure.Config;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Application.Books
{
    public class Create
    {
        public class Command: IRequest<Unit>
        {
            public Book Book { get; set; }
        }

        public class Handler:IRequestHandler<Command, Unit>
        {
            private readonly IDynamoDBContext _dataContext;
            private readonly IAmazonSQS _amazonSqs;
            private readonly SQSOptions _sqsConfig;

            public Handler(IDynamoDBContext dataContext, IAmazonSQS amazonSqs, IOptions<SQSOptions> sqsConfig)
            {
                _dataContext = dataContext;
                _amazonSqs = amazonSqs;
                _sqsConfig = sqsConfig.Value;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                await _dataContext.SaveAsync(request.Book, cancellationToken);
                var sendMessageRequest = new SendMessageRequest(_sqsConfig.QueueUrl, $"Created: { JsonConvert.SerializeObject(request.Book)}");
                await _amazonSqs.SendMessageAsync(sendMessageRequest, cancellationToken);
                return Unit.Value;
            }
        }
    }
}
