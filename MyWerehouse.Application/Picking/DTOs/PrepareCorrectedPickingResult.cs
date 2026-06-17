using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking;

namespace MyWerehouse.Application.Picking.DTOs
{
	public sealed class PrepareCorrectedPickingResult
	{
		public bool Success { get; init; }
		public bool RequiresIssueSelection { get; init; }
		public string Message { get; init; } = string.Empty;//
		public string? ProductInfo { get; init; }
		public IReadOnlyList<IssueOptions> IssueOptions { get; init; } = [];
		public static PrepareCorrectedPickingResult Fail(string message)
		=> new() { Success = false, Message = message };
		public static PrepareCorrectedPickingResult RequiresOrder(
			string productInfo,
			IReadOnlyList<IssueOptions> issueOptions,
			string message)
			=> new()
			{
				Success = true,
				RequiresIssueSelection = true,
				ProductInfo = productInfo,
				IssueOptions = issueOptions,
				Message = message
			};
	}
}