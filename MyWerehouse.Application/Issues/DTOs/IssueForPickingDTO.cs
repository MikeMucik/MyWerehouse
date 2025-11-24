using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueForPickingDTO
	{
		public int IssueId { get; set; }
		public List<ProductOnPalletPickingDTO> Products { get; set; } = new List<ProductOnPalletPickingDTO>();
	}
}
