using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class Location
	{
		public int Id { get; set; }
		public int Bay {  get; set; }
		public int Aisle { get; set; }
		public int Position { get; set; }
		public int Height { get; set; }			
		public virtual ICollection<Pallet> Pallets { get; set; }
	}
}
