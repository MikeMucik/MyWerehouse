using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.LocationModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface ILocationReadService
	{
		Task<LocationDTO?> GetLocationByIdAsync(int id, CancellationToken ct);
		Task<int?> FindLocationIdAsync(int bay, int aisle, int position, int height, CancellationToken ct);
	}
}
