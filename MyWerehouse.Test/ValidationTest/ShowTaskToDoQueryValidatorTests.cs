using FluentValidation.TestHelper;
using MyWerehouse.Application.Picking.Queries.ShowTaskToDo;

namespace MyWerehouse.Test.ValidationTest
{
	public class ShowTaskToDoQueryValidatorTests
	{
		private readonly ShowTaskToDoQueryValidator _validator = new();

		[Fact]
		public void MissingPickingDate_ShouldNotReturnValidationError()
		{
			var query = new ShowTaskToDoQuery(Guid.NewGuid(), null, 1, 10);

			_validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(request => request.PickingDate);
		}

		[Fact]
		public void MissingPalletId_ShouldReturnValidationError()
		{
			var query = new ShowTaskToDoQuery(Guid.Empty, TestDates.Today, 1, 10);

			_validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(request => request.PalletSourceScannedId);
		}

		[Fact]
		public void ValidQuery_ShouldNotReturnValidationError()
		{
			var query = new ShowTaskToDoQuery(Guid.NewGuid(), TestDates.Today, 1, 10);

			_validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
		}
	}
}
