using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueMarkAsLoadedServiceTests : TestBase
	{
		[Fact]
		public async Task MarkAsLoaded_ChangeStatus_HappyPath()
		{
			//Arrange
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 9, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
					}
				}
			};
			DbContext.Pallets.AddRange(availablePallets);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new MarkAsLoadedCommand("P1", "User123"));
			//Assert
			Assert.True(result.Success);
			var pallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "P1");
			Assert.NotNull(pallet);
			Assert.Equal(PalletStatus.Loaded, pallet.Status);
		}

		[Fact]
		public async Task MarkAsLoaded_ChangeStatus_SadPath()
		{
			//Arrange
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.Damaged,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 9, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
					}
				}
			};
			DbContext.Pallets.AddRange(availablePallets);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new MarkAsLoadedCommand("P2", "User123"));
			//Assert
			Assert.False(result.Success);
			var pallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "P2");
			Assert.NotNull(pallet);
			Assert.Equal(PalletStatus.Damaged, pallet.Status);
		}
	}
}
