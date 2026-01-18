using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Results
{
	public class LocationResult
	{
		public bool Succes { get; set; }
		public string Message { get; set; }
		public int LocationId { get; set; }
		public LocationResult()	{}
		public static LocationResult Ok(string message,
			int locationId)
		{
			return new LocationResult() { Succes = true, Message = message, LocationId = locationId };
		}
		public static LocationResult Fail(string message,
			int locationId)
		{
			return new LocationResult() { Succes = false, Message = message, LocationId = locationId };
		}
	}
}
