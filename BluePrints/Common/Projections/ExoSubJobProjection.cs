using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using BaseModel.Data.Helpers;
using BluePrints.Common.Resources;
using BaseModel.DataModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.Mvvm;
using BaseModel.Attributes;
using BaseModel.ViewModel.Dialogs;
using System.Text.RegularExpressions;
using System.Data.Linq.SqlClient;
using BluePrints.Common.Misc;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.BluePrintsEntitiesDataModel;

namespace BluePrints.Common.Projections
{
    public class ExoSubJobAuth
    {
        public ExoSubJobAuth()
        {

        }

        public USER User { get; set; }
        public bool ShouldAssign { get; set; }
        public bool? IsAssigned { get; set; }

        public void Update()
        {
            this.RaisePropertiesChanged();
        }
    }

    public class ForecastEACProjection
    {
        public ExoSubJobProjection Job { get; set; }
        public List<DatatableDateCost> DateCosts { get; set; }
    }

    public class DatatableDateCost
    {
        public decimal Cost { get; set; }
        public DateTime Date { get; set; }
    }

    //[ConstraintAttributes("SubJobCode, DisciplineCode, CommodityCode, StockCode, VariationCode")]
    public class ExoSubJobProjection : CodesValidationModel, IGuidEntityKey
    {
        public ExoSubJobProjection()
        {
            AuthUserIds = new List<int>();
            this.VariationCode = string.Empty;
        }
        
        [Key]
        public int? LineId { get; set; }
        public int? SubJobId { get; set; }

        public string SubJobCode { get; set; }
        public string PhaseCode => BluePrintsDataUtils.GetPhaseCode(SubJobCode);

        public string SubJobTitle { get; set; }
        public ChargeType? SubJobChargeType { get; set; }
        public int? DisciplineId { get; set; }

        public string DisciplineCode { get; set; }

        public string DisciplineName { get; set; }
        public int? CommodityId { get; set; }

        public string CommodityCode { get; set; }
        public string CommodityName { get; set; }
        public string CommodityDescription { get; set; }
        public string CommodityUOM { get; set; }
        public string StockCode { get; set; }
        public string StockName { get; set; }

        public decimal ExoForecastRate { get; set; }
        public decimal ExoBudgetQty { get; set; }
        public decimal ExoBudgetCosts { get; set; }
        public string ForecastErrorString { get; set; }

        public string GetStockCode()
        {
            if(StockCode == null || StockCode == string.Empty)
            {
                StockCode = CommodityCode;
            }

            return StockCode;
        }


        public bool CommodityIsIndirectOnly { get; set; }
        public string VariationCode { get; set; }
        public decimal ExoBudget { get; set; }
        public decimal Budget { get; set; }
        public bool IgnoreExoBudgetError { get; set; }
        public decimal Rate { get; set; }

        public List<int> AuthUserIds { get; set; }

        #region Summary in EXO_SubJobCollectionView
        public decimal SubJobActualCostSummary { get; set; }
        public decimal SubJobMaterialCostSummary { get; set; }
        public decimal SubJobRemainingPOCostSummary { get; set; }
        #endregion

        public bool IsSubJobExistsInExo => SubJobCode != string.Empty;
        public bool IsDisciplineExistsInExo => DisciplineCode != string.Empty;
        public bool IsCommodityExistsInExo => CommodityCode != string.Empty;
        bool? isLineExistInExo;
        public bool IsLineExistsInExo
        {
            get
            {
                if (isLineExistInExo == null)
                    isLineExistInExo = LineId != null;

                return (bool)isLineExistInExo;
            }
            set => isLineExistInExo = value;
        }
        
        public bool HasBudget { get; set; }
        public string NullText => IsLineExistsInExo ? "Double click this cell to change title" : "Title can only be changed when job is bookable";

        //Used so that class can be used in view model
        #region View Model Compatibility Members
        public Guid guid { get; set; }

        public Guid GUID { get => guid; set => guid = value; }
        #endregion

        #region Code validation
        protected override string disciplineCodePropertyName => BindableBase.GetPropertyName(() => new ExoSubJobProjection().DisciplineCode);

        protected override string commodityCodePropertyName => BindableBase.GetPropertyName(() => new ExoSubJobProjection().CommodityCode);

        protected override string stockCodePropertyName => BindableBase.GetPropertyName(() => new ExoSubJobProjection().StockCode);

        protected override string exoBudgetPropertyName => BindableBase.GetPropertyName(() => new ExoSubJobProjection().ExoBudget);

        protected override string subJobCode => SubJobCode;

        protected override string disciplineCode => DisciplineCode;

        protected override string commodityCode => CommodityCode;

        protected override string variationCode => VariationCode;

        protected override string stockCode => StockCode;

        protected override decimal exoBudget => ExoBudget;

        protected override decimal budget => Budget;

        protected override bool isLineExists => IsLineExistsInExo;

        protected override bool ignoreBudgetError => IgnoreExoBudgetError;
        #endregion

        public override string ToString()
        {
            return FullCode;
        }

        public string AreaCode
        {
            get
            {
                if (SubJobCode == string.Empty)
                    return string.Empty;
                else if (SubJobCode.Length < 15)
                    return string.Empty;

                return SubJobCode.Substring(6, 3);
            }
        }

        public string SubAreaCode
        {
            get
            {
                if (SubJobCode == string.Empty)
                    return string.Empty;
                else if (SubJobCode.Length < 15)
                    return string.Empty;

                return SubJobCode.Substring(10, 2);
            }
        }

        public string FullCode
        {
            get
            {
                if (SubJobId == null || DisciplineId == null || CommodityId == null)
                    return string.Empty;

                string fullCode = SubJobCode + "-" + DisciplineCode + "-" + CommodityCode;
                if (VariationCode != string.Empty && VariationCode != null)
                    fullCode += "-" + VariationCode;

                return fullCode;
            }
        }

        public string ErrorMessageIdentificationCode
        {
            get
            {
                string subJobCode = SubJobCode == null || SubJobCode == string.Empty ? "XXXXX-XXX-XX-XX" : SubJobCode;
                string disciplineCode = DisciplineCode == null || DisciplineCode == string.Empty ? "XX00" : DisciplineCode;
                string commodityCode = CommodityCode == null || CommodityCode == string.Empty ? "X00" : CommodityCode;

                return subJobCode + "-" + disciplineCode + "-" + commodityCode;
            }
        }
    }

    public static class ExoMethods
    {
        public static bool CommitLineSubJob(ExoSubJobProjection projection, bool editLineAfterCommit, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, string projectNumber, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            if (projection.SubJobCode == null || projection.SubJobCode == string.Empty)
                return false;

            JOBCOST_HDR subjob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, projectNumber, projection.SubJobCode);
            if (projection.SubJobTitle == null || projection.SubJobTitle == string.Empty)
            {
                if (subjob == null)
                {
                    projection.SubJobTitle = "";
                    //var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(string.Empty, projection.SubJobCode + " Title:");
                    //if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input title", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                    //{
                    //    projection.SubJobTitle = bulkEditStringsViewModel.EditValue;
                    //}
                    //else
                    //    return false;
                }
                else
                {
                    projection.SubJobTitle = subjob.TITLE;
                }
            }

            if(subjob == null)
            {
                subjob = CreateNewSubJob(projection.SubJobCode, projection.SubJobTitle, masterJob);
                primeroUnitOfWork.JOBCOST_HDR.Add(subjob);
                primeroUnitOfWork.SaveChanges();
            }

            projection.SubJobId = subjob.JOBNO;
            if (editLineAfterCommit)
            {
                if (projection.LineId != null)
                {
                    JOBCOST_LINES line = primeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                    if (line != null)
                    {
                        line.JOBNO = subjob.JOBNO;
                        primeroUnitOfWork.SaveChanges();
                        return true;
                    }
                    else
                        return false;
                }
            }
            else
                return true;

            return false;
        }

        public static bool CommitLineDiscipline(ExoSubJobProjection projection, bool editLineAfterCommit, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, string projectNumber, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            if (projection.DisciplineCode == null || projection.DisciplineCode == string.Empty)
                return false;

            JOB_COSTGROUPS discipline = ExoQueries.GetCostGroup(primeroUnitOfWork, projection.DisciplineCode);
            if (projection.DisciplineName == null || projection.DisciplineName == string.Empty)
            {
                if (discipline == null)
                {
                    var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(string.Empty, projection.DisciplineCode + " Name:");
                    if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input discipline name", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                    {
                        projection.DisciplineName = bulkEditStringsViewModel.EditValue;
                    }
                    else
                        return false;
                }
                else
                {
                    projection.DisciplineName = discipline.COSTDESC;
                }
            }

            if (discipline == null)
            {
                discipline = CreateNewCostGroup(projection.DisciplineCode, projection.DisciplineName);
                primeroUnitOfWork.JOB_COSTGROUPS.Add(discipline);
                primeroUnitOfWork.SaveChanges();
            }

            projection.DisciplineId = discipline.SEQNO;
            if (editLineAfterCommit)
            {
                if (projection.LineId != null)
                {
                    JOBCOST_LINES line = primeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                    if (line != null)
                    {
                        line.COST_CENTRE2 = discipline.SEQNO;
                        primeroUnitOfWork.SaveChanges();
                        return true;
                    }
                    else
                        return false;
                }
            }
            else
                return true;

            return false;
        }


        public static bool CommitLineBudgetCost(ExoSubJobProjection projection, IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork, IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork)
        {
            if (projection.LineId != null)
            {
                JOBCOST_LINES line = localPrimeroUnitOfWork.JOBCOST_LINES.FirstOrDefault(x => x.SEQNO == projection.LineId);
                if (line != null)
                {
                    UpdateJOBCOST_LINES_AUDIT(bluePrintsUnitOfWork, projection, line);
                    line.QUOTE_QTY = 1;
                    line.ACTUAL_UNITCOST = Convert.ToDouble(projection.ExoBudget);
                    localPrimeroUnitOfWork.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<ExoSubJobProjection> CommitToExo(IEnumerable<ExoSubJobProjection> projections, IMessageBoxService MessageBoxService, JOBCOST_HDR masterJob, JOBCOST_LINES copyLine, PROJECT loadPROJECT, IEnumerable<USER> USERCollection, IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork, IDialogService BulkColumnEditDialogService, out List<ErrorMessage> errorMessages, bool updateBudgetIfExist = false, bool ignoreCostGroupCostType = false)
        {
            errorMessages = new List<ErrorMessage>();
            List<ExoSubJobProjection> addedProjections = new List<ExoSubJobProjection>();
            if (masterJob == null)
            {
                MessageBoxService.ShowMessage("Cannot upload to exo because job " + loadPROJECT.NUMBER + " is not created\nPlease contact " + BluePrintsResources.Default_CFO + " to add project", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return addedProjections;
            }

            if (masterJob.CATEGORY == null || ((int)masterJob.CATEGORY) >= 5)
            {
                string projectManagername = string.Empty;
                USER pmUSER = null;

                if (loadPROJECT.GUID_PROJUSER != null)
                {
                    pmUSER = USERCollection.FirstOrDefault(x => x.GUID == loadPROJECT.GUID_PROJUSER);
                    if (pmUSER != null)
                        projectManagername = pmUSER.Full_Name;
                }

                string defaultTenderPhaseErrorMessage = "Uploading to EXO is disabled because this job is in tender phase";
                if (projectManagername != string.Empty)
                    MessageBoxService.ShowMessage(defaultTenderPhaseErrorMessage + "\nPlease contact " + projectManagername + " to change project category", "Warning", MessageButton.OK, MessageIcon.Exclamation);
                else
                    MessageBoxService.ShowMessage(defaultTenderPhaseErrorMessage, "Warning", MessageButton.OK, MessageIcon.Exclamation);

                return addedProjections;
            }

            if (masterJob == null)
            {
                MessageBoxService.ShowMessage("Project master job doesn't exists in EXO\nPlease request " + BluePrintsResources.Default_CFO + " to add a job with job code " + loadPROJECT.NUMBER, "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return addedProjections;
            }

            if (copyLine == null)
            {
                MessageBoxService.ShowMessage("Project master line is not setup in exo\nPlease request " + BluePrintsResources.Default_CFO + " to add a job line linked to master job with job code " + loadPROJECT.NUMBER, "Warning", MessageButton.OK, MessageIcon.Exclamation);
                return addedProjections;
            }
            
            int updatedLineCount = 0;
            foreach (ExoSubJobProjection projection in projections)
            {
                if (projection.SubJobCode == null || projection.SubJobCode == string.Empty || !Regex.IsMatch(projection.SubJobCode, loadPROJECT.NUMBER + BluePrintsResources.Regex_SUBJOB))
                {
                    errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, "Invalid sub job code"));
                    continue;
                }
                else if (projection.SubJobCode.Length > 15)
                {
                    errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, "Subjob code must be 15 characters"));
                    continue;
                }
                else if (projection.PhaseType != null && projection.PhaseType == PhaseType.Design && projection.CommodityIsIndirectOnly)
                {
                    errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, projection.CommodityCode + " can only be assigned to indirect subjobs, please change the subjob or assign a different commodity in the deliverable's list"));
                    continue;
                }
                else if(projection.VariationCode != null && projection.VariationCode.Length > 50)
                {
                    errorMessages.Add(new ErrorMessage(projection.VariationCode, "Variation code cannot be more than 50 characters"));
                    continue;
                }

                if (!ignoreCostGroupCostType)
                {
                    if(projection.StockCode != BluePrintsResources.VariationStockCode)
                    {
                        if (projection.DisciplineCode == null || projection.DisciplineCode == string.Empty || !Regex.IsMatch(projection.DisciplineCode, BluePrintsResources.Regex_DISCIPLINE))
                        {
                            errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, "Invalid discipline code"));
                            continue;
                        }
                        else if (projection.DisciplineCode.Length > 4)
                        {
                            errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, "discipline code must be 4 characters"));
                            continue;
                        }
                        else if (projection.CommodityCode == null || projection.CommodityCode == string.Empty)
                        {
                            errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, "missing commodity code"));
                            continue;
                        }
                        else if (projection.CommodityCode.Length > 4)
                        {
                            errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, "commodity code must be 4 characters"));
                            continue;
                        }
                    }
                }

                if (!projection.IsLineExistsInExo)
                {
                    if (ExoMethods.CommitLineSubJob(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                    {
                        if (ignoreCostGroupCostType || ExoMethods.CommitLineDiscipline(projection, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                        {
                            //stock item cannot be added, so it must exists before commodity can be added using it
                            string stockCode = projection.GetStockCode();
                            STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(localPrimeroUnitOfWork, stockCode);
                            if (stock_item != null)
                            {
                                projection.StockName = stock_item.DESCRIPTION;
                                if (ignoreCostGroupCostType || ExoMethods.CommitLineCommodity(projection, stock_item, false, BulkColumnEditDialogService, masterJob, loadPROJECT.NUMBER, localPrimeroUnitOfWork))
                                {
                                    JOBCOST_LINES findExistingOrAddLine = ExoMethods.findExistingOrAddLine(localPrimeroUnitOfWork, bluePrintsEntitiesUnitOfWork, projection, copyLine, loadPROJECT.NUMBER, updateBudgetIfExist, ignoreCostGroupCostType);
                                    projection.LineId = findExistingOrAddLine.SEQNO;
                                    if (projection.LineId != null)
                                    {
                                        projection.IsLineExistsInExo = true;
                                        addedProjections.Add(projection);
                                        updatedLineCount += 1;
                                    }

                                    projection.Update();
                                }
                                else
                                {
                                    errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, projection.CommodityCode + " commodity code does not exists in exo, please request it from " + BluePrintsResources.Default_CFO));
                                    continue;
                                }
                            }
                            else
                            {
                                errorMessages.Add(new ErrorMessage(projection.ErrorMessageIdentificationCode, stockCode + " stock code does not exists in exo, please request it from " + BluePrintsResources.Default_CFO));
                                continue;
                            }
                        }
                    }
                }
            }

            return addedProjections;
        }

        public static bool CommitLineVariation(ExoSubJobProjection projection, IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork)
        {
            if (projection.LineId != null)
            {
                JOBCOST_LINES line = localPrimeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                if (line != null)
                {
                    line.X_VARIATION_CODE = projection.VariationCode;
                    localPrimeroUnitOfWork.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        public static void CommitSubJobTitle(ExoSubJobProjection projection, string projectNumber, IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork, IMessageBoxService MessageBoxService)
        {
            JOBCOST_HDR existingSubJob = ExoQueries.GetProjectSubJob(localPrimeroUnitOfWork, projectNumber, projection.SubJobCode);
            if (existingSubJob == null)
            {
                MessageBoxService.ShowMessage(projection.SubJobCode + " doesn't exists in exo yet, please upload to exo before clicking edit title");
                return;
            }

            existingSubJob.TITLE = projection.SubJobTitle;
            localPrimeroUnitOfWork.SaveChanges();
        }

        public static void CommitCostGroupName(ExoSubJobProjection projection, IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork, IMessageBoxService MessageBoxService)
        {
            JOB_COSTGROUPS costGroup = ExoQueries.GetCostGroup(localPrimeroUnitOfWork, projection.DisciplineCode);
            if (costGroup == null)
            {
                MessageBoxService.ShowMessage(projection.DisciplineCode + " doesn't exists in exo yet, please upload to exo before clicking edit discipline code title");
                return;
            }

            costGroup.COSTDESC = projection.DisciplineName;
            localPrimeroUnitOfWork.SaveChanges();
        }

        public static bool CommitLineCommodity(ExoSubJobProjection projection, STOCK_ITEMS stock_item, bool editLineAfterCommit, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, string projectNumber, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            if (projection.CommodityCode == null || projection.CommodityCode == string.Empty || projection.DisciplineId == null)
                return false;

            JOB_COSTTYPES commodity = ExoMethods.findExistingCommodity(primeroUnitOfWork, projection.CommodityCode, string.Empty, (int)projection.DisciplineId);
            if (commodity != null)
            {
                projection.CommodityId = commodity.SEQNO;
                if (editLineAfterCommit)
                {
                    if (projection.LineId != null)
                    {
                        JOBCOST_LINES line = primeroUnitOfWork.JOBCOST_LINES.First(x => x.SEQNO == projection.LineId);
                        if (line != null)
                        {
                            line.COST_CENTRE = commodity.SEQNO;
                            line.STOCKCODE = stock_item.STOCKCODE;
                            line.DESCRIPTION = stock_item.DESCRIPTION;

                            primeroUnitOfWork.SaveChanges();
                            return true;
                        }
                        else
                            return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static void ViewUpdateSubJobTitle(ExoSubJobProjection projection, IEnumerable<ExoSubJobProjection> projections, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, string newSubJobCode, bool updateRelatedSubjobsEntries)
        {
            if (newSubJobCode == null)
                return;

            JOBCOST_HDR existingSubJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, projectNumber, newSubJobCode);
            if (existingSubJob != null)
            {
                projection.SubJobTitle = existingSubJob.TITLE;
                projection.Update();

                if (updateRelatedSubjobsEntries)
                {
                    foreach (ExoSubJobProjection relatedProjection in projections.Where(x => x.SubJobCode == newSubJobCode && x.IsLineExistsInExo))
                    {
                        relatedProjection.SubJobTitle = existingSubJob.TITLE;
                        relatedProjection.Update();
                    }
                }
            }
        }

        public static void ViewUpdateCostGroupTitle(ExoSubJobProjection projection, IEnumerable<ExoSubJobProjection> projections, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string newCostGroupCode, bool updateRelatedDisciplineCodeEntries)
        {
            if (newCostGroupCode == null)
                return;

            JOB_COSTGROUPS existingCostGroup = ExoQueries.GetCostGroup(primeroUnitOfWork, newCostGroupCode);
            if (existingCostGroup != null)
            {
                projection.DisciplineName = existingCostGroup.COSTDESC;
                projection.Update();

                if (updateRelatedDisciplineCodeEntries)
                {
                    foreach (ExoSubJobProjection relatedProjection in projections.Where(x => x.DisciplineCode == newCostGroupCode))
                    {
                        relatedProjection.DisciplineName = projection.DisciplineName;
                        relatedProjection.Update();
                    }
                }
            }
        }

        //public static bool CommitJOB_COSTTYPES(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IDialogService BulkColumnEditDialogService, COMMODITY_CODEProjection projection)
        //{
        //    if (projection.Entity.CODE == string.Empty || projection.EXO_COSTGROUP_SEQNO == null)
        //        return false;

        //    JOB_COSTTYPES costType = ExoQueries.GetJOB_COSTTYPES(primeroUnitOfWork, projection.Entity.CODE);
        //    if (costType == null)
        //    {
        //        var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(string.Empty, projection.Entity.CODE + " Description:");
        //        if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input description", "BulkEditStrings", bulkEditStringsViewModel) != MessageResult.OK)
        //            return false;

        //        costType = CreateNewJOB_COSTTYPE(projection.Entity.CODE, bulkEditStringsViewModel.EditValue, (int)projection.EXO_COSTGROUP_SEQNO);
        //        primeroUnitOfWork.JOB_COSTTYPES.Add(costType);
        //        primeroUnitOfWork.SaveChanges();
        //        return true;
        //    }
        //    else
        //    {
        //        costType.DEF_COSTGROUP = (int)projection.EXO_COSTGROUP_SEQNO;
        //        primeroUnitOfWork.SaveChanges();
        //        return true;
        //    }
        //}

        //public static bool CommitJOB_COSTGROUPS(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IDialogService BulkColumnEditDialogService, COMMODITY_CODEProjection projection)
        //{
        //    if (projection.Entity.CODE == string.Empty || projection.EXO_COSTGROUP_SEQNO == null)
        //        return false;

        //    JOB_COSTTYPES costType = ExoQueries.GetJOB_COSTTYPES(primeroUnitOfWork, projection.Entity.CODE);
        //    if (costType == null)
        //    {
        //        var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(string.Empty, projection.Entity.CODE + " Description:");
        //        if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input description", "BulkEditStrings", bulkEditStringsViewModel) != MessageResult.OK)
        //            return false;

        //        costType = CreateNewJOB_COSTTYPE(projection.Entity.CODE, bulkEditStringsViewModel.EditValue, (int)projection.EXO_COSTGROUP_SEQNO);
        //        primeroUnitOfWork.JOB_COSTTYPES.Add(costType);
        //        primeroUnitOfWork.SaveChanges();
        //        return true;
        //    }
        //    else
        //    {
        //        costType.DEF_COSTGROUP = (int)projection.EXO_COSTGROUP_SEQNO;
        //        primeroUnitOfWork.SaveChanges();
        //        return true;
        //    }
        //}

        public static JOBCOST_LINES findExistingOrAddLine(IPrimeroEntitiesUnitOfWork pUnitOfWork, IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork, ExoSubJobProjection exoLine, JOBCOST_LINES copyLine, string projectNumber, bool updateBudgetIfExists = false, bool ignoreCostGroupCostType = false)
        {
            if (!ignoreCostGroupCostType && (exoLine.SubJobId == null || exoLine.DisciplineId == null || exoLine.CommodityId == null))
                return null;
            else
            {
                JOBCOST_LINES line = ExoQueries.GetProjectLine(pUnitOfWork, projectNumber, exoLine, ignoreCostGroupCostType);

                if (line != null)
                {
                    if(updateBudgetIfExists)
                    {
                        double exoBudget = Convert.ToDouble(exoLine.ExoBudget);
                        if (line.ACTUAL_UNITCOST != exoBudget)
                        {
                            UpdateJOBCOST_LINES_AUDIT(bluePrintsUnitOfWork, exoLine, line);
                            line.ACTUAL_UNITCOST = exoLine.ExoBudget == 0 ? (Double?)null : Convert.ToDouble(exoLine.ExoBudget);
                            pUnitOfWork.SaveChanges();

                        }
                    }

                    return line;
                }

                int? maxJOBCOSTLINEID = ExoQueries.GetJOBCODELINEID(pUnitOfWork);
                if (maxJOBCOSTLINEID != null)
                {
                    JOBCOST_LINES newLine = CreateNewLine(copyLine, exoLine, (int)maxJOBCOSTLINEID, ignoreCostGroupCostType);
                    pUnitOfWork.JOBCOST_LINES.Add(newLine);
                    pUnitOfWork.SaveChanges();
                    UpdateJOBCOST_LINES_AUDIT(bluePrintsUnitOfWork, exoLine, newLine);

                    return newLine;
                }
                else
                {
                    return null;
                }
            }
        }

        public static JOBCOST_LINES_AUDIT UpdateJOBCOST_LINES_AUDIT(IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork, ExoSubJobProjection exoLine, JOBCOST_LINES line, bool isDelete = false)
        {
            JOBCOST_LINES_AUDIT jobCostLinesAudit = FindExistingOrAddJOBCOST_LINES_AUDIT(bluePrintsEntitiesUnitOfWork, line.SEQNO);
            jobCostLinesAudit.JOBCODE = DataUtils.NormalizeString(exoLine.SubJobCode);
            jobCostLinesAudit.DISCIPLINE_CODE = DataUtils.NormalizeString(exoLine.DisciplineCode);
            jobCostLinesAudit.COMMODITY_CODE = DataUtils.NormalizeString(exoLine.CommodityCode);
            jobCostLinesAudit.STOCK_CODE = DataUtils.NormalizeString(exoLine.StockCode);
            jobCostLinesAudit.VARIATION_CODE = DataUtils.NormalizeString(exoLine.VariationCode);
            jobCostLinesAudit.BUDGET_FROM = line.ACTUAL_UNITCOST == null ? 0 : Convert.ToDecimal((double)line.ACTUAL_UNITCOST);
            jobCostLinesAudit.BUDGET_TO = exoLine.ExoBudget;

            if(jobCostLinesAudit.BUDGET_FROM != jobCostLinesAudit.BUDGET_TO)
            {
                jobCostLinesAudit.BUDGET_UPDATED = DateTime.Now;
                jobCostLinesAudit.BUDGET_UPDATEDBY = LoginCredentials.CurrentUserGuid;
            }

            bluePrintsEntitiesUnitOfWork.SaveChanges();
            if(isDelete)
            {
                bluePrintsEntitiesUnitOfWork.JOBCOST_LINES_AUDITS.Remove(jobCostLinesAudit);
                bluePrintsEntitiesUnitOfWork.SaveChanges();
            }

            return jobCostLinesAudit;
        }

        public static JOBCOST_LINES_AUDIT FindExistingOrAddJOBCOST_LINES_AUDIT(IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork, int JOBCOST_LINES_SEQNO)
        {
            JOBCOST_LINES_AUDIT jobCostLinesAudit = bluePrintsEntitiesUnitOfWork.JOBCOST_LINES_AUDITS.FirstOrDefault(x => x.JOBCOST_LINES_SEQNO == JOBCOST_LINES_SEQNO);
            if (jobCostLinesAudit != null)
            {
                jobCostLinesAudit.UPDATED = DateTime.Now;
                jobCostLinesAudit.UPDATEDBY = LoginCredentials.CurrentUserGuid;
                return jobCostLinesAudit;
            }
            else
            {
                jobCostLinesAudit = new JOBCOST_LINES_AUDIT();
                jobCostLinesAudit.JOBCOST_LINES_SEQNO = JOBCOST_LINES_SEQNO;
                jobCostLinesAudit.CREATED = DateTime.Now;
                jobCostLinesAudit.CREATEDBY = LoginCredentials.CurrentUserGuid;
                bluePrintsEntitiesUnitOfWork.JOBCOST_LINES_AUDITS.Add(jobCostLinesAudit);
                return jobCostLinesAudit;
            }
        }

        public static STOCK_ITEMS FindExistingOrAddStockItem(IPrimeroEntitiesUnitOfWork pUnitOfWork, string shortCode, string description, double? sellPrice, int? salesGLCode, int? purchGLCode, int? cosGLCode, double? stdCost, int costGroup, int costType, string department)
        {
            STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(pUnitOfWork, shortCode);
            if (stock_item != null)
            {
                stock_item.ISACTIVE = "Y";
                stock_item.SELLPRICE1 = sellPrice;
                stock_item.SALES_GL_CODE = salesGLCode;
                stock_item.PURCH_GL_CODE = purchGLCode;
                stock_item.COS_GL_CODE = cosGLCode;
                stock_item.STDCOST = stdCost;
                stock_item.COSTGROUP = costGroup;
                stock_item.COSTTYPE = costType;
                stock_item.X_DEPARTMENT = department;
                return stock_item;
            }
            else
            {
                STOCK_ITEMS newSTOCK_ITEM = createNewStockItem(shortCode, description, sellPrice, salesGLCode, purchGLCode, cosGLCode, stdCost, costGroup, costType, department);
                pUnitOfWork.STOCK_ITEMS.Add(newSTOCK_ITEM);
                pUnitOfWork.SaveChanges();
                return newSTOCK_ITEM;
            }
        }

        public static JOBCOST_RESOURCE FindExistingOrAddResource(IPrimeroEntitiesUnitOfWork pUnitOfWork, int? staffId, int? seqNo, string name, string title, string defaultStockCode, string shortCode, string forceSearchName)
        {
            string uppercaseName = name.ToUpper();
            string uppercaseTitle = title == null ? string.Empty : title.ToUpper();
            string uppercaseDefaultStockCode = defaultStockCode == null ? string.Empty : defaultStockCode.ToUpper();
            string uppercaseShortCode = shortCode == null ? string.Empty : shortCode.ToUpper();

            JOBCOST_RESOURCE resource = ExoQueries.FindJOBCOST_RESOURCE(pUnitOfWork, seqNo, forceSearchName);
            if(resource != null)
            {
                resource.ISACTIVE = "Y";
                resource.RESOURCENAME = uppercaseName;
                resource.TITLE = uppercaseTitle;
                resource.DEFAULT_STOCKCODE = uppercaseDefaultStockCode == string.Empty ? resource.DEFAULT_STOCKCODE : uppercaseDefaultStockCode;
                resource.SHORTCODE = uppercaseShortCode == string.Empty ? resource.SHORTCODE : uppercaseShortCode;
                return resource;
            }
            else
            {
                if (staffId != null)
                {
                    JOBCOST_RESOURCE newJOBCOST_RESOURCE = createNewResource(pUnitOfWork, (int)staffId, uppercaseName, uppercaseTitle, uppercaseDefaultStockCode, uppercaseShortCode);
                    pUnitOfWork.JOBCOST_RESOURCE.Add(newJOBCOST_RESOURCE);
                    pUnitOfWork.SaveChanges();
                    return newJOBCOST_RESOURCE;
                }
                else
                {
                    return null;
                }
            }
        }


        public static STAFF FindExistingOrAddStaff(IPrimeroEntitiesUnitOfWork pUnitOfWork, int? staffNo, string name, string title, int securityProfileId, int userProfileId, int? reportToStaffId, string payrollId, string forceSearchName, out string primaryDbName, out bool isNew)
        {
            string uppercaseName = name.ToUpper();
            string uppercaseTitle = title == null ? string.Empty : title.ToUpper();
            STAFF staff = ExoQueries.FindSTAFF(pUnitOfWork, staffNo, forceSearchName, out primaryDbName);
            if (staff != null)
            {
                if (staff.ISACTIVE != "N")
                    isNew = false;
                else
                    isNew = true;

                staff.ISACTIVE = "Y";
                staff.NAME = uppercaseName;
                staff.JOBTITLE = uppercaseTitle;
                staff.SECURITYPROFILEID = securityProfileId;
                staff.USERPROFILEID = userProfileId;
                staff.REPORTS_TO_STAFFNO = reportToStaffId == null ? staff.STAFFNO : reportToStaffId;
                staff.PAYROLL_ID = payrollId;

                return staff;
            }
            else
            {
                STAFF newSTAFF = createNewStaff(pUnitOfWork, uppercaseName, uppercaseTitle, securityProfileId, userProfileId, reportToStaffId, payrollId);
                pUnitOfWork.STAFF.Add(newSTAFF);
                primaryDbName = newSTAFF.NAME;
                //need to save changes here to get new staff id;
                pUnitOfWork.SaveChanges();
                if (newSTAFF.REPORTS_TO_STAFFNO == null)
                {
                    newSTAFF.REPORTS_TO_STAFFNO = newSTAFF.STAFFNO;
                    pUnitOfWork.SaveChanges();
                }
                isNew = true;

                return newSTAFF;
            }
        }

        public static void RemoveStockItem(IPrimeroEntitiesUnitOfWork pUnitOfWork, ExoResourceProjection projection)
    {
            STOCK_ITEMS stockItem = ExoQueries.FindSTOCK_ITEM(pUnitOfWork, projection.SHORTCODE);
            if (stockItem != null)
                stockItem.ISACTIVE = "N";
        }

        public static void RemoveResources(IPrimeroEntitiesUnitOfWork pUnitOfWork, ExoResourceProjection projection, string forceSearchName)
        {
            JOBCOST_RESOURCE resource = ExoQueries.FindJOBCOST_RESOURCE(pUnitOfWork, projection.RESOURCE_SEQNO, forceSearchName);
            if (resource != null)
                resource.ISACTIVE = "N";
        }

        public static void RemoveStaff(IPrimeroEntitiesUnitOfWork pUnitOfWork, ExoResourceProjection projection, string forceSearchName, out string primaryDbName)
        {
            STAFF staff = ExoQueries.FindSTAFF(pUnitOfWork, projection.STAFFNO, forceSearchName, out primaryDbName);
            if (staff != null)
                staff.ISACTIVE = "N";
        }

        private static JOBCOST_RESOURCE createNewResource(IPrimeroEntitiesUnitOfWork pUnitOfWork, int staffId, string name, string title, string defaultStockCode, string shortCode)
        {
            JOBCOST_RESOURCE newJOBCOST_RESOURCE = new JOBCOST_RESOURCE();
            newJOBCOST_RESOURCE.RESOURCENAME = name;
            newJOBCOST_RESOURCE.COSTRATE0 = 0;
            newJOBCOST_RESOURCE.COSTRATE1 = 0;
            newJOBCOST_RESOURCE.COSTRATE2 = 0;
            newJOBCOST_RESOURCE.COSTRATE3 = 0;
            newJOBCOST_RESOURCE.SELLRATE0 = 0;
            newJOBCOST_RESOURCE.SELLRATE1 = 0;
            newJOBCOST_RESOURCE.SELLRATE2 = 0;
            newJOBCOST_RESOURCE.SELLRATE3 = 0;
            newJOBCOST_RESOURCE.NORMALHOURS = 0;
            newJOBCOST_RESOURCE.TITLE = title;
            newJOBCOST_RESOURCE.ISACTIVE = "Y";

            string generatedShortCode = string.Empty;
            if (shortCode == string.Empty)
            {
                string partialShortCode;
                //use new unit of work to prevent concurrency issues
                List<int> availableEnumerations = ExoQueries.GetAvailableStaffEnumerations(pUnitOfWork, name, out partialShortCode);
                if (availableEnumerations.Count == 0)
                    generatedShortCode = "N/A";
                else
                    generatedShortCode = string.Concat(partialShortCode, availableEnumerations.First().ToString());
            }

            newJOBCOST_RESOURCE.STAFFNO = staffId;
            newJOBCOST_RESOURCE.SHORTCODE = shortCode == string.Empty ? generatedShortCode : shortCode;
            newJOBCOST_RESOURCE.DEFAULT_STOCKCODE = defaultStockCode == string.Empty ? shortCode != string.Empty ? shortCode : generatedShortCode : defaultStockCode;

            return newJOBCOST_RESOURCE;
        }

        private static STOCK_ITEMS createNewStockItem(string shortCode, string description, double? sellPrice, int? salesGLCode, int? purchGLCode, int? cosGLCode, double? stdCost, int costGroup, int costType, string department)
        {
            STOCK_ITEMS newSTOCK_ITEM = new STOCK_ITEMS();
            newSTOCK_ITEM.STOCKCODE = shortCode;
            newSTOCK_ITEM.DESCRIPTION = description;
            newSTOCK_ITEM.STOCKGROUP = 2;
            newSTOCK_ITEM.STATUS = "L";
            newSTOCK_ITEM.SELLPRICE1 = sellPrice;
            newSTOCK_ITEM.SELLPRICE2 = 0;
            newSTOCK_ITEM.SELLPRICE3 = 0;
            newSTOCK_ITEM.SELLPRICE4 = 0;
            newSTOCK_ITEM.SELLPRICE5 = 0;
            newSTOCK_ITEM.SELLPRICE6 = 0;
            newSTOCK_ITEM.SELLPRICE7 = 0;
            newSTOCK_ITEM.SELLPRICE8 = 0;
            newSTOCK_ITEM.SELLPRICE9 = 0;
            newSTOCK_ITEM.SELLPRICE10 = 0;
            newSTOCK_ITEM.LATESTCOST = 0;
            newSTOCK_ITEM.AVECOST = 0;
            newSTOCK_ITEM.MINSTOCK = 0;
            newSTOCK_ITEM.MAXSTOCK = 0;
            newSTOCK_ITEM.SUPPLIERNO = 0;
            newSTOCK_ITEM.MONTHUNITS = 0;
            newSTOCK_ITEM.YEARUNITS = 0;
            newSTOCK_ITEM.LASTYEARUNITS = 0;
            newSTOCK_ITEM.MONTHVALUE = 0;
            newSTOCK_ITEM.YEARVALUE = 0;
            newSTOCK_ITEM.LASTYEARVALUE = 0;
            newSTOCK_ITEM.DISCOUNTLEVEL = 0;
            newSTOCK_ITEM.DEFDAYS = 0;
            newSTOCK_ITEM.LASTMONTHVALUE = 0;
            newSTOCK_ITEM.LASTMONTHUNITS = 0;
            newSTOCK_ITEM.SALES_GL_CODE = salesGLCode;
            newSTOCK_ITEM.PURCH_GL_CODE = purchGLCode;
            newSTOCK_ITEM.WEB_SHOW = "N";
            newSTOCK_ITEM.ISACTIVE = "Y";
            newSTOCK_ITEM.WEIGHT = 0;
            newSTOCK_ITEM.CUBIC = 0;
            newSTOCK_ITEM.PQTY = 1;
            newSTOCK_ITEM.HAS_SN = "N";
            newSTOCK_ITEM.STDCOST = stdCost;
            newSTOCK_ITEM.SALES_GLSUBCODE = 0;
            newSTOCK_ITEM.PURCH_GLSUBCODE = 0;
            newSTOCK_ITEM.BRANCHNO = 0;
            newSTOCK_ITEM.SALESTAXRATE = -1;
            newSTOCK_ITEM.PURCHTAXRATE = -1;
            newSTOCK_ITEM.LAST_UPDATED = DateTime.Now;
            newSTOCK_ITEM.UPDATEITEM_QTY = 0;
            newSTOCK_ITEM.COS_GL_CODE = cosGLCode;
            newSTOCK_ITEM.COS_GLSUBCODE = 0;
            newSTOCK_ITEM.STOCKPRICEGROUP = 0;
            newSTOCK_ITEM.SUPPLIERCOST = 0;
            newSTOCK_ITEM.ECONORDERQTY = 1;
            newSTOCK_ITEM.STOCK_CLASSIFICATION = 0;
            newSTOCK_ITEM.STOCKGROUP2 = 0;
            newSTOCK_ITEM.TOTALSTOCK = 0;
            newSTOCK_ITEM.HAS_BN = "N";
            newSTOCK_ITEM.HAS_EXPIRY = "N";
            newSTOCK_ITEM.EXPIRY_DAYS = 1;
            newSTOCK_ITEM.DUTY = 0;
            newSTOCK_ITEM.SERIALNO_TYPE = 0;
            newSTOCK_ITEM.LABEL_QTY = 1;
            newSTOCK_ITEM.IS_DISCOUNTABLE = "Y";
            newSTOCK_ITEM.RESTRICTED_ITEM = "N";
            newSTOCK_ITEM.NUMDECIMALS = -1;
            newSTOCK_ITEM.COGSMETHOD = 0;
            newSTOCK_ITEM.DEFAULTWARRANTYNO = -2;
            newSTOCK_ITEM.DIMENSIONS = 0;
            newSTOCK_ITEM.AUTO_NARRATIVE = 0;
            newSTOCK_ITEM.X_SIZEID = 0;
            newSTOCK_ITEM.X_COLOURID = 0;
            newSTOCK_ITEM.VARIABLECOST = "N";
            newSTOCK_ITEM.COSTTYPE = costType;
            newSTOCK_ITEM.COSTGROUP = costGroup;
            newSTOCK_ITEM.LOOKUP_RECOVERABLE = 'Y';
            newSTOCK_ITEM.X_PAYTYPE = 'H';
            newSTOCK_ITEM.X_ALLOWNO = 0;
            newSTOCK_ITEM.X_DEPARTMENT = department;

            return newSTOCK_ITEM;
        }

        private static STAFF createNewStaff(IPrimeroEntitiesUnitOfWork pUnitOfWork, string name, string title, int securityProfileId, int userProfileId, int? reportToStaffId, string payrollId)
        {
            STAFF newSTAFF = new STAFF();
            newSTAFF.NAME = name;
            newSTAFF.JOBTITLE = title;
            newSTAFF.MENU_NO = 2;
            newSTAFF.SECURITYPROFILEID = securityProfileId;
            newSTAFF.USERPROFILEID = userProfileId;
            newSTAFF.LOGINID = ExoQueries.GetLoginId(pUnitOfWork, name);
            newSTAFF.PASSWORD_CHANGED = DateTime.Now;
            newSTAFF.BAD_LOGIN_COUNT = 0;
            newSTAFF.ACCOUNT_STATUS = 0;
            newSTAFF.DISCOUNTRATE = 0;
            newSTAFF.IS_SUPERVISOR = "N";
            newSTAFF.ABSENT = "N";
            newSTAFF.EMPLOYEE_CODE = -1;
            newSTAFF.SMTP_SEQNO = -1;
            newSTAFF.HAS_BUDGETS = "N";
            newSTAFF.REPORTS_TO_STAFFNO = reportToStaffId;
            newSTAFF.API_ACCESS = "N";
            newSTAFF.MOBILE_ACCESS = "N";
            newSTAFF.LAST_ACKNOWLEDGED_VERSION = 0;
            newSTAFF.ISACTIVE = "Y";
            newSTAFF.PAYROLL_ID = payrollId;

            return newSTAFF;
        }

        public static JOBCOST_LINES CreateNewLine(JOBCOST_LINES copyLine, ExoSubJobProjection projection, int maxJobLineId, bool ignoreCostGroupCostType = false)
        {
            JOBCOST_LINES newLINE = new JOBCOST_LINES();
            newLINE.QUOTE_QTY = 1;
            newLINE.QUOTE_UNITPR = projection.Rate == 0 ? (Double?)null : Convert.ToDouble(projection.Rate);
            newLINE.ACTUAL_UNITCOST = projection.ExoBudget == 0 ? (Double?)null : Convert.ToDouble(projection.ExoBudget);
            newLINE.TRANSDATE = DateTime.Now.Date;
            newLINE.EXCHRATE = copyLine.EXCHRATE;
            newLINE.DISCOUNT = 0;
            newLINE.UNITPRICE_INCTAX = 0;
            newLINE.JOBNO = (int)projection.SubJobId;
            newLINE.STOCKCODE = projection.StockCode.ToUpper();
            newLINE.DESCRIPTION = projection.StockName;
            newLINE.SHOW_ON_INVOICE = copyLine.SHOW_ON_INVOICE;
            newLINE.COST_CENTRE = ignoreCostGroupCostType ? -1 : projection.CommodityId;
            newLINE.COST_CENTRE2 = ignoreCostGroupCostType ? -1 : projection.DisciplineId;
            newLINE.NARRATIVE = "N";
            newLINE.LINE_STATUS = "Q";
            newLINE.TAXNO = copyLine.TAXNO;
            newLINE.BRANCHNO = 0;
            newLINE.SUBCODE = 0;
            newLINE.ANALYSIS = 0;
            newLINE.CURRENCYNO = 0;
            newLINE.ALINENO = 100;
            newLINE.GLCODE = 0;
            newLINE.MASTER_JOBNO = copyLine.MASTER_JOBNO;
            newLINE.COPY_FROM_QUOTE = "N";
            newLINE.DIM_LENGTH = 1;
            newLINE.DIM_WIDTH = 1;
            newLINE.DIM_DEPTH = 1;
            newLINE.TOTAL_QUANTITY = 1;
            newLINE.LINETYPE = 0;
            newLINE.KITSEQNO = -1;
            newLINE.KITCODE = string.Empty;
            newLINE.PRICE_OVERRIDDEN = "N";
            newLINE.LINKED_STOCKCODE = projection.StockCode.ToUpper();
            newLINE.LINKED_QTY = 1;
            newLINE.HIDDEN_COST = 0;
            newLINE.HIDDEN_SELL = 0;
            newLINE.SUPPLIERNO = 0;
            newLINE.FROMLOC = 1;
            newLINE.LINETOTAL = 0;
            newLINE.BOMTYPE = "N";
            newLINE.SHOWLINE = "Y";
            newLINE.BOMPRICING = "N";
            newLINE.LINKEDSTATUS = "L";
            newLINE.LISTPRICE = 0;
            newLINE.NUNITPR = 0;
            newLINE.OPTION_NO = 0;
            newLINE.X_LABOUR_ALLOWANCE = 0;
            newLINE.SPREADVALUE = "Y";
            newLINE.TAXRATE = copyLine.TAXRATE;
            newLINE.LINETOTAL_TAX = 0;
            newLINE.LINETOTAL_INCTAX = 0;
            newLINE.LINE_TAX = 0;
            newLINE.HIDDEN_LINETOTAL = 0;
            newLINE.SCHEDULE_SEQNO = 0;
            newLINE.JOBCOSTLINEID = maxJobLineId + 1;
            newLINE.SNTYPE = 0;
            newLINE.SNEXPDAYS = -2;
            newLINE.OPPLINEID = -1;
            newLINE.COST_LINENO = -1;
            newLINE.X_VARIATION_CODE = projection.VariationCode;

            return newLINE;
        }

        public static JOB_COSTTYPES findExistingCommodity(IPrimeroEntitiesUnitOfWork pUnitOfWork, string commodityCode, string commodityName, int defaultDisciplineId)
        {
            JOB_COSTTYPES costType = ExoQueries.GetCommodity(pUnitOfWork, commodityCode);

            if (costType != null)
                return costType;
            //else
            //{
            //    JOB_COSTTYPES newCOSTTYPE = new JOB_COSTTYPES();
            //    newCOSTTYPE.DEF_MARKUP = 0;
            //    newCOSTTYPE.DEF_OVERHEAD = 0;
            //    newCOSTTYPE.COSTDESC = commodityCode.ToUpper() + " - " + commodityName.ToUpper();
            //    newCOSTTYPE.GLCODE = -1;
            //    newCOSTTYPE.GLSUBCODE = 0;
            //    newCOSTTYPE.SHOWONQUOTE = "F";
            //    newCOSTTYPE.SHORTCODE = commodityCode.ToUpper();
            //    newCOSTTYPE.DEF_COSTGROUP = defaultDisciplineId;
            //    newCOSTTYPE.DEF_PURCH_GLCODE = -1;
            //    newCOSTTYPE.DEF_PURCH_GLSUBCODE = 0;
            //    newCOSTTYPE.CONSOLIDATE = "F";
            //    newCOSTTYPE.COPY_FROM_QUOTE = "N";
            //    pUnitOfWork.JOB_COSTTYPES.Add(newCOSTTYPE);
            //    pUnitOfWork.SaveChanges();
            //    return newCOSTTYPE.SEQNO;
            //}
            else
                return null;
        }

        public static JOB_COSTGROUPS CreateNewCostGroup(string disciplineCode, string title)
        {
            JOB_COSTGROUPS newCOSTGROUP = new JOB_COSTGROUPS();
            newCOSTGROUP.DEF_MARKUP = 0;
            newCOSTGROUP.DEF_OVERHEAD = 0;
            newCOSTGROUP.COSTDESC = title;
            newCOSTGROUP.SHORTCODE = disciplineCode.ToUpper();
            newCOSTGROUP.SHOWONQUOTE = "F";
            newCOSTGROUP.CONSOLIDATE = "F";
            newCOSTGROUP.COPY_FROM_QUOTE = "N";
            return newCOSTGROUP;
        }

        public static void updateSubJobTitle(IPrimeroEntitiesUnitOfWork pUnitOfWork, string projectNumber, string jobCode)
        {
            JOBCOST_HDR existingSubJobs = ExoQueries.GetProjectSubJob(pUnitOfWork, projectNumber, jobCode);
        }

        public static JOBCOST_HDR CreateNewSubJob(string jobCode, string title, JOBCOST_HDR masterJob)
        {
            JOBCOST_HDR newExoSubJob = new JOBCOST_HDR();
            newExoSubJob.ESTIMATE = 0;
            newExoSubJob.INVOICED = 0;
            newExoSubJob.THETIME = 0;
            newExoSubJob.MATERIALS = 0;
            newExoSubJob.DEF_OVERHEAD = 0;
            newExoSubJob.MATERIALSCOST = 0;
            newExoSubJob.ESTIMATECOST = 0;
            newExoSubJob.THETIMECOST = 0;
            newExoSubJob.INVOICEDCOST = 0;
            newExoSubJob.JOBCODE = jobCode;
            newExoSubJob.ACCNO = masterJob.ACCNO;
            newExoSubJob.CUSTORDNO = string.Empty;
            newExoSubJob.STATUS = masterJob.STATUS;
            newExoSubJob.TITLE = title;
            newExoSubJob.CATEGORY = masterJob.CATEGORY;
            newExoSubJob.JOBTYPE = masterJob.JOBTYPE;
            newExoSubJob.STAFFNO = masterJob.STAFFNO;
            newExoSubJob.ACTIONBY = masterJob.ACTIONBY;
            newExoSubJob.MASTER_JOBNO = masterJob.JOBNO;
            newExoSubJob.COSTGL = 0;
            newExoSubJob.SALESGL = 0;
            newExoSubJob.SERIALNO = string.Empty;
            newExoSubJob.CONTACT = string.Empty;
            newExoSubJob.PRIVATE_NOTE = string.Empty;
            newExoSubJob.COSTSUBGL = 0;
            newExoSubJob.SALESSUBGL = 0;
            newExoSubJob.CONTACTNO = masterJob.CONTACTNO;
            newExoSubJob.DELADDR1 = masterJob.DELADDR1;
            newExoSubJob.DELADDR2 = masterJob.DELADDR2;
            newExoSubJob.DELADDR3 = masterJob.DELADDR3;
            newExoSubJob.DELADDR4 = masterJob.DELADDR4;
            newExoSubJob.DELADDR5 = masterJob.DELADDR5;
            newExoSubJob.DELADDR6 = masterJob.DELADDR6;
            newExoSubJob.WRITE_OFF_COST = masterJob.WRITE_OFF_COST;
            newExoSubJob.TOTAL_HOURS = 0;
            newExoSubJob.EST_HOURS = 0;
            newExoSubJob.ASSET_COST = 0;
            newExoSubJob.ASSET_VALUE = 0;
            newExoSubJob.BRANCHNO = masterJob.BRANCHNO;
            newExoSubJob.ISACTIVE = "Y";
            newExoSubJob.HASUNBILLED = "N";
            newExoSubJob.INVOICEREADY = "N";
            newExoSubJob.CALLBACKDATE = DateTime.Now;
            newExoSubJob.ENTRYDATE = DateTime.Now;
            newExoSubJob.TOTALVALUE = 0;
            newExoSubJob.TOTALCOST = 0;
            newExoSubJob.WIPLOC = masterJob.WIPLOC;
            newExoSubJob.EXCHRATE = masterJob.EXCHRATE;
            newExoSubJob.RETENTION_RATE = 0;
            newExoSubJob.RETENTION2_MIN = 0;
            newExoSubJob.RETENTION2_RATE = 0;
            newExoSubJob.RETENTION3_MIN = 0;
            newExoSubJob.RETENTION3_RATE = 0;
            newExoSubJob.ALLOWANCE = 0;
            newExoSubJob.BILLINGMODE = 0;
            newExoSubJob.DESCRIPTION = string.Empty;
            newExoSubJob.CAMPAIGN_WAVE_SEQNO = -1;
            newExoSubJob.OPPORTUNITY_SEQNO = -1;
            newExoSubJob.LINECHARGE_WRITEOFF = 0;
            newExoSubJob.INVOICE_VIA_MASTER = "Y";

            return newExoSubJob;
        }

        public static JOB_COSTTYPES CreateNewJOB_COSTTYPE(string shortCode, string costDesc, int costGroup)
        {
            JOB_COSTTYPES newJOB_COSTTYPE = new JOB_COSTTYPES();
            newJOB_COSTTYPE.DEF_MARKUP = 0;
            newJOB_COSTTYPE.DEF_OVERHEAD = 0;
            newJOB_COSTTYPE.COSTDESC = costDesc;
            newJOB_COSTTYPE.GLCODE = -1;
            newJOB_COSTTYPE.GLSUBCODE = 0;
            newJOB_COSTTYPE.SHOWONQUOTE = "F";
            newJOB_COSTTYPE.SHORTCODE = shortCode;
            newJOB_COSTTYPE.DEF_COSTGROUP = costGroup;
            newJOB_COSTTYPE.DEF_PURCH_GLCODE = -1;
            newJOB_COSTTYPE.DEF_PURCH_GLSUBCODE = 0;
            newJOB_COSTTYPE.CONSOLIDATE = "F";
            newJOB_COSTTYPE.COPY_FROM_QUOTE = "N";

            return newJOB_COSTTYPE;
        }

        public static JOB_COSTGROUPS CreateNewJOB_COSTGROUPS(string shortCode, string costDesc)
        {
            JOB_COSTGROUPS newJOB_COSTGROUP = new JOB_COSTGROUPS();
            newJOB_COSTGROUP.DEF_MARKUP = 0;
            newJOB_COSTGROUP.DEF_OVERHEAD = 0;
            newJOB_COSTGROUP.COSTDESC = costDesc;
            newJOB_COSTGROUP.SHOWONQUOTE = "F";
            newJOB_COSTGROUP.SHORTCODE = shortCode;
            newJOB_COSTGROUP.CONSOLIDATE = "F";
            newJOB_COSTGROUP.COPY_FROM_QUOTE = "N";

            return newJOB_COSTGROUP;
        }

        /// <returns>Whether new record is added</returns>
        public static bool findExistingOrAddResourceAllocation(IPrimeroEntitiesUnitOfWork pUnitOfWork, int jobNo, int staffId)
        {
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, jobNo, true, staffId);

            if (resourceAllocation != null)
            {
                if (resourceAllocation.END_DATE < DateTime.Now)
                {
                    resourceAllocation.START_DATE = BluePrintsConstants.DefaultFirstDay;
                    resourceAllocation.END_DATE = BluePrintsConstants.DefaultLastDay;
                    resourceAllocation.START_TIME = BluePrintsConstants.DefaultStartTime;
                    resourceAllocation.END_TIME = BluePrintsConstants.DefaultStartTime;
                    pUnitOfWork.SaveChanges();
                }

                return false;
            }
            else
            {
                int? resourceNo = ExoQueries.GetStaffResourceNo(pUnitOfWork, staffId);
                if (resourceNo == null)
                    return false;

                return addResourceAllocation(pUnitOfWork, resourceNo, jobNo);
            }
        }

        public static bool addResourceAllocation(IPrimeroEntitiesUnitOfWork pUnitOfWork, int? resourceNo, int jobNo)
        {
            if (resourceNo == null)
                return false;

            if (resourceNo != null)
            {
                JOB_RESOURCE_ALLOCATION newAllocation = new JOB_RESOURCE_ALLOCATION();
                newAllocation.RESOURCE_SEQNO = (int)resourceNo;
                newAllocation.JOBNO = jobNo;

                newAllocation.START_DATE = BluePrintsConstants.DefaultFirstDay;
                newAllocation.END_DATE = BluePrintsConstants.DefaultLastDay;
                newAllocation.START_TIME = BluePrintsConstants.DefaultStartTime;
                newAllocation.END_TIME = BluePrintsConstants.DefaultStartTime;
                newAllocation.TOTAL_HOURS = 999999;
                newAllocation.APPOINTMENT_SCHEDULED = "N";
                pUnitOfWork.JOB_RESOURCE_ALLOCATION.Add(newAllocation);
                pUnitOfWork.SaveChanges();
                
                return true;
            }
            else
                return false;
        }

        public static void deleteResourceAllocation(IPrimeroEntitiesUnitOfWork pUnitOfWork, int jobNo, int staffId)
        {
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, jobNo, false, staffId);

            if (resourceAllocation != null)
            {
                resourceAllocation.END_DATE = DateTime.Now.AddDays(-7);
                //Cannot delete resources anymore from horizon changes
                //pUnitOfWork.JOB_RESOURCE_ALLOCATION.Remove(resourceAllocation);
                pUnitOfWork.SaveChanges();
            }
        }
    }

    public static class ExoQueries
    {
        public static IQueryable<ExoSubJobProjection> GetNativeExoSubJobProjection(
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork, Data.PROJECT PROJECT, ref List<ExoTimeAuthorisation> exoLines)
        {
            exoLines = GetProjectLinesIgnoreCostCentres(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();
            foreach (ExoTimeAuthorisation exoLine in exoLines)
            {
                ExoSubJobProjection newSubJobProjection = ViewModelSource.Create(() => new ExoSubJobProjection());
                newSubJobProjection.LineId = exoLine.LineSeqNo;

                newSubJobProjection.SubJobId = exoLine.SubJobNo;
                newSubJobProjection.SubJobCode = exoLine.SubJobCode;
                newSubJobProjection.SubJobTitle = exoLine.SubJobTitle;
                newSubJobProjection.DisciplineId = exoLine.DisciplineId;
                newSubJobProjection.DisciplineCode = exoLine.DisciplineCode;
                newSubJobProjection.DisciplineName = exoLine.DisciplineName;
                newSubJobProjection.CommodityId = exoLine.CommodityId;
                newSubJobProjection.CommodityCode = exoLine.CommodityCode;
                newSubJobProjection.CommodityName = exoLine.CommodityName;
                newSubJobProjection.StockCode = exoLine.StockCode;
                newSubJobProjection.VariationCode = exoLine.VariationCode;
                newSubJobProjection.AuthUserIds = new List<int>();

                exoSubJobs.Add(newSubJobProjection);
            }

            return exoSubJobs.OrderBy(x => x.SubJobCode).AsQueryable();
        }

        public static IQueryable<ExoSubJobProjection> GetExoSubJob(
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork, Data.PROJECT PROJECT, IEnumerable<STAFF> ExoSTAFFS, string officeName, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, IEnumerable<STOCK_ITEMS> STOCK_ITEMSCollection)
        {
            List<ExoTimeAuthorisation> exoLines = GetSubJob(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();
            foreach (ExoTimeAuthorisation exoLine in exoLines)
            {
                ExoSubJobProjection projection = ViewModelSource.Create(() => new ExoSubJobProjection());
                projection.IgnoreExoBudgetError = true;
                projection.LineId = exoLine.LineSeqNo;
                projection.SubJobId = exoLine.SubJobNo;
                projection.SubJobCode = exoLine.SubJobCode;
                projection.SubJobTitle = exoLine.SubJobTitle;
                projection.PopulateCommodityCodes(COMMODITY_CODECollection);
                projection.PopulateStockItems(STOCK_ITEMSCollection);

                List<ExoTimeAuthorisation> authorisations = exoAuthorisations.Where(x => x.SubJobNo == exoLine.SubJobNo).ToList();
                projection.AuthUserIds = new List<int>();
                if (authorisations.Count > 0)
                    projection.AuthUserIds = authorisations.Where(x => x.ResourceStaffId != null).Select(x => (int)x.ResourceStaffId).Distinct().ToList();

                exoSubJobs.Add(projection);
            }

            return exoSubJobs.OrderBy(x => x.SubJobCode).AsQueryable();
        }

        public static IQueryable<ExoSubJobProjection> GetNativeExoSubJobEditableProjection(
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork, Data.PROJECT PROJECT, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, IEnumerable<STOCK_ITEMS> STOCK_ITEMSCollection, IEnumerable<STAFF> ExoSTAFFS, string officeName, bool ignoreValidationError = false)
        {
            List<ExoTimeAuthorisation> exoLines = GetProjectLinesIgnoreCostCentres(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();
            foreach (ExoTimeAuthorisation exoLine in exoLines)
            {
                ExoSubJobProjection projection = ViewModelSource.Create(() => new ExoSubJobProjection());
                projection.IgnoreExoBudgetError = true;
                projection.LineId = exoLine.LineSeqNo;
                projection.SubJobId = exoLine.SubJobNo;
                projection.SubJobCode = exoLine.SubJobCode;
                projection.SubJobTitle = exoLine.SubJobTitle;
                projection.DisciplineId = exoLine.DisciplineId;
                projection.DisciplineCode = exoLine.DisciplineCode;
                projection.DisciplineName = exoLine.DisciplineName;
                projection.CommodityId = exoLine.CommodityId;
                projection.CommodityCode = exoLine.CommodityCode;
                projection.CommodityName = exoLine.CommodityName;
                projection.StockCode = exoLine.StockCode;
                projection.StockName = exoLine.StockName;
                projection.VariationCode = exoLine.VariationCode;
                projection.ExoBudget = exoLine.BudgetCosts;
                projection.IgnoreValidationError = ignoreValidationError;
                projection.PopulateCommodityCodes(COMMODITY_CODECollection);
                projection.PopulateStockItems(STOCK_ITEMSCollection);

                exoSubJobs.Add(projection);
            }

            return exoSubJobs.OrderBy(x => x.SubJobCode).AsQueryable();
        }

        public static IQueryable<ExoSubJobProjection> GetExoConstructionSubJobProjection(
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS, PROJECT PROJECT,
            IEnumerable<RATE> RATES, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, bool useReportDate, IEnumerable<STOCK_CODE> STOCK_CODES,
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, IEnumerable<STAFF> ExoSTAFFS, string officeName)
        {
            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER);
            List<ESTIMATE_ITEMProgress> estimateItems = ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(ESTIMATE_ITEMS, PROJECT, RATES, PROGRESS, PROGRESS_ITEMS, false, STOCK_CODES).ToList();
            List<ExoSubJobProjection> exoSubJobs = GetProactiveExoSubJobs(estimateItems, primeroUnitOfWork, PROJECT, COMMODITY_CODECollection, exoAuthorisations, true);

            return exoSubJobs.OrderBy(x => x.SubJobCode).AsQueryable();
        }

        public static IQueryable<ExoSubJobProjection> GetExoDesignSubJobProjection(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<WORKPACK> WORKPACKS,
            Data.PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES, IEnumerable<VARIATION> VARIATIONS, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IEnumerable<USER> userCollection, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, IEnumerable<DOCTYPE> DOCTYPECollection, string officeName)
        {
            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER);
            List<BASELINE_ITEMProgress> baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, PROJECT, PROGRESS, RATES, null, VARIATIONS, false, null, DeliverableInternalNumberMode.Default, false, null, null, null, null, null, null, null, null, DOCTYPECollection).ToList();
            List<ExoSubJobProjection> exoSubJobs = GetProactiveExoSubJobs(baseline_item_progresses, primeroUnitOfWork, PROJECT, COMMODITY_CODECollection, exoAuthorisations, false);

            foreach(ExoSubJobProjection exoSubJob in exoSubJobs)
            {
                DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.CODE == exoSubJob.CommodityCode);
                if (findDOCTYPE != null)
                    exoSubJob.CommodityIsIndirectOnly = findDOCTYPE.IS_INDIRECT_ONLY;
            }

            return exoSubJobs.OrderBy(x => x.SubJobCode).AsQueryable();
        }

        public static List<ExoSubJobProjection> GetProactiveExoSubJobs(IEnumerable<IReportable> deliverables, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, PROJECT project, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, IEnumerable<ExoTimeAuthorisation> exoAuthorisations, bool ignoreExoBudgetError = false)
        {
            var groupedDeliverables = deliverables.GroupBy(x => new { ChargeType = x.Charge, SubJob = x.Subjob_Name, DisciplineCode = x.Discipline_Code, CommodityCode = x.Commodity_Code })
                          .Select(group => new { group.Key.SubJob, group.Key.ChargeType, group.Key.DisciplineCode, group.Key.CommodityCode, ApprovedVariations = group.SelectMany(x => x.ApprovedVariations), BudgetInternalCosts = group.Sum(x => x.Budget_InternalCost), TotalHours = group.Sum(x => x.Total_Units) });

            List<ExoTimeAuthorisation> exoLines = GetProjectLines(primeroUnitOfWork, project.NUMBER);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();

            foreach (var groupedDeliverable in groupedDeliverables)
            {
                if (groupedDeliverable.SubJob == null || groupedDeliverable.CommodityCode == null)
                    continue;

                decimal baseTotalUnits = groupedDeliverable.TotalHours;
                decimal variationTotalUnits = groupedDeliverable.ApprovedVariations.Sum(x => x.AdjustmentUnits);

                //add main job when there are no approved variations
                //or only add codes when the WBS code is not entirely variation
                if(groupedDeliverable.ApprovedVariations.Count() == 0 || baseTotalUnits != variationTotalUnits)
                    exoSubJobs.Add(getProactiveSubJob(project.NUMBER, groupedDeliverable.SubJob, groupedDeliverable.DisciplineCode, groupedDeliverable.CommodityCode, string.Empty, groupedDeliverable.BudgetInternalCosts, groupedDeliverable.ChargeType, ignoreExoBudgetError, exoLines, primeroUnitOfWork, COMMODITY_CODECollection, exoAuthorisations));

                if (groupedDeliverable.ApprovedVariations.Count() > 0)
                {
                    var groupedVariations = groupedDeliverable.ApprovedVariations.GroupBy(x => x.VariationName).Select(group => new { VariationName = group.Key, VariationTotalCosts = group.Sum(x => x.AdjustmentInternalCosts) });
                    foreach (var groupedVariation in groupedVariations)
                    {
                        exoSubJobs.Add(getProactiveSubJob(project.NUMBER, groupedDeliverable.SubJob, groupedDeliverable.DisciplineCode, groupedDeliverable.CommodityCode, groupedVariation.VariationName, groupedVariation.VariationTotalCosts, groupedDeliverable.ChargeType, ignoreExoBudgetError, exoLines, primeroUnitOfWork, COMMODITY_CODECollection, exoAuthorisations));
                    }
                }
            }

            return exoSubJobs.OrderBy(x => x.SubJobCode).ToList();
        }

        private static ExoSubJobProjection getProactiveSubJob(string projectNumber, string subJobCode, string disciplineCode, string commodityCode, string variationCode, decimal budgetInternalCosts, ChargeType? chargeType, bool ignoreExoBudgetError, List<ExoTimeAuthorisation> exoLines, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, IEnumerable<ExoTimeAuthorisation> exoAuthorisations)
        {
            ExoSubJobProjection newSubJobProjection = ViewModelSource.Create(() => new ExoSubJobProjection());
            ExoTimeAuthorisation exoSubJobLine;
            if(variationCode == string.Empty || variationCode == null)
                exoSubJobLine = exoLines.FirstOrDefault(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && (x.VariationCode == null || x.VariationCode == string.Empty));
            else
                exoSubJobLine = exoLines.FirstOrDefault(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && x.VariationCode == variationCode);

            newSubJobProjection.IgnoreExoBudgetError = ignoreExoBudgetError;
            newSubJobProjection.Budget = budgetInternalCosts;

            //do not allow engineering to set budget
            //newSubJobProjection.ExoBudget = newSubJobProjection.Budget;
            if (exoSubJobLine != null)
            {
                newSubJobProjection.SubJobId = exoSubJobLine.SubJobNo;
                newSubJobProjection.SubJobCode = exoSubJobLine.SubJobCode;
                newSubJobProjection.SubJobTitle = exoSubJobLine.SubJobTitle;
                newSubJobProjection.ExoBudget = exoSubJobLine.BudgetRate;
            }
            else
            {
                JOBCOST_HDR findSubJob = GetProjectSubJob(primeroUnitOfWork, projectNumber, subJobCode);
                if (findSubJob != null)
                    newSubJobProjection.SubJobId = findSubJob.JOBNO;

                newSubJobProjection.SubJobCode = subJobCode;
            }

            newSubJobProjection.SubJobChargeType = chargeType;
            ExoTimeAuthorisation exoDisciplineLines = exoLines.FirstOrDefault(x => x.DisciplineCode == disciplineCode);
            if (exoDisciplineLines != null)
            {
                newSubJobProjection.DisciplineId = exoDisciplineLines.DisciplineId;
                newSubJobProjection.DisciplineCode = exoDisciplineLines.DisciplineCode;
                newSubJobProjection.DisciplineName = exoDisciplineLines.DisciplineName;
            }
            else
            {
                newSubJobProjection.DisciplineCode = disciplineCode;
            }

            ExoTimeAuthorisation exoCommodityLines = exoLines.FirstOrDefault(x => x.CommodityCode == commodityCode);
            if (exoCommodityLines != null)
            {
                newSubJobProjection.CommodityId = exoCommodityLines.CommodityId;
                newSubJobProjection.CommodityCode = exoCommodityLines.CommodityCode;
                newSubJobProjection.CommodityName = commodityCode;
            }
            else
            {
                newSubJobProjection.CommodityCode = commodityCode;
            }

            newSubJobProjection.VariationCode = variationCode;
            newSubJobProjection.PopulateCommodityCodes(COMMODITY_CODECollection);
            newSubJobProjection.AuthUserIds = new List<int>();
            ExoTimeAuthorisation exoLine;
            if(variationCode == string.Empty || variationCode == null)
                exoLine = exoLines.FirstOrDefault(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && (x.VariationCode == string.Empty || x.VariationCode == null));
            else

                exoLine = exoLines.FirstOrDefault(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && x.VariationCode == variationCode);

            if (exoLine != null)
            {
                newSubJobProjection.LineId = exoLine.LineSeqNo;
                List<ExoTimeAuthorisation> currentJobExoAuthorisations = exoAuthorisations.Where(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && x.VariationCode == variationCode).ToList();
                if (currentJobExoAuthorisations.Count > 0)
                {
                    List<int> userIds = currentJobExoAuthorisations.Where(x => x.ResourceStaffId != null).Select(x => (int)x.ResourceStaffId).Distinct().ToList();
                    newSubJobProjection.AuthUserIds = new List<int>(userIds);
                }
                else
                    newSubJobProjection.AuthUserIds = new List<int>();
            }

            return newSubJobProjection;
        }

        public static JOB_COSTGROUPS GetJOB_COSTGROUPS(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string shortCode)
        {
            return primeroUnitOfWork.JOB_COSTGROUPS.FirstOrDefault(x => x.SHORTCODE == shortCode);
        }

        public static JOB_COSTTYPES GetJOB_COSTTYPES(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string shortCode)
        {
            return primeroUnitOfWork.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == shortCode);
        }

        public static JOBCOST_HDR GetProjectSubJob(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, string subJobCode)
        {
            //remove the need to have a master subjob code
            var subJobs = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                          join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                          on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                          where MAINJOB.JOBCODE == projectNumber && SUBJOB.JOBCODE == subJobCode
                          select SUBJOB;

            if (subJobs.Count() == 0)
                return null;

            return subJobs.First();
        }


        public static JOBCOST_HDR GetProjectSubJob(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            //remove the need to have a master subjob code
            var subJobs = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                          join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                          on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                          where MAINJOB.JOBCODE == projectNumber
                          select SUBJOB;

            if (subJobs.Count() == 0)
                return null;

            return subJobs.First();
        }

        public static IQueryable<STAFF> GetStaffs(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            var querySTAFF = from STAFF in primeroUnitOfWork.STAFF
                             join JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                             on STAFF.STAFFNO equals JOBCOST_RESOURCE.STAFFNO
                             join STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                             on JOBCOST_RESOURCE.DEFAULT_STOCKCODE equals STOCK_ITEMS.STOCKCODE
                             where STAFF.ISACTIVE == "Y"
                             select STAFF;

            return querySTAFF;
        }

        public static IEnumerable<STOCK_ITEMS> GetMiscStockItems(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            var querySTOCK_ITEMS = from STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                             where STOCK_ITEMS.STOCKGROUP == 0
                             select STOCK_ITEMS;

            return querySTOCK_ITEMS;
        }

        public static IEnumerable<JOBCOST_HDR> GetProjectSubJobs(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var subJobs = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                          join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                          on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                          where MAINJOB.JOBCODE == projectNumber
                          select SUBJOB;

            if (subJobs.Count() == 0)
                return null;

            return subJobs;
        }

        public static JOB_RESOURCE_ALLOCATION GetResourceAllocation(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int jobNo, bool includeDisabled, int staffId)
        {
            DateTime disabledDateTime;
            if (includeDisabled)
                disabledDateTime = BluePrintsConstants.DefaultStartTime;
            else
                disabledDateTime = DateTime.Now;

            var resourceAllocation = from JOB_RESOURCE_ALLOCATION in primeroUnitOfWork.JOB_RESOURCE_ALLOCATION
                                     join JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                     on JOB_RESOURCE_ALLOCATION.RESOURCE_SEQNO equals JOBCOST_RESOURCE.SEQNO
                                     join STAFF in primeroUnitOfWork.STAFF
                                     on JOBCOST_RESOURCE.STAFFNO equals STAFF.STAFFNO
                                     where STAFF.STAFFNO == staffId && JOB_RESOURCE_ALLOCATION.JOBNO == jobNo && JOB_RESOURCE_ALLOCATION.END_DATE >= disabledDateTime
                                     select JOB_RESOURCE_ALLOCATION;

            if (resourceAllocation.Count() == 0)
                return null;

            return resourceAllocation.First();
        }

        public static void SetResourceAllocation(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int resourceId, int jobNo)
        {
            JOBCOST_RESOURCE resource = primeroUnitOfWork.JOBCOST_RESOURCE.FirstOrDefault(x => x.STAFFNO == resourceId);
            if(resource != null)
            {
                JOB_RESOURCE_ALLOCATION newRESOURCE_ALLOCATION = new JOB_RESOURCE_ALLOCATION();
                newRESOURCE_ALLOCATION.JOBNO = jobNo;
                newRESOURCE_ALLOCATION.RESOURCE_SEQNO = resource.SEQNO;
                
            }
        }

        public static int? GetStaffResourceNo(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int? staffId)
        {
            if (staffId == null)
                return null;

            var resources = from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                     join STAFF in primeroUnitOfWork.STAFF
                                     on JOBCOST_RESOURCE.STAFFNO equals STAFF.STAFFNO
                                     where STAFF.STAFFNO == staffId
                                     select JOBCOST_RESOURCE;

            if (resources.Count() == 0)
                return null;

            return resources.First().SEQNO;
        }

        public static string GetLoginId(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string name)
        {
            if (name == string.Empty)
                return null;

            List<string> nameSplit = name.Split(' ').ToList();
            string firstName = nameSplit.First();

            var staffs = from STAFF in primeroUnitOfWork.STAFF
                            where STAFF.LOGINID.StartsWith(firstName)
                            select STAFF;

            int largestNumber = 0;
            foreach (var staff in staffs)
            {
                string resultString = Regex.Match(staff.LOGINID, @"\d+").Value;
                if (resultString != string.Empty)
                {
                    int affixValue = Int32.Parse(resultString);
                    affixValue += 1;
                    if (affixValue > largestNumber)
                        largestNumber = affixValue;
                }
            }

            return string.Concat(firstName, largestNumber.ToString());
        }

        public static List<int> GetAvailableStaffEnumerations(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string name, out string partialShortCode)
        {
            Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
            partialShortCode = initials.Replace(name, "$1");

            string formatPartialShortCode = partialShortCode.Length > 2 ? partialShortCode.Substring(0, 2) : partialShortCode;
            partialShortCode = formatPartialShortCode;
            var resourceShortCodes = from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                            where JOBCOST_RESOURCE.SHORTCODE.StartsWith(formatPartialShortCode) || JOBCOST_RESOURCE.DEFAULT_STOCKCODE.StartsWith(formatPartialShortCode)
                            select JOBCOST_RESOURCE;

            List<JOBCOST_RESOURCE> allResourceShortCodes = resourceShortCodes.ToList();
            List<int> availableStaffEnumerations = new List<int>();
            List<int> occupiedStaffEnumerations = new List<int>();

            string regexString = @"\d+";
            foreach (var resource in allResourceShortCodes)
            {
                string shortCodeString = Regex.Match(resource.SHORTCODE, regexString).Value;
                string defaultShortCodeString = string.Empty;
                if (resource.DEFAULT_STOCKCODE != null)
                    defaultShortCodeString = resource.DEFAULT_STOCKCODE.Contains(formatPartialShortCode) ? Regex.Match(resource.DEFAULT_STOCKCODE, regexString).Value : string.Empty;

                string s;
                if (shortCodeString != defaultShortCodeString)
                    s = string.Empty;
                if (shortCodeString != string.Empty)
                {
                    int affixShortCodeValue = Int32.Parse(shortCodeString);
                    occupiedStaffEnumerations.Add(affixShortCodeValue);
                }

                if (defaultShortCodeString != string.Empty)
                {
                    int affixdefaultShortCodeValue = Int32.Parse(defaultShortCodeString);
                    occupiedStaffEnumerations.Add(affixdefaultShortCodeValue);
                }
            }

            for(int i=0;i < 99;i++)
            {
                if (!occupiedStaffEnumerations.Any(x => x == i))
                    availableStaffEnumerations.Add(i);
            }

            return availableStaffEnumerations;
        }

        public static JOB_COSTTYPES GetCommodity(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string commodityCode)
        {
            var costTypes = from JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                             where JOB_COSTTYPES.SHORTCODE == commodityCode
                             select JOB_COSTTYPES;

            if (costTypes.Count() == 0)
                return null;

            return costTypes.First();
        }

        public static JOB_COSTGROUPS GetCostGroup(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string disciplineCode)
        {
            var costGroups = from COSTGROUP in primeroUnitOfWork.JOB_COSTGROUPS
                             where COSTGROUP.SHORTCODE == disciplineCode
                             select COSTGROUP;

            if (costGroups.Count() == 0)
                return null;

            return costGroups.First();
        }

        public static IEnumerable<JOB_COSTGROUPS> GetCostGroups(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            var costGroups = from COSTGROUP in primeroUnitOfWork.JOB_COSTGROUPS
                             select COSTGROUP;

            return costGroups;
        }

        public static int? GetJOBCODELINEID(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            int? maxLineId = (from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                  select JOBCOST_LINES.JOBCOSTLINEID).Max();

            return maxLineId;
        }

        public static JOBCOST_RESOURCE FindJOBCOST_RESOURCE(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int? seqNo, string name)
        {
            IQueryable<JOBCOST_RESOURCE> resources = null;
            
            if(name != string.Empty)
            {
                resources = (from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                             where JOBCOST_RESOURCE.RESOURCENAME == name
                             select JOBCOST_RESOURCE);
            }
            else
            {
                resources = (from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                             where JOBCOST_RESOURCE.SEQNO == seqNo
                             select JOBCOST_RESOURCE);
            }

            if (resources.Count() > 0)
                return resources.First();

            return null;
        }

        public static STOCK_ITEMS FindSTOCK_ITEM(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string shortCode)
        {
            IQueryable<STOCK_ITEMS> stock_item = (from STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                             where STOCK_ITEMS.STOCKCODE == shortCode
                             select STOCK_ITEMS);

            if (stock_item.Count() > 0)
                return stock_item.First();

            return null;
        }

        public static STAFF FindSTAFF(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int? staffNo, string name, out string primaryDbName)
        {
            IQueryable<STAFF> staffs = null;
            
            if(name != string.Empty)
            {
                staffs = (from STAFF in primeroUnitOfWork.STAFF
                          where STAFF.NAME == name
                          select STAFF);
            }
            else
            {
                staffs = (from STAFF in primeroUnitOfWork.STAFF
                          where STAFF.STAFFNO == staffNo
                          select STAFF);
            }

            if (staffs.Count() > 0)
            {
                primaryDbName = staffs.First().NAME;
                return staffs.First();
            }

            primaryDbName = string.Empty;
            return null;
        }

        public static int? GetSTAFFID(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            int? maxLineId = (from STAFF in primeroUnitOfWork.STAFF
                              select STAFF.STAFFNO).Max();

            return maxLineId + 1;
        }

        public static IQueryable<JOB_RESOURCE_ALLOCATION> GetAuthSubJobs(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var subJobAuths = from JOB_RESOURCE_ALLOCATION in primeroUnitOfWork.JOB_RESOURCE_ALLOCATION
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOB_RESOURCE_ALLOCATION.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber
                                 select JOB_RESOURCE_ALLOCATION;

            return subJobAuths;
        }

        public static IQueryable<JOB_TRANSACTIONS> GetJOB_TRANSACTIONS(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var transactions = from JOB_TRANSACTIONS in primeroUnitOfWork.JOB_TRANSACTIONS
                                 join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                                 on JOB_TRANSACTIONS.COST_GROUP equals JOB_COSTGROUPS.SEQNO
                                 join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                                 on JOB_TRANSACTIONS.COST_TYPE equals JOB_COSTTYPES.SEQNO
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOB_TRANSACTIONS.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber
                                 select JOB_TRANSACTIONS;

            return transactions;
        }

        public static JOBCOST_LINES GetProjectLine(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, ExoSubJobProjection line, bool ignoreCostGroupCostType = false)
        {
            if (line.SubJobCode == null || line.SubJobCode == string.Empty || (!ignoreCostGroupCostType && (line.DisciplineCode == null || line.DisciplineCode == string.Empty || line.CommodityCode == null || line.CommodityCode == string.Empty)))
                return null;

            string stockCode = line.StockCode == null || line.StockCode == string.Empty ? line.CommodityCode : line.StockCode;

            IQueryable<JOBCOST_LINES> projectLines;
            
            if(!ignoreCostGroupCostType)
            {
                projectLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                               join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                               on JOBCOST_LINES.COST_CENTRE2 equals JOB_COSTGROUPS.SEQNO
                               join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                               on JOBCOST_LINES.COST_CENTRE equals JOB_COSTTYPES.SEQNO
                               join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                               on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                               join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                               on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                               where MAINJOB.JOBCODE == projectNumber && JOBCOST_LINES.LINKED_STOCKCODE == stockCode.ToUpper() && SUBJOB.JOBCODE == line.SubJobCode.ToUpper() && JOB_COSTGROUPS.SHORTCODE == line.DisciplineCode.ToUpper() && JOB_COSTTYPES.SHORTCODE == line.CommodityCode.ToUpper()
                               select JOBCOST_LINES;
            }
            else
            {
                projectLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                               join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                               on JOBCOST_LINES.COST_CENTRE2 equals JOB_COSTGROUPS.SEQNO
                               into JobCostCentre2
                               from JobCostCentre2DefaultIfEmpty in JobCostCentre2.DefaultIfEmpty()
                               join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                               on JOBCOST_LINES.COST_CENTRE equals JOB_COSTTYPES.SEQNO
                               into JobCostCentre
                               from JobCostCentreDefaultIfEmpty in JobCostCentre.DefaultIfEmpty()
                               join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                               on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                               join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                               on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                               where MAINJOB.JOBCODE == projectNumber && JOBCOST_LINES.LINKED_STOCKCODE == stockCode.ToUpper() && SUBJOB.JOBCODE == line.SubJobCode.ToUpper()
                               select JOBCOST_LINES;
            }


            List<JOBCOST_LINES> listProjectLines = projectLines.ToList();
            if (line.VariationCode == string.Empty || line.VariationCode == null)
                return listProjectLines.FirstOrDefault(x => x.X_VARIATION_CODE == string.Empty || x.X_VARIATION_CODE == null);
            else
                return listProjectLines.FirstOrDefault(x => x.X_VARIATION_CODE == line.VariationCode);
        }

        public static JOBCOST_LINES GetAnyProjectLineByJobNumber(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber
                                 select JOBCOST_LINES;

            if (availableLines.Count() == 0)
                return null;

            return availableLines.First();
        }

        public static List<ExoTimeAuthorisation> GetSubJob(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber
                                 select new { MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE };

            List<ExoTimeAuthorisation> exoTimes = availableLines.ToList().Select
                (x =>
                {
                    ExoTimeAuthorisation exoTime = new ExoTimeAuthorisation();
                    exoTime.MasterJobNo = x.MASTERJOBNO;
                    exoTime.SubJobNo = x.SUBJOBNO;
                    exoTime.SubJobCode = x.SUBJOBNAME;
                    exoTime.SubJobTitle = x.SUBJOBTITLE;
                    return exoTime;
                }).ToList();

            return exoTimes;
        }

        public static List<ExoTimeAuthorisation> GetProjectLinesIgnoreCostCentres(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                                 on JOBCOST_LINES.COST_CENTRE2 equals JOB_COSTGROUPS.SEQNO
                                 into JobCostCentre2
                                 from JobCostCentre2DefaultIfEmpty in JobCostCentre2.DefaultIfEmpty()
                                 join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                                 on JOBCOST_LINES.COST_CENTRE equals JOB_COSTTYPES.SEQNO
                                 into JobCostCentre
                                 from JobCostCentreDefaultIfEmpty in JobCostCentre.DefaultIfEmpty()
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 join STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                                 on JOBCOST_LINES.LINKED_STOCKCODE equals STOCK_ITEMS.STOCKCODE
                                 where MAINJOB.JOBCODE == projectNumber
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, MASTERJOBCODE = MAINJOB.JOBCODE, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JobCostCentre2DefaultIfEmpty.SHORTCODE, DISCIPLINE_NAME = JobCostCentre2DefaultIfEmpty.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JobCostCentreDefaultIfEmpty.SHORTCODE, STOCK_CODE = STOCK_ITEMS.STOCKCODE, STOCK_NAME = STOCK_ITEMS.DESCRIPTION, COMMODITY_NAME = JobCostCentreDefaultIfEmpty.COSTDESC, VARIATION_CODE = JOBCOST_LINES.X_VARIATION_CODE, BUDGETED_QTY = JOBCOST_LINES.QUOTE_QTY, BUDGETED_REV = JOBCOST_LINES.LINETOTAL, BUDGETED_RATE = JOBCOST_LINES.ACTUAL_UNITCOST, FORECAST_RATE = JOBCOST_LINES.QUOTE_UNITPR };

            List<ExoTimeAuthorisation> exoTimes = availableLines.ToList().Select(x => populateExoLine(x)).ToList();
            return exoTimes;
        }

        public static List<ExoTimeAuthorisation> GetProjectLines(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                                 on JOBCOST_LINES.COST_CENTRE2 equals JOB_COSTGROUPS.SEQNO
                                 join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                                 on JOBCOST_LINES.COST_CENTRE equals JOB_COSTTYPES.SEQNO
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 join STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                                 on JOBCOST_LINES.STOCKCODE equals STOCK_ITEMS.STOCKCODE
                                 where MAINJOB.JOBCODE == projectNumber 
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, MASTERJOBCODE = MAINJOB.JOBCODE, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOB_COSTTYPES.SHORTCODE, STOCK_CODE = STOCK_ITEMS.STOCKCODE, STOCK_NAME = STOCK_ITEMS.DESCRIPTION, COMMODITY_NAME = JOB_COSTTYPES.COSTDESC, VARIATION_CODE = JOBCOST_LINES.X_VARIATION_CODE, BUDGETED_QTY = JOBCOST_LINES.QUOTE_QTY, BUDGETED_REV = JOBCOST_LINES.LINETOTAL, BUDGETED_RATE = JOBCOST_LINES.ACTUAL_UNITCOST, FORECAST_RATE = JOBCOST_LINES.QUOTE_UNITPR };

            List<ExoTimeAuthorisation> exoTimes = availableLines.ToList().Select(x => populateExoLine(x)).ToList();
            return exoTimes;
        }

        public static List<ExoTimeAuthorisation> GetProjectLinesWithoutCostInfo(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, STOCK_CODE = JOBCOST_LINES.STOCKCODE, VARIATION_CODE = JOBCOST_LINES.X_VARIATION_CODE, BUDGETED_QTY = JOBCOST_LINES.QUOTE_QTY, BUDGETED_REV = JOBCOST_LINES.LINETOTAL, BUDGETED_RATE = JOBCOST_LINES.ACTUAL_UNITCOST, FORECAST_RATE = JOBCOST_LINES.QUOTE_UNITPR };

            List<ExoTimeAuthorisation> exoTimes = availableLines.ToList().Select(x => populateExoLineWithoutCostInfo(x)).ToList();
            return exoTimes;
        }

        public static dynamic GetProjectRevenue(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber && JOBCOST_LINES.STOCKCODE == BluePrintsResources.Default_Revenue_StockCode
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, STOCK_CODE = JOBCOST_LINES.STOCKCODE, BUDGETED_QTY = JOBCOST_LINES.QUOTE_QTY, BUDGETED_REV = JOBCOST_LINES.LINETOTAL, BUDGETED_RATE = JOBCOST_LINES.ACTUAL_UNITCOST };

            IEnumerable<dynamic> dbTimes = availableLines.ToList();
            if (dbTimes.Count() > 0)
                return dbTimes.First();
            else
                return null;
        }


        public static decimal GetProjectClaims(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var jobClaims = from JOBTRANS in primeroUnitOfWork.JOB_TRANSACTIONS
                            join JOBCOST_HDR in primeroUnitOfWork.JOBCOST_HDR
                            on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR.JOBNO
                            where JOBCOST_HDR.JOBCODE == projectNumber
                            select new { JOBCOST_HDR.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, VARIATIONCODE = JOBTRANS.X_VARIATIONCODE, JOBTRANS.INVOICED, JOBTRANS.INVOICEDATE, JOBTRANS.INVSEQNO };

            IEnumerable<dynamic> dbTimes = jobClaims.ToList();
            if (dbTimes.Count() > 0)
            {
                return Convert.ToDecimal(dbTimes.Sum(x => (double)x.INVOICED));
            }
            else
                return 0;
        }

        public static List<ExoTimeAuthorisation> GetExoLinesAuthorisations(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber = null, int? staffNo = null)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                                 on JOBCOST_LINES.COST_CENTRE2 equals JOB_COSTGROUPS.SEQNO
                                 into JobCostCentre2
                                 from JobCostCentre2DefaultIfEmpty in JobCostCentre2.DefaultIfEmpty()
                                 join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                                 on JOBCOST_LINES.COST_CENTRE equals JOB_COSTTYPES.SEQNO
                                 into JobCostCentre
                                 from JobCostCentreDefaultIfEmpty in JobCostCentre.DefaultIfEmpty()
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 join STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                                 on JOBCOST_LINES.STOCKCODE equals STOCK_ITEMS.STOCKCODE
                                 join JOB_RESOURCE_ALLOCATION in primeroUnitOfWork.JOB_RESOURCE_ALLOCATION
                                 on JOBCOST_LINES.JOBNO equals JOB_RESOURCE_ALLOCATION.JOBNO
                                 join JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                 on JOB_RESOURCE_ALLOCATION.RESOURCE_SEQNO equals JOBCOST_RESOURCE.SEQNO
                                 where MAINJOB.JOBCODE == projectNumber && JOB_RESOURCE_ALLOCATION.START_DATE <= DateTime.Now && JOB_RESOURCE_ALLOCATION.END_DATE > DateTime.Now
                                 select new { MAINJOB.JOBCODE, MASTERJOBCODE = MAINJOB.JOBCODE, SUBJOB, MAINJOB, JOBCOST_RESOURCE.STAFFNO, LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JobCostCentre2DefaultIfEmpty.SHORTCODE, DISCIPLINE_NAME = JobCostCentre2DefaultIfEmpty.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JobCostCentreDefaultIfEmpty.SHORTCODE, COMMODITY_NAME = JobCostCentreDefaultIfEmpty.COSTDESC, RESOURCE_SEQNO = JOBCOST_RESOURCE.SEQNO, RESOURCE_STAFF_ID = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.DEFAULT_STOCKCODE, STOCK_CODE_DESC = STOCK_ITEMS.DESCRIPTION, END_DATE = JOB_RESOURCE_ALLOCATION.END_DATE, VARIATIONCODE = JOBCOST_LINES.X_VARIATION_CODE, JOBSTATUS = SUBJOB.STATUS };

            var availableLinesByProject = projectNumber == null ? availableLines : availableLines.Where(x => x.JOBCODE == projectNumber);
            var availableLinesByStaffNo = staffNo == null ? availableLinesByProject : availableLinesByProject.Where(x => x.STAFFNO == staffNo);

            List<ExoTimeAuthorisation> exoTimes = availableLinesByStaffNo.ToList().Select(x => populateExoTimeAuth(x)).ToList();
            return exoTimes;
        }

        public static List<string> GetJobNarratives(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var timesheetLineNarratives = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 join TIMESHEET in primeroUnitOfWork.JOB_TIMESHEETS
                                 on SUBJOB.JOBNO equals TIMESHEET.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber && TIMESHEET.X_NARRATIVE != null
                                 select new { Narrative = TIMESHEET.X_NARRATIVE };

            return timesheetLineNarratives.Select(x => x.Narrative).Distinct().OrderBy(x => x).ToList();
        }

        public static IQueryable<ExoResourceProjection> GetResources(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IEnumerable<USER> USERCollection)
        {
            var resources = from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                     join STAFF in primeroUnitOfWork.STAFF
                                     on JOBCOST_RESOURCE.STAFFNO equals STAFF.STAFFNO
                                     join STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                                     on JOBCOST_RESOURCE.SHORTCODE equals STOCK_ITEMS.STOCKCODE
                                     where JOBCOST_RESOURCE.ISACTIVE == "Y"
                            select new { JOBCOST_RESOURCE.SEQNO, STAFF.STAFFNO, STAFF.PAYROLL_ID, JOBCOST_STAFFNO = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.TITLE, JOBCOST_RESOURCE.DEFAULT_STOCKCODE, JOBCOST_RESOURCE.SHORTCODE, STAFF.SECURITYPROFILEID, STAFF.USERPROFILEID, STAFF.REPORTS_TO_STAFFNO, STOCK_ITEMS.SELLPRICE1, STOCK_ITEMS.STDCOST, STOCK_ITEMS.SALES_GL_CODE, STOCK_ITEMS.PURCH_GL_CODE, STOCK_ITEMS.COS_GL_CODE, STOCK_ITEMS.COSTTYPE, STOCK_ITEMS.COSTGROUP, STOCK_ITEMS.X_DEPARTMENT };

            List<ExoResourceProjection> exoResources = resources.ToList().Select(x => ViewModelSource.Create(() => new ExoResourceProjection() { GUID = Guid.NewGuid(), STAFFNO = x.STAFFNO, PAYROLL_ID = x.PAYROLL_ID, RESOURCE_SEQNO = x.SEQNO, RESOURCENAME = x.RESOURCENAME, TITLE = x.TITLE, DEFAULT_STOCKCODE = x.DEFAULT_STOCKCODE, SECURITYPROFILEID = x.SECURITYPROFILEID, USERPROFILEID = x.USERPROFILEID, REPORTS_TO_STAFFNO = x.REPORTS_TO_STAFFNO, SHORTCODE = x.SHORTCODE, IsViewNewRow = false, STDCOST = x.STDCOST, SELLPRICE1 = x.SELLPRICE1, SALES_GL_CODE = x.SALES_GL_CODE, PURCH_GL_CODE = x.PURCH_GL_CODE, COS_GL_CODE = x.COS_GL_CODE, COSTGROUP = x.COSTGROUP, COSTTYPE = x.COSTTYPE, DEPARTMENT = x.X_DEPARTMENT})).ToList();
            foreach(ExoResourceProjection exoResource in exoResources)
            {
                if (USERCollection.Any(x => x.EXO_STAFF_ID == exoResource.STAFFNO))
                    exoResource.IsExistInBP = true;
            }
            
            return exoResources.AsQueryable();
        }

        public static IEnumerable<JOBCOST_HDR> GetSlaveExoLines(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int masterJobNo)
        {
            var availableLines = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 where SUBJOB.MASTER_JOBNO == masterJobNo
                                 select SUBJOB;

            return availableLines;
        }

        private static ExoTimeAuthorisation populateExoLine(dynamic dbTime)
        {
            ExoTimeAuthorisation exoTime = new ExoTimeAuthorisation();
            exoTime.LineSeqNo = dbTime.LINEID;
            exoTime.MasterJobNo = dbTime.MASTERJOBNO;
            exoTime.MasterJobCode = dbTime.MASTERJOBCODE;
            exoTime.SubJobNo = dbTime.SUBJOBNO;
            exoTime.SubJobCode = dbTime.SUBJOBNAME;
            exoTime.SubJobTitle = dbTime.SUBJOBTITLE;
            exoTime.DisciplineId = dbTime.DISCIPLINE_ID;
            exoTime.DisciplineCode = dbTime.DISCIPLINE_CODE == null ? string.Empty : dbTime.DISCIPLINE_CODE;
            exoTime.DisciplineName = dbTime.DISCIPLINE_NAME;
            exoTime.CommodityId = dbTime.COMMODITY_ID;
            exoTime.CommodityCode = dbTime.COMMODITY_CODE == null ? string.Empty : dbTime.COMMODITY_CODE;
            exoTime.CommodityName = dbTime.COMMODITY_NAME;
            exoTime.StockCode = dbTime.STOCK_CODE;
            exoTime.StockName = dbTime.STOCK_NAME;
            exoTime.VariationCode = dbTime.VARIATION_CODE == null ? string.Empty : dbTime.VARIATION_CODE;
            exoTime.BudgetQty = Convert.ToDecimal(dbTime.BUDGETED_QTY);
            exoTime.BudgetRev = Convert.ToDecimal(dbTime.BUDGETED_REV);
            exoTime.BudgetRate = Convert.ToDecimal(dbTime.BUDGETED_RATE);
            exoTime.ForecastRate = Convert.ToDecimal(dbTime.FORECAST_RATE);

            return exoTime;
        }

        private static ExoTimeAuthorisation populateExoLineWithoutCostInfo(dynamic dbTime)
        {
            ExoTimeAuthorisation exoTime = new ExoTimeAuthorisation();
            exoTime.LineSeqNo = dbTime.LINEID;
            exoTime.MasterJobNo = dbTime.MASTERJOBNO;
            exoTime.SubJobNo = dbTime.SUBJOBNO;
            exoTime.SubJobCode = dbTime.SUBJOBNAME;
            exoTime.SubJobTitle = dbTime.SUBJOBTITLE;
            exoTime.DisciplineId = dbTime.DISCIPLINE_ID;
            exoTime.CommodityId = dbTime.COMMODITY_ID;
            exoTime.VariationCode = dbTime.VARIATION_CODE;
            exoTime.BudgetQty = Convert.ToDecimal(dbTime.BUDGETED_QTY);
            exoTime.BudgetRev = Convert.ToDecimal(dbTime.BUDGETED_REV);
            exoTime.BudgetRate = Convert.ToDecimal(dbTime.BUDGETED_RATE);
            exoTime.ForecastRate = Convert.ToDecimal(dbTime.FORECAST_RATE);

            return exoTime;
        }

        private static ExoTimeAuthorisation populateExoTimeAuth(dynamic dbTime)
        {
            ExoTimeAuthorisation exoTime = new ExoTimeAuthorisation();
            exoTime.LineSeqNo = dbTime.LINEID;
            exoTime.MasterJobNo = dbTime.MASTERJOBNO;
            exoTime.MasterJobCode = dbTime.MASTERJOBCODE;
            exoTime.SubJobNo = dbTime.SUBJOBNO;
            exoTime.SubJobCode = dbTime.SUBJOBNAME;
            exoTime.SubJobTitle = dbTime.SUBJOBTITLE;
            exoTime.DisciplineId = dbTime.DISCIPLINE_ID;
            exoTime.DisciplineCode = dbTime.DISCIPLINE_CODE;
            exoTime.DisciplineName = dbTime.DISCIPLINE_NAME;
            exoTime.CommodityId = dbTime.COMMODITY_ID;
            exoTime.CommodityCode = dbTime.COMMODITY_CODE;
            exoTime.CommodityName = dbTime.COMMODITY_NAME;
            exoTime.ResourceSeqNo = dbTime.RESOURCE_SEQNO;
            exoTime.ResourceStaffId = dbTime.RESOURCE_STAFF_ID;
            exoTime.ResourceName = dbTime.RESOURCENAME;
            exoTime.ResourceEndDate = dbTime.END_DATE;
            exoTime.ResourceStockCode = dbTime.DEFAULT_STOCKCODE;
            exoTime.StockCode = dbTime.DEFAULT_STOCKCODE; //when this projection is used in timesheet, the stock code will be resources instead of stock code in jobcost_lines because time booking is only to commodity level
            exoTime.StockName = dbTime.STOCK_CODE_DESC;
            exoTime.VariationCode = dbTime.VARIATIONCODE;

            if (exoTime.VariationCode == null)
                exoTime.VariationCode = string.Empty;
            exoTime.JobStatus = dbTime.JOBSTATUS;

            return exoTime;
        }
    }

    public class ExoTimeAuthorisation
    {
        public int LineSeqNo { get; set; }
        public int MasterJobNo { get; set; }
        public string MasterJobCode { get; set; }
        public int SubJobNo { get; set; }
        public string SubJobCode { get; set; }
        public string SubJobTitle { get; set; }
        public int? DisciplineId { get; set; }
        public string DisciplineCode { get; set; }
        public string DisciplineName { get; set; }
        public int? CommodityId { get; set; }
        public string CommodityCode { get; set; }
        public string CommodityName { get; set; }
        public int ResourceSeqNo { get; set; }
        public int? ResourceStaffId { get; set; }
        public string ResourceName { get; set; }
        public DateTime ResourceEndDate { get; set; }
        public string StockCode { get; set; }
        public string ResourceStockCode { get; set; }
        public string StockName { get; set; }
        public string VariationCode { get; set; }
        public string JobStatus { get; set; }
        public decimal BudgetQty { get; set; }
        public decimal BudgetRev { get; set; }
        public decimal BudgetRate { get; set; }
        public decimal ForecastRate { get; set; }
        public decimal BudgetCosts => BudgetQty * BudgetRate;
        public string OfficeName { get; set; }
        public string AreaCode
        {
            get
            {
                if (SubJobCode == string.Empty)
                    return string.Empty;
                else if (SubJobCode.Length < 15)
                    return string.Empty;

                return SubJobCode.Substring(6, 3);
            }
        }

        public string PhaseCode
        {
            get
            {
                if (SubJobCode == string.Empty)
                    return string.Empty;
                else if (SubJobCode.Length < 15)
                    return string.Empty;

                return SubJobCode.Substring(13, 2);
            }
        }
    }

    public class PrimeroSubJob
    {
        public int? Id { get; set; }
        public int? MasterId { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int? DebtorId { get; set; }
        public int JobCategory { get; set; }
        public int JobType { get; set; }
        public string JobStatus { get; set; }
        public ChargeType? ChargeType { get; set; }

        public string AreaCode => Code.Length < 9 ? string.Empty : Code.Substring(6, 3);
        public string SubAreaCode => Code.Length < 11 ? string.Empty : Code.Substring(10, 2);
        public string PhaseCode => Code.Length < 14 ? string.Empty : Code.Substring(13, 2);

        public int? ResourceSeqNo { get; set; }
    }

    public class PrimeroDiscipline
    {
        public int? Id { get; set; }
        public int? SubjobId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public int? ResourceSeqNo { get; set; }
    }

    public class PrimeroCommodity
    {
        public int? Id { get; set; }
        public int? DisciplineId { get; set; }
        public int? ResourceSeqNo { get; set; }
        public int? SubJobNo { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UOM { get; set; }
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
        public bool IsIndirectOnly { get; set; }
    }

    public class PrimeroResource
    {
        public int? Id { get; set; }
        public int? SeqNo { get; set; }
        public string Name { get; set; }
        public string StockCode { get; set; }
    }

    public class TimesheetDate
    {
        public DateTime WeekStartDate { get; set; }
        public int DayNumber { get; set; }
    }
}
