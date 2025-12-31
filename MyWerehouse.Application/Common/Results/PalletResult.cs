using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Results
{
	public class PalletResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string PalletId { get; set; }
		public PalletResult() { }
		public static PalletResult Ok(
			string message,
			string palletId)
		{
			return new PalletResult
			{
				Success = true,
				Message = message,
				PalletId = palletId
			};
		}
		public static PalletResult Ok(
			string message)
		{
			return new PalletResult
			{
				Success = true,
				Message = message
			};
		}
		public static PalletResult Fail(string message,
			string palletId)
		{
			return new PalletResult
			{
				Success = false,
				Message = message,
				PalletId = palletId
			};
		}
		public static PalletResult Fail(string message)
		{
			return new PalletResult
			{
				Success = false,
				Message = message
			};
		}
	}
}
