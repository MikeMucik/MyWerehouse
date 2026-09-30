using Microsoft.AspNetCore.Mvc;

namespace MyWerehouse.Server.ApiProblemDetails
{
	public static class ApiProblemDetailsFactory
	{
		public static ProblemDetails Create(
			ErrorType errorType,
			string detail,
			object? details = null)
		{
			var (status, title) = errorType switch
			{
				ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
				ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
				ErrorType.Conflict => (StatusCodes.Status409Conflict, "Business rule violation"),
				ErrorType.InternalError => (StatusCodes.Status500InternalServerError, "Internal server eror"),
				_ => (StatusCodes.Status500InternalServerError, "Internal server error")
			};
			var problem = new ProblemDetails
			{
				Title = title,
				Detail = status == StatusCodes.Status500InternalServerError ? 
				"An unexpected internal error occured.": detail,
				Status = status,
			};
			if (details != null)
			{
				problem.Extensions["details"] = details;
			}
			return problem;
		}
	}
}
