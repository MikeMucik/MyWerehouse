using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Results
{
	public class PickingResult
	{
		public bool Success { get; set; }
		public bool RequiresOrderNumber { get; set; }
		public string Message { get; set; }
		public List<IssueOptions> IssueOptions { get; set; } = new List<IssueOptions>();
		public string ProductInfo { get; set; }
		public PickingResult() { }
		public static PickingResult Ok(
			string message ,
			string productInfo = null,
			List<IssueOptions> issueOptions = null)
		{
			return new PickingResult
			{
				Success = true,
				Message = message,
				ProductInfo = productInfo,
				IssueOptions = issueOptions ?? new List<IssueOptions>()
			};
		}	
		public static PickingResult RequiresOrder(
			string productInfo,
			List<IssueOptions> issueOptions,
			string message)
		{
			return new PickingResult
			{
				Success = false,
				RequiresOrderNumber = true,
				ProductInfo = productInfo,
				IssueOptions = issueOptions ?? new List<IssueOptions>(),
				Message = message
			};
		}
		public static PickingResult Fail(string message)
		{
			return new PickingResult
			{
				Success = false,
				Message = message
			};
		}		
	}
}
