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
		//public int IssueNumber { get; set; }
		public Issue Issue { get; private set; }
		public Guid ProductId { get; private set; }
		public Product Product { get; private set; }
		public int Quantity { get; private set; }
		public DateOnly BestBefore { get; private set; }
		public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
		private IssueItem() { }
		internal IssueItem(Guid issueId,  Guid productId, int quantity, DateOnly bestBefore)
		{
			IssueId = issueId;				
			ProductId = productId;			
			Quantity = quantity;
			BestBefore = bestBefore;
			CreatedAt = DateTime.UtcNow;
		}
		//public static IssueItem Create(Guid issueId, Guid productId, int quantity, DateOnly bestBefore)
		//	=> new IssueItem(issueId, productId, quantity, bestBefore);

		private IssueItem(int id, Guid issueId, Guid productId, int quantity, DateOnly bestBefore, DateTime createAt)
		{
			Id = id;
			ProductId = productId;
			Quantity = quantity;
			BestBefore = bestBefore;
			CreatedAt = createAt;
		}
		public static IssueItem CreateForSeed(int id, Guid issueId, Guid productId, int quantity, DateOnly bestBefore, DateTime createAt)
			=> new IssueItem(id, issueId, productId, quantity, bestBefore, createAt);
	}
}
