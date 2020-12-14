using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Misc
{
    public class TenderProfileItemEditModel
    {
        public Guid TenderProfileItemGuid { get; set; }
        public Guid DepartmentGuid { get; set; }
        public Guid DisciplineGuid { get; set; }
        public decimal HoursPercentageFrom { get; set; }
        public decimal HoursPercentageTo { get; set; }
    }
}
