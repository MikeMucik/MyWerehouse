using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundPickingTaskException : NotFoundException
	{
		public int PickingTaskId { get; }
		public NotFoundPickingTaskException(int pickingTaskId)
			: base($"Alokacja o numerze {pickingTaskId} nie istnieje")
		{
			PickingTaskId = pickingTaskId;
		}
		public NotFoundPickingTaskException(string message): base(message) {	}
	}
}
