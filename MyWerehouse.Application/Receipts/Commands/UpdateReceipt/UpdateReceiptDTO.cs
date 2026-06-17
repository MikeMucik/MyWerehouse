using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptDTO : IMapFrom<Receipt>
	{		
		public int ClientId { get; init; }
		public DateTime ReceiptDateTime { get; init; }
		public ICollection<EditPalletInReceiptDTO> Pallets { get; init; } = new List<EditPalletInReceiptDTO>();
		public string PerformedBy { get; init; } 
		public ReceiptStatus ReceiptStatus { get; init; }
		public int RampNumber { get; init; }		
	}
}