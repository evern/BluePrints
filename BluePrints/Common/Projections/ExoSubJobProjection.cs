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

    public static class ExoQueries
    {
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

            return exoSubJobs.AsQueryable();
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
    }

    public class PrimeroDiscipline
    {
        public int? Id { get; set; }
        public int? SubjobId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class PrimeroCommodity
    {
        public int? Id { get; set; }
        public int? DisciplineId { get; set; }
        public int? ResourceId { get; set; }
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
