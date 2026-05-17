using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class ProductAlreadyExistDomainException : DomainException
	{
		public Guid ProductId { get; }
		public ProductAlreadyExistDomainException(Guid productId)
			: base("Product already in Issue")
		{
			ProductId = productId;
		}
	}
}
