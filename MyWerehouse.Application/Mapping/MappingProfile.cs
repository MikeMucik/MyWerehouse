using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace MyWerehouse.Application.Mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile() 
		{
			ApplyMappingsProfile(Assembly.GetExecutingAssembly());
		}
		private void ApplyMappingsProfile(Assembly assembly)
		{
			var types = assembly.GetExportedTypes()
				.Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MappingProfile))).ToList();
			foreach (var type in types)
			{
				var instance = Activator.CreateInstance(type);
				var methodInfo = type.GetMethod("Mapping");
				methodInfo?.Invoke(instance, new object[] { this });
			}
		}
	}
}
