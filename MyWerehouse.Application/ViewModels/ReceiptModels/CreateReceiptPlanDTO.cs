using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.ReceiptModels
{
	public class CreateReceiptPlanDTO :IMapFrom<Receipt>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }		
		public DateTime ReceiptDateTime { get; set; }		
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<CreateReceiptPlanDTO, Receipt>();
		}
	}
}
