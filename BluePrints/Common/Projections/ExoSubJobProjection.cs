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

namespace BluePrints.Common.Projections
{
    public class ExoSubJobProjection : IGuidEntityKey, ICanAssignP6
    {
        public ExoSubJobProjection()
        {

        }

        public PrimeroSubJob SubJob { get; set; }
        public PrimeroDiscipline Discipline { get; set; }
        public PrimeroCommodity Commodity { get; set; }

        public bool IsSubJobExistsInExo => SubJob != null && SubJob.Id != null;
        public bool IsDisciplineExistsInExo => Discipline != null && Discipline.Id != null;
        public bool IsCommodityExistsInExo => Commodity != null && Commodity.Id != null;
        public bool IsLineExistsInExo { get; set; }

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
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            List<BASELINE_ITEMProgress> baseline_item_progresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS, null, true, null).ToList();

            var groupedDeliverables = baseline_item_progresses.GroupBy(x => new { SubJob = x.Entity.Entity.SUBJOB, DisciplineCode = x.Discipline_Code, DisciplineName = x.Entity.Entity.Discipline_Name, Commodity = x.Entity.Entity.DOCTYPE })
                                      .Select(group => new { group.Key.SubJob, group.Key.DisciplineCode, group.Key.DisciplineName, group.Key.Commodity });

            List<ExoTimeAuthorisation> exoAuthorisations = GetExoTimeAuthorisation(primeroUnitOfWork, PROJECT.NUMBER);
            List<ExoSubJobProjection> exoSubJobs = new List<ExoSubJobProjection>();
            foreach(var groupedDeliverable in groupedDeliverables)
            {
                if (groupedDeliverable.SubJob == null || groupedDeliverable.Commodity == null)
                    continue;

                ExoSubJobProjection newSubJobProjection = new ExoSubJobProjection();
                ExoTimeAuthorisation exoSubJobAuthorisation = exoAuthorisations.FirstOrDefault(x => x.SubJobCode == groupedDeliverable.SubJob.INTERNAL_NAME1);
                PrimeroSubJob newSubJob = new PrimeroSubJob();
                if(exoSubJobAuthorisation != null)
                {
                    newSubJob.Id = exoSubJobAuthorisation.SubJobNo;
                    newSubJob.Code = exoSubJobAuthorisation.SubJobCode;
                    newSubJob.Title = exoSubJobAuthorisation.SubJobTitle;
                }
                else
                {
                    newSubJob.Code = groupedDeliverable.SubJob.INTERNAL_NAME1;
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
                    newCommodity.Name = exoCommodityAuthorisation.CommodityName;
                }
                else
                {
                    newCommodity.Code = groupedDeliverable.Commodity.CODE;
                    newCommodity.Name = groupedDeliverable.Commodity.NAME;
                }

                newSubJobProjection.SubJob = newSubJob;
                newSubJobProjection.Discipline = newDiscipline;
                newSubJobProjection.Commodity = newCommodity;

                newSubJobProjection.IsLineExistsInExo = exoAuthorisations.Any(x => x.SubJobCode == groupedDeliverable.SubJob.INTERNAL_NAME1 && x.DisciplineCode == groupedDeliverable.DisciplineCode && x.CommodityCode == groupedDeliverable.Commodity.CODE);
                exoSubJobs.Add(newSubJobProjection);
            }

            return exoSubJobs.AsQueryable();
        }

        public static JOBCOST_HDR GetProjectSubJob(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, Data.PROJECT project, string subJobCode)
        {
            var availableLine = from SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                 join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                 on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                 where MAINJOB.JOBCODE == project.NUMBER && SUBJOB.JOBCODE == subJobCode
                                 select SUBJOB;

            if (availableLine.Count() == 0)
                return null;

            return availableLine.First();
        }

        public static List<ExoTimeAuthorisation> GetExoTimeAuthorisation(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, string projectNumber)
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
                                 //where MAINJOB.JOBCODE == projectNumber
                                 where MAINJOB.JOBCODE == projectNumber && JOBCOST_RESOURCE.STAFFNO == LoginCredentials.CurrentUser.EXO_STAFF_ID
                                 select new { MASTERJOBNO = MAINJOB.JOBNO, SUBJOBNO = SUBJOB.JOBNO, SUBJOBTITLE = SUBJOB.TITLE, SUBJOBNAME = SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.SHORTCODE, RESOURCE_SEQNO = JOBCOST_RESOURCE.SEQNO, RESOURCE_STAFF_ID = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.DEFAULT_STOCKCODE, STOCK_CODE_DESC = STOCK_ITEMS.DESCRIPTION };

            List<ExoTimeAuthorisation> exoTimes = availableLines.Select(x => new ExoTimeAuthorisation()
            {
                MasterJobNo = x.MASTERJOBNO,
                SubJobNo = x.SUBJOBNO,
                SubJobCode = x.SUBJOBNAME,
                SubJobTitle = x.SUBJOBTITLE,
                DisciplineId = x.DISCIPLINE_ID,
                DisciplineCode = x.DISCIPLINE_CODE,
                DisciplineName = x.DISCIPLINE_NAME,
                CommodityId = x.COMMODITY_ID,
                CommodityCode = x.COMMODITY_CODE,
                CommodityName = x.COMMODITY_NAME,
                ResourceSeqNo = x.RESOURCE_SEQNO,
                ResourceStaffId = x.RESOURCE_STAFF_ID,
                ResourceName = x.RESOURCENAME,
                StockCode = x.DEFAULT_STOCKCODE,
                StockCodeDescription = x.STOCK_CODE_DESC
            }).ToList();

            return exoTimes;
        }
    }

    public class ExoTimeAuthorisation
    {
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
