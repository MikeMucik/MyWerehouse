using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.Queries.GetIssueById;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/issues")]
	public class IssuesController : ControllerBase
	{
		private readonly IMediator _mediator;

		public IssuesController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
		public async Task<IActionResult> Create([FromBody]CreateNewIssueCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var result = await _mediator.Send(new GetIssueByIdQuery(id));
			return Ok(result);
		}
	}
}
