using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VARIATION_CONSTRUCTION_ITEM
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_VARIATION { get; set; }

        public VariationConstructionItemType TYPE { get; set; }

        [Required]
        [StringLength(50)]
        public string ITEM_ID { get; set; }

        [StringLength(50)]
        public string SUBJOB { get; set; }

        [StringLength(50)]
        public string COSTGROUP { get; set; }

        [StringLength(50)]
        public string COSTTYPE { get; set; }

        [StringLength(50)]
        public string STOCKCODE { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [StringLength(500)]
        public string RESOURCENAME { get; set; }

        [StringLength(1000)]
        public string NOTES { get; set; }

        public decimal HOURS { get; set; }

        public decimal RATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual VARIATION_CONSTRUCTION VARIATION_CONSTRUCTION { get; set; }
    }
}
