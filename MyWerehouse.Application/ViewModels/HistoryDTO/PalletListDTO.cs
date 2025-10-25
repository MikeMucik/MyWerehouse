using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.HistoryDTO
{
	public class PalletListDTO : IMapFrom<HistoryReceiptDetail>, IMapFrom<HistoryIssueDetail>
								
	{
		public string PalletId { get; set; }
		public int LocationId { get; set; }
		public string LocationSnapShot { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HistoryReceiptDetail, PalletListDTO>();
			profile.CreateMap<HistoryIssueDetail, PalletListDTO>();
		}
	}
}
