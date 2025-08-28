using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class Allocation
	{
		public int Id { get; set; }
		public int PickingPalletId { get; set; }
		public  virtual PickingPallet PickingPallet { get; set; }
		public int IssueId { get; set; }
		public Issue Issue { get; set; }
		public int Quantity { get; set; }
		public PickingStatus PickingStatus { get; set; }
	}
}
