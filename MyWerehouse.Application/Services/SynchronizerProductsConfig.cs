using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Services
{
	public class SynchronizerProductsConfig  : ISynchronizerProductsConfig
	{
		private readonly IMapper _mapper;
		public SynchronizerProductsConfig(IMapper mapper)
		{
			_mapper = mapper;
		}
		public void SynchronizeProducts(Pallet pallet, IEnumerable<ProductOnPalletDTO> productDto)
		{
			foreach (var dto in productDto)
			{
				dto.PalletId = pallet.Id;
				var existing = pallet.ProductsOnPallet
					.FirstOrDefault(p => p.ProductId == dto.ProductId && p.PalletId == dto.PalletId);

				if (existing != null)
				{
					dto.Id = existing.Id; // przypisz faktyczne Id, jeśli istnieje
				}
			}
			CollectionSynchronizer.SynchronizeCollection(
				pallet.ProductsOnPallet,
				productDto,
				product => product.Id,
				dto => dto.Id,
				dto => _mapper.Map<ProductOnPallet>(dto),
			(dto, entity) => _mapper.Map(dto, entity)
				);
		}
	}
}
