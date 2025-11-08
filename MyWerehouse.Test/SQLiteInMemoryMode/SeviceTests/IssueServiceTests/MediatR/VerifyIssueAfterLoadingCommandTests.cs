using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Commands.Issue.VerifyIssueAfterLoading;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.MediatR
{
	public class VerifyIssueAfterLoadingCommandTests : TestBase
	{		
		//[Fact]
		//public async Task VerifyIssueAfterLoading_Should_Update_Status_And_History()
		//{
		//	// Arrange
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
		//	var category = new Category { Name = "Cat" };
		//	var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
		//	var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
		//	var product1 = new Product { Name = "Prod2", SKU = "SKU2", Category = category, CartonsPerPallet = 20 };
		//	var pallet = new Pallet
		//	{
		//		Id = "P1",
		//		Location = location,
		//		Status = PalletStatus.ToIssue,
		//		ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
		//	};
		//	var pallet1 = new Pallet
		//	{
		//		Id = "P2",
		//		Location = location,
		//		Status = PalletStatus.ToIssue,
		//		ProductsOnPallet = new List<ProductOnPallet>{
		//					new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
		//	};
		//	var issue = new Issue
		//	{
		//		Allocations = new List<Allocation>(),
		//		Client = client,
		//		IssueItems = new List<IssueItem> { new IssueItem
		//	{
		//		//ProductId = product.Id,
		//		Product = product,
		//		Quantity = 20,
		//		BestBefore = new DateOnly(2026, 1, 1)
		//	}},
		//		IssueStatus = IssueStatus.IsShipped,
		//		PerformedBy = "user1",
		//		IssueDateTimeCreate = DateTime.Now.AddDays(-7),
		//		IssueDateTimeSend = DateTime.Now.AddDays(1),
		//		Pallets = new List<Pallet> { pallet, pallet1 }
		//	};
		//	DbContext.Issues.Add(issue);
		//	var inventory = new Inventory
		//	{
		//		Product = product,
		//		LastUpdated = DateTime.Now.AddDays(-7),
		//		Quantity = 100
		//	};
		//	var inventory1 = new Inventory
		//	{
		//		Product = product1,
		//		LastUpdated = DateTime.Now.AddDays(-7),
		//		Quantity = 100
		//	};
		//	DbContext.AddRange(inventory, inventory1);
		//	await DbContext.SaveChangesAsync();

		//	var command = new VerifyIssueAfterLoadingCommand(issue.Id, "user1");

		//	// Act
		//	var result = await Mediator.Send(command);

		//	// Assert
		//	Assert.NotNull(result);
		//	Assert.True(result.Success);
		//	Assert.Equal("Załadunek zatwierdzony, zasoby uaktulanione.", result.Message);

		//	// Sprawdzenie zmian w bazie
		//	var updatedIssue = await DbContext.Issues
		//		.Include(i => i.Pallets)
		//		.FirstAsync(i => i.Id == issue.Id);

		//	Assert.Equal(IssueStatus.Archived, updatedIssue.IssueStatus);
		//	Assert.All(updatedIssue.Pallets, p => Assert.Equal(PalletStatus.Archived, p.Status));

		//	// Sprawdzenie historii Issue
		//	var issueHistory = await DbContext.HistoryIssues
		//		.Include(h => h.Details)
		//		.FirstOrDefaultAsync(h => h.IssueId == issue.Id);
		//	Assert.NotNull(issueHistory);

		//	// Sprawdzenie historii palet
		//	var palletHistory = await DbContext.PalletMovements
		//		.Include(h => h.PalletMovementDetails)
		//		.FirstOrDefaultAsync(h => h.PalletId == pallet.Id);
		//	Assert.NotNull(palletHistory);
		//	Assert.Equal(PalletStatus.Archived, palletHistory.PalletStatus);
		//}
	}
}


