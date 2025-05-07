using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.CategoryModels
{
	public class CategoryDTO : IMapFrom<Category>
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Category, CategoryDTO>()
				.ReverseMap();
		}
	}
}
