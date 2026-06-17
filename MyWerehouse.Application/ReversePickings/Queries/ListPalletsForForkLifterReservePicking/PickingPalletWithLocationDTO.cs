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
		public string PalletNumber { get; init; }
		public string LocationName { get; init; }
		public int LocationId { get; init; }
		public PalletStatus Status { get; init; }
	}
}
