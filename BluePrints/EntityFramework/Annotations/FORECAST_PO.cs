namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class FORECAST_PO : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        //used for collection mapping which allows null values
        [NotMapped]
        public Guid? MappingEntityKey
        {
            get
            {
                if (GUID == Guid.Empty)
                    return null;

                return GUID;
            }
        }

        public string ViewStockCode => STOCK_CODE == null || STOCK_CODE == string.Empty? COMMODITY_CODE : STOCK_CODE;

        public string Office => PROJECT.Office;
    }
}