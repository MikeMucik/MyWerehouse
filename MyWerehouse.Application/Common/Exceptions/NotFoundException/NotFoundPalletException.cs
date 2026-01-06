using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundPalletException : NotFoundException
	{
		public NotFoundPalletException(string palletId)
			: base($"Paleta o numerze {palletId} nie istnieje.") { }
	}
}
