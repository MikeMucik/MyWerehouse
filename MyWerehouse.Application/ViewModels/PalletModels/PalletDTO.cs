using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.PalletModels
{
	public class PalletDTO : IMapFrom<Pallet>//TODO Search
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }		
		public PalletStatus Status { get; set; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPallet> ProductsOnPallet { get; set; } = new HashSet<ProductOnPallet>();
		public ICollection<PalletMovement> PalletMovements { get; set; } = new List<PalletMovement>();
		public int? ReceiptId { get; set; }		
		public int? IssueId { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, PalletDTO>();
		}
	}
}
