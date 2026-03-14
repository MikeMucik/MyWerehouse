using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssuePalletsWithLocationDTO
	{
		public int IssueNumber { get; set; }
		public required List<PalletWithLocationDTO> PalletList { get; set; } = new List<PalletWithLocationDTO>();
	}
}
