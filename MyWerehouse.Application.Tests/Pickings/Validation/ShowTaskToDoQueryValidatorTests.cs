using FluentValidation.TestHelper;
using MyWerehouse.Application.Pickings.Queries.ShowTaskToDo;
using TestSupport;

namespace MyWerehouse.Application.Tests.Pickings.Validation
{
	public class ShowTaskToDoQueryValidatorTests
	{
		private readonly ShowTaskToDoQueryValidator _validator = new();

		[Fact]
		public void MissingPickingDate_ShouldNotReturnValidationError()
		{
			//Arrange
			var query = new ShowTaskToDoQuery(Guid.NewGuid(), null, 1, 10);
			//Act&Assert
			_validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(request => request.PickingDate);
		}

		[Fact]
		public void MissingPalletId_ShouldReturnValidationError()
		{
			//Arrange
			var query = new ShowTaskToDoQuery(Guid.Empty, TestDates.Today, 1, 10);
			//Act&Assert
			_validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(request => request.PalletSourceScannedId);
		}

		[Fact]
		public void ValidQuery_ShouldNotReturnValidationError()
		{
			//Arrange
			var query = new ShowTaskToDoQuery(Guid.NewGuid(), TestDates.Today, 1, 10);
			//Act&Assert
			_validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
		}
	}
}
