using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Histories.Filters;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Services
{
	public class HistoryService : IHistoryService
	{
		private readonly IPalletMovementRepo _palletMovementRepo;		
		private readonly IPalletRepo _palletRepo;
		private readonly IMapper _mapper;

		public HistoryService(
			IPalletMovementRepo palletMovementRepo		
			, IPalletRepo palletRepo
			, IMapper mapper)
		{
			_palletMovementRepo = palletMovementRepo;			
			_palletRepo = palletRepo;
			_mapper = mapper;			
		}		
		//Read history
		public async Task<PalletHistoryDTO> GetHistoryPalletByIdAsync(Guid id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id);
			var history = _mapper.Map<PalletHistoryDTO>(pallet);
			var filter = new PalletMovementSearchFilter { };
			var details = await _palletMovementRepo.GetDataByFilter(filter, id)
				.OrderByDescending(a => a.MovementDate)
			 .ProjectTo<PalletMovementDTO>(_mapper.ConfigurationProvider)
			 .ToListAsync();
			foreach (var item in details)
			{
				history.PalletMovementsDTO.Add(item);
			}
			return history;
		}

		public Task<PickingPalletHistoryDTO> GetHistoryPickingPalletByIdAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task<ReceiptHistoryDTO> GetHistoryReceiptByIdAsync(string id)
		{
			throw new NotImplementedException();
		}

		Task<IssueHistoryDTO> IHistoryService.GetHistoryIssueByIdAsync(string id)
		{
			throw new NotImplementedException();
		}
	}
}
