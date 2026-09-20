using MyWerehouse.Application.Common.Interfaces;

namespace TestSupport
{
    public class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => TestDates.UtcNow;

        public DateTime TodayDateTime => TestDates.TodayDateTime;

        public DateOnly Today => TestDates.Today;
    }
}
