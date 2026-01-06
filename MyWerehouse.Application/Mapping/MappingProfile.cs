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
			//ApplyMappingsProfile(Assembly.GetExecutingAssembly());
			ApplyMappingsProfile(typeof(MappingProfile).Assembly);
		}
		private void ApplyMappingsProfile(Assembly assembly)
		{
			var types = assembly.GetExportedTypes()
				.Where(t => t.GetInterfaces().Any(i =>
				i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
				.ToList();
			if (!types.Any())
			{
				throw new Exception($"[DEBUG] SKANER: Przeszukałem assembly '{assembly.FullName}' i znalazłem 0 klas z IMapFrom! Sprawdź czy DTO implementują interfejs.");
			}

			// Jeśli znalazł, wypisz jakie (zobaczymy to w błędzie testu)
			var names = string.Join(", ", types.Select(t => t.Name));
			// Odkomentuj linię poniżej, żeby zobaczyć listę znalezionych klas w wynikach testu
			// throw new Exception($"[DEBUG] SKANER ZNALAZŁ: {names}");
			// --- DEBUG END ---
			foreach (var type in types)
			{
				var instance = Activator.CreateInstance(type);

				var methodInfo = type.GetMethod("Mapping")
					?? type.GetInterface("IMapFrom`1")?.GetMethod("Mapping");


				//var methodInfo = type.GetMethod("Mapping");
				methodInfo?.Invoke(instance, new object[] { this });
			}
		}
	}
}
