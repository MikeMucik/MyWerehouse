using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Test;

namespace MyWerehouse.Domain.Tests.Pallets
{
	internal class PalletTestData
	{
		internal static Pallet CreatePallet()
		{
			return  Pallet.Create("Q1000", 1, TestDates.Now);
		}
		internal static Pallet CreatePalletWithStatus(PalletStatus status)
		{
			return Pallet.CreateForSeed(Guid.NewGuid(), "Q1000", TestDates.DaysAgo(30), 1, status, null, null);
		}
		internal static Pallet CreatePalletFromReceipt(string palletNumber = "Q1000", int locationId = 1, PalletStatus status = PalletStatus.Available)
		{
			var pallet = Pallet.CreateForTests(palletNumber, TestDates.DaysAgo(30), locationId, status, Guid.NewGuid(), null);
			pallet.AddProduct(Guid.NewGuid(), 10, TestDates.DaysAgo(30), TestDates.DateBestBeforeInMonths(12));
			return pallet;
		}
		internal static Pallet CreatePalletForIssue(string palletNumber = "Q1000", int locationId = 1, PalletStatus status = PalletStatus.Available)
		{
			var pallet = Pallet.CreateForTests(palletNumber, TestDates.DaysAgo(30), locationId, status, Guid.NewGuid(), Guid.NewGuid());
			pallet.AddProduct(Guid.NewGuid(), 10, TestDates.DaysAgo(30), TestDates.DateBestBeforeInMonths(12));
			return pallet;
		}
		
		internal static Product CreateProduct(string name, int categoryId)
		{
			return Product.Create(name, "SKU1", TestDates.UtcNow, categoryId, 10, 30, 30, 30, 30, "TestDetails");
		}
		internal static Product CreateProductWithCartonsOnPallet(string name, int categoryId, int cartonsOnPallet)
		{
			return Product.Create(name, "SKU1", TestDates.UtcNow, categoryId, cartonsOnPallet, 30, 30, 30, 30, "TestDetails");
		}
		internal static Location CreateLocation(int position)
		{
			return new Location
			{
				Id = position,
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		//internal static ProductOnPallet Create(Guid palletId, int quantity)
		//{
		//	return ProductOnPallet.Create(Guid.NewGuid(), palletId, quantity,)
		//}
	}
}
