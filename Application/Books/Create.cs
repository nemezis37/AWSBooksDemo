using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SQS;
using Amazon.SQS.Model;
using Domain;
using MediatR;
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

            public Handler(IDynamoDBContext dataContext, IAmazonSQS amazonSqs)
            {
                _dataContext = dataContext;
                _amazonSqs = amazonSqs;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                await _dataContext.SaveAsync(request.Book, cancellationToken);
                var sendMessageRequest = new SendMessageRequest(@"https://sqs.us-east-1.amazonaws.com/236504649196/BookActionsSQS", $"Created: { JsonConvert.SerializeObject(request.Book)}");
                var sendResult = await _amazonSqs.SendMessageAsync(sendMessageRequest, cancellationToken);
                return Unit.Value;
            }
        }
    }
}
