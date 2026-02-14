using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainIssueException : DomainException
	{
		public string PalletId { get; set; }
		public int IssueId { get; set; }
		public DomainIssueException(string palletId)
			: base($"Błąd przy zapisie do bazy palety o numerze {palletId}.")
		{
			PalletId = palletId;
		}
		public DomainIssueException(int issueId)
			: base($"Błąd przy operacji przyjęcia {issueId}.")
		{
			IssueId = issueId;
		}
	}
}
