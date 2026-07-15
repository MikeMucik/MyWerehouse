using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Histories.Models
{
	public enum ReasonForPallet
	{
		New = 0, // nowa paleta 
		Received = 1,//nowa paleta w przyjęciu
		Picking = 2,//paleta źródło kompletacji
		Moved = 3,//paleta została przeniesiona z miejsca na inne miejsce
		Correction = 4,//paleta została poprawiona
		Merge = 5,//paleta została połączona z inną paletą
		ToLoad = 6,//paleta dodana do wydania
		Loaded = 7,//paleta załadowana
		CancelIssue = 8,//paleta została wycofana z issue z powodu kasacji issue
		ReversePicking = 9,//paleta pod wpływem działania dekompletacyjnego		
	}
}
