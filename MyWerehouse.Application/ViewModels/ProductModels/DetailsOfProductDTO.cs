namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class DetailsOfProductDTO 
	{
		public Guid Id { get; init; }
		public string Name { get; init; } = string.Empty;
		public string CategoryName { get; init; } = string.Empty;
		public int CategoryId { get; init; }		
		public int CartonsPerPallet { get; init; }
		public int Length { get; init; } //cm
		public int Height { get; init; } //cm
		public int Width { get; init; } //cm
		public int Weight { get; init; } //kg
		public string Description { get; init; } = string.Empty;		
	}
}
