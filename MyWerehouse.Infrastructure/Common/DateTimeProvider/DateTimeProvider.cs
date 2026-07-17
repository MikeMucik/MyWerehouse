using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Infrastructure.Common.DateTimeProvider
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => DateTime.UtcNow;

		public DateTime TodayDateTime => DateTime.UtcNow.Date;

		public DateOnly Today => DateOnly.FromDateTime(UtcNow);
	}
}
