using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.Queries.LoadingIssueList
{
	public class PalletToLoadDTO
	{
		public Guid PalletId { get; init;}
		public string PalletNumber { get; init;}
		public string LocationName { get; init;}//
		public int LocationId { get; init;}
		public PalletStatus PalletStatus { get; init;}
		public List<ProductOnPalletIssueDTO> ProductOnPalletIssue { get; init;} = new List<ProductOnPalletIssueDTO>();
	}
}
