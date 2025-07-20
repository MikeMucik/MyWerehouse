using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class PalletToLoadDTO
	{
		public string PalletId { get; set;}
		public string LocationName { get; set;}
		public int LocationId { get; set;}
		public PalletStatus PalletStatus { get; set;}
		public List<ProductOnPalletIssueDTO> ProductOnPalletIssue { get; set;} = new List<ProductOnPalletIssueDTO>();
	}
}
