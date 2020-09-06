using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    public partial class PIPELINE_PROFILE_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PIPELINE { get; set; }

        public Guid GUID_DEPARTMENT { get; set; }

        public Guid GUID_DISCIPLINE { get; set; }

        public decimal HOURS_PERCENTAGE { get; set; }

        public decimal SCHEDULE_START_PERCENTAGE { get; set; }

        public decimal SCHEDULE_FINISH_PERCENTAGE { get; set; }

        public int? BELLCURVESHAPE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PIPELINE PIPELINE { get; set; }
    }
}
