using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IIssueRepo
	{
		void AddIssue(Issue issue);
		void DeleteIssue(Issue issue);
		Task<Issue?> GetIssueByIdAsync(Guid id, CancellationToken ct);
		Task<Issue?> GetIssueByIdForModifyAsync(Guid id, CancellationToken ct);
		Task<List<Issue>> GetIssuesByIdsAsync(List<Guid> ids, CancellationToken ct);
		Task<List<Issue>> GetIssuesByDates(DateOnly? startDate, DateOnly? endDate, CancellationToken ct);
		Task<int> GetNextNumberOfIssue(CancellationToken ct);
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid id, CancellationToken ct);
		Task<bool> HasIssueClient(int clientId, CancellationToken ct);
	}
}
