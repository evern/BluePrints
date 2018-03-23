using BaseModel.DataModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Linq;
using System.Collections.Generic;
using BluePrints.Data;
using BluePrints.Common;
using DevExpress.Mvvm;

namespace BaseModel.ViewModel.Dialogs
{
    public class BookTimeSheetViewModel
    {
        public static BookTimeSheetViewModel Create(PROJECT project, IDeliverable deliverable, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            return ViewModelSource.Create(() => new BookTimeSheetViewModel(project, deliverable, primeroUnitOfWork));
        }

        public DateTime BookDate { get; set; }
        private readonly IDeliverable deliverable;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        public List<JOBCOST_HDR> PSUBJOBCollection { get; set; }
        public JOBCOST_HDR Selected_SubJob { get; set; }
        public List<PrimeroDiscipline> PDISCIPLINECollection { get; set; }
        public PrimeroDiscipline Selected_Discipline { get; set; }
        public List<PrimeroCommodity> PCOMMODITYCollection { get; set; }
        public PrimeroCommodity Selected_Commodity { get; set; }
        public List<PrimeroResource> PRESOURCECollection { get; set; }
        public PrimeroResource Selected_Resource { get; set; }
        private JOB_TIMESHEETS Existing_TimeSheet { get; set; }
        public decimal BookHours { get; set; }
        protected BookTimeSheetViewModel(PROJECT project, IDeliverable deliverable, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            BookDate = DateTime.Now.Date;
            initializeCollection();
            this.deliverable = deliverable;
            this.primeroUnitOfWork = primeroUnitOfWork;

            JOBCOST_HDR masterJob = primeroUnitOfWork.JOBCOST_HDR.FirstOrDefault(x => x.JOBCODE == project.NUMBER);
            if(masterJob != null)
            {
                PSUBJOBCollection = primeroUnitOfWork.JOBCOST_HDR.Where(x => x.MASTER_JOBNO == masterJob.JOBNO).ToList();
            }

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
                                  where MAINJOB.JOBCODE == project.NUMBER
                                  select new { SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.SHORTCODE, RESOURCE_SEQNO = JOBCOST_RESOURCE.SEQNO, RESOURCE_STAFF_ID = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.DEFAULT_STOCKCODE };

            var availableLinesList = availableLines.ToList();
            foreach(var availableLine in availableLinesList)
            {
                if(!PDISCIPLINECollection.Any(x => x.Id == availableLine.DISCIPLINE_ID))
                {
                    PrimeroDiscipline newDiscipline = new PrimeroDiscipline();
                    newDiscipline.Id = availableLine.DISCIPLINE_ID;
                    newDiscipline.Code = availableLine.DISCIPLINE_CODE;
                    newDiscipline.Name = availableLine.DISCIPLINE_NAME;
                    PDISCIPLINECollection.Add(newDiscipline);
                }

                if(!PCOMMODITYCollection.Any(x => x.Id == availableLine.COMMODITY_ID))
                {
                    PrimeroCommodity newCommodity = new PrimeroCommodity();
                    newCommodity.Id = availableLine.COMMODITY_ID;
                    newCommodity.Code = availableLine.COMMODITY_CODE;
                    PCOMMODITYCollection.Add(newCommodity);
                }

                if(!PRESOURCECollection.Any(x => x.Id == availableLine.RESOURCE_STAFF_ID))
                {
                    PrimeroResource newResource = new PrimeroResource();
                    newResource.Id = availableLine.RESOURCE_STAFF_ID;
                    newResource.SeqNo = availableLine.RESOURCE_SEQNO;
                    newResource.Name = availableLine.RESOURCENAME;
                    newResource.StockCode = availableLine.DEFAULT_STOCKCODE;
                    PRESOURCECollection.Add(newResource);
                }
            }

            defaultDeliverableSelection(deliverable);
            setResource();
            establishDefaultTime();
        }

        private void defaultDeliverableSelection(IDeliverable deliverable)
        {
            Selected_SubJob = PSUBJOBCollection.FirstOrDefault(x => x.JOBCODE == deliverable.Subjob_Name);
            Selected_Discipline = PDISCIPLINECollection.FirstOrDefault(x => x.Code == deliverable.Discipline_Code);
            Selected_Commodity = PCOMMODITYCollection.FirstOrDefault(x => x.Code == deliverable.Commodity_Code);
        }

        private void establishDefaultTime()
        {
            JOBCOST_HDR subJob = GetSubJob();
            PrimeroResource bookResource = GetResource();
            TimesheetDate bookDate = GetTimesheetDate();
            PrimeroDiscipline bookCostGroup = GetCostGroup();
            PrimeroCommodity bookCostType = GetCostType();
            decimal bookTime = BookHours;

            JOB_TIMESHEETS timesheet = primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == bookResource.SeqNo && x.JOBNO == subJob.JOBNO && x.STOCKCODE == bookResource.StockCode && x.COST_GROUP == bookCostGroup.Id && x.COST_TYPE == bookCostType.Id && x.WEEK_START_DATE == bookDate.WeekStartDate);
            Existing_TimeSheet = timesheet;
            if(Existing_TimeSheet != null)
            {
                TimesheetDate timesheetDate = GetTimesheetDate();
                double? timeSheetHours = getTimesheetHours(timesheet, timesheetDate);
                if(timeSheetHours != null)
                {
                    BookHours = Convert.ToDecimal(timeSheetHours);
                }
            }
        }

        private double? getTimesheetHours(JOB_TIMESHEETS timesheet, TimesheetDate bookDate)
        {
            switch (bookDate.DayNumber)
            {
                case 1:
                    return timesheet.DAY1;
                case 2:
                    return timesheet.DAY2;
                case 3:
                    return timesheet.DAY3;
                case 4:
                    return timesheet.DAY4;
                case 5:
                    return timesheet.DAY5;
                case 6:
                    return timesheet.DAY6;
                case 7:
                    return timesheet.DAY7;
                default:
                    return 0;
            }
        }

        private void setResource()
        {
            Selected_Resource = PRESOURCECollection.FirstOrDefault(x => x.Id == LoginCredentials.CurrentUser.EXO_STAFF_ID);
        }

        public PrimeroResource GetResource()
        {
            return Selected_Resource;
        }

        public PrimeroDiscipline GetCostGroup()
        {
            return Selected_Discipline;
        }

        public PrimeroCommodity GetCostType()
        {
            return Selected_Commodity;
        }

        public JOBCOST_HDR GetSubJob()
        {
            return Selected_SubJob;
        }

        public TimesheetDate GetTimesheetDate()
        {
            DateTime startOfWeek = BookDate.StartOfWeek(DayOfWeek.Monday);
            int DayNum = (BookDate - startOfWeek).Days + 1;

            return new TimesheetDate() { WeekStartDate = startOfWeek, DayNumber = DayNum };
        }

        private void initializeCollection()
        {
            PDISCIPLINECollection = new List<PrimeroDiscipline>();
            PCOMMODITYCollection = new List<PrimeroCommodity>();
            PRESOURCECollection = new List<PrimeroResource>();
        }
    }

    public static class DateTimeExtension
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }

    public class PrimeroDiscipline
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class PrimeroCommodity
    {
        public int? Id { get; set; }
        public string Code { get; set; }
    }

    public class PrimeroStockCode
    {
        public string Code { get; set; }
    }

    public class PrimeroResource
    {
        public int? Id { get; set; }
        public int? SeqNo { get; set; }
        public string StockCode { get; set; }
        public string Name { get; set; }
    }

    public class TimesheetDate
    {
        public DateTime WeekStartDate { get; set; }
        public int DayNumber { get; set; }
    }
}