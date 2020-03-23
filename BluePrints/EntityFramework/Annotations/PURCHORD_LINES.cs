namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PURCHORD_LINES : EntityBase
    {
        [NotMapped]
        public string Subjob_Name { get; set; }

        [NotMapped]
        public string Discipline_Code { get; set; }

        [NotMapped]
        public string Commodity_Code { get; set; }

        [NotMapped]
        public int? Status { get; set; }

        [NotMapped]
        public string Narrative { get; set; }

        [NotMapped]
        public double ExchangeRate { get; set; }

        [NotMapped]
        public DateTime? OrderDate { get; set; }

        [NotMapped]
        public string SupplierName { get; set; }

        [NotMapped]
        public decimal AdjustedUnitPrice => ExchangeRate == 0 ? (decimal)UNITPRICE : ((decimal)UNITPRICE) / ((decimal)ExchangeRate);

        [NotMapped]
        public decimal OrderQty => ORD_QUANT == null ? 0 : ((decimal)ORD_QUANT);

        [NotMapped]
        public decimal SupplyQty => SUP_QUANT == null ? 0 : ((decimal)SUP_QUANT);

        [NotMapped]
        public decimal RemainingQty => OrderQty - SupplyQty;

        [NotMapped]
        public decimal TotalCosts => OrderQty * AdjustedUnitPrice;

        [NotMapped]
        public decimal ReceiptedCosts => SupplyQty * AdjustedUnitPrice;

        [NotMapped]
        public decimal RemainingCosts => RemainingQty * AdjustedUnitPrice;

    }
}