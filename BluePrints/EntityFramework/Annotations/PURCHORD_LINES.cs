namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using BluePrints.Common.Projections;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PURCHORD_LINES : CodesValidationModel
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

        protected override string disciplineCodePropertyName => BindableBase.GetPropertyName(() => new PURCHORD_LINES().COSTGROUP);

        protected override string commodityCodePropertyName => BindableBase.GetPropertyName(() => new PURCHORD_LINES().COSTTYPE);

        protected override string stockCodePropertyName => BindableBase.GetPropertyName(() => new PURCHORD_LINES().STOCKCODE);

        //this is use in DXDataError and the validation error will not be thrown there because this property name doesn't exist
        protected override string exoBudgetPropertyName => "DontExist";

        protected override string subJobCode => Subjob_Name;

        protected override string disciplineCode => Discipline_Code;

        protected override string commodityCode => Commodity_Code;

        protected override string stockCode => STOCKCODE;

        //this property will not be used here because it validates on exoBudgetPropertyName
        protected override decimal exoBudget => 0;

        //this property will not be used here because it validates on exoBudgetPropertyName
        protected override decimal budget => 0;

        //this property will not be used here because it validates on exoBudgetPropertyName
        protected override bool isLineExists => true;

        protected override bool ignoreBudgetError => true;
    }
}