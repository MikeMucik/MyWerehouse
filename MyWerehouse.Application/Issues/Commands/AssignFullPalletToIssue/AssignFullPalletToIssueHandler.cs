using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.AssignFullPalletToIssue
{
	public class AssignFullPalletToIssueHandler :IRequestHandler<AssignFullPalletToIssueCommand, List<Pallet>>
	{
		//private readonly WerehouseDbContext _werehouseDbContext;		
		private readonly IEventCollector _eventCollector;
		public AssignFullPalletToIssueHandler(
			//WerehouseDbContext werehouseDbContext,			
			IEventCollector eventCollector)
		{
			//_werehouseDbContext = werehouseDbContext;			
			_eventCollector = eventCollector;
		}
		public  Task<List<Pallet>> Handle(AssignFullPalletToIssueCommand request, CancellationToken ct)
		{
			var palletsToAsign = request.Pallets
					.OrderByDescending(p => p.ProductsOnPallet.First(po => po.Quantity > 0).Quantity)
					.Take(request.FullPalletCount)
					.ToList();
			foreach (var pallet in palletsToAsign)// adding full pallets *
			{
				pallet.IssueId = request.Issue.Id;
				//pallet.Status = PalletStatus.InTransit;

				_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id,
				pallet.LocationId,
				ReasonMovement.ToLoad,
				request.Issue.PerformedBy,
				PalletStatus.InTransit,
				null));

				request.Issue.Pallets.Add(pallet);
				//Czy tu powinienem wywołać rezerwację palety wraz zapisem do bazy
			}
			return Task.FromResult(palletsToAsign);
		}
	}
}
