using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pickings.PickingExceptions
{
	public class TaskWithOutSourceDomainException :DomainException
	{
		public TaskWithOutSourceDomainException()
			:base("Task can't be allocated without source.", Common.ValueObject.ErrorType.InternalError) { }
	}
}
