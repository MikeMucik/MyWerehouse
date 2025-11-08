using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Commands.History
{
	public interface IHistoryEvent
	{
		string UserId { get; }
		DateTime DateTime { get; }
	}
}
