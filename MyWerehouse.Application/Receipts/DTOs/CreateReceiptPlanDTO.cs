using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.DTOs
{
	public class CreateReceiptPlanDTO :IMapFrom<Receipt>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }		
		public DateTime ReceiptDateTime { get; set; }		
		public string PerformedBy { get; set; } 
		public ReceiptStatus ReceiptStatus { get; set; }		
		public int RampNumber { get; set; }		
	}
}
