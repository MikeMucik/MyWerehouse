using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class PalletToLoadDTO
	{
		public Guid PalletId { get; set;}
		public string PalletNumber { get; set;}
		public string LocationName { get; set;}//
		public int LocationId { get; set;}
		public PalletStatus PalletStatus { get; set;}
		public List<ProductOnPalletIssueDTO> ProductOnPalletIssue { get; set;} = new List<ProductOnPalletIssueDTO>();
	}
}
