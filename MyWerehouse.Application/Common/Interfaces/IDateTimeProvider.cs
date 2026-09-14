using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Interfaces
{
	public interface IDateTimeProvider
	{
		DateTime UtcNow { get; }
		DateTime TodayDateTime { get; }
		DateOnly Today { get; }
	}
}
