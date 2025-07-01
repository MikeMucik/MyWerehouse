using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class PaletMovementDetailsRepo : IPalletMovementDetailsRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PaletMovementDetailsRepo( WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public void AddPalletMovementDetails(PalletMovementDetails palletMovementDetails)
		{
			_werehouseDbContext.PalletMovementDetails.Add(palletMovementDetails);
		}
	}
}
