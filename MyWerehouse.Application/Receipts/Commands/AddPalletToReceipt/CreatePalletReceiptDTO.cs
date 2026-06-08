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
		public Guid Id { get; set ; }
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();	
		public Guid ReceiptId { get; set; }		
		public int ReceiptNumber { get; set; }		
		public string UserId { get; set; }		
	}	
}
