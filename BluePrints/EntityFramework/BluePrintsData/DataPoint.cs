using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DataPoint")]
    public partial class DataPoint
    {
        [Key]
        public Guid Guid_DataPoint { get; set; }

        [Required]
        [StringLength(50)]
        public string ProjectNumber { get; set; }

        public bool IsPlanned { get; set; }

        public Guid Deliverable_Guid { get; set; }

        public Guid Original_Guid { get; set; }

        public DateTime UniversalPeriodStartDate { get; set; }

        public DateTime UniversalPeriodEndDate { get; set; }

        public double PeriodUnits { get; set; }

        public double PeriodPrice { get; set; }

        public bool IsFromP6 { get; set; }
    }
}
