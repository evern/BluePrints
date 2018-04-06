using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

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

    public class ExoSubJobProjection : IGuidEntityKey, ICanAssignP6
    {
        public ExoSubJobProjection()
        {

        }

        public int? LineId { get; set; }
        public PrimeroSubJob SubJob { get; set; }
        public PrimeroDiscipline Discipline { get; set; }
        public PrimeroCommodity Commodity { get; set; }
        public ObservableCollection<ExoSubJobAuth> AuthUsers { get; set; }

        public bool IsSubJobExistsInExo => SubJob != null && SubJob.Id != null;
        public bool IsDisciplineExistsInExo => Discipline != null && Discipline.Id != null;
        public bool IsCommodityExistsInExo => Commodity != null && Commodity.Id != null;
        public bool IsLineExistsInExo => LineId != null;

        //used to trick view model
        public Guid EntityKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public void SetOriginalEntityKey(Guid newGuid)
        {

        }

        public void Update()
        {
            this.RaisePropertiesChanged();
        }
    }

    public static class ExoMethods
    {
        public static int? findExistingOrAddLine(ExoSubJobProjection exoLine, JOBCOST_LINES masterLine, string projectNumber)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            if (exoLine.SubJob.Id == null || exoLine.Discipline.Id == null || exoLine.Commodity.Id == null)
                return null;
            else
            {
                int? maxJOBCOSTLINEID = ExoQueries.GetJOBCODELINEID(pUnitOfWork);
                JOBCOST_LINES line = ExoQueries.GetProjectLine(pUnitOfWork, projectNumber, exoLine);
                if (line != null)
                    return line.SEQNO;
                else if (maxJOBCOSTLINEID != null)
                {
                    JOBCOST_LINES newLINE = new JOBCOST_LINES();
                    newLINE.QUOTE_QTY = masterLine.QUOTE_QTY;
                    newLINE.QUOTE_UNITPR = 0;
                    newLINE.ACTUAL_UNITCOST = 0;
                    newLINE.TRANSDATE = DateTime.Now.Date;
                    newLINE.EXCHRATE = masterLine.EXCHRATE;
                    newLINE.DISCOUNT = 0;
                    newLINE.UNITPRICE_INCTAX = 0;
                    newLINE.JOBNO = (int)exoLine.SubJob.Id;
                    newLINE.STOCKCODE = exoLine.Commodity.Code.ToUpper();
                    newLINE.DESCRIPTION = exoLine.Commodity.Name;
                    newLINE.SHOW_ON_INVOICE = masterLine.SHOW_ON_INVOICE;
                    newLINE.COST_CENTRE = exoLine.Commodity.Id;
                    newLINE.COST_CENTRE2 = exoLine.Discipline.Id;
                    newLINE.NARRATIVE = "N";
                    newLINE.LINE_STATUS = "Q";
                    newLINE.TAXNO = masterLine.TAXNO;
                    newLINE.BRANCHNO = 0;
                    newLINE.SUBCODE = 0;
                    newLINE.ANALYSIS = 0;
                    newLINE.CURRENCYNO = 0;
                    newLINE.ALINENO = 100;
                    newLINE.GLCODE = 0;
                    newLINE.MASTER_JOBNO = masterLine.JOBNO;
                    newLINE.COPY_FROM_QUOTE = "N";
                    newLINE.DIM_LENGTH = 1;
                    newLINE.DIM_WIDTH = 1;
                    newLINE.DIM_DEPTH = 1;
                    newLINE.TOTAL_QUANTITY = 1;
                    newLINE.LINETYPE = 0;
                    newLINE.KITSEQNO = -1;
                    newLINE.KITCODE = string.Empty;
                    newLINE.PRICE_OVERRIDDEN = "N";
                    newLINE.LINKED_STOCKCODE = exoLine.Commodity.Code.ToUpper();
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
                    newLINE.TAXRATE = masterLine.TAXRATE;
                    newLINE.LINETOTAL_TAX = 0;
                    newLINE.LINETOTAL_INCTAX = 0;
                    newLINE.LINE_TAX = 0;
                    newLINE.HIDDEN_LINETOTAL = 0;
                    newLINE.SCHEDULE_SEQNO = 0;
                    newLINE.JOBCOSTLINEID = ((int)maxJOBCOSTLINEID) + 1;
                    newLINE.SNTYPE = 0;
                    newLINE.SNEXPDAYS = -2;
                    newLINE.OPPLINEID = -1;
                    newLINE.COST_LINENO = -1;
                    newLINE.X_VARIATION_CODE = string.Empty;
                    pUnitOfWork.JOBCOST_LINES.Add(newLINE);
                    pUnitOfWork.SaveChanges();

                    return newLINE.SEQNO;
                }
                else
                {
                    return null;
                }
            }
        }

        public static int? findExistingOrAddCommodity(string commodityCode, string commodityName, int defaultDisciplineId)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_COSTTYPES costTypes = ExoQueries.GetCommodity(pUnitOfWork, commodityCode);

            if (costTypes != null)
                return costTypes.SEQNO;
            else
            {
                JOB_COSTTYPES newCOSTTYPE = new JOB_COSTTYPES();
                newCOSTTYPE.DEF_MARKUP = 0;
                newCOSTTYPE.DEF_OVERHEAD = 0;
                newCOSTTYPE.COSTDESC = commodityCode.ToUpper() + " - " + commodityName.ToUpper();
                newCOSTTYPE.GLCODE = -1;
                newCOSTTYPE.GLSUBCODE = 0;
                newCOSTTYPE.SHOWONQUOTE = "F";
                newCOSTTYPE.SHORTCODE = commodityCode.ToUpper();
                newCOSTTYPE.DEF_COSTGROUP = defaultDisciplineId;
                newCOSTTYPE.DEF_PURCH_GLCODE = -1;
                newCOSTTYPE.DEF_PURCH_GLSUBCODE = 0;
                newCOSTTYPE.CONSOLIDATE = "F";
                newCOSTTYPE.COPY_FROM_QUOTE = "N";
                pUnitOfWork.JOB_COSTTYPES.Add(newCOSTTYPE);
                pUnitOfWork.SaveChanges();
                return newCOSTTYPE.SEQNO;
            }
        }

        public static int? findExistingOrAddDiscipline(string disciplineCode)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_COSTGROUPS costGroups = ExoQueries.GetDiscipline(pUnitOfWork, disciplineCode);

            if (costGroups != null)
                return costGroups.SEQNO;
            else
            {
                JOB_COSTGROUPS newCOSTGROUP = new JOB_COSTGROUPS();
                newCOSTGROUP.DEF_MARKUP = 0;
                newCOSTGROUP.DEF_OVERHEAD = 0;
                newCOSTGROUP.COSTDESC = disciplineCode.ToUpper();
                newCOSTGROUP.SHORTCODE = disciplineCode.ToUpper();
                newCOSTGROUP.SHOWONQUOTE = "F";
                newCOSTGROUP.CONSOLIDATE = "F";
                newCOSTGROUP.COPY_FROM_QUOTE = "N";
                pUnitOfWork.JOB_COSTGROUPS.Add(newCOSTGROUP);
                pUnitOfWork.SaveChanges();
                return newCOSTGROUP.SEQNO;
            }
        }

        public static int? findExistingOrAddSubJob(string jobCode, JOBCOST_HDR masterJob, string projectNumber)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOBCOST_HDR existingSubJobs = ExoQueries.GetProjectSubJob(pUnitOfWork, projectNumber, jobCode);
            if (existingSubJobs != null)
                return (int)existingSubJobs.JOBNO;
            else
            {
                if (masterJob != null)
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
                    newExoSubJob.STATUS = "C";
                    newExoSubJob.TITLE = string.Empty;
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
                    newExoSubJob.BRANCHNO = 0;
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
                    pUnitOfWork.JOBCOST_HDR.Add(newExoSubJob);
                    pUnitOfWork.SaveChanges();
                    return newExoSubJob.JOBNO;
                }
                else
                    return null;
            }
        }

        /// <returns>Whether new record is added</returns>
        public static bool findExistingOrAddResourceAllocation(ExoSubJobAuth existingPermission, int jobNo)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, existingPermission, jobNo);

            if (resourceAllocation != null)
                return false;
            else
            {
                int? resourceNo = ExoQueries.GetStaffResourceNo(pUnitOfWork, existingPermission.User.EXO_STAFF_ID);
                if (resourceNo != null)
                {
                    JOB_RESOURCE_ALLOCATION newAllocation = new JOB_RESOURCE_ALLOCATION();
                    newAllocation.RESOURCE_SEQNO = (int)resourceNo;
                    newAllocation.JOBNO = jobNo;

                    int year = DateTime.Now.Year;
                    DateTime firstDay = new DateTime(year, 1, 1);
                    DateTime startTime = new DateTime(1899, 12, 30, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    DateTime lastDay = new DateTime(2099, 1, 1);

                    newAllocation.START_DATE = firstDay;
                    newAllocation.END_DATE = lastDay;
                    newAllocation.START_TIME = startTime;
                    newAllocation.END_TIME = startTime;
                    newAllocation.TOTAL_HOURS = 999999;
                    newAllocation.APPOINTMENT_SCHEDULED = "N";
                    pUnitOfWork.JOB_RESOURCE_ALLOCATION.Add(newAllocation);
                    pUnitOfWork.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        public static void deleteResourceAllocation(ExoSubJobAuth existingPermission, int jobNo)
        {
            var pUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            JOB_RESOURCE_ALLOCATION resourceAllocation = ExoQueries.GetResourceAllocation(pUnitOfWork, existingPermission, jobNo);

            if (resourceAllocation != null)
            {
                pUnitOfWork.JOB_RESOURCE_ALLOCATION.Remove(resourceAllocation);
                pUnitOfWork.SaveChanges();
            }
        }
    }

    public static class ExoQueries
    {
        public static IQueryable<ExoSubJobProjection> GetNativeExoSubJobProjection(
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork, Data.PROJECT PROJECT, IEnumerable<STAFF> ExoSTAFFS)
        {
            List<ExoTimeAuthorisation> exoLines = GetAllExoLines(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER, false);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();
            foreach (ExoTimeAuthorisation exoLine in exoLines)
            {
                ExoSubJobProjection newSubJobProjection = ViewModelSource.Create(() => new ExoSubJobProjection());
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

                newSubJobProjection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                IEnumerable<ExoTimeAuthorisation> exoAuths = exoAuthorisations.Where(x => x.SubJobCode == exoLine.SubJobCode && x.DisciplineCode == exoLine.DisciplineCode && x.CommodityCode == exoLine.CommodityCode);
                newSubJobProjection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                if (exoLines.Count() > 0)
                {
                    newSubJobProjection.LineId = exoLines.First().LineSeqNo;
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

        public static IQueryable<ExoSubJobProjection> GetExoSubJobProjection(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<WORKPACK> WORKPACKS,
            Data.PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, IEnumerable<USER> userCollection)
        {
            List<BASELINE_ITEMProgress> baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS, null, true, null).ToList();

            var groupedDeliverables = baseline_item_progresses.GroupBy(x => new { SubJob = x.Entity.Entity.SUBJOB, DisciplineCode = x.Discipline_Code, DisciplineName = x.Entity.Entity.Discipline_Name, Commodity = x.Entity.Entity.DOCTYPE })
                                      .Select(group => new { group.Key.SubJob, group.Key.DisciplineCode, group.Key.DisciplineName, group.Key.Commodity });

            List<ExoTimeAuthorisation> exoAuthorisations = GetExoLinesAuthorisations(primeroUnitOfWork, PROJECT.NUMBER, false);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();
            foreach(var groupedDeliverable in groupedDeliverables)
            {
                if (groupedDeliverable.SubJob == null || groupedDeliverable.Commodity == null)
                    continue;

                ExoSubJobProjection newSubJobProjection = ViewModelSource.Create(() => new ExoSubJobProjection());
                ExoTimeAuthorisation exoSubJobAuthorisation = exoAuthorisations.FirstOrDefault(x => x.SubJobCode == groupedDeliverable.SubJob.INTERNAL_NAME1);
                PrimeroSubJob newSubJob = new PrimeroSubJob();
                if(exoSubJobAuthorisation != null)
                {
                    newSubJob.Id = exoSubJobAuthorisation.SubJobNo;
                    newSubJob.MasterId = exoSubJobAuthorisation.MasterJobNo;
                    newSubJob.Code = exoSubJobAuthorisation.SubJobCode;
                    newSubJob.Title = exoSubJobAuthorisation.SubJobTitle;
                }
                else
                {
                    newSubJob.Code = groupedDeliverable.SubJob.INTERNAL_NAME1;
                    if(exoAuthorisations.Count > 0)
                        newSubJob.MasterId = exoAuthorisations.First().MasterJobNo;
                }

                ExoTimeAuthorisation exoDisciplineAuthorisation = exoAuthorisations.FirstOrDefault(x => x.DisciplineCode == groupedDeliverable.DisciplineCode);
                PrimeroDiscipline newDiscipline = new PrimeroDiscipline();
                if(exoDisciplineAuthorisation != null)
                {
                    newDiscipline.Id = exoDisciplineAuthorisation.DisciplineId;
                    newDiscipline.Code = exoDisciplineAuthorisation.DisciplineCode;
                    newDiscipline.Name = exoDisciplineAuthorisation.DisciplineName;
                }
                else
                {
                    newDiscipline.Code = groupedDeliverable.DisciplineCode;
                    newDiscipline.Name = groupedDeliverable.DisciplineName;
                }

                ExoTimeAuthorisation exoCommodityAuthorisation = exoAuthorisations.FirstOrDefault(x => x.CommodityCode == groupedDeliverable.Commodity.CODE);
                PrimeroCommodity newCommodity = new PrimeroCommodity();
                if(exoCommodityAuthorisation != null)
                {
                    newCommodity.Id = exoCommodityAuthorisation.CommodityId;
                    newCommodity.Code = exoCommodityAuthorisation.CommodityCode;
                    //newCommodity.Name = exoCommodityAuthorisation.CommodityName;
                    //use system name instead
                    newCommodity.Name = groupedDeliverable.Commodity.NAME;
                }
                else
                {
                    newCommodity.Code = groupedDeliverable.Commodity.CODE;
                    newCommodity.Name = groupedDeliverable.Commodity.NAME;
                }

                newSubJobProjection.SubJob = newSubJob;
                newSubJobProjection.Discipline = newDiscipline;
                newSubJobProjection.Commodity = newCommodity;

                IEnumerable<ExoTimeAuthorisation> exoLines = exoAuthorisations.Where(x => x.SubJobCode == groupedDeliverable.SubJob.INTERNAL_NAME1 && x.DisciplineCode == groupedDeliverable.DisciplineCode && x.CommodityCode == groupedDeliverable.Commodity.CODE);
                newSubJobProjection.AuthUsers = new ObservableCollection<ExoSubJobAuth>();
                if (exoLines.Count() > 0)
                {
                    newSubJobProjection.LineId = exoLines.First().LineSeqNo;
                    foreach (ExoTimeAuthorisation exoLine in exoLines)
                    {
                        USER findUSER = userCollection.FirstOrDefault(x => x.EXO_STAFF_ID == exoLine.ResourceStaffId);
                        if(findUSER != null)
                        {
                            ExoSubJobAuth newAuth = new ExoSubJobAuth();
                            newAuth.User = findUSER;
                            newAuth.ShouldAssign = findUSER.ROLE.ROLE_COMMODITY.Any(x => x.DOCTYPE.CODE == exoLine.CommodityCode);
                            newAuth.IsAssigned = true;
                            newSubJobProjection.AuthUsers.Add(newAuth);
                        }
                    }
                }

                exoSubJobs.Add(newSubJobProjection);
            }

            return exoSubJobs.OrderBy(x => x.SubJob.Code).AsQueryable();
        }

        public static JOBCOST_HDR GetProjectSubJob(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, string subJobCode)
        {
            var subJobs = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == projectNumber && SUBJOB.JOBCODE == subJobCode
                                 select SUBJOB;

            if (subJobs.Count() == 0)
                return null;

            return subJobs.First();
        }

        public static JOB_RESOURCE_ALLOCATION GetResourceAllocation(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, ExoSubJobAuth existingAuth, int jobNo)
        {
            if (existingAuth.User == null || existingAuth.User.EXO_STAFF_ID == null)
                return null;

            int staffId = (int)existingAuth.User.EXO_STAFF_ID;
            var resourceAllocation = from JOB_RESOURCE_ALLOCATION in primeroUnitOfWork.JOB_RESOURCE_ALLOCATION
                                     join JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                     on JOB_RESOURCE_ALLOCATION.RESOURCE_SEQNO equals JOBCOST_RESOURCE.SEQNO
                                     join STAFF in primeroUnitOfWork.STAFF
                                     on JOBCOST_RESOURCE.STAFFNO equals STAFF.STAFFNO
                                     where STAFF.STAFFNO == staffId && JOB_RESOURCE_ALLOCATION.JOBNO == jobNo
                                     select JOB_RESOURCE_ALLOCATION;

            if (resourceAllocation.Count() == 0)
                return null;

            return resourceAllocation.First();
        }

        public static int? GetStaffResourceNo(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int? staffId)
        {
            if (staffId == null)
                return null;

            var resourceAllocation = from JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                     join STAFF in primeroUnitOfWork.STAFF
                                     on JOBCOST_RESOURCE.STAFFNO equals STAFF.STAFFNO
                                     where STAFF.STAFFNO == staffId
                                     select JOBCOST_RESOURCE;

            if (resourceAllocation.Count() == 0)
                return null;

            return resourceAllocation.First().SEQNO;
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

        public static int? GetJOBCODELINEID(IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            int? maxLineId = (from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                  select JOBCOST_LINES.JOBCOSTLINEID).Max();

            return maxLineId;
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

        public static JOBCOST_LINES GetProjectLine(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, ExoSubJobProjection line)
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
                                 where MAINJOB.JOBCODE == projectNumber && SUBJOB.JOBCODE == line.SubJob.Code.ToUpper() && JOB_COSTGROUPS.SHORTCODE == line.Discipline.Code.ToUpper() && JOB_COSTTYPES.SHORTCODE == line.Commodity.Code.ToUpper()
                                 select JOBCOST_LINES;


            if (availableLines.Count() == 0)
                return null;

            return availableLines.First();
        }

        public static JOBCOST_LINES GetProjectLineByCode(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
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
                                 where MAINJOB.JOBCODE == projectNumber && SUBJOB.JOBCODE == projectNumber
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
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.COSTDESC };


            List<ExoTimeAuthorisation> exoTimes = availableLines.ToList().Select(x => populateExoTime(x)).ToList();
            return exoTimes;
        }

        public static List<ExoTimeAuthorisation> GetExoLinesAuthorisations(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber, bool byUser = true)
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
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.COSTDESC, RESOURCE_SEQNO = JOBCOST_RESOURCE.SEQNO, RESOURCE_STAFF_ID = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.DEFAULT_STOCKCODE, STOCK_CODE_DESC = STOCK_ITEMS.DESCRIPTION };


            List<ExoTimeAuthorisation> exoTimes;
            if(byUser)
                exoTimes = availableLines.Where(x => x.RESOURCE_STAFF_ID == LoginCredentials.CurrentUser.EXO_STAFF_ID).ToList().Select(x => populateExoTimeAuth(x)).ToList();
            else
                exoTimes = availableLines.ToList().Select(x => populateExoTimeAuth(x)).ToList();

            return exoTimes;
        }


        public static List<ExoTimeAuthorisation> GetAllExoLines(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
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
                                 where MAINJOB.JOBCODE == projectNumber && !SUBJOB.JOBCODE.Contains("D1") && !SUBJOB.JOBCODE.Contains("I1")
                                 select new { LINEID = JOBCOST_LINES.SEQNO, MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.COSTDESC };


            List<ExoTimeAuthorisation> exoTimes = availableLines.ToList().Select(x => populateExoLine(x)).ToList();
            return exoTimes;
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
            exoTime.StockCode = dbTime.DEFAULT_STOCKCODE;
            exoTime.StockCodeDescription = dbTime.STOCK_CODE_DESC;
            return exoTime;
        }

        private static ExoTimeAuthorisation populateExoTime(dynamic dbTime)
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
        public string StockCode { get; set; }
        public string StockCodeDescription { get; set; }
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
        public string StockCode { get; set; }
        public string StockDescription { get; set; }
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
