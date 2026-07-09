using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Warehouse.Models
{
	public class Location
	{
		public int Id { get; set; }
		public int Bay { get; set; }
		public int Aisle { get; set; }
		public int Position { get; set; }
		public int Height { get; set; }
		public ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public string ToSnapshot() => $"{Bay}-{Aisle}-{Position}-{Height}";		
	}
}
