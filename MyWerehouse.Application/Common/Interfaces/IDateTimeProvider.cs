namespace MyWerehouse.Application.Common.Interfaces
{
	public interface IDateTimeProvider
	{
		DateTime UtcNow { get; }
		DateTime TodayDateTime { get; }
		DateOnly Today { get; }
	}
}
