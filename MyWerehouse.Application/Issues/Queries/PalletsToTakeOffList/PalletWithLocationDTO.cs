using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList
{
	public class PalletWithLocationDTO 
	{
		public Guid PalletId { get; set; }
		public string PalletNumber { get; set; } = string.Empty;
		public string LocationName { get; set; } = string.Empty;
		public int LocationId { get; set; }
		public PalletStatus Status { get; set; }
	}
}
