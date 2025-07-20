using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class IssuePalletsWithLocationDTO
	{
		public int IssueId { get; set; }
		public required List<PalletWithLocation> PalletList { get; set; } = new List<PalletWithLocation>();
	}
}
