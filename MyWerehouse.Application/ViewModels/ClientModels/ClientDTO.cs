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
		public int Id { get; init; }
		public string Name { get; init; } = string.Empty;
		public string Email { get; init; } = string.Empty;
		public string Description { get; init; } = string.Empty;
		public string FullName { get; init; } = string.Empty;
		public ICollection<AddAddressDTO> Addresses { get; init; } = new List<AddAddressDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Client, ClientDTO>()
				.ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses));
		}
	}	
}
