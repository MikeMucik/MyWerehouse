using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.DomainExceptions;

namespace MyWerehouse.Server.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		public ExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}					
			catch (DomainException ex)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsJsonAsync(ex.Message);
			}
			catch (ValidationException ex)
			{
				context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
				await context.Response.WriteAsJsonAsync(new
				{
					//Error = ex.Error.Select(ex => new
					//{
					//	Field = ex.PropertName,
					//	Message = ex.ErrorMessage
					//})
					error = ex.Message
				});
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);				
			}
		}
		private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
		{
			context.Response.StatusCode = 500;
			context.Response.ContentType = "application/json";
			var respone = new
			{
				StatusCodes = 500,
				Message = "Wystąpił błąd serwera",
				
			};
			await context.Response.WriteAsJsonAsync(respone);	
		}
	}
}
//catch (NotFoundException ex)
//{
//	context.Response.StatusCode = StatusCodes.Status404NotFound;
//	await context.Response.WriteAsJsonAsync(ex.Message);
//}