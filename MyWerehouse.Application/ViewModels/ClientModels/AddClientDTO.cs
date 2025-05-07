using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class AddClientDTO : IMapFrom<Client>
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
		public string FullName { get; set; }
		public ICollection<AddressDTO> Addresses { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Client, AddClientDTO>()
				.ForMember(dest=>dest.Addresses, opt=>opt.MapFrom(src=>src.Addresses))
				.ReverseMap();
		}
	}
}
