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
    using BluePrints.Common.ViewModel.Reporting;

    [ConstraintAttributes("GUID_AREA, GUID_SUBAREA, GUID_DISCIPLINE, GUID_DEPARTMENT, COMMODITY_CODE, VARIATION_CODE, IsRateExists")]
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
        public IEnumerable<AREA> SubAreaCollection
        {
            get
            {
                //when it's in read only mode we can use navigational properties to get sub areas
                if (AREA != null)
                    return AREA.AREA1;

                if (GUID_AREA == null || NewItemRowSubAREACollection == null)
                    return null;

                return NewItemRowSubAREACollection.Where(x => x.GUID_PARENT == GUID_AREA);
            }
        }

        public IEnumerable<AREA> NewItemRowSubAREACollection { get; set; }

        [NotMapped]
        private IEnumerable<CombinedCommodityCode> allCommodityCodes;

        [NotMapped]
        private IEnumerable<DISCIPLINE> allDISCIPLINES;

        [NotMapped]
        private List<CombinedCommodityCode> validCommodityCodes;

        [NotMapped]
        private List<CombinedCommodityCode> validCommodityCodesByDiscipline;

        [NotMapped]
        private List<CombinedCommodityCode> validCommodityCodesByDepartment;

        public IEnumerable<CombinedCommodityCode> ValidCommodityCodes
        {
            get
            {
                if (allCommodityCodes == null)
                    return new List<CombinedCommodityCode>();

                if(GUID_PHASE == null)
                    return allCommodityCodes;

                if (validCommodityCodes == null)
                {
                    IEnumerable<CombinedCommodityCode> validCommodityCodesByPhase = allCommodityCodes.Where(x => x.PhaseType == PHASE_TYPE);

                    if(COST_TYPE == CostType.Charge)
                    {  
                        validCommodityCodesByDepartment = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE && x.GuidDepartment == GUID_DEPARTMENT).ToList();
                        validCommodityCodesByDiscipline =  validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE && (x.GuidDiscipline == GUID_DISCIPLINE || x.GuidDiscipline == null)).ToList();
                        if (GUID_DEPARTMENT != null && GUID_DISCIPLINE != null)
                            validCommodityCodes = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE && x.GuidDepartment == GUID_DEPARTMENT && (x.GuidDiscipline == GUID_DISCIPLINE || x.GuidDiscipline == null)).ToList();
                        else if (GUID_DEPARTMENT == null && GUID_DISCIPLINE == null)
                            validCommodityCodes = validCommodityCodesByPhase.ToList();
                        else if (GUID_DEPARTMENT == null)
                            validCommodityCodes = validCommodityCodesByDiscipline;
                        else if (GUID_DISCIPLINE == null)
                            validCommodityCodes = validCommodityCodesByDepartment;
                    }
                    else
                    {
                        //commodity codes doesn't have department
                        validCommodityCodesByDepartment = validCommodityCodesByPhase.ToList();
                        validCommodityCodesByDiscipline = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE && (x.GuidDiscipline == null || x.GuidDiscipline == GUID_DISCIPLINE)).ToList();

                        if (GUID_DISCIPLINE == null)
                            validCommodityCodes = validCommodityCodesByPhase.Where(x => x.PhaseType == PHASE_TYPE).ToList();
                        else
                            validCommodityCodes = validCommodityCodesByDiscipline;
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
        public void SetLookupProperties(IEnumerable<CombinedCommodityCode> commodityCodes, IEnumerable<DISCIPLINE> disciplines, IEnumerable<AREA> subAreas)
        {
            validCommodityCodes = null;
            validDISCIPLINES = null;
            this.allCommodityCodes = commodityCodes;
            this.allDISCIPLINES = disciplines;
            this.NewItemRowSubAREACollection = subAreas;

            Update();
        }

        public bool IsGangRateCalculatable
        {
            get
            {
                if (this.ManagerPercent > 0 && this.ManagerRate == 0)
                    return false;
                if (this.ManagerPercent == 0 && this.ManagerRate > 0)
                    return false;
                if (this.PrincipalPercent > 0 && this.PrincipalRate == 0)
                    return false;
                if (this.PrincipalPercent == 0 && this.PrincipalRate > 0)
                    return false;
                if (this.LeadPercent > 0 && this.LeadRate == 0)
                    return false;
                if (this.SeniorPercent == 0 && this.SeniorRate > 0)
                    return false;
                if (this.SeniorPercent > 0 && this.SeniorRate == 0)
                    return false;
                if (this.EngineerPercent == 0 && this.EngineerRate > 0)
                    return false;
                if (this.EngineerPercent > 0 && this.EngineerRate == 0)
                    return false;
                if (this.GraduatePercent == 0 && this.GraduateRate > 0)
                    return false;
                if (this.GraduatePercent > 0 && this.GraduateRate == 0)
                    return false;
                if (this.UndergraduatePercent == 0 && this.UndergraduateRate > 0)
                    return false;
                if (this.UndergraduatePercent > 0 && this.UndergraduateRate == 0)
                    return false;

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

                if (GUID_AREA != null)
                    constraint += GUID_AREA.ToString();
                if (GUID_SUBAREA != null)
                    constraint += GUID_SUBAREA.ToString();
                if (GUID_DISCIPLINE != null)
                    constraint += GUID_DISCIPLINE.ToString();
                if (DISCIPLINE_NUM != null)
                    constraint += DISCIPLINE_NUM.ToString();
                if (GUID_DEPARTMENT != null)
                    constraint += GUID_DEPARTMENT.ToString();
                if (COMMODITY_CODE != null)
                    constraint += COMMODITY_CODE;
                if (VARIATION_CODE != null)
                    constraint += VARIATION_CODE;

                constraint += IsRateExists;
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

        [NotMapped]
        public bool IsRateExists
        {
            get
            {
                return GUID != Guid.Empty;
            }
            set
            {
            }
        }

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

        private string rateError = "Rate must be set in order for gang rate to be calculated";
        private string percentError = "Percent must be set in order for gang rate to be calculated";
        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().COMMODITY_CODE)))
            {
                if (COMMODITY_CODE != string.Empty && COMMODITY_CODE != null)
                {
                    if (GUID_DISCIPLINE != null && IsNotValidByDiscipline)
                    {
                        info.ErrorText = "Document type is not valid for selected discipline";
                    }
                    else if (GUID_DEPARTMENT != null && IsNotValidByDepartment)
                    {
                        info.ErrorText = "Document type is not valid for selected department";
                    }
                    else if (IsNotValidByAllAttributes)
                    {
                        string commodityName;
                        if (COST_TYPE == CostType.Cost)
                            commodityName = "Commodity code";
                        else
                            commodityName = "Document type";

                        info.ErrorText = commodityName + " is not valid for selected department and discipline";
                    }
                }
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().RATE1)))
            {
                if (TotalPercent < 1 && (ManagerPercent > 0 || PrincipalPercent > 0 || LeadPercent > 0 || LeadPercent > 0 || SeniorPercent > 0 || EngineerPercent > 0 || GraduatePercent > 0 || UndergraduatePercent > 0))
                {
                    decimal remainingPercent = 1 - TotalPercent;
                    info.ErrorText = String.Format("{0:P2}.", remainingPercent) + " remaining for gang rate to be calculatable";
                }
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().MANAGER_RATE)))
            {
                if (ManagerPercent > 0 && ManagerRate == 0)
                    info.ErrorText = rateError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().PRINCIPAL_RATE)))
            {
                if (PrincipalPercent > 0 && PrincipalRate == 0)
                    info.ErrorText = rateError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().LEAD_RATE)))
            {
                if (LeadPercent > 0 && LeadRate == 0)
                    info.ErrorText = rateError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().SENIOR_RATE)))
            {
                if (SeniorPercent > 0 && SeniorRate == 0)
                    info.ErrorText = rateError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().ENGINEER_RATE)))
            {
                if (EngineerPercent > 0 && EngineerRate == 0)
                    info.ErrorText = rateError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().GRADUATE_RATE)))
            {
                if (GraduatePercent > 0 && GraduateRate == 0)
                    info.ErrorText = rateError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().UNDERGRADUATE_RATE)))
            {
                if (UndergraduatePercent > 0 && UndergraduateRate == 0)
                    info.ErrorText = rateError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().MANAGER_PERCENT)))
            {
                if (ManagerPercent == 0 && ManagerRate > 0)
                    info.ErrorText = percentError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().PRINCIPAL_PERCENT)))
            {
                if (PrincipalPercent == 0 && PrincipalRate > 0)
                    info.ErrorText = percentError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().LEAD_PERCENT)))
            {
                if (LeadPercent == 0 && LeadRate > 0)
                    info.ErrorText = percentError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().SENIOR_PERCENT)))
            {
                if (SeniorPercent == 0 && SeniorRate > 0)
                    info.ErrorText = percentError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().ENGINEER_PERCENT)))
            {
                if (EngineerPercent == 0 && EngineerRate > 0)
                    info.ErrorText = percentError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().GRADUATE_PERCENT)))
            {
                if (GraduatePercent == 0 && GraduateRate > 0)
                    info.ErrorText = percentError;
            }
            else if (propertyName.Contains(BindableBase.GetPropertyName(() => new RATE().UNDERGRADUATE_PERCENT)))
            {
                if (UndergraduatePercent == 0 && UndergraduateRate > 0)
                    info.ErrorText = percentError;
            }
        }

        private bool IsNotValidByAllAttributes => !ValidCommodityCodes.Any(x => x.Code == COMMODITY_CODE);

        private bool IsNotValidByDepartment => validCommodityCodesByDepartment == null ? false : !validCommodityCodesByDepartment.Any(x => x.Code == COMMODITY_CODE);

        private bool IsNotValidByDiscipline => validCommodityCodesByDiscipline == null ? false : !validCommodityCodesByDiscipline.Any(x => x.Code == COMMODITY_CODE);

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

        [NotMapped]
        public decimal RecommendedRate => TransactionCount == 0 ? 0 : Transactions.Average(x => x.CostPerQty);

        [NotMapped]
        public decimal TransactionCount => Transactions == null ? 0 : Transactions.Count;

        [NotMapped]
        public List<ExoDataPoint> Transactions { get; set; }

        public string ErrorMessageCommodityCode
        {
            get
            {
                if (COMMODITY_CODE == null || COMMODITY_CODE == string.Empty)
                    return "Any";

                return COMMODITY_CODE;
            }
        }

        public string ErrorMessageDisciplineCode
        {
            get
            {
                if (DISCIPLINE != null)
                    return DISCIPLINE.CODE;

                if(allDISCIPLINES != null)
                {
                    DISCIPLINE discipline = allDISCIPLINES.FirstOrDefault(x => x.GUID == GUID_DISCIPLINE);
                    if (discipline != null)
                        return discipline.CODE;
                }

                return string.Empty;
            }
        }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}