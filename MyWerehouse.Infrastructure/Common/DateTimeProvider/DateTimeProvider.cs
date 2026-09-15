using MyWerehouse.Application.Common.Interfaces;

namespace MyWerehouse.Infrastructure.Common.DateTimeProvider
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => DateTime.UtcNow;

		public DateTime TodayDateTime => DateTime.UtcNow.Date;

		public DateOnly Today => DateOnly.FromDateTime(UtcNow);
	}
}
