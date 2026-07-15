using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.DateTimeProvider;

namespace MyWerehouse.Test
{
	public class TestDateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => new(2026, 7, 15, 16, 0, 0);

		public DateTime TodayDateTime => throw new NotImplementedException();

		public DateOnly Today => throw new NotImplementedException();
	}
}
