using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class PalletWithLocationDTO : IMapFrom<Pallet>
	{
		public string PalletId { get; set; }
		public string LocationName { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet,  PalletWithLocationDTO>()
				.ForMember(dest=>dest.LocationName, opt=>opt.MapFrom(src=>src.Location.Bay + " " + src.Location.Aisle + " " + src.Location.Position + " " + src.Location.Height));
		}
	}
}
