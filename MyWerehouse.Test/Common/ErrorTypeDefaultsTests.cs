using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Test.Common
{
	public class ErrorTypeDefaultsTests
	{
		[Fact]
		public void AppResultFail_ShouldUseNotFound_WhenErrorTypeIsNotProvided()
		{
			var result = AppResult<int>.Fail("Error");

			Assert.Equal(ErrorType.NotFound, result.ErrorType);
		}

		[Fact]
		public void DomainException_ShouldUseConflict_WhenErrorTypeIsNotProvided()
		{
			var exception = new TestDomainException();

			Assert.Equal(ErrorType.Conflict, exception.ErrorType);
		}

		private sealed class TestDomainException : DomainException
		{
			public TestDomainException()
				: base("Error")
			{
			}
		}
	}
}
