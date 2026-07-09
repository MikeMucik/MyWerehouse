using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Server.Extensions
{
	public static class ResultExtensions
	{
		public static IActionResult ToActionResult<T>(this AppResult<T> result)
		{
			if (result.IsSuccess)
			{
				if (typeof(T) == typeof(Unit))
				{
					return new OkObjectResult(new
					{
						result.IsSuccess,
						result.Message,
					});
				}
				return new OkObjectResult(result.Result);
			}
			return result.ErrorType switch
			{
				ErrorType.NotFound => new NotFoundObjectResult(new ProblemDetails
				{
					Title = "Resource not found",
					Detail = result.Error,
					Status = StatusCodes.Status404NotFound,
				}),
				ErrorType.Conflict => new ConflictObjectResult(new ProblemDetails
				{
					Title = "Resource conflict",
					Detail = result.Error,
					Status = StatusCodes.Status409Conflict
				}),
				ErrorType.Validation => new BadRequestObjectResult(new ProblemDetails				
				{
					Title = "Validation error",
					Detail = result.Error,
					Status = StatusCodes.Status400BadRequest
				}),
				_ => new ObjectResult(new ProblemDetails
				{
					Title = "Internal server error",
					Detail = "Unexpected error",
					Status = StatusCodes.Status500InternalServerError
				})
				{
					StatusCode = StatusCodes.Status500InternalServerError
				}
			};
		}
	}
}
