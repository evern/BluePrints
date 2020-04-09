namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using DevExpress.Mvvm.POCO;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class JOB_TRANSACTIONS : EntityBase
    {
        [NotMapped]
        public string SupplierName { get; set; }

        [NotMapped]
        public string InvoiceNumber { get; set; }

        [NotMapped]
        public int? PONumber { get; set; }
    }
}