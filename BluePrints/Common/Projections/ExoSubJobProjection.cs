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

    [ConstraintAttributes("SubJobCode, DisciplineCode, CommodityCode, VariationCode")]
    //ExoSubJobProjection is not flat so this is created
    public class ExoSubJobEditableProjection : EntityBase, IGuidEntityKey, IDXDataErrorInfo
    {
        public ExoSubJobEditableProjection()
        {
            AuthUsers = new ObservableCollection<ExoSubJobAuth>();
        }

        public ExoSubJobEditableProjection(ExoSubJobProjection entity)
        {
            this.SubJobId = entity.SubJob == null ? null : entity.SubJob.Id;
            this.SubJobCode = entity.SubJob == null ? string.Empty : entity.SubJob.Code;
            this.SubJobTitle = entity.SubJob == null ? string.Empty : entity.SubJob.Title;
            this.DisciplineId = entity.Discipline == null ? null : entity.Discipline.Id;
            this.DisciplineCode = entity.Discipline == null ? string.Empty : entity.Discipline.Code;
            this.DisciplineName = entity.Discipline == null ? string.Empty : entity.Discipline.Name;
            this.CommodityId = entity.Commodity == null ? null : entity.Commodity.Id;
            this.CommodityCode = entity.Commodity == null ? string.Empty : entity.Commodity.Code;
            this.CommodityName = entity.Commodity == null ? string.Empty : entity.Commodity.Name;
            this.VariationCode = entity.Variation_Code;
            this.Budget = entity.ExoBudgetCosts;
            this.Rate = entity.ExoForecastRate;
        }

        [Key]
        public int? LineId { get; set; }
        public int? SubJobId { get; set; }

        [Required]
        public string SubJobCode { get; set; }

        [Required]
        public string SubJobTitle { get; set; }
        public ChargeType? SubJobChargeType { get; set; }
        public int? DisciplineId { get; set; }

        [Required]
        public string DisciplineCode { get; set; }

        [Required]
        public string DisciplineName { get; set; }
        public int? CommodityId { get; set; }

        [Required]
        public string CommodityCode { get; set; }
        public string CommodityName { get; set; }
        public bool CommodityIsIndirectOnly { get; set; }
        public string VariationCode { get; set; }
        public decimal Budget { get; set; }
        public decimal Rate { get; set; }

        public ObservableCollection<ExoSubJobAuth> AuthUsers { get; set; }

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
                if (SubJobCode == null || SubJobCode.Length < 15)
                    return null;

                string phaseTypeString = SubJobCode.Substring(13, 1).ToUpper();
                if (phaseTypeString == "I")
                    return Common.PhaseType.Indirect;
                else if (phaseTypeString == "P")
                    return Common.PhaseType.Indirect;
                else if (phaseTypeString == "D")
                    return Common.PhaseType.Design;
                else if (phaseTypeString == "C")
                    return Common.PhaseType.Construct;

                return null;
            }
        }

        public bool IsCommodityCodeValid
        {
            get
            {
                if (CommodityCode == null || ValidCommodityCodes.Count() == 0)
                    return false;

                return ValidCommodityCodes.Any(x => x.CODE == CommodityCode);
            }
        }

        public IEnumerable<COMMODITY_CODE> ValidCommodityCodes
        {
            get
            {
                if (COMMODITY_CODES == null || DisciplineCode == null || DisciplineCode.Length < 2 || PhaseType == null)
                    return new List<COMMODITY_CODE>();

                string disciplineCode = DisciplineCode.Substring(0, 2);
                return COMMODITY_CODES.Where(x => x.PHASE_TYPE == PhaseType && (x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == disciplineCode))).OrderBy(x => x.CODE).ToList();
            }
        }

        public bool IsSubJobExistsInExo => SubJobCode != string.Empty;
        public bool IsDisciplineExistsInExo => DisciplineCode != string.Empty;
        public bool IsCommodityExistsInExo => CommodityCode != string.Empty;
        public bool IsLineExistsInExo => LineId != null;
        public bool HasBudget { get; set; }

        private IEnumerable<COMMODITY_CODE> COMMODITY_CODES { get; set; }
        public void PopulateCommodityCodes(IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection)
        {
            COMMODITY_CODES = COMMODITY_CODECollection;
        }


        public void PopulateLineAuthUsers(IEnumerable<ExoSubJobEditableProjection> projections)
        {
            ExoSubJobEditableProjection existingSameSubJobLine = projections.FirstOrDefault(x => x.SubJobCode == this.SubJobCode);
            if (existingSameSubJobLine != null)
            {
                foreach (ExoSubJobAuth authUser in existingSameSubJobLine.AuthUsers)
                {
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    DataUtils.ShallowCopy(newUser, authUser);
                    this.AuthUsers.Add(newUser);
                }
            }
        }

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if(propertyName == BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode) && !IsCommodityCodeValid)
            {
                info.ErrorText = "Invalid commodity code, please check phase and discipline";
            }
        }

        public void GetError(ErrorInfo info)
        {
        }

        //Used so that class can be used in view model
        #region View Model Compatibility Members
        public Guid guid { get; set; }

        public Guid GUID { get => guid; set => guid = value; }
        #endregion
    }

    public class ExoSubJobProjection : IGuidEntityKey, ICanAssignP6
    {
        public ExoSubJobProjection()
        {
            Variation_Code = string.Empty;
        }
        
        public int? LineId { get; set; }
        public PrimeroSubJob SubJob { get; set; }
        public PrimeroDiscipline Discipline { get; set; }
        public PrimeroCommodity Commodity { get; set; }
        public string Variation_Code { get; set; }
        public ObservableCollection<ExoSubJobAuth> AuthUsers { get; set; }

        public bool IsSubJobExistsInExo => SubJob != null && SubJob.Id != null;
        public bool IsDisciplineExistsInExo => Discipline != null && Discipline.Id != null;
        public bool IsCommodityExistsInExo => Commodity != null && Commodity.Id != null;
        public bool IsLineExistsInExo => LineId != null;
        public bool HasBudget { get; set; }
        //public bool IsLineExistsInExo => SubJob.Id != null;

        //used to trick view model
        public Guid GUID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<P6_ASSIGNMENT> P6_Assignments => throw new NotImplementedException();

        public IEnumerable<PROGRESS_ITEM> Progresses => throw new NotImplementedException();

        public Guid DeliverableKey => throw new NotImplementedException();

        public string P6AssignmentName => throw new NotImplementedException();

        public string P6AssignmentDescription => throw new NotImplementedException();

        public string P6AssignmentDescription2 => throw new NotImplementedException();

        public decimal Assigned_Percentage => throw new NotImplementedException();

        public decimal Remaining_Percentage => throw new NotImplementedException();

        public decimal P6_Assignment_Total_Quantity => throw new NotImplementedException();

        public string P6_Assignment_UOM => throw new NotImplementedException();

        public Guid? P6_WorkpackGuid => throw new NotImplementedException();

        public DateTime? TaskAssignmentStartDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public decimal EarnedUnitsAccountedFor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool NewEntityFromView { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Guid OriginalEntityKey => throw new NotImplementedException();

        public decimal Budget_Units => throw new NotImplementedException();

        public decimal Total_Units => throw new NotImplementedException();

        public decimal Variation_Units => throw new NotImplementedException();

        public decimal Variation_Costs => throw new NotImplementedException();

        decimal IHaveHours.Budget_Quantity => throw new NotImplementedException();

        public decimal Total_Quantity => throw new NotImplementedException();

        public void SetOriginalEntityKey(Guid newGuid)
        {

        }

        public void Update()
        {
            this.RaisePropertiesChanged();
        }

        public decimal ExoForecastRate { get; set; }
        public decimal ExoBudgetQty { get; set; }
        public decimal ExoBudgetCosts { get; set; }
        public DateTime CREATED { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? UPDATED { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? DELETED { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public decimal Earned_Units_ToDate => throw new NotImplementedException();

        public decimal Variation_Quantity => throw new NotImplementedException();

        public decimal Total_Costs => throw new NotImplementedException();
    }

    public static class ExoMethods
    {
        public static bool CommitLineSubJob(ExoSubJobEditableProjection projection, bool editLineAfterCommit, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, string projectNumber, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            if (projection.SubJobCode == null || projection.SubJobCode == string.Empty)
                return false;

            JOBCOST_HDR subjob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, projectNumber, projection.SubJobCode);
            if (projection.SubJobTitle == null || projection.SubJobTitle == string.Empty)
            {
                if (subjob == null)
                {
                    var bulkEditStringsViewModel = BulkEditStringsViewModel.Create(string.Empty, projection.SubJobCode + " Title:");
                    if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Please input title", "BulkEditStrings", bulkEditStringsViewModel) == MessageResult.OK)
                    {
                        projection.SubJobTitle = bulkEditStringsViewModel.EditValue;
                    }
                    else
                        return false;
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

        public static bool CommitLineDiscipline(ExoSubJobEditableProjection projection, bool editLineAfterCommit, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, string projectNumber, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            if (projection.DisciplineCode == null || projection.DisciplineCode == string.Empty)
                return false;

            JOB_COSTGROUPS discipline = ExoQueries.GetDiscipline(primeroUnitOfWork, projection.DisciplineCode);
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

        public static bool CommitLineCommodity(ExoSubJobEditableProjection projection, bool editLineAfterCommit, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, string projectNumber, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            if (projection.CommodityCode == null || projection.CommodityCode == string.Empty || projection.DisciplineId == null)
                return false;

            JOB_COSTTYPES commodity = ExoMethods.findExistingCommodity(projection.CommodityCode, string.Empty, (int)projection.DisciplineId);
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
                            line.STOCKCODE = commodity.SHORTCODE.ToUpper();

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

        public static JOBCOST_LINES findExistingOrAddLine(ExoSubJobEditableProjection exoLine, JOBCOST_LINES copyLine, string projectNumber)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            if (exoLine.SubJobId == null || exoLine.DisciplineId == null || exoLine.CommodityId == null)
                return null;
            else
            {
                JOBCOST_LINES line = ExoQueries.GetProjectLine(pUnitOfWork, projectNumber, exoLine);
                if (line != null)
                    return line;

                int? maxJOBCOSTLINEID = ExoQueries.GetJOBCODELINEID(pUnitOfWork);
                if (maxJOBCOSTLINEID != null)
                {
                    JOBCOST_LINES newLine = CreateNewLine(copyLine, exoLine, (int)maxJOBCOSTLINEID);
                    pUnitOfWork.JOBCOST_LINES.Add(newLine);
                    pUnitOfWork.SaveChanges();

                    return newLine;
                }
                else
                {
                    return null;
                }
            }
        }

        public static STOCK_ITEMS FindExistingOrAddStockItem(IPrimeroEntitiesUnitOfWork pUnitOfWork, string shortCode, string description, double? sellPrice, int? salesGLCode, int? purchGLCode, int? cosGLCode, double? stdCost, int costGroup, int costType)
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
                return stock_item;
            }
            else
            {
                STOCK_ITEMS newSTOCK_ITEM = createNewStockItem(shortCode, description, sellPrice, salesGLCode, purchGLCode, cosGLCode, stdCost, costGroup, costType);
                pUnitOfWork.STOCK_ITEMS.Add(newSTOCK_ITEM);
                return newSTOCK_ITEM;
            }
        }

        public static JOBCOST_RESOURCE FindExistingOrAddResource(IPrimeroEntitiesUnitOfWork pUnitOfWork, int? staffId, int? seqNo, string name, string title, string defaultStockCode, string shortCode)
        {
            string uppercaseName = name.ToUpper();
            string uppercaseTitle = title == null ? string.Empty : title.ToUpper();
            string uppercaseDefaultStockCode = defaultStockCode == null ? string.Empty : defaultStockCode.ToUpper();
            string uppercaseShortCode = shortCode == null ? string.Empty : shortCode.ToUpper();

            JOBCOST_RESOURCE resource = ExoQueries.FindJOBCOST_RESOURCE(pUnitOfWork, seqNo, uppercaseName);
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
                    JOBCOST_RESOURCE newJOBCOST_RESOURCE = createNewResource((int)staffId, uppercaseName, uppercaseTitle, uppercaseDefaultStockCode, uppercaseShortCode);
                    pUnitOfWork.JOBCOST_RESOURCE.Add(newJOBCOST_RESOURCE);
                    return newJOBCOST_RESOURCE;
                }
                else
                {
                    return null;
                }
            }
        }


        public static STAFF FindExistingOrAddStaff(IPrimeroEntitiesUnitOfWork pUnitOfWork, int? staffNo, string name, string title, int securityProfileId, int userProfileId, int? reportToStaffId)
        {
            string uppercaseName = name.ToUpper();
            string uppercaseTitle = title == null ? string.Empty : title.ToUpper();

            STAFF staff = ExoQueries.FindSTAFF(pUnitOfWork, staffNo, uppercaseName);
            if (staff != null)
            {
                staff.ISACTIVE = "Y";
                staff.NAME = uppercaseName;
                staff.JOBTITLE = uppercaseTitle;
                staff.SECURITYPROFILEID = securityProfileId;
                staff.USERPROFILEID = userProfileId;
                staff.REPORTS_TO_STAFFNO = reportToStaffId == null ? staff.STAFFNO : reportToStaffId;

                return staff;
            }
            else
            {
                STAFF newSTAFF = createNewStaff(uppercaseName, uppercaseTitle, securityProfileId, userProfileId, reportToStaffId);
                pUnitOfWork.STAFF.Add(newSTAFF);

                //need to save changes here to get new staff id;
                pUnitOfWork.SaveChanges();
                if (newSTAFF.REPORTS_TO_STAFFNO == null)
                {
                    newSTAFF.REPORTS_TO_STAFFNO = newSTAFF.STAFFNO;
                    pUnitOfWork.SaveChanges();
                }

                return newSTAFF;
            }
        }

        public static void RemoveStockItem(IPrimeroEntitiesUnitOfWork pUnitOfWork, IEnumerable<ExoResourceProjection> projections)
        {
            foreach (ExoResourceProjection projection in projections)
            {
                STOCK_ITEMS stockItem = ExoQueries.FindSTOCK_ITEM(pUnitOfWork, projection.DEFAULT_STOCKCODE);
                if (stockItem != null)
                    stockItem.ISACTIVE = "N";
            }
        }

        public static void RemoveResources(IPrimeroEntitiesUnitOfWork pUnitOfWork, IEnumerable<ExoResourceProjection> projections)
        {
            foreach(ExoResourceProjection projection in projections)
            {
                JOBCOST_RESOURCE resource = ExoQueries.FindJOBCOST_RESOURCE(pUnitOfWork, projection.RESOURCE_SEQNO, projection.RESOURCENAME);
                if (resource != null)
                    resource.ISACTIVE = "N";
            }
        }

        public static void RemoveStaff(IPrimeroEntitiesUnitOfWork pUnitOfWork, IEnumerable<ExoResourceProjection> projections)
        {
            foreach (ExoResourceProjection projection in projections)
            {
                STAFF staff = ExoQueries.FindSTAFF(pUnitOfWork, projection.STAFFNO, projection.RESOURCENAME);
                if (staff != null)
                    staff.ISACTIVE = "N";
            }
        }

        private static JOBCOST_RESOURCE createNewResource(int staffId, string name, string title, string defaultStockCode, string shortCode)
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
            Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
            string init = initials.Replace(name, "$1");
            newJOBCOST_RESOURCE.TITLE = title;
            newJOBCOST_RESOURCE.ISACTIVE = "Y";

            //use new unit of work to prevent concurrency issues
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            string generatedShortCode = ExoQueries.GetStaffShortcode(pUnitOfWork, init);
            newJOBCOST_RESOURCE.STAFFNO = staffId;
            newJOBCOST_RESOURCE.SHORTCODE = shortCode == string.Empty ? generatedShortCode : shortCode;
            newJOBCOST_RESOURCE.DEFAULT_STOCKCODE = defaultStockCode == string.Empty ? generatedShortCode : defaultStockCode;

            return newJOBCOST_RESOURCE;
        }

        private static STOCK_ITEMS createNewStockItem(string shortCode, string description, double? sellPrice, int? salesGLCode, int? purchGLCode, int? cosGLCode, double? stdCost, int costGroup, int costType)
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

            return newSTOCK_ITEM;
        }

        private static STAFF createNewStaff(string name, string title, int securityProfileId, int userProfileId, int? reportToStaffId)
        {
            //use new unit of work to prevent concurrency issues
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

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

            return newSTAFF;
        }

        public static JOBCOST_LINES CreateNewLine(JOBCOST_LINES copyLine, ExoSubJobEditableProjection projection, int maxJobLineId)
        {
            JOBCOST_LINES newLINE = new JOBCOST_LINES();
            newLINE.QUOTE_QTY = 1;
            newLINE.QUOTE_UNITPR = projection.Rate == 0 ? (Double?)null : Convert.ToDouble(projection.Rate);
            newLINE.ACTUAL_UNITCOST = projection.Budget == 0 ? (Double?)null : Convert.ToDouble(projection.Budget);
            newLINE.TRANSDATE = DateTime.Now.Date;
            newLINE.EXCHRATE = copyLine.EXCHRATE;
            newLINE.DISCOUNT = 0;
            newLINE.UNITPRICE_INCTAX = 0;
            newLINE.JOBNO = (int)projection.SubJobId;
            newLINE.STOCKCODE = projection.CommodityCode.ToUpper();
            newLINE.DESCRIPTION = projection.CommodityName;
            newLINE.SHOW_ON_INVOICE = copyLine.SHOW_ON_INVOICE;
            newLINE.COST_CENTRE = projection.CommodityId;
            newLINE.COST_CENTRE2 = projection.DisciplineId;
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
            newLINE.LINKED_STOCKCODE = projection.CommodityCode.ToUpper();
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

        public static JOB_COSTTYPES findExistingCommodity(string commodityCode, string commodityName, int defaultDisciplineId)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
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

        public static void updateSubJobTitle(string projectNumber, string jobCode)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
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
        public static bool findExistingOrAddResourceAllocation(ExoSubJobAuth existingPermission, int jobNo)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, existingPermission, jobNo, true);

            if (resourceAllocation != null)
            {
                if(resourceAllocation.END_DATE < DateTime.Now)
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
                int? resourceNo = ExoQueries.GetStaffResourceNo(pUnitOfWork, existingPermission.User.EXO_STAFF_ID);
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

        public static void deleteResourceAllocation(ExoSubJobAuth existingPermission, int jobNo)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, existingPermission, jobNo, false);

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
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork, Data.PROJECT PROJECT, IEnumerable<STAFF> ExoSTAFFS = null)
        {
            List<ExoTimeAuthorisation> exoLines = GetProjectLines(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER, false);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();
            foreach (ExoTimeAuthorisation exoLine in exoLines)
            {
                ExoSubJobProjection newSubJobProjection = ViewModelSource.Create(() => new ExoSubJobProjection());
                newSubJobProjection.LineId = exoLine.LineSeqNo;

                PrimeroSubJob newSubJob = new PrimeroSubJob();
                newSubJob.Id = exoLine.SubJobNo;
                newSubJob.MasterId = exoLine.MasterJobNo;
                newSubJob.Code = exoLine.SubJobCode;
                newSubJob.Title = exoLine.SubJobTitle;

                PrimeroDiscipline newDiscipline = new PrimeroDiscipline();
                newDiscipline.Id = exoLine.DisciplineId;
                newDiscipline.Code = exoLine.DisciplineCode;
                newDiscipline.Name = exoLine.DisciplineName;

                PrimeroCommodity newCommodity = new PrimeroCommodity();
                newCommodity.Id = exoLine.CommodityId;
                newCommodity.Code = exoLine.CommodityCode;
                newCommodity.Name = exoLine.CommodityName;

                newSubJobProjection.SubJob = newSubJob;
                newSubJobProjection.Discipline = newDiscipline;
                newSubJobProjection.Commodity = newCommodity;

                newSubJobProjection.Variation_Code = exoLine.VariationCode;
                newSubJobProjection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                IEnumerable<ExoTimeAuthorisation> exoAuths = exoAuthorisations.Where(x => x.SubJobCode == exoLine.SubJobCode && x.DisciplineCode == exoLine.DisciplineCode && x.CommodityCode == exoLine.CommodityCode);
                newSubJobProjection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                if(ExoSTAFFS != null)
                {
                    foreach (ExoTimeAuthorisation exoAuth in exoAuths)
                    {
                        STAFF findSTAFF = ExoSTAFFS.FirstOrDefault(x => x.STAFFNO == exoAuth.ResourceStaffId);
                        if (findSTAFF != null)
                        {
                            ExoSubJobAuth newAuth = new ExoSubJobAuth();
                            USER newUser = new USER();
                            newUser.NAME = findSTAFF.NAME;
                            newUser.EXO_STAFF_ID = findSTAFF.STAFFNO;
                            newAuth.User = newUser;
                            newAuth.ShouldAssign = false;
                            newAuth.IsAssigned = true;
                            newSubJobProjection.AuthUsers.Add(newAuth);
                        }
                    }
                }

                exoSubJobs.Add(newSubJobProjection);
            }

            return exoSubJobs.OrderBy(x => x.SubJob.Code).AsQueryable();
        }

        public static IQueryable<ExoSubJobEditableProjection> GetNativeExoSubJobEditableProjection(
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork, Data.PROJECT PROJECT, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, IEnumerable<STAFF> ExoSTAFFS = null)
        {
            List<ExoTimeAuthorisation> exoLines = GetProjectLines(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER, false);
            List<ExoSubJobEditableProjection> exoSubJobs = new List<ExoSubJobEditableProjection>();
            foreach (ExoTimeAuthorisation exoLine in exoLines)
            {
                ExoSubJobEditableProjection projection = ViewModelSource.Create(() => new ExoSubJobEditableProjection());
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
                projection.VariationCode = exoLine.VariationCode;
                projection.Budget = exoLine.BudgetCosts;
                projection.PopulateCommodityCodes(COMMODITY_CODECollection);
                projection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                IEnumerable<ExoTimeAuthorisation> exoAuths = exoAuthorisations.Where(x => x.SubJobCode == exoLine.SubJobCode && x.DisciplineCode == exoLine.DisciplineCode && x.CommodityCode == exoLine.CommodityCode && x.VariationCode == exoLine.VariationCode);
                projection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                if (exoLines.Count() > 0)
                {
                    projection.LineId = exoLine.LineSeqNo;
                    if (ExoSTAFFS != null)
                    {
                        foreach (ExoTimeAuthorisation exoAuth in exoAuths)
                        {
                            STAFF findSTAFF = ExoSTAFFS.FirstOrDefault(x => x.STAFFNO == exoAuth.ResourceStaffId);
                            if (findSTAFF != null)
                            {
                                ExoSubJobAuth newAuth = new ExoSubJobAuth();
                                USER newUser = new USER();
                                newUser.NAME = findSTAFF.NAME;
                                newUser.EXO_STAFF_ID = findSTAFF.STAFFNO;
                                newAuth.User = newUser;
                                newAuth.ShouldAssign = false;
                                newAuth.IsAssigned = true;
                                projection.AuthUsers.Add(newAuth);
                            }
                        }
                    }
                }

                exoSubJobs.Add(projection);
            }

            return exoSubJobs.OrderBy(x => x.SubJobCode).AsQueryable();
        }

        public static IQueryable<ExoSubJobEditableProjection> GetExoSubJobProjection(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<WORKPACK> WORKPACKS,
            Data.PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<VARIATION> VARIATIONS, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IEnumerable<USER> userCollection, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection)
        {
            List<BASELINE_ITEMProgress> baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS, VARIATIONS, false, null).ToList();

            var groupedDeliverables = baseline_item_progresses.GroupBy(x => new { SubJob = x.Entity.Entity.SUBJOB, DisciplineCode = x.Discipline_Code, Commodity = x.Entity.Entity.DOCTYPE })
                                      .Select(group => new { group.Key.SubJob, group.Key.DisciplineCode, group.Key.Commodity, TotalCosts = group.Sum(x => x.Total_Costs) });

            List<ExoTimeAuthorisation> exoLines = GetProjectLines(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER, false);
            List<ExoSubJobEditableProjection> exoSubJobs = new List<ExoSubJobEditableProjection>();

            foreach(var groupedDeliverable in groupedDeliverables)
            {
                if (groupedDeliverable.SubJob == null || groupedDeliverable.Commodity == null)
                    continue;

                ExoSubJobEditableProjection newSubJobProjection = ViewModelSource.Create(() => new ExoSubJobEditableProjection());
                ExoTimeAuthorisation exoSubJobLines = exoLines.FirstOrDefault(x => x.SubJobCode == groupedDeliverable.SubJob.INTERNAL_NAME1);
                if(exoSubJobLines != null)
                {
                    newSubJobProjection.SubJobId = exoSubJobLines.SubJobNo;
                    newSubJobProjection.SubJobCode = exoSubJobLines.SubJobCode;
                    newSubJobProjection.SubJobTitle = exoSubJobLines.SubJobTitle;
                }
                else
                {
                    JOBCOST_HDR findSubJob = GetProjectSubJob(primeroUnitOfWork, PROJECT.NUMBER, groupedDeliverable.SubJob.INTERNAL_NAME1);
                    if (findSubJob != null)
                        newSubJobProjection.SubJobId = findSubJob.JOBNO;

                    newSubJobProjection.SubJobCode = groupedDeliverable.SubJob.INTERNAL_NAME1;
                }

                newSubJobProjection.SubJobChargeType = groupedDeliverable.SubJob == null ? null : groupedDeliverable.SubJob.PHASE == null ? null : groupedDeliverable.SubJob.PHASE.CHARGE_TYPE;
                ExoTimeAuthorisation exoDisciplineLines = exoLines.FirstOrDefault(x => x.DisciplineCode == groupedDeliverable.DisciplineCode);
                if(exoDisciplineLines != null)
                {
                    newSubJobProjection.DisciplineId = exoDisciplineLines.DisciplineId;
                    newSubJobProjection.DisciplineCode = exoDisciplineLines.DisciplineCode;
                    newSubJobProjection.DisciplineName = exoDisciplineLines.DisciplineName;
                }
                else
                {
                    newSubJobProjection.DisciplineCode = groupedDeliverable.DisciplineCode;
                }

                ExoTimeAuthorisation exoCommodityLines = exoLines.FirstOrDefault(x => x.CommodityCode == groupedDeliverable.Commodity.CODE);
                if(exoCommodityLines != null)
                {
                    newSubJobProjection.CommodityId = exoCommodityLines.CommodityId;
                    newSubJobProjection.CommodityCode = exoCommodityLines.CommodityCode;
                    newSubJobProjection.CommodityName = groupedDeliverable.Commodity.NAME;
                }
                else
                {
                    newSubJobProjection.CommodityCode = groupedDeliverable.Commodity.CODE;
                }

                newSubJobProjection.Budget = groupedDeliverable.TotalCosts;
                newSubJobProjection.PopulateCommodityCodes(COMMODITY_CODECollection);
                newSubJobProjection.CommodityIsIndirectOnly = groupedDeliverable.Commodity == null ? false : groupedDeliverable.Commodity.IS_INDIRECT_ONLY;
                newSubJobProjection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                ExoTimeAuthorisation exoLine = exoLines.FirstOrDefault(x => x.SubJobCode == groupedDeliverable.SubJob.INTERNAL_NAME1 && x.DisciplineCode == groupedDeliverable.DisciplineCode && x.CommodityCode == groupedDeliverable.Commodity.CODE && (x.VariationCode == string.Empty || x.VariationCode == null));
                if(exoLine != null)
                {
                    newSubJobProjection.LineId = exoLine.LineSeqNo;
                    IEnumerable<ExoTimeAuthorisation> exoUserAuths = exoAuthorisations.Where(x => x.SubJobCode == groupedDeliverable.SubJob.INTERNAL_NAME1 && x.DisciplineCode == groupedDeliverable.DisciplineCode && x.CommodityCode == groupedDeliverable.Commodity.CODE);
                    if (exoUserAuths.Count() > 0)
                    {
                        foreach (ExoTimeAuthorisation exoUserAuth in exoUserAuths)
                        {
                            USER findUSER = userCollection.FirstOrDefault(x => x.EXO_STAFF_ID == exoUserAuth.ResourceStaffId);
                            if (findUSER != null && findUSER.ROLE != null)
                            {
                                ExoSubJobAuth newAuth = new ExoSubJobAuth();
                                newAuth.User = findUSER;
                                newAuth.ShouldAssign = findUSER.ROLE.ROLE_COMMODITY.Any(x => x.DOCTYPE.CODE == exoUserAuth.CommodityCode);
                                newAuth.IsAssigned = true;
                                newSubJobProjection.AuthUsers.Add(newAuth);
                            }
                        }
                    }
                }

                exoSubJobs.Add(newSubJobProjection);
            }

            return exoSubJobs.OrderBy(x => x.SubJobCode).AsQueryable();
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

        public static JOB_RESOURCE_ALLOCATION GetResourceAllocation(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, ExoSubJobAuth existingAuth, int jobNo, bool includeDisabled)
        {
            if (existingAuth.User == null || existingAuth.User.EXO_STAFF_ID == null)
                return null;

            DateTime disabledDateTime;
            if (includeDisabled)
                disabledDateTime = BluePrintsConstants.DefaultStartTime;
            else
                disabledDateTime = DateTime.Now;

            int staffId = (int)existingAuth.User.EXO_STAFF_ID;
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

        public static string GetStaffShortcode(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string partialShortCode)
        {
            string formatPartialShortCode = partialShortCode.Length > 2 ? partialShortCode.Substring(0, 2) : partialShortCode;
            var resources = from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                            where JOBCOST_RESOURCE.SHORTCODE.StartsWith(formatPartialShortCode)
                            select JOBCOST_RESOURCE;

            List<JOBCOST_RESOURCE> allResources = resources.ToList();
            int largestNumber = 0;
            foreach(var resource in allResources)
            {
                string resultString = Regex.Match(resource.SHORTCODE, @"\d+").Value;
                if(resultString != string.Empty)
                {
                    int affixValue = Int32.Parse(resultString);
                    affixValue += 1;
                    if (affixValue > largestNumber)
                        largestNumber = affixValue;
                }
            }

            return string.Concat(formatPartialShortCode, largestNumber.ToString());
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

        public static JOB_COSTGROUPS GetDiscipline(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string disciplineCode)
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
            IQueryable<JOBCOST_RESOURCE> resources;
            
            if(seqNo == null)
                resources = (from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                              where JOBCOST_RESOURCE.RESOURCENAME == name
                              select JOBCOST_RESOURCE);
            else
                resources = (from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                             where JOBCOST_RESOURCE.SEQNO == seqNo
                             select JOBCOST_RESOURCE);

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

        public static STAFF FindSTAFF(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int? staffNo, string name)
        {
            IQueryable<STAFF> staffs;
            if(staffNo == null)
                staffs = (from STAFF in primeroUnitOfWork.STAFF
                                                      where STAFF.NAME == name
                                                      select STAFF);
            else
                staffs = (from STAFF in primeroUnitOfWork.STAFF
                          where STAFF.STAFFNO == staffNo
                          select STAFF);

            if (staffs.Count() > 0)
                return staffs.First();

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

        public static JOBCOST_LINES GetProjectLine(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, ExoSubJobEditableProjection line)
        {
            if (line.SubJobCode == null || line.SubJobCode == string.Empty || line.DisciplineCode == null || line.DisciplineCode == string.Empty || line.CommodityCode == null || line.CommodityCode == string.Empty)
                return null;

            IQueryable<JOBCOST_LINES> projectLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                                 on JOBCOST_LINES.COST_CENTRE2 equals JOB_COSTGROUPS.SEQNO
                                 join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                                 on JOBCOST_LINES.COST_CENTRE equals JOB_COSTTYPES.SEQNO
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber && SUBJOB.JOBCODE == line.SubJobCode.ToUpper() && JOB_COSTGROUPS.SHORTCODE == line.DisciplineCode.ToUpper() && JOB_COSTTYPES.SHORTCODE == line.CommodityCode.ToUpper()
                                 select JOBCOST_LINES;

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
                                 where MAINJOB.JOBCODE == projectNumber
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, STOCK_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.COSTDESC, VARIATION_CODE = JOBCOST_LINES.X_VARIATION_CODE, BUDGETED_QTY = JOBCOST_LINES.QUOTE_QTY, BUDGETED_REV = JOBCOST_LINES.LINETOTAL, BUDGETED_RATE = JOBCOST_LINES.ACTUAL_UNITCOST, FORECAST_RATE = JOBCOST_LINES.QUOTE_UNITPR };

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

        public static JOBCOST_LINES GetProjectRevenue(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber && JOBCOST_LINES.STOCKCODE == BluePrintsResources.Default_Revenue_StockCode && (JOBCOST_LINES.X_VARIATION_CODE == string.Empty || JOBCOST_LINES.X_VARIATION_CODE == null)
                                 select JOBCOST_LINES;

            if (availableLines.Count() > 0)
                return availableLines.First();
            else
                return null;
        }

        public static dynamic GetProjectVariationRevenue(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                 join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber && JOBCOST_LINES.STOCKCODE == BluePrintsResources.Default_Revenue_StockCode && JOBCOST_LINES.X_VARIATION_CODE != string.Empty && JOBCOST_LINES.X_VARIATION_CODE != null
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

        public static List<JOBCOST_HDR> GetJOBCOST_HDR(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
        {
            var jobLines = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                           join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                           on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                           where MAINJOB.JOBCODE == projectNumber
                           select SUBJOB;

            return jobLines.ToList();
        }

        public static List<STOCK_ITEMS> GetSTOCK_ITEMS(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            return primeroUnitOfWork.STOCK_ITEMS.ToList();
        }

        public static List<JOB_COSTGROUPS> GetJOB_COSTGROUPS(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            return primeroUnitOfWork.JOB_COSTGROUPS.ToList();
        }

        public static List<JOB_COSTTYPES> GetJOB_COSTTYPES(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            return primeroUnitOfWork.JOB_COSTTYPES.ToList();
        }

        public static List<ExoTimeAuthorisation> GetExoLinesAuthorisations(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, bool byUser = true, bool showDisabledUsers = false)
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
                                 join JOB_RESOURCE_ALLOCATION in primeroUnitOfWork.JOB_RESOURCE_ALLOCATION
                                 on JOBCOST_LINES.JOBNO equals JOB_RESOURCE_ALLOCATION.JOBNO
                                 join JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                 on JOB_RESOURCE_ALLOCATION.RESOURCE_SEQNO equals JOBCOST_RESOURCE.SEQNO
                                 join STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                                 on JOBCOST_RESOURCE.DEFAULT_STOCKCODE equals STOCK_ITEMS.STOCKCODE
                                 where MAINJOB.JOBCODE == projectNumber
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.COSTDESC, RESOURCE_SEQNO = JOBCOST_RESOURCE.SEQNO, RESOURCE_STAFF_ID = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.DEFAULT_STOCKCODE, STOCK_CODE_DESC = STOCK_ITEMS.DESCRIPTION, END_DATE = JOB_RESOURCE_ALLOCATION.END_DATE, VARIATIONCODE = JOBCOST_LINES.X_VARIATION_CODE };

            List<ExoTimeAuthorisation> exoTimes;
            if (byUser)
                exoTimes = availableLines.Where(x => x.RESOURCE_STAFF_ID == LoginCredentials.CurrentUser.EXO_STAFF_ID).ToList().Select(x => populateExoTimeAuth(x)).ToList();
            else
                exoTimes = availableLines.ToList().Select(x => populateExoTimeAuth(x)).ToList();

            if (!showDisabledUsers)
                exoTimes = exoTimes.Where(x => x.ResourceEndDate >= DateTime.Now).ToList();

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

        public static List<ExoSubJobProjection> GetMasterExoLines(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            var availableLines = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 where SUBJOB.JOBNO == SUBJOB.MASTER_JOBNO
                                 select new { SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE };

            List<ExoSubJobProjection> exoTimes = availableLines.ToList().Select(x => new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Id = x.SUBJOBNO, Code = x.SUBJOBNAME, Title = x.SUBJOBTITLE } }).ToList();
            return exoTimes;
        }

        public static IQueryable<ExoResourceProjection> GetResources(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            var resources = from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                     join STAFF in primeroUnitOfWork.STAFF
                                     on JOBCOST_RESOURCE.STAFFNO equals STAFF.STAFFNO
                                     join STOCK_ITEMS in primeroUnitOfWork.STOCK_ITEMS
                                     on JOBCOST_RESOURCE.DEFAULT_STOCKCODE equals STOCK_ITEMS.STOCKCODE
                                     where JOBCOST_RESOURCE.ISACTIVE == "Y"
                            select new { JOBCOST_RESOURCE.SEQNO, STAFF.STAFFNO, JOBCOST_STAFFNO = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.TITLE, JOBCOST_RESOURCE.DEFAULT_STOCKCODE, JOBCOST_RESOURCE.SHORTCODE, STAFF.SECURITYPROFILEID, STAFF.USERPROFILEID, STAFF.REPORTS_TO_STAFFNO, STOCK_ITEMS.SELLPRICE1, STOCK_ITEMS.STDCOST, STOCK_ITEMS.SALES_GL_CODE, STOCK_ITEMS.PURCH_GL_CODE, STOCK_ITEMS.COS_GL_CODE, STOCK_ITEMS.COSTTYPE, STOCK_ITEMS.COSTGROUP };

            //EntityKey is used to prevent duplicate error message
            return resources.ToList().Select(x => ViewModelSource.Create(() => new ExoResourceProjection() { GUID = Guid.NewGuid(), STAFFNO = x.STAFFNO, RESOURCE_SEQNO = x.SEQNO, RESOURCENAME = x.RESOURCENAME, TITLE = x.TITLE, DEFAULT_STOCKCODE = x.DEFAULT_STOCKCODE, SECURITYPROFILEID = x.SECURITYPROFILEID, USERPROFILEID = x.USERPROFILEID, REPORTS_TO_STAFFNO = x.REPORTS_TO_STAFFNO, SHORTCODE = x.SHORTCODE, RESOURCE_STAFFNO = x.JOBCOST_STAFFNO, IsNewRow = false, STDCOST = x.STDCOST, SELLPRICE1 = x.SELLPRICE1, SALES_GL_CODE = x.SALES_GL_CODE, PURCH_GL_CODE = x.PURCH_GL_CODE, COS_GL_CODE = x.COS_GL_CODE, COSTGROUP = x.COSTGROUP, COSTTYPE = x.COSTTYPE })).AsQueryable();
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
            exoTime.SubJobNo = dbTime.SUBJOBNO;
            exoTime.SubJobCode = dbTime.SUBJOBNAME;
            exoTime.SubJobTitle = dbTime.SUBJOBTITLE;
            exoTime.DisciplineId = dbTime.DISCIPLINE_ID;
            exoTime.DisciplineCode = dbTime.DISCIPLINE_CODE;
            exoTime.DisciplineName = dbTime.DISCIPLINE_NAME;
            exoTime.CommodityId = dbTime.COMMODITY_ID;
            exoTime.CommodityCode = dbTime.COMMODITY_CODE;
            exoTime.CommodityName = dbTime.COMMODITY_NAME;
            exoTime.VariationCode = dbTime.VARIATION_CODE;
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
            exoTime.StockCode = dbTime.DEFAULT_STOCKCODE;
            exoTime.StockCodeDescription = dbTime.STOCK_CODE_DESC;
            exoTime.VariationCode = dbTime.VARIATIONCODE;
            return exoTime;
        }
    }

    public class ExoTimeAuthorisation
    {
        public int LineSeqNo { get; set; }
        public int MasterJobNo { get; set; }
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
        public string StockCodeDescription { get; set; }
        public string VariationCode { get; set; }
        public decimal BudgetQty { get; set; }
        public decimal BudgetRev { get; set; }
        public decimal BudgetRate { get; set; }
        public decimal ForecastRate { get; set; }
        public decimal BudgetCosts => BudgetQty * BudgetRate;

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
    }

    public class TimesheetDate
    {
        public DateTime WeekStartDate { get; set; }
        public int DayNumber { get; set; }
    }
}
