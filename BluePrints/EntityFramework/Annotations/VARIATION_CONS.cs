namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("GUID_PROJECT, NAME")]
    public partial class VARIATION_CONS : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public ObservableCollection<VARIATION_CONS_ITEM> DetailEntities
        {
            get { return GetProperty(() => DetailEntities); }
            set { SetProperty(() => DetailEntities, value, OnVARIATION_ITEMSChanged); }
        }

        void OnVARIATION_ITEMSChanged()
        {
            RaisePropertyChanged(() => Management_Costs);
            RaisePropertyChanged(() => Engineering_Costs);
            RaisePropertyChanged(() => Trades_Costs);
            RaisePropertyChanged(() => Equipments_Costs);
            RaisePropertyChanged(() => Materials_Costs);
            RaisePropertyChanged(() => Total_Costs);
        }

        public decimal Management_Costs
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Where(x => x.TYPE == Common.ConstructionVariationItemType.Indirect).Sum(x => x.Total);
            }
        }

        public decimal Engineering_Costs
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Where(x => x.TYPE == Common.ConstructionVariationItemType.Engineering).Sum(x => x.Total);
            }
        }

        public decimal Trades_Costs
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Where(x => x.TYPE == Common.ConstructionVariationItemType.Trade).Sum(x => x.Total);
            }
        }

        public decimal Equipments_Costs
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Where(x => x.TYPE == Common.ConstructionVariationItemType.Equipment).Sum(x => x.Total);
            }
        }

        public decimal Materials_Costs
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Where(x => x.TYPE == Common.ConstructionVariationItemType.Material).Sum(x => x.Total);
            }
        }

        public decimal Total_Costs => Management_Costs + Engineering_Costs + Trades_Costs + Equipments_Costs + Materials_Costs;

        public decimal Outstanding_Value => Total_Costs - (APPROVED_VALUE == null ? 0 : (decimal)APPROVED_VALUE);

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}