using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.DateTimeProvider
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => DateTime.UtcNow;

		public DateTime TodayDateTime => DateTime.UtcNow.Date;

		public DateOnly Today => DateOnly.FromDateTime(UtcNow);
	}
}
