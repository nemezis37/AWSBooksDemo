using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Infrastructure.Config;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Application.Common.Behavior
{
    public class SendSQSMessageBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly SQSOptions _sqsConfig;

        public SendSQSMessageBehavior(IAmazonSQS amazonSqs, IOptions<SQSOptions> sqsConfig)
        {
            _amazonSqs = amazonSqs;
            _sqsConfig = sqsConfig.Value;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var sendMessageRequest = new SendMessageRequest(_sqsConfig.QueueUrl,
                $"{request.GetType().Namespace}.{request.GetType().Name}: {JsonConvert.SerializeObject(request)}");
            await _amazonSqs.SendMessageAsync(sendMessageRequest, cancellationToken);
            return await next();
        }
    }
}
