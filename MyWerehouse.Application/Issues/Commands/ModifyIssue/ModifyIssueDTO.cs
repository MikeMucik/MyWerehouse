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
		public int ClientId { get; set; }
		public string PerformedBy { get; set; }
		public List<IssueItemDTO> IssueItems { get; set; } = new List<IssueItemDTO>();		
	}
}
