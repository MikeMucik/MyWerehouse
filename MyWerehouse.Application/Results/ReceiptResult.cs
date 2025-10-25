using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Results
{
	public class ReceiptResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int ReceiptId { get; set; }		
		public string PalletId { get; set; }

		public ReceiptResult() { }

		public static ReceiptResult Ok(
			string message,
			int receiptId)
		{
			return new ReceiptResult
			{
				Success = true,
				Message = message,
				ReceiptId = receiptId
			};
		}
		public static ReceiptResult Ok(
			string message,
			string palletId)
		{
			return new ReceiptResult
			{
				Success = true,
				Message = message,
				PalletId = palletId,
			};
		}
		public static ReceiptResult Fail(
			string message,
			string palletId)
		{
			return new ReceiptResult
			{
				Success = false,
				Message = message,
				PalletId = palletId,
			};
		}
		public static ReceiptResult Fail(string message)
		{
			return new ReceiptResult
			{
				Success = false,
				Message = message
			};
		}
	}
}
