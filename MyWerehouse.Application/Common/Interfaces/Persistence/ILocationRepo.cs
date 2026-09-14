using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface ILocationRepo
	{
		Location AddLocation(Location location);
		void DeleteLocation(Location location);
		Task<Location?> GetLocationByIdAsync(int locationId, CancellationToken ct);
		IQueryable<Location> GetAllAvailableLocations();
		IEnumerable<Location> CreateListLocationForBay(int Bay, int StartAisle, int EndAisle, int AmountPosition, int AmountHeight);
		Task<bool> ReceivingRampExistsAsync(int locationId, CancellationToken ct);
		Task<bool> ExistsByCoordinatesAsync(int bay, int aisle, int position, int height, CancellationToken ct);
	}
}
