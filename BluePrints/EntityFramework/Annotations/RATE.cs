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

        [NotMapped]
        public bool IsGangRateTableCompletable
        {
            get
            {
                return this.MANAGER_PERCENT != null && this.MANAGER_RATE != null && this.PRINCIPAL_PERCENT != null && this.PRINCIPAL_RATE != null && this.LEAD_PERCENT != null && this.LEAD_RATE != null && this.SENIOR_PERCENT != null && this.SENIOR_RATE != null && this.ENGINEER_PERCENT != null && this.ENGINEER_RATE != null && this.GRADUATE_PERCENT != null && this.GRADUATE_RATE != null && this.UNDERGRADUATE_RATE != null && this.UNDERGRADUATE_PERCENT != null;
            }
        }

        public bool IsGangRateCalculatable
        {
            get
            {
                if (!IsGangRateTableCompletable)
                    return false;

                return (this.MANAGER_PERCENT + this.PRINCIPAL_PERCENT + this.LEAD_PERCENT + this.SENIOR_PERCENT + this.ENGINEER_PERCENT + this.GRADUATE_PERCENT + this.UNDERGRADUATE_PERCENT) == 1;
            }
        }

        public List<Tuple<decimal, decimal>> GetGangRateTable()
        {
            if (!IsGangRateTableCompletable)
                return null;

            List<Tuple<decimal, decimal>> ratesTable = new List<Tuple<decimal, decimal>>();
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.MANAGER_PERCENT, (decimal)this.MANAGER_RATE));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.PRINCIPAL_PERCENT, (decimal)this.PRINCIPAL_RATE));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.LEAD_PERCENT, (decimal)this.LEAD_RATE));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.SENIOR_PERCENT, (decimal)this.SENIOR_RATE));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.ENGINEER_PERCENT, (decimal)this.ENGINEER_RATE));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.GRADUATE_PERCENT, (decimal)this.GRADUATE_RATE));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.UNDERGRADUATE_PERCENT, (decimal)this.UNDERGRADUATE_RATE));

            return ratesTable;
        }

        [NotMapped]
        public decimal? GangRate
        {
            get
            {
                if (!IsGangRateTableCompletable)
                    return null;

                if (!IsGangRateCalculatable)
                    return null;

                return GetGangRateTable().Sum(x => x.Item1 * x.Item2);
            }
        }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}