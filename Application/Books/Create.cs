﻿using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Domain;
using MediatR;

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

            public Handler(IDynamoDBContext dataContext)
            {
                _dataContext = dataContext;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                await _dataContext.SaveAsync(request.Book, cancellationToken);
                return Unit.Value;
            }
        }
    }
}
