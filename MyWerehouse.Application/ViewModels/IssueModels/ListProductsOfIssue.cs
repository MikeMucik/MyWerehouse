using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class ListProductsOfIssue 
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public required List<IssueItemDTO> Values { get; set; }		
	}
}
