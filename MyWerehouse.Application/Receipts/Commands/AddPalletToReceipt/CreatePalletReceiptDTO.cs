using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class CreatePalletReceiptDTO
	{		
		public Guid Id { get; init ; }
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletDTO>();	
		public Guid ReceiptId { get; init; }		
		public int ReceiptNumber { get; init; }		
		public string UserId { get; init; }		
	}	
}
