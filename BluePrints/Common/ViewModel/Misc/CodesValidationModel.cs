using BaseModel.DataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using DevExpress.Mvvm;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public abstract class CodesValidationModel : EntityBase, IDXDataErrorInfo
    {
        protected abstract string disciplineCodePropertyName { get; }
        protected abstract string commodityCodePropertyName { get; }
        protected abstract string stockCodePropertyName { get; }
        protected abstract string exoBudgetPropertyName { get; }
        protected abstract string subJobCode { get; }
        protected abstract string disciplineCode { get; }
        protected abstract string commodityCode { get; }
        protected abstract string stockCode { get; }
        protected abstract decimal exoBudget { get; }
        protected abstract decimal budget { get; }
        protected abstract bool isLineExists { get; }
        protected abstract bool ignoreBudgetError { get; }

        #region Commodity Codes
        private IEnumerable<COMMODITY_CODE> COMMODITY_CODES { get; set; }
        public void PopulateCommodityCodes(IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection)
        {
            validCommodityCodes = null;
            COMMODITY_CODES = COMMODITY_CODECollection;
        }

        public bool IsCommodityCodeValid
        {
            get
            {
                if (commodityCode == null || ValidCommodityCodes.Count() == 0 || commodityCode.Length < 2)
                    return false;

                if (PhaseType == Common.PhaseType.Tender)
                {
                    if (commodityCode.Substring(0, 2) == BluePrintsResources.Default_TenderCommodityCode)
                        return true;
                    else
                        return false;
                }

                return ValidCommodityCodes.Any(x => x.CODE == commodityCode);
            }
        }

        public List<COMMODITY_CODE> validCommodityCodes = null;
        public IEnumerable<COMMODITY_CODE> ValidCommodityCodes
        {
            get
            {
                if (COMMODITY_CODES == null || disciplineCode == null || disciplineCode.Length < 2 || PhaseType == null)
                    return new List<COMMODITY_CODE>();

                if (validCommodityCodes == null)
                {
                    validCommodityCodes = BluePrintsDataUtils.FilterForValidCommodityCodes(COMMODITY_CODES, PhaseType, disciplineCode).ToList();
                }

                return validCommodityCodes;
            }
        }
        #endregion

        #region Stock Codes
        private IEnumerable<STOCK_ITEMS> STOCK_ITEMS { get; set; }
        public void PopulateStockCodes(IEnumerable<STOCK_ITEMS> STOCK_ITEMSCollection)
        {
            STOCK_ITEMS = STOCK_ITEMSCollection;
        }

        public bool IsStockCodeValid
        {
            get
            {
                if (commodityCode == null || ValidStockCodes.Count() == 0)
                    return false;

                string stockCodeForSearching = stockCode == null || stockCode == string.Empty ? commodityCode : stockCode;
                if (PhaseType == Common.PhaseType.Tender)
                {
                    if (stockCodeForSearching == BluePrintsResources.Default_TenderStockCode)
                        return true;
                    else
                        return false;
                }

                return ValidStockCodes.Any(x => x == stockCodeForSearching);
            }
        }

        List<string> validStockCodes = null;
        public IEnumerable<string> ValidStockCodes
        {
            get
            {
                if (COMMODITY_CODES == null || disciplineCode == null || disciplineCode.Length < 2 || PhaseType == null || commodityCode == null)
                    return new List<string>();

                if (validStockCodes == null)
                {
                    if (PhaseType == Common.PhaseType.Tender)
                        validStockCodes = COMMODITY_CODES.Where(x => x.CODE == commodityCode && (x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == "CO"))).Select(x => x.DEFAULT_STOCKCODE).OrderBy(x => x).ToList();
                    else
                    {
                        string disciplineCodeWithoutEnumeration = disciplineCode.Substring(0, 2);
                        validStockCodes = COMMODITY_CODES.Where(x => x.PHASE_TYPE == PhaseType && x.CODE == commodityCode && (x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == disciplineCodeWithoutEnumeration))).Select(x => x.DEFAULT_STOCKCODE).OrderBy(x => x).ToList();
                    }
                }

                return validStockCodes;
            }
        }
        #endregion

        public string PhaseTypeStr
        {
            get
            {
                if (PhaseType == null)
                    return string.Empty;
                else
                    return PhaseType.ToString();
            }
        }

        public PhaseType? PhaseType
        {
            get
            {
                if (subJobCode == null)
                    return null;

                if (subJobCode.Length < 15)
                    return Common.PhaseType.Tender;

                string phaseTypeString = subJobCode.Substring(13, 1).ToUpper();
                if (phaseTypeString == "I")
                    return Common.PhaseType.Indirect;
                else if (phaseTypeString == "P")
                    return Common.PhaseType.Procurement;
                else if (phaseTypeString == "D")
                    return Common.PhaseType.Design;
                else if (phaseTypeString == "C")
                    return Common.PhaseType.Construct;

                return null;
            }
        }

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (subJobCode != null && subJobCode.Length == 15)
            {
                if (propertyName == commodityCodePropertyName && !IsCommodityCodeValid)
                {
                    info.ErrorText = "Invalid commodity code, please check phase and discipline";
                }

                if (propertyName == stockCodePropertyName && !IsStockCodeValid)
                {
                    info.ErrorText = "Invalid stock code, please check commodity code";
                }

                if (propertyName == exoBudgetPropertyName)
                {
                    if (isLineExists && !ignoreBudgetError && Math.Round(exoBudget, 0) != Math.Round(budget, 0))
                        info.ErrorText = "Exo budget doesn't equal to budget from deliverables list";
                }
            }
            else
            {
                if (propertyName == disciplineCodePropertyName && disciplineCode != BluePrintsResources.Default_TenderDisciplineCode)
                {
                    info.ErrorText = "Discipline code must be " + BluePrintsResources.Default_TenderDisciplineCode;
                }

                if (propertyName == commodityCodePropertyName && !IsCommodityCodeValid)
                {
                    info.ErrorText = "Commodity code must be " + BluePrintsResources.Default_TenderCommodityCode;
                }

                if (propertyName == stockCodePropertyName && !IsStockCodeValid)
                {
                    info.ErrorText = "Stock code must be " + BluePrintsResources.Default_TenderStockCode;
                }
            }
        }

        public void GetError(ErrorInfo info)
        {
        }
    }
}
