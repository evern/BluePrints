namespace BluePrints.Data
{
    using BluePrints.Common;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TRANSACTION_APPROVAL")]

    public partial class TRANSACTION_APPROVAL
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public int JOB_TRANSACTION_SEQNO { get; set; }

        [StringLength(15)]
        public string OLD_JOBCODE { get; set; }

        public int? OLD_JOBNO { get; set; }

        public int? NEW_JOBNO { get; set; }

        [StringLength(5)]
        public string OLD_DISCIPLINECODE { get; set; }

        public int? OLD_COST_GROUP_NO { get; set; }

        public int? NEW_COST_GROUP_NO { get; set; }

        [StringLength(5)]
        public string OLD_COMMODITYCODE { get; set; }

        public int? OLD_COST_TYPE_NO { get; set; }

        public int? NEW_COST_TYPE_NO { get; set; }

        [StringLength(20)]
        public string OLD_STOCK_CODE { get; set; }

        [StringLength(20)]
        public string NEW_STOCK_CODE { get; set; }

        public TransactionApprovalStatus STATUS { get; set; }

        public DateTime? APPROVEDON { get; set; }

        public Guid? APPROVEDBY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
