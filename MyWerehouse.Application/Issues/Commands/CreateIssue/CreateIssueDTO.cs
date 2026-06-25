using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Commands.CreateIssue
{
	public class CreateIssueDTO
	{		
		public int ClientId { get; init; }
		public string PerformedBy { get; init; }
		//public IssueStatus IssueStatus { get; init; } = IssueStatus.New;
		public List<IssueItemDTO> Items { get; init; } = new List<IssueItemDTO>();		
	}
}
