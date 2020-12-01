using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Misc
{
    public class DeliverableEditModel
    {
        public Guid DeliverableGuid { get; set; }
        public RowEditAction Action { get; set; }
        public string Name { get; set; }
        public decimal UnitsFrom { get; set; }
        public decimal UnitsTo { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public BellCurveShape? BellCurveShape { get; set; }
        public Guid? DisciplineGuid { get; set; }
    }
}
