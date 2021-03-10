using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using MediatR;

namespace Application.Books
{
    public class Get
    {
        public class Query : IRequest<Book>
        {
            public string ISBN { get; set; }
        }

        public class Handler : IRequestHandler<Query, Book>
        {
            private readonly IDynamoDBContext _dataContext;

            public Handler(IDynamoDBContext dataContext)
            {
                _dataContext = dataContext;
            }

            public async Task<Book> Handle(Query request, CancellationToken cancellationToken)
            {
                var bookRetrieved = await _dataContext.LoadAsync<Book>(request.ISBN, cancellationToken);
                return bookRetrieved;
            }
        }
    }
}
