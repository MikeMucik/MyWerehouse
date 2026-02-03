using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Common.Results
{
	public class AssignPallestResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int FullPallet { get; set; }
		//public List<Pallet> ListPallet { get; set; }
		public int Rest {  get; set; }
		public AssignPallestResult()	{	}
		public static AssignPallestResult Ok(//List<Pallet> listPallest,
											 int fullPallet,  int rest)
		{
			return new AssignPallestResult
			{
				Success = true,
				FullPallet = fullPallet,
				Rest = rest
			};
		}
		public static AssignPallestResult Fail(string message)
		{
			return new AssignPallestResult
			{
				Success = false,
				Message = message
			};
		}
	}
}
