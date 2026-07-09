using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Server.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionMiddleware> _logger;
		public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}
		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}					
			catch (DomainException ex)
			{
				_logger.LogWarning(ex, "Domain exception while processing request {Method} {Path}",
					context.Request.Method, context.Request.Path);

				await HandleDomainException(context, ex);				
			}
			catch (FluentValidation.ValidationException ex)
			{
				await HandleValidationException(context, ex);				
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception occurred while processing request {Method} {Path}",
					context.Request.Method, context.Request.Path);				

				await HandleExceptionAsync(context);
			}
		}
		private static Task HandleValidationException(HttpContext context, FluentValidation.ValidationException ex)
		{
			var errors = ex.Errors
				.GroupBy(e => e.PropertyName)
				.ToDictionary(
				g => g.Key,
				g => g.Select(e => e.ErrorMessage)
				.ToArray());
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			context.Response.ContentType = "application/problem+json";
			var response = new ValidationProblemDetails(errors)
			{
				Title = "Validation error",				
				Status = StatusCodes.Status400BadRequest
			};
			return context.Response.WriteAsJsonAsync(response);
		}
		private static Task HandleDomainException(HttpContext context, Exception ex)
		{
			context.Response.StatusCode = StatusCodes.Status409Conflict;
			context.Response.ContentType = "application/problem+json";
			var response = new ProblemDetails
			{
				Title = "Business rule violation",
				Detail = ex.Message,
				Status = StatusCodes.Status409Conflict,
			};
			return context.Response.WriteAsJsonAsync(response);
		}
		private static async Task HandleExceptionAsync(HttpContext context)
		{
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			context.Response.ContentType = "application/problem+json";
			var response = new ProblemDetails
			{
				Title = "Internal server error",
				Status = StatusCodes.Status500InternalServerError,
				Detail = "Unexpected Error"
			};
			response.Extensions["traceId"] = context.TraceIdentifier;
			await context.Response.WriteAsJsonAsync(response);	
		}
	}
}