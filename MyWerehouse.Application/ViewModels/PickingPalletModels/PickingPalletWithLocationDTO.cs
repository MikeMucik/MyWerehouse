using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.PickingPalletModels
{
	public class PickingPalletWithLocationDTO
	{
		public string PalletId { get; set;}
		public string LocationName { get; set; }
		public DateTime AddedToPicking { get; set; }		
	}
}
