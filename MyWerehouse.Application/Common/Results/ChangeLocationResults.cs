using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Results
{
	public class ChangeLocationResults
	{
		public bool Success { get; set; }
		public bool RequiresConfirmation { get; set; }
		public string Message { get; set; }
	}
}
