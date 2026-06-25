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
	public record IssueItemDTO : IMapFrom<IssueItem> 
	{	
		public Guid ProductId { get; init; }
		public int Quantity { get; init; }
		public DateOnly BestBefore { get; init; }			
	}
}
