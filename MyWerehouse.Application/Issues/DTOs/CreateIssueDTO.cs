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
	public class CreateIssueDTO : IMapFrom<Issue>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public DateTime IssueDateTime { get; set; } = DateTime.UtcNow;
		public string PerformedBy { get; set; }
		public IssueStatus IssueStatus { get; set; } = IssueStatus.New;
		public List<IssueItemDTO> Items { get; set; } = new List<IssueItemDTO>();
		
	}
}
