using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.LocationTestsRepo
{
	public class AddDeleteLocationTests : TestBase
	{		
		[Fact]
		public void AddNewLocation_AddLocation_AddedLoaction()
		{
			//Arrange
			var location = new Location
			{
				Aisle = 10,
				Bay = 20,
				Height = 6,
				Position = 10
			};
			var locationRepo = new LocationRepo(DbContext);
			//Act
			var result = locationRepo.AddLocation(location);
			DbContext.SaveChanges();
			//Arrange
			var existingLocation = DbContext.Locations
				.FirstOrDefault(l => l.Id == location.Id);
			Assert.NotNull(existingLocation);
			Assert.NotEqual(0, result.Id);
			Assert.Equal(location.Bay, existingLocation.Bay);
			Assert.Equal(location.Height, existingLocation.Height);
			Assert.Equal(location.Position, existingLocation.Position);
		}		
		[Fact]
		public void Removeloaction_DeleteLocationAsync_RemoveFromList()
		{
			//Arrange
			var location = new Location
			{
				Aisle = 10,
				Bay = 20,
				Height = 6,
				Position = 10
			};
			 DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var locationId = 1;
			var locationRepo = new LocationRepo(DbContext);
			//Act
			locationRepo.DeleteLocation(location);
			 DbContext.SaveChanges();
			//Assert
			var result = DbContext.Locations.Find(locationId);
			Assert.Null(result);		
		}
		
		[Fact]
		public void CreateListLocationForBayRangeAisle_ValidInput_ReturnsExpectedLocations()
		{
			// Arrange
			var bay = 5;
			var startAisle = 1;
			var endAisle = 3;
			var amountPosition = 2;
			var amountHeight = 2;
			var locationRepo = new LocationRepo(DbContext); // jeśli to metoda repo
															// lub: var service = new LocationService(); jeśli jest w serwisie

			// Act
			var result = locationRepo.CreateListLocationForBayRangeAisle(
				bay,
				startAisle,
				endAisle,
				amountPosition,
				amountHeight);

			// Assert
			// 1️⃣ Sprawdź liczbę wygenerowanych lokalizacji
			var expectedCount = (endAisle - startAisle + 1) * amountPosition * amountHeight;
			Assert.Equal(expectedCount, result.Count());

			// 2️⃣ Sprawdź, że wszystkie lokalizacje mają ten sam bay
			Assert.All(result, loc => Assert.Equal(bay, loc.Bay));

			// 3️⃣ Sprawdź poprawność zakresów wartości
			Assert.All(result, loc =>
			{
				Assert.InRange(loc.Aisle, startAisle, endAisle);
				Assert.InRange(loc.Position, 1, amountPosition);
				Assert.InRange(loc.Height, 1, amountHeight);
			});

			// 4️⃣ Sprawdź, że wartości się nie powtarzają (unikalne kombinacje)
			var distinctCount = result
				.Select(l => (l.Aisle, l.Position, l.Height))
				.Distinct()
				.Count();
			Assert.Equal(expectedCount, distinctCount);
		}

	}
}
