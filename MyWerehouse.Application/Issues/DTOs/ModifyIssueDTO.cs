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
	public class ModifyIssueDTO 
	{
		public int ClientId { get; set; }
		public string PerformedBy { get; set; }
		public List<IssueItemDTO> IssueItems { get; set; } = new List<IssueItemDTO>();		
	}
}
