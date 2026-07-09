using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class DetailsOfClientDTO : IMapFrom<Client>
	{
		public int Id { get; init; }
		public string Name { get; init; } = string.Empty;
		public string Email { get; init; } = string.Empty;
		public string Description { get; init; } = string.Empty;
		public ICollection<Address> Address { get; init; } = new List<Address>();
		public ICollection<Receipt> Receipts { get; init; } = new List<Receipt>();
		public ICollection<Issue> Issues { get; init; } = new List<Issue>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Client, DetailsOfClientDTO>()
				.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Addresses))
				.ForMember(dest => dest.Receipts, opt => opt.MapFrom(src => src.Receipts))
				.ForMember(dest => dest.Issues, opt => opt.MapFrom(src => src.Issues));
		}
	}
}
