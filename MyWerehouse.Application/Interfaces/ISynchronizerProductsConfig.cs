using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface ISynchronizerProductsConfig
	{
		void SynchronizeProducts(Pallet pallet, IEnumerable<ProductOnPalletDTO> productDto);
	}
}
