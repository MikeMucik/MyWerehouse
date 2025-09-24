using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Results
{
	public class ManualPickingResult
	{
		public bool Success { get; set; }
		public bool RequiresOrderNumber { get; set; }
		public string Message { get; set; }
		public List<IssueOptions> IssueOptions { get; set; } = new List<IssueOptions>();
		public string ProductInfo { get; set; }

		public ManualPickingResult() { }

		public static ManualPickingResult Ok(
			string message ,
			string productInfo = null,
			List<IssueOptions> issueOptions = null)
		{
			return new ManualPickingResult
			{
				Success = true,
				Message = message,
				ProductInfo = productInfo,
				IssueOptions = issueOptions ?? new List<IssueOptions>()
			};
		}	

		public static ManualPickingResult RequiresOrder(
			string productInfo,
			List<IssueOptions> issueOptions,
			string message)
		{
			return new ManualPickingResult
			{
				Success = false,
				RequiresOrderNumber = true,
				ProductInfo = productInfo,
				IssueOptions = issueOptions ?? new List<IssueOptions>(),
				Message = message
			};
		}

		public static ManualPickingResult Fail(string message)
		{
			return new ManualPickingResult
			{
				Success = false,
				Message = message
			};
		}		
	}
}
