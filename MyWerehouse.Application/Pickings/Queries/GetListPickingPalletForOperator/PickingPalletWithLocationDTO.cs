namespace MyWerehouse.Application.Pickings.Queries.GetListPickingPalletForOperator
{
	public class PickingPalletWithLocationDTO
	{
		public Guid PalletId { get; init;}
		public string PalletNumber { get; init; } = string.Empty;
		public int LocationId { get; init;}
		public string LocationName { get; init; } = string.Empty;
		public DateTime AddedToPicking { get; init; }		
	}
}
