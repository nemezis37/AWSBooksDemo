using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using MediatR;

namespace Application.Books
{
    public class Delete
    {
        public class Command: IRequest<Unit>
        {
            public string ISBN { get; set; }
        }

        public class Handler : IRequestHandler<Command, Unit>
        {
            private readonly IDynamoDBContext _dataContext;

            public Handler(IDynamoDBContext dataContext)
            {
                _dataContext = dataContext;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                await _dataContext.DeleteAsync<Book>(request.ISBN, cancellationToken);
                return Unit.Value;
            }
        }
    }
}
