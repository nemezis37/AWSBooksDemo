using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using MediatR;

namespace Application.Books
{
    public class ListItems
    {
        public class Query : IRequest<List<Book>>
        {
        }

        public class Handler : IRequestHandler<Query, List<Book>>
        {
            private readonly IDynamoDBContext _dataContext;

            public Handler(IDynamoDBContext dataContext)
            {
                _dataContext = dataContext;
            }

            public async Task<List<Book>> Handle(Query request, CancellationToken cancellationToken)
            {
                var conditions = new List<ScanCondition>();
                var books = await _dataContext.ScanAsync<Book>(conditions).GetRemainingAsync(cancellationToken);
                return books;
            }
        }
    }
}
