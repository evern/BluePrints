using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common
{
    public class CombinedCommodityCode
    {
        public PhaseType PhaseType { get; set; }
        public Guid Key { get; set; }
        public Guid? GuidDepartment { get; set; }
        public Guid? GuidDiscipline { get; set; }
        public Guid? GuidCommodity { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Code;
        }
    }
}
