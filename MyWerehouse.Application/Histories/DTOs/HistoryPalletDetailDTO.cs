using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class HistoryPalletDetailDTO 
	{		
		public Guid ProductId { get; set; }		
		public int QuantityChange { get; set; } //+/-		
	}
}
