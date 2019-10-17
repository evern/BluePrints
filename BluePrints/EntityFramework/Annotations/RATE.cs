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

    [ConstraintAttributes("GUID_DISCIPLINE, GUID_DISCIPLINE, GUID_COMMODITY, GUID_DOCTYPE")]
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

        //so that user doesn't have to exit lookupedit to see property changes and description can be populated immediately
        [NotMapped]
        public DOCTYPE DisplayDOCTYPE => ManualDOCTYPE != null ? ManualDOCTYPE : DOCTYPE;

        [NotMapped]
        public COMMODITY_CODE ManualCOMMODITY_CODE { get; set; }

        //so that user doesn't have to exit lookupedit to see property changes and description can be populated immediately
        [NotMapped]
        public COMMODITY_CODE DisplayCOMMODITY_CODE => ManualCOMMODITY_CODE != null ? ManualCOMMODITY_CODE : COMMODITY_CODE;

        [NotMapped]
        public Guid? CommodityCodeId => COST_TYPE == CostType.Charge ? GUID_DOCTYPE : GUID_COMMODITY;

        [NotMapped]
        private IEnumerable<CombinedCommodityCode> allCommodityCodes;

        [NotMapped]
        private IEnumerable<DISCIPLINE> allDISCIPLINES;

        [NotMapped]
        private List<CombinedCommodityCode> validCommodityCodes;
        public IEnumerable<CombinedCommodityCode> ValidCommodityCodes
        {
            get
            {
                if (allCommodityCodes == null)
                    return new List<CombinedCommodityCode>();

                if(GUID_PHASE == null)
                    return allCommodityCodes;

                if (COST_TYPE == CostType.Charge && GUID_DISCIPLINE == null)
                    return allCommodityCodes;

                if (validCommodityCodes == null)
                {
                    IEnumerable<CombinedCommodityCode> validCommodityCodesByPhase = allCommodityCodes.Where(x => x.PhaseType == PHASE_TYPE);

                    //doc types doesn't have discipline
                    if(COST_TYPE == CostType.Charge)
                    {
                        validCommodityCodes = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE && x.GuidDepartment == GUID_DISCIPLINE).ToList();
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

        [NotMapped]
        private List<DISCIPLINE> validDISCIPLINES;
        public IEnumerable<DISCIPLINE> ValidDISCIPLINES
        {
            get
            {
                if (allDISCIPLINES == null || allCommodityCodes == null)
                    return new List<DISCIPLINE>();

                if (GUID_PHASE == null)
                    return allDISCIPLINES;
                
                if (validDISCIPLINES == null)
                {
                    IEnumerable<Guid?> validDISCIPLINESGuid = allCommodityCodes.Where(x => x.PhaseType == PHASE_TYPE).Select(x => x.GuidDiscipline);
                    if (validDISCIPLINESGuid.Any(x => x == null))
                        validDISCIPLINES = allDISCIPLINES.ToList();
                    else
                    {
                        List<Guid?> tempValidUniqueDISCIPLINEGuids = validDISCIPLINESGuid.Where(x => x != null).Distinct().ToList();

                        validDISCIPLINES = new List<DISCIPLINE>();
                        foreach (DISCIPLINE department in allDISCIPLINES)
                        {
                            if (tempValidUniqueDISCIPLINEGuids.Any(x => x == department.GUID))
                                validDISCIPLINES.Add(department);
                        }
                    }
                }

                return validDISCIPLINES;
            }
        }
        public void SetLookupProperties(IEnumerable<CombinedCommodityCode> commodityCodes, IEnumerable<DISCIPLINE> disciplines)
        {
            validCommodityCodes = null;
            validDISCIPLINES = null;
            this.allCommodityCodes = commodityCodes;
            this.allDISCIPLINES = disciplines;
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

                if (GUID_DISCIPLINE != null)
                    constraint += GUID_DISCIPLINE.ToString();
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
                if (GUID_DISCIPLINE != null && !ValidCommodityCodes.Any(x => x.Key == GUID_DOCTYPE))
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