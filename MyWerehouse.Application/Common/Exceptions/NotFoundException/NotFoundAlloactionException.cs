using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundAlloactionException : NotFoundException
	{
		public int AllocationId { get; }
		public NotFoundAlloactionException(int allocationId)
			: base($"Alokacja o numerze {allocationId} nie istnieje")
		{
			AllocationId = allocationId;
		}
		public NotFoundAlloactionException(string message): base(message) {	}
	}
}
