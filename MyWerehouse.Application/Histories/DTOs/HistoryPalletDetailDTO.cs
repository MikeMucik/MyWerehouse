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
	public class HistoryPalletDetailDTO : IMapFrom<HistoryPalletDetail>
	{
		public int Id { get; set; }
		public int PalletMovementId { get; set; }		
		public Guid ProductId { get; set; }		
		public int QuantityChange { get; set; } //+/-
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HistoryPalletDetail, HistoryPalletDetailDTO>()
				.ForMember(dest=>dest.QuantityChange, opt=>opt.MapFrom(src=>src.Quantity));
		}
	}
}
