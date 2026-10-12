using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptTestData
	{
		internal static Receipt CreateNewReceipt()
		{
			return Receipt.Create(1000, 1, "user", 1, TestDates.UtcNow);
		}

		internal static Receipt CreateReceiptWithPallet(ReceiptStatus status = ReceiptStatus.InProgress)
		{
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), status, 1);
			var product1 = CreateProduct("chairs", 1);
			var product2 = CreateProduct("eggs", 1);
			var location1 = CreateLocation(1);
			var location2 = CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id,PalletStatus.New,receipt.Id,null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location2.Id, PalletStatus.Receiving,receipt.Id,null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			return receipt;
		}
		
		internal static Location CreateLocation(int position = 1)
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
		internal static Product CreateProduct(string name, int categoryId)
		{
			return Product.Create(name, "SKU1", TestDates.UtcNow, categoryId, 10, 30, 30, 30, 30, "TestDetails");
		}
	}
}
