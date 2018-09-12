using BluePrints.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    public partial class VARIATION_REGISTER
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [Required]
        [StringLength(50)]
        public string SUBJOB_CODE { get; set; }

        [Required]
        [StringLength(50)]
        public string DISCIPLINE_CODE { get; set; }

        [StringLength(50)]
        public string COMMODITY_CODE { get; set; }

        [Required]
        [StringLength(50)]
        public string VARIATION_CODE { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        public decimal ORIGINAL_VALUE { get; set; }

        public decimal CURRENT_VALUE { get; set; }

        [StringLength(50)]
        public string COSTCODE { get; set; }

        public decimal COST { get; set; }

        public VariationRegisterStatus STATUS { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
