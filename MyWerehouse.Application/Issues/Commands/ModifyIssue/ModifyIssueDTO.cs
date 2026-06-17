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

namespace MyWerehouse.Application.Issues.Commands.ModifyIssue
{
	public class ModifyIssueDTO 
	{
		public int ClientId { get; init; }
		public string PerformedBy { get; init; }
		public List<IssueItemDTO> IssueItems { get; init; } = new List<IssueItemDTO>();		
	}
}
