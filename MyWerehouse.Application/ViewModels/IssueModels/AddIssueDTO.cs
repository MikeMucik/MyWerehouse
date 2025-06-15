using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class AddIssueDTO
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public DateTime IssueDateTime { get; set; }
		public string? PerfomedBy { get; set; }
		public required List<IssueItemDTO> Values { get; set; }
	}
}
