namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using DevExpress.Mvvm.POCO;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using BaseModel.DataModel;

    [ConstraintAttributes("GUID_DEPARTMENT, GUID_DISCIPLINE, GUID_COMMODITY")]
    public partial class RATE : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public PhaseType Phase_Type
        {
            get { return PHASE_TYPE; }
            set
            {
                PHASE_TYPE = value;
                this.Update();
            }
        }

        [NotMapped]
        public Guid? CommodityCodeId
        {
            get { return GUID_COMMODITY; }
            set
            {
                if (CommodityCodes == null)
                    GUID_COMMODITY = value;
                else if (value == null || CommodityCodes.Any(x => x.Key.ToString().ToUpper() == value.ToString().ToUpper()))
                    GUID_COMMODITY = value;
            }
        }

        [NotMapped]
        private IEnumerable<CombinedCommodityCode> commodityCodes;
        public IEnumerable<CombinedCommodityCode> CommodityCodes
        {
            get
            {
                if(commodityCodes != null)
                    return commodityCodes.Where(x => x.PhaseType == PHASE_TYPE);

                return null;
            }
        }

        public void SetCommodityCodes(IEnumerable<CombinedCommodityCode> commodityCodes)
        {
            this.commodityCodes = commodityCodes;
        }
    }
}