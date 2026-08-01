using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Issuing.Models
{
	public class IssueItem
	{
		public int Id { get; private set; }
		public Guid IssueId { get;private set; }
		public Issue Issue { get; private set; } = null!;
		public Guid ProductId { get; private set; }
		public Product Product { get; private set; } = null!;
		public int Quantity { get; private set; }
		public DateOnly? BestBefore { get; private set; }
		public DateTime CreatedAt { get; private set; }
		private IssueItem() { }
		internal IssueItem(Guid issueId,  Guid productId, int quantity, DateOnly? bestBefore, DateTime createdAt)
		{
			IssueId = issueId;				
			ProductId = productId;			
			Quantity = quantity;
			BestBefore = bestBefore;
			CreatedAt = createdAt;
		}

		private IssueItem(int id, Guid issueId, Guid productId, int quantity, DateOnly? bestBefore, DateTime createAt)
		{
			Id = id;
			ProductId = productId;
			Quantity = quantity;
			BestBefore = bestBefore;
			CreatedAt = createAt;
		}
		public static IssueItem CreateForSeed(int id, Guid issueId, Guid productId, int quantity, DateOnly? bestBefore, DateTime createAt)
			=> new IssueItem(id, issueId, productId, quantity, bestBefore, createAt);
	}
}
