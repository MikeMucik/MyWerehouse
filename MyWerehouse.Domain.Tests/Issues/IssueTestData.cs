using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	internal class IssueTestData
	{		
		internal static Issue CreateIssue(IssueStatus status = IssueStatus.Pending, List<IssueItem> items = null!)
		{
			return Issue.CreateForSeed(Guid.NewGuid(), 1, 1, TestDates.UtcNow, TestDates.Today, "InitialUser", status, items);
		}
		internal static Client CreateClient(int id =1)
		{
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			return new Client
			{
				Id = id,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
		}
		internal static Product CreateProduct(string name, int categoryId)
		{
			return Product.Create(name, "SKU1", TestDates.UtcNow, categoryId, 10, 30, 30, 30, 30, "TestDetails");
		}
		internal static Location CreateLocation(int position)
		{
			return new Location
			{
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}		
	}
}
