using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common.ValueObject;

namespace MyWerehouse.Application.Tests.Common
{
	public class AppResultTests
	{
		[Fact]
		public void AppResultFail_ShouldUseNotFound_WhenErrorTypeIsNotProvided()
		{
			var result = AppResult<int>.Fail("Error");

			Assert.Equal(ErrorType.NotFound, result.ErrorType);
		}
	}
}
