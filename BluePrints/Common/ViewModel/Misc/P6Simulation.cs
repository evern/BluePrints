using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Misc
{
    public class P6Simulation
    {
        readonly P6_ASSIGNMENT assignment;
        public P6Simulation(P6_ASSIGNMENT assignment)
        {
            this.assignment = assignment;
        }

        public Guid DeliverableOriginalEntityKey { get; set; }
        public P6_ASSIGNMENT Assignment => assignment;
        public decimal PushUnits { get; set; }
        public decimal PostPushUnits { get; set; }
        public decimal MaxUnits { get; set; }
        public decimal CurrentTaskAssignmentPct { get; set; }
        public DateTime? TaskStartDate { get; set; }
        public DateTime? TaskEndDate { get; set; }
    }
}
