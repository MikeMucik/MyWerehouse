using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Results
{
	public enum ErrorType
	{
		Validation,
		NotFound,
		Conflict,
		Technical
	}
	public class AppResult<T>
	{
		public bool IsSuccess { get; private set; }
		public T? Result { get; private set; }
		public string? Error { get; private set; }
		public string? Message { get; private set; }
		public ErrorType ErrorType { get; private set; }
		private AppResult() { }
		//sukces
		public static AppResult<T> Success(T value, string message) =>
			new AppResult<T> { IsSuccess = true, Message= message,  Result = value };
		public static AppResult<T> Success(T value) =>
			new AppResult<T> { IsSuccess = true, Result = value };
		//porażka
		public static AppResult<T> Fail(string error, ErrorType errorType = ErrorType.Validation) =>
			new AppResult<T> { IsSuccess = false, Error = error, ErrorType = errorType };
	}
}
