using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundLocationException :NotFoundException
	{
		public int LocationId { get; set; }
		//public LocationException() { }
		public NotFoundLocationException(int locationId)
			: base($"Lokalizacja o numerze {locationId} nie została znaleziona")
		{
			LocationId = locationId;
		}
	}
}
