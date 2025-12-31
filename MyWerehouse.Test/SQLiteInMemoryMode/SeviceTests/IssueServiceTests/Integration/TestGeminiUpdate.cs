using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Inventories.Queries.GetProductCount;
using MyWerehouse.Application.Issues.Commands.AssignFullPalletToIssue;
using MyWerehouse.Application.Issues.Commands.UpdateIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class TestGeminiUpdate : IssueIntegrationCommandService
	{
		//[Fact]
		//public async Task UpdateIssue_WhenErrorOccursAfterSync_ShouldCleanMemory_AndNotSaveFailedItem()
		//{
		//	// ----------------------------------------------------------------------------------
		//	// 1. KONFIGURACJA ŚRODOWISKA (SQLite In-Memory dla obsługi Transakcji)
		//	// ----------------------------------------------------------------------------------

		//	// Tworzymy połączenie SQLite w pamięci (wymagane dla poprawnego działania Rollback)
		//	var connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
		//	connection.Open();

		//	var options = new DbContextOptionsBuilder<WerehouseDbContext>() // Podmień na nazwę swojego DbContextu
		//		.UseSqlite(connection)
		//		.Options;

		//	// Tworzymy kontekst
		//	using var dbContext = new WerehouseDbContext(options); // Podmień na nazwę swojego DbContextu
		//	dbContext.Database.EnsureCreated();

		//	// Mocki
		//	var mediatorMock = new Mock<IMediator>();
		//	var eventCollectorMock = new Mock<IEventCollector>(); // Zakładam interfejs IEventCollector

		//	// Symulacja listy eventów w collectorze (żeby weszło do pętli foreach i wywołało Publish)
		//	var eventsList = new List<INotification> { new Mock<INotification>().Object };
		//	eventCollectorMock.Setup(x => x.Events).Returns(eventsList);
		//	eventCollectorMock.Setup(x => x.DeferredEvents).Returns(new List<Func<Task<INotification>>>());

		//	// Repozytoria (jeśli używasz w serwisie repozytoriów, musisz je tu utworzyć przekazując dbContext)
		//	// Jeśli wstrzykujesz DbContext bezpośrednio do serwisu, to pomiń ten krok.
		//	var issueRepo = new IssueRepo(dbContext); // Przykład

		//	// Tworzymy Serwis (SUT - System Under Test)
		//	// UWAGA: Dopasuj konstruktor do swojego serwisu!
		//	var issueService = new IssueService(
		//		mediatorMock.Object
		//	// ... inne zależnosci (np. Logger)
		//	);

		//	// ----------------------------------------------------------------------------------
		//	// 2. DANE TESTOWE
		//	// ----------------------------------------------------------------------------------

		//	var productFail = new Product { Id = 1, Name = "FailProd", SKU = "F1", CartonsPerPallet = 10 };
		//	var productSuccess = new Product { Id = 2, Name = "OkProd", SKU = "S1", CartonsPerPallet = 10 };

		//	var palletFail = new Pallet { Id = "P_FAIL", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 1, Quantity = 10 } } };
		//	var palletSuccess = new Pallet { Id = "P_OK", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 2, Quantity = 10 } } };

		//	var issue = new Issue
		//	{
		//		Id = 1,
		//		IssueStatus = IssueStatus.New,
		//		Pallets = new List<Pallet>(), // Pusta lista
		//		IssueItems = new List<IssueItem>

		//{
		//	new IssueItem { ProductId = 1, Quantity = 0 }, // Stare ilości
  //          new IssueItem { ProductId = 2, Quantity = 0 }
		//}
		//	};

		//	dbContext.Products.AddRange(productFail, productSuccess);
		//	dbContext.Pallets.AddRange(palletFail, palletSuccess);
		//	dbContext.Issues.Add(issue);
		//	await dbContext.SaveChangesAsync();

		//	// ----------------------------------------------------------------------------------
		//	// 3. MOCKOWANIE ZACHOWANIA (Happy Path + Pułapka)
		//	// ----------------------------------------------------------------------------------

		//	// Setup: Zawsze dostępny towar
		//	mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCountQuery>(), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(100);

		//	// Setup: Zawsze 1 pełna paleta
		//	mediatorMock.Setup(m => m.Send(It.IsAny<GetNumberPalletsAndRestQuery>(), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new AssignPallestResult(1, 0));

		//	// Setup: Zwracanie konkretnych palet dla konkretnych produktów
		//	mediatorMock.Setup(m => m.Send(It.Is<GetAvailablePalletsByProductQuery>(q => q.ProductId == 1), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new List<Pallet> { palletFail }); // Dla FailProd -> P_FAIL

		//	mediatorMock.Setup(m => m.Send(It.Is<GetAvailablePalletsByProductQuery>(q => q.ProductId == 2), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new List<Pallet> { palletSuccess }); // Dla OkProd -> P_OK

		//	// Setup: Przypisanie (symulacja, że komenda zwraca to, co dostała)
		//	mediatorMock.Setup(m => m.Send(It.IsAny<AssignFullPalletToIssueCommand>(), It.IsAny<CancellationToken>()))
		//		.Returns<AssignFullPalletToIssueCommand, CancellationToken>((cmd, ct) => Task.FromResult(cmd.Pallets));

		//	// === PUŁAPKA (THE TRAP) ===
		//	// Rzucamy wyjątek przy publikacji eventu, ALE tylko wtedy, gdy w kontekście EF
		//	// znajduje się "brudna" paleta P_FAIL przypisana do zlecenia.
		//	// To symuluje błąd, który dzieje się PO Synchronizerze, PO SaveChanges, ale PRZED Commitem.

		//	mediatorMock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
		//		.Callback(() =>
		//		{
		//			// Sprawdzamy "na żywo" co EF ma w pamięci
		//			var dirtyPallet = dbContext.Pallets.Local.FirstOrDefault(p => p.Id == "P_FAIL");

		//			// Jeśli paleta P_FAIL jest przypisana do zlecenia (IssueId != null), rzucamy błąd!
		//			if (dirtyPallet != null && dirtyPallet.IssueId == 1)
		//			{
		//				throw new Exception("Symulowany błąd po zapisie P_FAIL!");
		//			}
		//		});

		//	// ----------------------------------------------------------------------------------
		//	// 4. WYKONANIE (ACT)
		//	// ----------------------------------------------------------------------------------

		//	var updateDto = new UpdateIssueDTO
		//	{
		//		Id = 1,
		//		PerformedBy = "TestUser",
		//		Items = new List<IssueItemDTO>
		//{
		//	new IssueItemDTO { ProductId = 1, Quantity = 10 }, // FailProd (rzuci błąd przy Publish)
  //          new IssueItemDTO { ProductId = 2, Quantity = 10 }  // OkProd (powinien przejść)
  //      }
		//	};

		//	// Ignorujemy wynik metody (lista błędów nas nie interesuje, interesuje nas stan bazy)
		//	try
		//	{
		//		//await issueService.Handle(new UpdateIssueCommand(updateDto, DateTime.Now.AddDays(7)), CancellationToken.None);
		//		await issueService.UpdateIssueAsync(updateDto, DateTime.Now.AddDays(7));
		//	}
		//	catch
		//	{
		//		// Połykamy ewentualny throw z handlera, jeśli taki masz
		//	}

		//	// ----------------------------------------------------------------------------------
		//	// 5. SPRAWDZENIE (ASSERT)
		//	// ----------------------------------------------------------------------------------

		//	// Czyścimy ChangeTracker, żeby pobrać świeże dane prosto z "bazy" SQLite
		//	dbContext.ChangeTracker.Clear();

		//	var issueFromDb = await dbContext.Issues
		//		.Include(i => i.Pallets)
		//		.FirstOrDefaultAsync(i => i.Id == 1);

		//	// DEBUG: Co faktycznie jest w bazie?
		//	var palletsInDb = string.Join(", ", issueFromDb.Pallets.Select(p => p.Id));

		//	// WARUNEK ZALICZENIA:
		//	// P_FAIL nie może być w bazie (bo był Rollback).
		//	// P_OK powinna być w bazie (bo druga iteracja się udała).

		//	Assert.Contains(issueFromDb.Pallets, p => p.Id == "P_OK");     // To powinno przejść
		//	Assert.DoesNotContain(issueFromDb.Pallets, p => p.Id == "P_FAIL"); // TU SIĘ WYWALI STARY KOD!
		//	Assert.Equal(1, issueFromDb.Pallets.Count);
		//}
		//[Fact]
		//public async Task UpdateIssueAsync_WhenOneItemFails_ShouldCleanMemory_AndSaveTheOther()
		//{
		//	// ====================================================================
		//	// 1. ARRANGE (Twój kod + dodanie drugiego produktu do testu błędu)
		//	// ====================================================================

		//	// --- Twój oryginalny Setup ---
		//	var address = new Address { City = "Warsaw", Country = "Poland", PostalCode = "00-999", StreetName = "Wiejska", Phone = 4444444, Region = "Mazowieckie", StreetNumber = "23/3" };
		//	var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
		//	var category = new Category { Name = "Cat" };
		//	var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };

		//	// PRODUKT 1 (To będzie ten "zepsuty" - FAIL)
		//	var productFail = new Product { Name = "ProdFail", SKU = "SKU_F", Category = category, CartonsPerPallet = 10 };

		//	// PRODUKT 2 (To będzie ten poprawny - SUCCESS - dodany przeze mnie)
		//	var productSuccess = new Product { Name = "ProdSuccess", SKU = "SKU_S", Category = category, CartonsPerPallet = 10 };

		//	// Palety w magazynie
		//	var palletFail = new Pallet
		//	{
		//		Id = "P_FAIL",
		//		Location = location,
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = productFail, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
		//	};

		//	var palletSuccess = new Pallet
		//	{
		//		Id = "P_OK",
		//		Location = location,
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = productSuccess, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
		//	};

		//	DbContext.Clients.Add(client);
		//	DbContext.Categories.Add(category);
		//	DbContext.Locations.Add(location);
		//	DbContext.Products.AddRange(productFail, productSuccess);
		//	DbContext.Pallets.AddRange(palletFail, palletSuccess);
		//	await DbContext.SaveChangesAsync();

		//	// Tworzymy zlecenie, które będziemy edytować (puste lub z czymś innym)
		//	var issue = new Issue
		//	{
		//		ClientId = client.Id,
		//		PerformedBy = "User1",
		//		IssueStatus = IssueStatus.New, // Ważne: Status edytowalny
		//		Pallets = new List<Pallet>(), // Puste na start
		//		IssueItems = new List<IssueItem>()
		//	};
		//	DbContext.Issues.Add(issue);
		//	await DbContext.SaveChangesAsync();

		//	// ====================================================================
		//	// 2. MOCKOWANIE MEDIATORA (Symulacja logiki + PUŁAPKA)
		//	// ====================================================================

		//	var mediatorMock = new Mock<IMediator>();

		//	// Setup: Dostępność towaru (zawsze jest)
		//	mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCountQuery>(), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(100);

		//	// Setup: Przeliczanie palet (zawsze 1 pełna)
		//	mediatorMock.Setup(m => m.Send(It.IsAny<GetNumberPalletsAndRestQuery>(), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new AssignPallestResult(1, 0));

		//	// Setup: Zwracanie konkretnych palet z bazy dla konkretnych produktów
		//	mediatorMock.Setup(m => m.Send(It.Is<GetAvailablePalletsByProductQuery>(q => q.ProductId == productFail.Id), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new List<Pallet> { palletFail });

		//	mediatorMock.Setup(m => m.Send(It.Is<GetAvailablePalletsByProductQuery>(q => q.ProductId == productSuccess.Id), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new List<Pallet> { palletSuccess });

		//	// Setup: Przypisanie (symulacja sukcesu przypisania)
		//	mediatorMock.Setup(m => m.Send(It.IsAny<AssignFullPalletToIssueCommand>(), It.IsAny<CancellationToken>()))
		//		.Returns<AssignFullPalletToIssueCommand, CancellationToken>((cmd, ct) => Task.FromResult(cmd.Pallets));

		//	// --- PUŁAPKA (THE TRAP) ---
		//	// Tutaj symulujemy błąd "po fakcie" - czyli po dodaniu palety do pamięci EF, a przed Commitem.
		//	mediatorMock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
		//		.Callback(() =>
		//		{
		//			// Sprawdzamy, czy w pamięci EF (Local) paleta P_FAIL została przypisana do zlecenia
		//			var dirtyPallet = DbContext.Pallets.Local.FirstOrDefault(p => p.Id == "P_FAIL");

		//			// Jeśli EF "trzyma" przypisaną paletę P_FAIL, rzucamy błąd!
		//			if (dirtyPallet != null && dirtyPallet.IssueId == issue.Id)
		//			{
		//				throw new ProductException("Symulowany błąd biznesowy po synchronizacji!");
		//			}
		//		});

		//	// ====================================================================
		//	// 3. KONSTRUKCJA SERWISU (Musimy użyć Mocka Mediatora!)
		//	// ====================================================================

		//	// UWAGA: Tutaj musisz ręcznie stworzyć instancję swojego serwisu/handlera, 
		//	// przekazując mu prawdziwy DbContext i zmockowanego Mediatora.
		//	// Dostosuj nazwy parametrów do swojego konstruktora!

		//	var serviceWithMock = new IssueService(
		//		              // Prawdziwa baza (SQLite In-Memory)
		//		mediatorMock.Object     // Zmockowany mediator
		//		       // Twój collector
		//	);

		//	// ====================================================================
		//	// 4. ACT (Wykonanie aktualizacji)
		//	// ====================================================================

		//	var updateDto = new UpdateIssueDTO
		//	{
		//		Id = issue.Id,
		//		PerformedBy = "User2",
		//		DateToSend = DateTime.UtcNow.AddDays(1),
		//		Items = new List<IssueItemDTO>
		//{
  //          // Produkt 1: Wywoła błąd w Mediatorze (zostanie wycofany)
  //          new IssueItemDTO { ProductId = productFail.Id, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) },
  //          // Produkt 2: Powinien przejść poprawnie (jeśli pamięć zostanie wyczyszczona)
  //          new IssueItemDTO { ProductId = productSuccess.Id, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) }
		//}
		//	};

		//	// Ignorujemy błędy zwrócone w resultacie (bo testujemy stan bazy)
		//	try
		//	{
		//		await serviceWithMock.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));
		//	}
		//	catch (Exception)
		//	{
		//		// W Twoim kodzie błędy są łapane i zwracane jako IssueResult, więc catch może nie być potrzebny,
		//		// ale dajemy go dla bezpieczeństwa, gdyby coś wybuchło.
		//	}

		//	// ====================================================================
		//	// 5. ASSERT (Sprawdzenie bazy)
		//	// ====================================================================

		//	DbContext.ChangeTracker.Clear(); // Czyścimy pamięć EF, żeby pobrać surowe dane z bazy

		//	var dbIssue = await DbContext.Issues
		//		.Include(i => i.Pallets)
		//		.FirstAsync(i => i.Id == issue.Id);

		//	// Wnioski:
		//	// 1. P_FAIL nie może być przypisana (był błąd -> rollback).
		//	// 2. P_OK musi być przypisana (sukces).

		//	// Twój STARY kod tutaj obleje: P_FAIL wciąż będzie przypisana (duch z pamięci RAM).
		//	Assert.DoesNotContain(dbIssue.Pallets, p => p.Id == "P_FAIL");

		//	// Twój NOWY kod (z ReloadAsync) tutaj przejdzie.
		//	Assert.Contains(dbIssue.Pallets, p => p.Id == "P_OK");
		//	Assert.Equal(1, dbIssue.Pallets.Count);
		//}
	}
}
