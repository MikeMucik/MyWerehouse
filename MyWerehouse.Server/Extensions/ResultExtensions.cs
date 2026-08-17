using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Server.ApiProblemDetails;

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
			var problem = ApiProblemDetailsFactory.Create(
				result.ErrorType,
				result.Error ?? "Unexpected Error",
				result.Result);
			return new ObjectResult(problem)
			{
				StatusCode = problem.Status,
			};
		}		
	}
}
