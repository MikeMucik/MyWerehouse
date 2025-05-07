using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.AddressModels
{
	public class AddressDTO : IMapFrom<Address>
	{
		public int Id { get; set; }
		public string FullName { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public int Phone { get; set; }
		public string PostalCode { get; set; }
		public string StreetName { get; set; }
		public string StreetNumber { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Address, AddressDTO>()
				.ReverseMap();
		}
	}
}
