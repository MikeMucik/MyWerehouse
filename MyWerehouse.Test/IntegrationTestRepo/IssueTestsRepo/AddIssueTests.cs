using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.IssueTestsRepo
{
	public class AddIssueTests :CommandTestBase
	{
		private readonly IssueRepo _issueRepo;
		public AddIssueTests() : base()
		{
			_issueRepo = new IssueRepo(_context);
		}
		//[Fact]
		//public  void AddIssue_AddIssue_AddToCollection ()
		//{
		//	//Arrange
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q1010",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q1011",
		//		DateReceived = DateTime.Now,
		//		LocationId = 2,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	_context.Pallets.AddRange(pallet1, pallet2);
		//	_context.SaveChanges();	
		//	//Act		
		//	var issue = new Issue
		//	{				
		//		IssueDateTime = new DateTime(2025,5,15),
		//		ClientId = 1,
		//		Pallets = new List<Pallet>
		//	{
		//		pallet1,
		//		pallet2
		//	},
		//		PerformedBy = "U003"
		//	};			
		//	_issueRepo.AddIssue(issue);
		//	//Assert
		//	var result = _context.Issues
		//		.Include(p=>p.Pallets)
		//		.FirstOrDefault(i => i.Id == issue.Id);
		//	Assert.NotNull(result);
		//	Assert.Equal(2, result.Pallets.Count);

		//	Assert.Equal("U003", result.PerformedBy);
		//	Assert.Equal(1,result.ClientId);
		//	Assert.Contains(result.Pallets, p => p.Id == "Q1010");
		//	Assert.Contains(result.Pallets, p => p.Id == "Q1011");

		//	foreach (var item in result.Pallets)
		//	{
		//		Assert.Equal(issue.Id, item.IssueId);
		//	}
		//}
		//[Fact]
		//public async Task AddIssue_AddIssueAsync_AddToCollection()
		//{
		//	//Arrange
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q1010",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q1011",
		//		DateReceived = DateTime.Now,
		//		LocationId = 2,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	_context.Pallets.AddRange(pallet1, pallet2);
		//	_context.SaveChanges();
		//	//Act
		//	var issue = new Issue
		//	{
		//		IssueDateTime = new DateTime(2025, 5, 15),
		//		ClientId = 1,
		//		Pallets = new List<Pallet>
		//	{
		//		pallet1,
		//		pallet2
		//	},
		//		PerformedBy = "U003"
		//	};
			
		//	await _issueRepo.AddIssueAsync(issue);
		//	//Assert
		//	var result = _context.Issues
		//		.Include(p => p.Pallets)
		//		.FirstOrDefault(i => i.Id == issue.Id);
		//	Assert.NotNull(result);
		//	Assert.Equal(2, result.Pallets.Count);

		//	Assert.Equal("U003", result.PerformedBy);
		//	Assert.Equal(1, result.ClientId);
		//	Assert.Contains(result.Pallets, p => p.Id == "Q1010");
		//	Assert.Contains(result.Pallets, p => p.Id == "Q1011");

		//	foreach (var item in result.Pallets)
		//	{
		//		Assert.Equal(issue.Id, item.IssueId);
		//	}
		//}		
	}
}
