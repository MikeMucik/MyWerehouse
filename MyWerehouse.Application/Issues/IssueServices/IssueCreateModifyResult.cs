using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public sealed class IssueCreateModifyResult
	{
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public string Message { get; set; } = string.Empty;
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public IReadOnlyList<AssignProductToIssueResult>? Results { get; init; } 

		public IssueCreateModifyResult() { }
		public IssueCreateModifyResult (
			Guid issueId, 
			int issueNumber,
			string message,
			IReadOnlyList<AssignProductToIssueResult>? results = null)
		{
			{
				IssueId = issueId;
				IssueNumber = issueNumber;
				Message = message;
				Results = results;
			};
		}		
	}
}
