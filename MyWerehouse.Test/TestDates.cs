using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Test
{
	public class TestDates
	{
		public static readonly DateTime Now = new DateTime(2026, 1, 15, 10, 0, 0);
		public static readonly DateOnly Today = DateOnly.FromDateTime(Now);

		public static DateTime DaysAgo(int days) => Now.AddDays(-days);
		public static DateTime DaysIn(int days) => Now.AddDays(days);
		public static DateOnly DateDaysIn(int days) => Today.AddDays(days);
		public static DateOnly DateBestBeforeInMonth(int month) => Today.AddMonths(month);
	}
}
