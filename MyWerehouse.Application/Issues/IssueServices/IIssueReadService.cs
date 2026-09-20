using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Issues.Queries.GetIssueById;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFilter;
using MyWerehouse.Application.Issues.Queries.IssueProductsSummary;
using MyWerehouse.Application.Issues.Queries.LoadingIssueList;
using MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList;
using MyWerehouse.Domain.Receiving.Filters;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public interface IIssueReadService
	{
		Task<IssueDTO?> GetIssueById(Guid id, CancellationToken ct);
		Task<PagedResult<IssueSimplyDTO>> GetIssueByFilter(IssueReceiptSearchFilter filter, int pageNumber, int pageSize, CancellationToken ct);
		Task<SummaryProductsIssueDTO?> SummaryProductsIssue(Guid id, CancellationToken ct);
		Task<ListPalletsToLoadDTO?> ListPalletsToLoad(Guid id, CancellationToken ct);
		Task<PagedResult<PalletWithLocationDTO>> GetPalletToTakeOff(Guid id, int pageNumber, int pageSize, CancellationToken ct);
	}
}
