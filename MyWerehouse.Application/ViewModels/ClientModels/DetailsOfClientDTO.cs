using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class DetailsOfClientDTO :IMapFrom<Client>
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
		public virtual ICollection<Address> Address { get; set; }
		public virtual ICollection<Receipt> Receipts { get; set; } 
		public virtual ICollection<Issue> Issues { get; set; } 
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Client, DetailsOfClientDTO>()
				.ForMember(dest=>dest.Address, opt=>opt.MapFrom(src=>src.Addresses))
				.ForMember(dest=>dest.Receipts, opt=>opt.MapFrom(src=>src.Receipts))
				.ForMember(dest=>dest.Issues, opt=>opt.MapFrom(src=>src.Issues));
		}
	}
}
