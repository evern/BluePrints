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
    using DevExpress.XtraEditors.DXErrorProvider;

    [ConstraintAttributes("GUID_DEPARTMENT, GUID_DISCIPLINE, GUID_COMMODITY, GUID_DOCTYPE")]
    public partial class RATE : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IDXDataErrorInfo
    {
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
        public DOCTYPE ManualDOCTYPE { get; set; }

        [NotMapped]
        public DOCTYPE DisplayDOCTYPE => ManualDOCTYPE != null ? ManualDOCTYPE : DOCTYPE;

        [NotMapped]
        public COMMODITY_CODE ManualCOMMODITY_CODE { get; set; }

        [NotMapped]
        public COMMODITY_CODE DisplayCOMMODITY_CODE => ManualCOMMODITY_CODE != null ? ManualCOMMODITY_CODE : COMMODITY_CODE;

        [NotMapped]
        public Guid? CommodityCodeId => COST_TYPE == CostType.Charge ? GUID_DOCTYPE : GUID_COMMODITY;

        [NotMapped]
        private IEnumerable<CombinedCommodityCode> allCommodityCodes;
        [NotMapped]
        private IEnumerable<CombinedCommodityCode> validCommodityCodes;

        public IEnumerable<CombinedCommodityCode> ValidCommodityCodes
        {
            get
            {
                if (allCommodityCodes == null)
                    return new List<CombinedCommodityCode>();

                if(GUID_PHASE == null)
                    return allCommodityCodes;

                if (COST_TYPE == CostType.Charge && GUID_DEPARTMENT == null)
                    return allCommodityCodes;

                if (validCommodityCodes == null)
                {
                    IEnumerable<CombinedCommodityCode> validCommodityCodesByPhase = allCommodityCodes.Where(x => x.PhaseType == PHASE_TYPE);

                    //doc types doesn't have discipline
                    if(COST_TYPE == CostType.Charge)
                    {
                        validCommodityCodes = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE && x.GuidDepartment == GUID_DEPARTMENT).ToList();
                    }
                    //commodity codes doesn't have department
                    else
                    {
                        if (GUID_DISCIPLINE == null)
                            validCommodityCodes = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE).ToList();
                        else
                            validCommodityCodes = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE && (x.GuidDiscipline == null || x.GuidDiscipline == GUID_DISCIPLINE)).ToList();
                    }
                }

                return validCommodityCodes;
            }
        }

        public void SetCommodityCodes(IEnumerable<CombinedCommodityCode> commodityCodes)
        {
            validCommodityCodes = null;
            this.allCommodityCodes = commodityCodes;
            Update();
        }

        public bool IsGangRateCalculatable
        {
            get
            {
                return TotalPercent == 1;
            }
        }

        public decimal TotalPercent
        {
            get
            {
                return (this.ManagerPercent + this.PrincipalPercent + this.LeadPercent + this.SeniorPercent + this.EngineerPercent + this.GraduatePercent + this.UndergraduatePercent);
            }
        }

        public string UniqueConstraint
        {
            get
            {
                string constraint = string.Empty;
                constraint += PHASE_TYPE.ToString();
                constraint += CHARGE_TYPE.ToString();

                if (GUID_DEPARTMENT != null)
                    constraint += GUID_DEPARTMENT.ToString();
                if (GUID_DISCIPLINE != null)
                    constraint += GUID_DISCIPLINE.ToString();
                if (GUID_DOCTYPE != null)
                    constraint += GUID_DOCTYPE.ToString();

                return constraint;
            }
        }

        public decimal ManagerPercent => this.MANAGER_PERCENT == null ? 0 : (decimal)this.MANAGER_PERCENT;
        public decimal PrincipalPercent => this.PRINCIPAL_PERCENT == null ? 0 : (decimal)this.PRINCIPAL_PERCENT;
        public decimal LeadPercent => this.LEAD_PERCENT == null ? 0 : (decimal)this.LEAD_PERCENT;
        public decimal SeniorPercent => this.SENIOR_PERCENT == null ? 0 : (decimal)this.SENIOR_PERCENT;
        public decimal EngineerPercent => this.ENGINEER_PERCENT == null ? 0 : (decimal)this.ENGINEER_PERCENT;
        public decimal GraduatePercent => this.GRADUATE_PERCENT == null ? 0 : (decimal)this.GRADUATE_PERCENT;
        public decimal UndergraduatePercent => this.UNDERGRADUATE_PERCENT == null ? 0 : (decimal)this.UNDERGRADUATE_PERCENT;

        public decimal ManagerRate => this.MANAGER_RATE == null ? 0 : (decimal)this.MANAGER_RATE;
        public decimal PrincipalRate => this.PRINCIPAL_RATE == null ? 0 : (decimal)this.PRINCIPAL_RATE;
        public decimal LeadRate => this.LEAD_RATE == null ? 0 : (decimal)this.LEAD_RATE;
        public decimal SeniorRate => this.SENIOR_RATE == null ? 0 : (decimal)this.SENIOR_RATE;
        public decimal EngineerRate => this.ENGINEER_RATE == null ? 0 : (decimal)this.ENGINEER_RATE;
        public decimal GraduateRate => this.GRADUATE_RATE == null ? 0 : (decimal)this.GRADUATE_RATE;
        public decimal UndergraduateRate => this.UNDERGRADUATE_RATE == null ? 0 : (decimal)this.UNDERGRADUATE_RATE;

        public List<Tuple<decimal, decimal>> GetGangRateTable()
        {
            if (!IsGangRateCalculatable)
                return null;

            List<Tuple<decimal, decimal>> ratesTable = new List<Tuple<decimal, decimal>>();
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.ManagerPercent, (decimal)this.ManagerRate));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.PrincipalPercent, (decimal)this.PrincipalRate));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.LeadPercent, (decimal)this.LeadRate));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.SeniorPercent, (decimal)this.SeniorRate));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.EngineerPercent, (decimal)this.EngineerRate));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.GraduatePercent, (decimal)this.GraduateRate));
            ratesTable.Add(new Tuple<decimal, decimal>((decimal)this.UndergraduatePercent, (decimal)this.UndergraduateRate));

            return ratesTable;
        }

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (GUID_DOCTYPE != null && propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().GUID_DOCTYPE)))
            {
                if (GUID_DEPARTMENT != null && !ValidCommodityCodes.Any(x => x.Key == GUID_DOCTYPE))
                {
                    info.ErrorText = "Document type is not valid for selected department";
                }
            }
            if (GUID_COMMODITY != null && propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().GUID_COMMODITY)))
            {
                if (GUID_DISCIPLINE != null && !ValidCommodityCodes.Any(x => x.Key == GUID_COMMODITY))
                {
                    info.ErrorText = "Document type is not valid for selected discipline";
                }
            }
        }

        public void GetError(ErrorInfo info)
        {
        }

        [NotMapped]
        public bool IsUsingGangRate
        {
            get
            {
                return IsGangRateCalculatable && RATE1 == GangRate;
            }
        }

        [NotMapped]
        public decimal? GangRate
        {
            get
            {
                if (!IsGangRateCalculatable)
                    return null;

                return GetGangRateTable().Sum(x => x.Item1 * x.Item2);
            }
        }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}