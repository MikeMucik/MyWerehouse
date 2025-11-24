using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Receipts.DTOs
{
	public class CreateReceiptPlanDTO :IMapFrom<Receipt>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }		
		public DateTime ReceiptDateTime { get; set; }		
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }		
		//public class CreateReceiptPlanDTOValidation : AbstractValidator<CreateReceiptPlanDTO>
		//{
		//	public CreateReceiptPlanDTOValidation()
		//	{
		//		RuleFor(x => x.ClientId)
		//			.GreaterThan(0).WithMessage("Numer klienta wymagany");
		//		RuleFor(x => x.PerformedBy)
		//			.NotEmpty().WithMessage("Użytkownik wymagany");
		//	}
		//}
	}
}
