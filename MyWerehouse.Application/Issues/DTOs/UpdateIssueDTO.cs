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
	public class UpdateIssueDTO : IMapFrom<Issue>
	{
		public Guid Id { get; set; }
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateToSend { get; set; }
		public List<IssueItemDTO> Items { get; set; } = new List<IssueItemDTO>();		
	}
}
