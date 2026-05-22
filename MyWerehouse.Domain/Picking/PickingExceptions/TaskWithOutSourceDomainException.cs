using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class TaskWithOutSourceDomainException :DomainException
	{
		public TaskWithOutSourceDomainException():base("Task can't be allocated without source") { }
	}
}
