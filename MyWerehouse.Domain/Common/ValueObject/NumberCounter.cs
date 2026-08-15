using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Common.ValueObject
{
	public class NumberCounter
	{
		public string Name { get; set; } = string.Empty;
		public int NextNumber { get; set; }
	}
}
