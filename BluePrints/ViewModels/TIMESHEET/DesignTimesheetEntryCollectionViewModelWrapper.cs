using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class DesignTimesheetEntryCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<BASELINE_ITEM, DesignTimesheet, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of DesignTimesheetEntryCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        public static DesignTimesheetEntryCollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DesignTimesheetEntryCollectionViewModelWrapper(unitOfWorkFactory));
        }

        private IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork;
        /// <summary>
        /// Initializes a new instance of the DesignTimesheetEntryCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the DesignTimesheetEntryCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        protected DesignTimesheetEntryCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            localPrimeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        }

        #region Database Operations
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        List<ExoTimeAuthorisation> localUserExoTimeAuthorisation;
        protected override void resolveParameters(object parameter)
        {
            WeekBeginningDate = DateTime.Now;
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<DesignTimesheet>> specifyMainViewModelProjection()
        {
            return query => designTimesheetQuery(query.Where(x => x.BASELINE.STATUS == BaselineStatus.Live));
        }

        public IQueryable<DesignTimesheet> designTimesheetQuery(IQueryable<BASELINE_ITEM> BASELINE_ITEMS)
        {
            List<DesignTimesheet> DesignTimesheets = new List<DesignTimesheet>();
            IQueryable<USER> USERCollection = bluePrintsUnitOfWorkFactory.CreateUnitOfWork().USERS;
            int? currentUserStaffNo = BluePrintsUtils.GetUpdatedProjectLocaleUserExoId(BluePrintsResources.OfficePerth, USERCollection, LoginCredentials.CurrentUserGuid);

            JOBCOST_RESOURCE jobCostResource = primeroUnitOfWorkFactory.CreateUnitOfWork().JOBCOST_RESOURCE.FirstOrDefault(x => x.STAFFNO == currentUserStaffNo);
            if (jobCostResource != null)
            {
                localUserExoTimeAuthorisation = ExoQueries.GetExoLinesAuthorisations(localPrimeroUnitOfWork, null, currentUserStaffNo);
                localUserExoTimeAuthorisation.ForEach(x => x.OfficeName = BluePrintsResources.OfficePerth);

                IQueryable<BASELINE_ITEM> BASELINE_ITEMCollection = bluePrintsUnitOfWorkFactory.CreateUnitOfWork().BASELINE_ITEMS.Where(x => x.BASELINE.STATUS == BaselineStatus.Live);
                IQueryable<JOB_TIMESHEETS> TIMESHEETS = primeroUnitOfWorkFactory.CreateUnitOfWork().JOB_TIMESHEETS.Where(x => x.WEEK_START_DATE == WeekBeginningDate && x.STAFFNO == jobCostResource.SEQNO);

                foreach (JOB_TIMESHEETS timesheet in TIMESHEETS)
                {
                    DesignTimesheets.Add(new DesignTimesheet(localUserExoTimeAuthorisation, BASELINE_ITEMCollection, timesheet));
                }
            }

            return DesignTimesheets.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DesignTimesheet> entities)
        {
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(DesignTimesheet projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(DesignTimesheet projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        DateTime weekBeginningDate;
        public DateTime WeekBeginningDate
        {
            get => weekBeginningDate;
            set
            {
                DateTime mondayDate = ChronologicalHelpers.StartOfWeek(value, DayOfWeek.Monday);
                weekBeginningDate = mondayDate;
            }
        }

        private List<DateTime> generateWeekDates()
        {
            List<DateTime> weekDates = new List<DateTime>();
            for(int i = 0; i < 7; i++)
            {
                weekDates.Add(WeekBeginningDate);
                WeekBeginningDate.AddDays(1);
            }

            return weekDates;
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "DesignTimesheetEntryCollectionViewModelWrapper_v1"; }
        }
        #endregion
    }
}