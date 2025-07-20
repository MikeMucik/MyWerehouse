using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.InventoryModels
{
	public class InventoryDTO :IMapFrom<Inventory>
	{
		public int ProductId { get; set; }		
		public int Quantity { get; set; }
		public DateTime LastUpdated { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<InventoryDTO, Inventory>();
		}
	}
}
