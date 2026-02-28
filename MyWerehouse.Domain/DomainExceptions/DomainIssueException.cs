using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainIssueException : DomainException
	{		
		public int IssueNumber { get; set; }
		public string Message { get; set; }
		
		public DomainIssueException(int issueId)
			: base($"Błąd przy operacji wydania {issueId}.")
		{
			IssueNumber = issueId;
		}
		public DomainIssueException(string message)	:
			base(message) 
		{
			Message = message;
		}
	}
}
