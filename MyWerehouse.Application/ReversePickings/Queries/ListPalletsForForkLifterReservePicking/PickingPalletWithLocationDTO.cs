using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking
{ 
	public class PickingPalletWithLocationDTO 
	{
		public Guid PalletId { get; init; }
		public string PalletNumber { get; init; } = string.Empty;
		public string LocationName { get; init; } = string.Empty;
		public int LocationId { get; init; }
		public PalletStatus Status { get; init; }
	}
}
