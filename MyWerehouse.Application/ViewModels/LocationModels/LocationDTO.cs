using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.ViewModels.LocationModels
{
	public class LocationDTO
	{
		public int Id { get; init; }
		public int Bay { get; init; }
		public int Aisle { get; init; }
		public int Position { get; init; }
		public int Height { get; init; }		
	}
}
