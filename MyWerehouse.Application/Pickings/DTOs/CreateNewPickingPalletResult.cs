namespace MyWerehouse.Application.Pickings.DTOs
{
	public record CreateNewPickingPalletResult(bool NewPalletCreated, Guid PalletId, string PalletNumber);	
}
