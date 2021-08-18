using BluePrints.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BluePrints.Data
{
    public partial class CONSTRUCTION_STAGE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CONSTRUCTION_STAGE()
        {
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [Required]
        [StringLength(500)]
        public string NAME { get; set; }

        public decimal WEIGHT_PERCENTAGE { get; set; }

        public int SORT_ORDER { get; set; }

        public ScoreCardDiscipline SCORE_CARD_DISCIPLINE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
