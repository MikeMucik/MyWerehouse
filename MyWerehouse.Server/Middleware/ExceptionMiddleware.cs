using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions;
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
				_logger.LogWarning(ex, "Domain exception while proccessing request{Methid} {Path}",
					context.Request.Method, context.Request.Path);

				await HandleDomainException(context, ex);				
			}
			catch (FluentValidation.ValidationException ex)
			{
				context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
				await context.Response.WriteAsJsonAsync(new
				{					
					error = ex.Message
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception occured while proccessing request{Methid} {Path}",
					context.Request.Method, context.Request.Path);				

				await HandleExceptionAsync(context);
			}
		}
		private static Task HandleDomainException(HttpContext context, Exception ex)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			var response = new
			{
				error = ex.GetType().Name,
				message = ex.Message,
			};
			return context.Response.WriteAsJsonAsync(response);
		}
		private static async Task HandleExceptionAsync(HttpContext context)
		{
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			context.Response.ContentType = "application/json";
			var response = new
			{
				StatusCodes = 500,
				Message = "Wystąpił błąd serwera",
				
			};
			await context.Response.WriteAsJsonAsync(response);	
		}
	}
}