using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.CategoryModels
{
	public class CategoryViewDTO :IMapFrom<Category>
	{
		public int Id { get; set; }
		public required string Name { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Category, CategoryViewDTO>();
		}
	}
}
