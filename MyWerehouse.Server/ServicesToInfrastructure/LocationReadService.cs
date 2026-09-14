using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Server.ServicesToInfrastructure
{
	public class LocationReadService(WerehouseDbContext werehouseDbContext) : ILocationReadService
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<int?> FindLocationIdAsync(int bay, int aisle, int position, int height, CancellationToken ct)
		{
			var id = await _werehouseDbContext.Locations
				.AsNoTracking()
				.Where(location =>
					location.Bay == bay &&
					location.Aisle == aisle &&
					location.Position == position &&
					location.Height == height)
				.Select(location => (int?)location.Id)
				.FirstOrDefaultAsync(ct);
			return id;
		}

		public Task<LocationDTO?> GetLocationByIdAsync(int id, CancellationToken ct)
		{
			var location = _werehouseDbContext.Locations
				.Where(x => x.Id == id)
				.Select(location => new LocationDTO
				{
					Id = id,
					Bay = location.Bay,
					Aisle = location.Aisle,
					Height = location.Height,
					Position = location.Position,
				})
				.FirstOrDefaultAsync(ct);
			return location;
		}
	}
}
