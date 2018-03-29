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
using BluePrints.Common.Projections;
using DevExpress.Mvvm;

namespace BaseModel.ViewModel.Dialogs
{
    public class BookTimeSheetViewModel
    {
        public static BookTimeSheetViewModel Create(PROJECT project, IDeliverable deliverable, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, List<ExoTimeAuthorisation> exoAuthorisations)
        {
            return ViewModelSource.Create(() => new BookTimeSheetViewModel(project, deliverable, primeroUnitOfWork, exoAuthorisations));
        }

        public DateTime BookDate { get; set; }
        private readonly IDeliverable deliverable;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        List<PrimeroSubJob> pSubJobCollection;
        public List<PrimeroSubJob> PSUBJOBCollection
        {
            get { return pSubJobCollection; }
            set { pSubJobCollection = value; }
        }

        PrimeroSubJob selectedSubJob;
        public PrimeroSubJob Selected_SubJob
        {
            get
            {
                return selectedSubJob;
            }
            set
            {
                selectedSubJob = value;
                selected_commoditycode = string.Empty;
                selectedDiscipline = null;
                selectedCommodity = null;
                this.RaisePropertyChanged(x => x.Selected_Discipline);
                this.RaisePropertyChanged(x => x.Selected_CommodityCode);
                this.RaisePropertyChanged(x => x.Selected_Commodity);
                this.RaisePropertyChanged(x => x.PDISCIPLINECollection);
                this.RaisePropertyChanged(x => x.COMMODITYCODECollection);
                this.RaisePropertyChanged(x => x.PCOMMODITYCollection);
            }
        }

        List<PrimeroDiscipline> pDisciplineCollection;
        public List<PrimeroDiscipline> PDISCIPLINECollection
        {
            get
            {
                if (selectedSubJob == null)
                    return new List<PrimeroDiscipline>();

                return pDisciplineCollection.Where(x => x.SubjobId == selectedSubJob.Id).ToList();
            }
            set
            {
                pDisciplineCollection = value;
            }
        }

        PrimeroDiscipline selectedDiscipline;
        public PrimeroDiscipline Selected_Discipline
        {
            get
            {
                return selectedDiscipline;
            }
            set
            {
                selectedDiscipline = value;
                selected_commoditycode = string.Empty;
                selectedCommodity = null;
                this.RaisePropertyChanged(x => x.Selected_CommodityCode);
                this.RaisePropertyChanged(x => x.Selected_Commodity);
                this.RaisePropertyChanged(x => x.COMMODITYCODECollection);
                this.RaisePropertyChanged(x => x.PCOMMODITYCollection);
            }
        }

        string selected_commoditycode;
        public string Selected_CommodityCode
        {
            get { return selected_commoditycode; }
            set
            {
                selected_commoditycode = value;
                Selected_Commodity = null;
                this.RaisePropertyChanged(x => x.COMMODITYCODECollection);
                this.RaisePropertyChanged(x => x.PCOMMODITYCollection);
            }
        }

        public List<string> COMMODITYCODECollection
        {
            get
            {
                if (selectedDiscipline == null || selectedSubJob == null)
                    return new List<string>();

                IEnumerable<PrimeroCommodity> commodities = pCommodityCollection.Where(x => x.SubJobNo == selectedSubJob.Id && x.DisciplineId == selectedDiscipline.Id);
                return commodities.Select(x => x.Code).Distinct().ToList();
            }
        }

        private List<PrimeroCommodity> pCommodityCollection;
        public List<PrimeroCommodity> PCOMMODITYCollection
        {
            get
            {
                if (selectedSubJob == null ||  selectedDiscipline == null || Selected_CommodityCode == null || Selected_CommodityCode == string.Empty)
                    return new List<PrimeroCommodity>();

                return pCommodityCollection.Where(x => x.SubJobNo == selectedSubJob.Id && x.DisciplineId == selectedDiscipline.Id && x.Code == Selected_CommodityCode).ToList();
            }

            set { pCommodityCollection = value; }
        }

        PrimeroCommodity selectedCommodity;
        public PrimeroCommodity Selected_Commodity
        {
            get { return selectedCommodity; }
            set
            {
                selectedCommodity = value;
                this.RaisePropertyChanged(x => x.Selected_Commodity);
            }
        }

        List<PrimeroResource> pResourceCollection;
        public List<PrimeroResource> PRESOURCECollection
        {
            get { return pResourceCollection; }
            set { pResourceCollection = value; }
        }

        public PrimeroResource Selected_Resource { get; set; }
        private JOB_TIMESHEETS Existing_TimeSheet { get; set; }
        public decimal BookHours { get; set; }
        private readonly IEnumerable<ExoTimeAuthorisation> exoAuthorisations;
        protected BookTimeSheetViewModel(PROJECT project, IDeliverable deliverable, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, List<ExoTimeAuthorisation> exoAuthorisations)
        {
            BookDate = DateTime.Now.Date;
            initializeCollection();
            this.deliverable = deliverable;
            this.primeroUnitOfWork = primeroUnitOfWork;
            this.exoAuthorisations = exoAuthorisations;

            foreach (var availableLine in exoAuthorisations)
            {
                if(!pSubJobCollection.Any(x => x.Id == availableLine.SubJobNo))
                {
                    PrimeroSubJob newSubJob = new PrimeroSubJob();
                    newSubJob.Id = availableLine.SubJobNo;
                    newSubJob.Code = availableLine.SubJobCode;
                    newSubJob.Title = availableLine.SubJobTitle;
                    pSubJobCollection.Add(newSubJob);
                }

                if(!pDisciplineCollection.Any(x => x.Id == availableLine.DisciplineId && x.SubjobId == availableLine.SubJobNo))
                {
                    PrimeroDiscipline newDiscipline = new PrimeroDiscipline();
                    newDiscipline.Id = availableLine.DisciplineId;
                    newDiscipline.SubjobId = availableLine.SubJobNo;
                    newDiscipline.Code = availableLine.DisciplineCode;
                    newDiscipline.Name = availableLine.DisciplineName;
                    pDisciplineCollection.Add(newDiscipline);
                }

                if(!pCommodityCollection.Any(x => x.Id == availableLine.CommodityId && x.DisciplineId == availableLine.DisciplineId && x.SubJobNo == availableLine.SubJobNo))
                {
                    PrimeroCommodity newCommodity = new PrimeroCommodity();
                    newCommodity.Id = availableLine.CommodityId;
                    newCommodity.DisciplineId = availableLine.DisciplineId;
                    newCommodity.ResourceId = availableLine.ResourceSeqNo;
                    newCommodity.Code = availableLine.CommodityCode;
                    newCommodity.SubJobNo = availableLine.SubJobNo;
                    newCommodity.StockCode = availableLine.StockCode;
                    newCommodity.StockDescription = availableLine.StockCodeDescription;
                    pCommodityCollection.Add(newCommodity);
                }

                if(!pResourceCollection.Any(x => x.Id == availableLine.ResourceStaffId))
                {
                    PrimeroResource newResource = new PrimeroResource();
                    newResource.Id = availableLine.ResourceStaffId;
                    newResource.SeqNo = availableLine.ResourceSeqNo;
                    newResource.Name = availableLine.ResourceName;
                    pResourceCollection.Add(newResource);
                }
            }

            pDisciplineCollection = pDisciplineCollection.OrderBy(x => x.Code).ToList();
            pCommodityCollection = pCommodityCollection.OrderBy(x => x.Code).ToList();
            pResourceCollection = pResourceCollection.OrderBy(x => x.Name).ToList();

            defaultDeliverableSelection(deliverable);
            setResource();
            establishDefaultTime();
        }

        private void defaultDeliverableSelection(IDeliverable deliverable)
        {
            Selected_SubJob = pSubJobCollection.FirstOrDefault(x => x.Code == deliverable.Subjob_Name);
            if(Selected_SubJob != null)
                Selected_Discipline = pDisciplineCollection.FirstOrDefault(x => x.Code == deliverable.Discipline_Code && x.SubjobId == Selected_SubJob.Id);
        }

        private void establishDefaultTime()
        {
            PrimeroSubJob subJob = GetSubJob();
            PrimeroResource bookResource = GetResource();
            TimesheetDate bookDate = GetTimesheetDate();
            PrimeroDiscipline bookCostGroup = GetCostGroup();
            PrimeroCommodity bookCostType = GetCostType();
            decimal bookTime = BookHours;

            if(bookResource != null && subJob != null && bookCostType != null && bookCostGroup != null)
            {
                JOB_TIMESHEETS timesheet = primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == bookResource.SeqNo && x.JOBNO == subJob.Id && x.STOCKCODE == bookCostType.StockCode && x.COST_GROUP == bookCostGroup.Id && x.COST_TYPE == bookCostType.Id && x.WEEK_START_DATE == bookDate.WeekStartDate);
                Existing_TimeSheet = timesheet;
                if (Existing_TimeSheet != null)
                {
                    TimesheetDate timesheetDate = GetTimesheetDate();
                    double? timeSheetHours = getTimesheetHours(timesheet, timesheetDate);
                    if (timeSheetHours != null)
                    {
                        BookHours = Convert.ToDecimal(timeSheetHours);
                    }
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
            Selected_Resource = pResourceCollection.FirstOrDefault(x => x.Id == LoginCredentials.CurrentUser.EXO_STAFF_ID);
            if(Selected_Resource != null && Selected_SubJob != null && Selected_Discipline != null)
            {
                PrimeroCommodity findCommodity = pCommodityCollection.FirstOrDefault(x => x.Id == 3417);
                IEnumerable<PrimeroCommodity> commodities = pCommodityCollection.Where(x => x.Code == deliverable.Commodity_Code);
                Selected_Commodity = commodities.FirstOrDefault(x => x.SubJobNo == selectedSubJob.Id && x.DisciplineId == selectedDiscipline.Id && x.Code == deliverable.Commodity_Code);
                if(Selected_Commodity != null)
                    selected_commoditycode = Selected_Commodity.Code;
            }
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

        public PrimeroSubJob GetSubJob()
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
            pSubJobCollection = new List<PrimeroSubJob>();
            pDisciplineCollection = new List<PrimeroDiscipline>();
            pCommodityCollection = new List<PrimeroCommodity>();
            pResourceCollection = new List<PrimeroResource>();
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
    
}