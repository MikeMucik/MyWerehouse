using FluentValidation;

namespace MyWerehouse.Application.Pickings.Queries.ShowTaskToDo
{
	public class ShowTaskToDoQueryValidator : AbstractValidator<ShowTaskToDoQuery>
	{
		public ShowTaskToDoQueryValidator()
		{
			RuleFor(query => query.PalletSourceScannedId)
				.NotEmpty()
				.WithMessage("Source pallet ID must be specified.");
		}
	}
}
