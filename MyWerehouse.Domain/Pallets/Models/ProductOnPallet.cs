using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class ProductOnPallet
	{
		public int Id { get; private set; }
		public Guid ProductId { get; private set; }
		public Product Product { get; private set; }
		public Guid PalletId { get; private set; }
		public Pallet Pallet { get; private set; }
		public int Quantity { get; private set; }
		public DateTime DateAdded { get; private set; }
		public DateOnly? BestBefore { get; private set; }

		private ProductOnPallet() { }

		private ProductOnPallet(int id, Guid productId, Guid palletId, int quantity, DateTime dateAdded, DateOnly? bestBefore)
		{
			Id = id;
			ProductId = productId;
			PalletId = palletId;
			Quantity = quantity;
			DateAdded = dateAdded;
			BestBefore = bestBefore;
		}
		public static ProductOnPallet CreateForSeed(int id, Guid productId, Guid palletId, int quantity, DateTime dateAdded, DateOnly? bestbefore)
			=> new ProductOnPallet(id, productId, palletId, quantity, dateAdded, bestbefore);

		//TODO usuń public static dla apki
		internal ProductOnPallet( Guid productId, Guid palletId, int quantity, DateTime dateAdded, DateOnly? bestBefore)
		{			
			ProductId = productId;
			PalletId = palletId;
			Quantity = quantity;
			DateAdded = dateAdded;
			BestBefore = bestBefore;
		}
		public static ProductOnPallet Create(Guid productId, Guid palletId, int quantity, DateTime dateAdded, DateOnly? bestbefore)
			=> new ProductOnPallet(productId, palletId, quantity, dateAdded, bestbefore);

		public void SetQuantity(int quantity)
		{
			if (quantity < 0) throw new InsufficientQunatityException(PalletId);
			Quantity = quantity;
		}
		public void ChangeQuantity(int quantity)
		{
			var newQuantity = Quantity + quantity;
			if (newQuantity <= 0) throw new InvalidQunatityException(PalletId);
			Quantity = newQuantity;
		}
		public void IncreaseQuantity(int quantity)
		{			
			if (quantity <= 0) throw new InvalidQunatityException(PalletId);
			Quantity += quantity;
		}
		public void DecreaseQuantity(int quantity)
		{
			if (quantity <= 0) throw new InvalidQunatityException(PalletId);
			var newQuantity = Quantity - quantity;
			if (newQuantity < 0) throw new InsufficientQunatityException(PalletId);
			Quantity = newQuantity;
		}
		public void SetBestBefore(DateOnly? bestBefore)
		{
			BestBefore = bestBefore;
		}
		
	}

}
