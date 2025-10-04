using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Unit
{
	public class IssueUpdateServiceTests :TestBase
	{
	//	[Fact]
	//	public async Task UpdateIssue_ConfirmedToLoad_WithAddition_CreatesNewIssue()
	//	{
	//		// Arrange
	//		var address = new Address
	//		{
	//			City = "Warsaw",
	//			Country = "Poland",
	//			PostalCode = "00-999",
	//			StreetName = "Wiejska",
	//			Phone = 4444444,
	//			Region = "Mazowieckie",
	//			StreetNumber = "23/3"
	//		};
	//		var client = new Client
	//		{
	//			Name = "TestCompany",
	//			Email = "123@op.pl",
	//			Description = "Description",
	//			FullName = "FullNameCompany",
	//			Addresses = new List<Address> { address }
	//		};
	//		var category = new Category { Name = "Cat" };
	//		var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
	//		var product = new Product
	//		{
	//			Name = "Prod1",
	//			SKU = "SKU1",
	//			Category = category,
	//			CartonsPerPallet = 10
	//		};
	//		var pallets = new List<Pallet>
	//			{
	//				new Pallet
	//				{
	//					Id = "P1",
	//					Location = location,
	//					Status = PalletStatus.Available,
	//					ProductsOnPallet = new List<ProductOnPallet>
	//					{
	//						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
	//					}
	//				},
	//				new Pallet
	//				{
	//					Id = "P2",
	//					Location = location,
	//					Status = PalletStatus.Available,
	//					ProductsOnPallet = new List<ProductOnPallet>
	//					{
	//						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
	//					}
	//				}
	//			};

	//		DbContext.Clients.Add(client);
	//		DbContext.Categories.Add(category);
	//		DbContext.Products.Add(product);
	//		DbContext.Locations.Add(location);
	//		DbContext.Pallets.AddRange(pallets);
	//		await DbContext.SaveChangesAsync();

	//		var movementServiceMock = new Mock<IHistoryService>();
	//		var pickingRepo = new PickingPalletRepo(DbContext);
	//		var palletRepo = new PalletRepo(DbContext);
	//		var productRepo = new ProductRepo(DbContext);
	//		var issueRepoMock = new Mock<IIssueRepo>();
	//		//issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id)).ReturnsAsync(issue);
	//		//var inventoryRepoMock = new Mock<IInventoryRepo>();
	//		var inventoryRepo = new InventoryRepo(DbContext);
	//		var mockMapper = new Mock<IMapper>();
	//		var inventory = new InventoryService(inventoryRepo, mockMapper.Object);
	//		var palletService = new Mock<IPalletService>();
	//		var itemIssue = new IssueItemRepo(DbContext);
	//		var validator = new Mock<IValidator<CreateIssueDTO>>();
	//		var validatorUpdate = new Mock<IValidator<UpdateIssueDTO>>();
	//		validatorUpdate.Setup(v => v.Validate(It.IsAny<UpdateIssueDTO>()))
	//			 .Returns(new FluentValidation.Results.ValidationResult());

	//		var service = new IssueService(
	//			issueRepoMock.Object,
	//			mockMapper.Object,
	//			DbContext,
	//			movementServiceMock.Object,
	//			inventory,
	//			palletRepo,
	//			productRepo,
	//			pickingRepo,
	//			palletService.Object,
	//			itemIssue,
	//			validator.Object,
	//			validatorUpdate.Object
	//		);
	//		// Act 1 – create issue with 1 pallet (10 szt.)
	//		var createIssueDto = new CreateIssueDTO
	//		{
	//			ClientId = client.Id,
	//			PerformedBy = "User1",
	//			IssueDateTime = DateTime.Now,
	//			Items = new List<IssueItemDTO>
	//			{
	//				new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
	//			}
	//		};

	//		var created = await service.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow);

	//		var issue = DbContext.Issues.Include(i => i.Pallets).First();
	//		Assert.Single(issue.Pallets); // powinien być przypisany P1
	//		Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			

	//		var issueDTO = new UpdateIssueDTO { /* z dodatkiem */ };
	//		var oldQuantityMock = 10;
	//		//itemIssue.Setup(r => r.GetQuantityByIssueAndProduct(It.IsAny<Issue>(), It.IsAny<int>())).ReturnsAsync(oldQuantityMock);
	//		// Mock CreateNewIssueAsync -> zwróć sukces

	//		// Act
	//		var result = await service.UpdateIssueAsync(issueDTO);

	//		// Assert
	//		Assert.True(result.All(r => r.Success));
	//		// Sprawdź, czy CreateNewIssueAsync wywołane z różnicą (15-10=5)
	//	}
	//	[Fact]
	//	public async Task UpdateIssueBetter_ConfirmedToLoad_WithAddition_CreatesNewIssue()
	//	{
	//		// Arrange – mocki zamiast real DB (szybciej, izolowane)
	//		var mockIssueRepo = new Mock<IIssueRepo>();
	//		var mockMapper = new Mock<IMapper>();
	//		var mockHistoryService = new Mock<IHistoryService>();
	//		var mockInventoryService = new Mock<IInventoryService>();  // Zakładam masz
	//		var mockPalletRepo = new Mock<IPalletRepo>();
	//		var mockProductRepo = new Mock<IProductRepo>();
	//		var mockPickingRepo = new Mock<IPickingPalletRepo>();
	//		var mockPalletService = new Mock<IPalletService>();
	//		var mockIssueItemRepo = new Mock<IIssueItemRepo>();

	//		// Mock validator dla CreateIssueDTO (sukces)
	//		var createValidatorMock = new Mock<IValidator<CreateIssueDTO>>();
	//		createValidatorMock.Setup(v => v.Validate(It.IsAny<CreateIssueDTO>()))
	//			.Returns(new FluentValidation.Results.ValidationResult());  // IsValid=true

	//		// Mock validator dla UpdateIssueDTO (sukces – dodaj, jeśli masz osobny)
	//		var updateValidatorMock = new Mock<IValidator<UpdateIssueDTO>>();
	//		updateValidatorMock.Setup(v => v.Validate(It.IsAny<UpdateIssueDTO>()))
	//			.Returns(new FluentValidation.Results.ValidationResult());  // IsValid=true

	//		// Setup serwisu z mockami (bez real DbContext – czysto unit)
	//		var service = new IssueService(
	//			mockIssueRepo.Object,
	//			mockMapper.Object,
	//			null,  // DbContext – mock lub null, jeśli nie używasz w Update
	//			mockHistoryService.Object,
	//			mockInventoryService.Object,
	//			mockPalletRepo.Object,
	//			mockProductRepo.Object,
	//			mockPickingRepo.Object,
	//			mockPalletService.Object,
	//			mockIssueItemRepo.Object,
	//			createValidatorMock.Object,
	//			updateValidatorMock.Object  // Validator dla Update
	//		);

	//		// Mock istniejący issue (z ConfirmedToLoad, oldQuantity=10)
	//		var existingIssue = new Issue
	//		{
	//			Id = 1,
	//			ClientId = 1,
	//			IssueStatus = IssueStatus.ConfirmedToLoad,
	//			PerformedBy = "User1",
	//			IssueItems = new List<IssueItem>
	//	{
	//		new IssueItem { ProductId = 1, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) }  // Stare 10 szt.
 //       }
	//		};
	//		mockIssueRepo.Setup(r => r.GetIssueByIdAsync(It.IsAny<int>()))
	//			.ReturnsAsync(existingIssue);

	//		// Mock GetQuantityByIssueAndProduct (oldQuantity=10)
	//		mockIssueItemRepo.Setup(r => r.GetQuantityByIssueAndProduct(It.IsAny<Issue>(), It.IsAny<int>()))
	//			.ReturnsAsync(10);

	//		// Mock CreateNewIssueAsync (wewnątrz Update – zwróć sukces z diff=5)
	//		var mockCreateResult = new List<IssueResult>
	//{
	//	IssueResult.Ok("Sukces dodatku", 1)  // ProductId=1, Success=true
 //   };
	//		// Zakładam, że Update woła CreateNewIssueAsync – mock to w serwisie lub użyj shim (lub przetestuj izolowanie)
	//		// Dla prostoty: Zakładam, że testujesz cały flow, ale mockuj AddPallets... jeśli potrzeba

	//		// Act 1 – symuluj Create (opcjonalnie, jeśli testujesz Update izolowanie)
	//		var createIssueDto = new CreateIssueDTO
	//		{
	//			ClientId = 1,
	//			PerformedBy = "User1",
	//			Items = new List<IssueItemDTO>
	//	{
	//		new IssueItemDTO { ProductId = 1, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) }
	//	}
	//		};
	//		createValidatorMock.Setup(v => v.Validate(createIssueDto)).Returns(new FluentValidation.Results.ValidationResult());
	//		await service.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow);  // Przechodzi walidację

	//		// Act 2 – Update: Zmień na 15 szt. (diff=5)
	//		var updateDto = new UpdateIssueDTO
	//		{
	//			Id = existingIssue.Id,  // Issue Id=1
	//			PerformedBy = "User2",
	//			DateToSend = DateTime.UtcNow.AddDays(1),
	//			Items = new List<IssueItemDTO>
	//	{
	//		new IssueItemDTO { ProductId = 1, Quantity = 15, BestBefore = new DateOnly(2026, 1, 1) }  // Nowe 15
 //       }
	//		};

	//		// Act
	//		var result = await service.UpdateIssueAsync(updateDto);

	//		// Assert
	//		Assert.True(result.All(r => r.Success));  // Walidacja przeszła, sukces
	//		Assert.Single(result);  // Jeden produkt
	//		Assert.Equal(1, result.First().ProductId);  // ProductId=1
	//		Assert.Contains("dodatkowe zlecenie", result.First().Message.ToLower());  // Komunikat z ConfirmedToLoad (dostosuj)

	//		// Mock verify: Sprawdź, czy GetQuantityByIssueAndProduct wołane
	//		mockIssueItemRepo.Verify(r => r.GetQuantityByIssueAndProduct(existingIssue, 1), Times.Once);

	//		// Opcjonalnie: Verify CreateNewIssueAsync wołane z diff=5 (jeśli możesz shim)
	//		// mockIssueRepo.Verify(r => r.AddIssueAsync(It.Is<Issue>(i => i.IssueItems.First().Quantity == 5)), Times.Once);
	//	}
	}
}
