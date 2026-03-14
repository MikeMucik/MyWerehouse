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
				if (typeof(T) == typeof(Unit)) return new OkResult();
				return new OkObjectResult(result.Result);
			}
			return result.ErrorType switch
			{
				ErrorType.NotFound => new NotFoundObjectResult(result.Error),
				ErrorType.Conflict => new ConflictObjectResult(result.Error),
				ErrorType.Validation => new BadRequestObjectResult(result.Error),
				_ => new BadRequestObjectResult(result.Error)
			};


			//if (result.IsSuccess)
			//	return new OkResult();

			//return new BadRequestObjectResult(result.Error);

		}
	}
}
