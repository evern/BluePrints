namespace BluePrints.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TRANSACTION_APPROVAL")]

    public partial class TRANSACTION_APPROVAL
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        [Required]
        [StringLength(15)]
        public string OLD_SUBJOB_CODE { get; set; }

        [Required]
        [StringLength(15)]
        public string NEW_SUBJOB_CODE { get; set; }

        [Required]
        [StringLength(4)]
        public string OLD_DISCIPLINE_CODE { get; set; }

        [Required]
        [StringLength(4)]
        public string NEW_DISCIPLINE_CODE { get; set; }

        [Required]
        [StringLength(3)]
        public string OLD_COMMODITY_CODE { get; set; }

        [Required]
        [StringLength(3)]
        public string NEW_COMMODITY_CODE { get; set; }

        [Required]
        [StringLength(20)]
        public string OLD_STOCK_CODE { get; set; }

        [Required]
        [StringLength(20)]
        public string NEW_STOCK_CODE { get; set; }

        public int STATUS { get; set; }

        public DateTime? APPROVEDON { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
