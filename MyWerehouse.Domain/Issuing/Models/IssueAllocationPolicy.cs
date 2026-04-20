using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Issuing.Models
{
	public enum IssueAllocationPolicy
	{
		FullPalletFirst, //pełne palety 
		//FefoStrict, //najpierw data później pełne palety TODO
		//FefoWithFullPalletPreference //najpierw pełne palety ale jeśli brak to zbiórka po dacie TODO
	}
}
