using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class ClientDTO : IMapFrom<Client>
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
		public string FullName { get; set; }
		public ICollection<AddAddressDTO> Addresses { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Client, ClientDTO>()
				.ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses))
				;
				//.ReverseMap();
		}
	}	
}
