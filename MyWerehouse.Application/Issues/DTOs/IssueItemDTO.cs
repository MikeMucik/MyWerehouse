using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public record IssueItemDTO :IMapFrom<IssueItem>
	{
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public DateOnly BestBefore { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<IssueItemDTO, IssueItem>();
		}
	}
}
