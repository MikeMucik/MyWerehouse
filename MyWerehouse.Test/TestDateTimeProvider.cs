using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Interfaces;

namespace MyWerehouse.Test
{
	public class TestDateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => TestDates.UtcNow;

		public DateTime TodayDateTime => TestDates.TodayDateTime;

		public DateOnly Today => TestDates.Today;
	}
}
