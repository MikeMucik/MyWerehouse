using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions.BuisnessRuleException;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class LocationException : BusinessRuleException
	{
		public int LocationId { get; set; }
		//public LocationException() { }
		public LocationException(int  locationId) 
			: base($"Lokalizacja o numerze {locationId} nie została znaleziona")
		{
			LocationId = locationId;
		} 
		public LocationException(string message) : base(message) { }
	}
}
