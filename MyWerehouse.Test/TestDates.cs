namespace MyWerehouse.Test
{
	public static class TestDates
	{
		public static readonly DateTime UtcNow = new(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);
		public static readonly DateTime Now = UtcNow;
		public static readonly DateTime TodayDateTime = UtcNow.Date;
		public static readonly DateOnly Today = DateOnly.FromDateTime(UtcNow);

		public static DateTime DaysAgo(int days) => UtcNow.AddDays(-days);
		public static DateTime DaysIn(int days) => UtcNow.AddDays(days);
		public static DateOnly DateDaysAgo(int days) => Today.AddDays(-days);
		public static DateOnly DateDaysIn(int days) => Today.AddDays(days);
		public static DateOnly DateBestBeforeInMonths(int month) => Today.AddMonths(month);
	}
}
