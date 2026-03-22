using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public record IssueItemDTO :IMapFrom<IssueItem>
	{
		//public int IssueId { get; set; }
		//public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
		public DateOnly BestBefore { get; set; }		
	}
}
