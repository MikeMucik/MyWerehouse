using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr
{
	public class PalletSimplyDTO : IMapFrom<Pallet>
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet,  PalletSimplyDTO>();
		}
	}
}
