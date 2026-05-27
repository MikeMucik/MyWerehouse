using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueOptions
	{
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public int QunatityToDo { get; set; }
	}
}
