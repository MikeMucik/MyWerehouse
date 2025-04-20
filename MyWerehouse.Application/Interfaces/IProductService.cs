using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface IProductService
	{
		int AddProduct(AddProductDTO model);
	}
}
