using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class ListClientsDTO
	{
		public List<ClientDTO> AddClients { get; set; }//zmiana na mniej szczegółów
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public int Count { get; set; }
	}
}
