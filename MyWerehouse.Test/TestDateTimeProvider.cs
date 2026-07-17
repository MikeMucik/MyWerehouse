using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Test
{
	public class TestDateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => TestDates.UtcNow;

		public DateTime TodayDateTime => TestDates.TodayDateTime;

		public DateOnly Today => TestDates.Today;
	}
}
