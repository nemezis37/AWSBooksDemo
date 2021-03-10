using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Books;
using Domain;
using MediatR;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BooksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Book book)
        {
            var result = await _mediator.Send(new Create.Command {Book = book});
            return Ok(result);
        }

        [HttpGet("{isbn}")]
        public async Task<IActionResult> Get(string isbn)
        {
            var result = await _mediator.Send(new Get.Query {ISBN = isbn});
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new ListItems.Query());
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Book book)
        {
            var result = await _mediator.Send(new Update.Command {Book = book});
            return Ok(result);
        }

        [HttpDelete("{isbn}")]
        public async Task<IActionResult> Delete(string isbn)
        {
            var result = await _mediator.Send(new Delete.Command {ISBN = isbn});
            return Ok(result);
        }
    }
}
