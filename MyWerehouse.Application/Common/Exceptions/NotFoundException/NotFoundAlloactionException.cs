using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundAlloactionException : NotFoundException
	{
		public int PickingTaskId { get; }
		public NotFoundAlloactionException(int pickingTaskId)
			: base($"Alokacja o numerze {pickingTaskId} nie istnieje")
		{
			PickingTaskId = pickingTaskId;
		}
		public NotFoundAlloactionException(string message): base(message) {	}
	}
}
