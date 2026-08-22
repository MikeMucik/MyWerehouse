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
		public bool Success { get; set; }
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public string Message { get; set; } = string.Empty;
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<AssignProductToIssueResult>? Results { get; init; }

		public IssueCreateModifyResult() { }
		public static IssueCreateModifyResult Ok(
			Guid issueId, 
			int issueNumber,
			string message,
			List<AssignProductToIssueResult> results)
		{
			return new IssueCreateModifyResult
			{
				Success = true,
				IssueId = issueId,
				IssueNumber = issueNumber,
				Message = message,
				Results = results
			};
		}
		public static IssueCreateModifyResult Ok(
			Guid issueId,
			int issueNumber,
			string message)
		{
			return new IssueCreateModifyResult
			{
				Success = true,
				IssueId = issueId,
				IssueNumber = issueNumber,
				Message = message
			};
		}
		public static IssueCreateModifyResult Fail(
			Guid issueId,
			int issueNumber,
			string message)
		{
			return new IssueCreateModifyResult
			{
				Success = false,
				IssueId = issueId,
				IssueNumber = issueNumber,
				Message = message
			};
		}
		public static IssueCreateModifyResult Fail(
			
			string message)
		{
			return new IssueCreateModifyResult
			{
				Success = false,				
				Message = message
			};
		}
	}
}
