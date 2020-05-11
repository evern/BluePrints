using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

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

        /// <summary>
        /// Initializes a new instance of the DesignTimesheetEntryCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the DesignTimesheetEntryCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        protected DesignTimesheetEntryCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork;
        List<ExoTimeAuthorisation> userExoTimeAuthorisation;
        int? currentUserSTAFFNO;
        JOBCOST_RESOURCE currentUserJOBCOST_RESOURCE;
        bool canUnsubmit;
        bool isRemoteEXODb;
        protected override void resolveParameters(object parameter)
        {
            initializeUnitOfWork();
            WeekBeginningDate = DateTime.Now;
            canUnsubmit = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_UserTimesheet_Unsubmit)) == LoginCredentials.PermissionStatus.All;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<DesignTimesheet>> specifyMainViewModelProjection()
        {
            return query => designTimesheetQuery(query.Where(x => x.BASELINE.STATUS == BaselineStatus.Live));
        }

        IQueryable<BASELINE_ITEM> deliverablesQueryable;
        IQueryable<JOB_TIMESHEETS> timesheetsQueryable;
        public IQueryable<DesignTimesheet> designTimesheetQuery(IQueryable<BASELINE_ITEM> BASELINE_ITEMS)
        {
            deliverablesQueryable = BASELINE_ITEMS;
            List<DesignTimesheet> DesignTimesheets = new List<DesignTimesheet>();
            if (currentUserJOBCOST_RESOURCE != null)
            {
                timesheetsQueryable = primeroEntitiesUnitOfWork.JOB_TIMESHEETS.Where(x => x.WEEK_START_DATE == WeekBeginningDate && x.STAFFNO == currentUserJOBCOST_RESOURCE.SEQNO);
                foreach (JOB_TIMESHEETS timesheet in timesheetsQueryable)
                {
                    DesignTimesheets.Add(new DesignTimesheet(userExoTimeAuthorisation, primeroEntitiesUnitOfWork, deliverablesQueryable, currentUserJOBCOST_RESOURCE, WeekBeginningDate, canUnsubmit, timesheet));
                }
            }

            return DesignTimesheets.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DesignTimesheet> entities)
        {
            MainViewModel.FuncManualCellPastingIsContinue = FuncManualCellPastingIsContinue;
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void UnifiedNewRowInitializationFromView(DesignTimesheet projection)
        {
            projection.SetInitProperties(userExoTimeAuthorisation, primeroEntitiesUnitOfWork, deliverablesQueryable, currentUserJOBCOST_RESOURCE, WeekBeginningDate, canUnsubmit);
            base.UnifiedNewRowInitializationFromView(projection);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(DesignTimesheet projection, out bool isNew)
        {
            isNew = !primeroEntitiesUnitOfWork.JOB_TIMESHEETS.Any(x => x.SEQNO == projection.Timesheet.SEQNO);

            if (isNew)
                primeroEntitiesUnitOfWork.JOB_TIMESHEETS.Add(projection.Timesheet);
            
            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        protected override void OnAfterProjectionsSave(IEnumerable<DesignTimesheet> projections)
        {
            primeroEntitiesUnitOfWork.SaveChanges();
            base.OnAfterProjectionsSave(projections);
        }

        public bool FuncManualCellPastingIsContinue(DesignTimesheet projection, ColumnBase column, string pasteValue, List<UndoRedoArg<DesignTimesheet>> undoRedoArgs)
        {
            if (DataUtils.FormatColumnFieldname(column.FieldName) == BindableBase.GetPropertyName(() => new DesignTimesheet().MasterJobNo))
            {
                int? oldValue = projection.MasterJobNo;
                JOBCOST_HDR findJob = AuthorisedJobCollection.FirstOrDefault(x => x.JOBCODE == pasteValue);
                if (findJob == null)
                    projection.MasterJobNo = null;
                else
                    projection.MasterJobNo = findJob.JOBNO;

                return false;
            }
            else if (DataUtils.FormatColumnFieldname(column.FieldName) == BindableBase.GetPropertyName(() => new DesignTimesheet().CostGroupNo))
            {
                int? oldValue = projection.CostGroupNo;
                JOB_COSTGROUPS findCostGroup = projection.CostGroupCollection.FirstOrDefault(x => x.SHORTCODE == pasteValue);
                if (findCostGroup == null)
                    projection.CostGroupNo = null;
                else
                    projection.CostGroupNo = findCostGroup.SEQNO;

                return false;
            }

            return true;
        }

        protected override OperationInterceptMode OnBeforeProjectionDeleteIsContinue(DesignTimesheet projection, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            if(projection.Timesheet.X_SUBMITTED == true)
                errorMessages.Add(new ErrorMessage(projection.DeliverableInternalName, "Cannot delete submitted timesheet"));
            else if (projection.Timesheet.DAY1_POSTED == "Y" || projection.Timesheet.DAY2_POSTED == "Y" || projection.Timesheet.DAY3_POSTED == "Y" || projection.Timesheet.DAY4_POSTED == "Y" || projection.Timesheet.DAY5_POSTED == "Y" || projection.Timesheet.DAY6_POSTED == "Y" || projection.Timesheet.DAY7_POSTED == "Y")
                errorMessages.Add(new ErrorMessage(projection.DeliverableInternalName, "Cannot delete posted timesheet"));
            else
                primeroEntitiesUnitOfWork.JOB_TIMESHEETS.Remove(projection.Timesheet);

            return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

        protected override void OnAfterProjectionsDeleted(IEnumerable<DesignTimesheet> projections)
        {
            primeroEntitiesUnitOfWork.SaveChanges();
            base.OnAfterProjectionsDeleted(projections);
        }

        public override string UnifiedValueValidation(DesignTimesheet projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(DesignTimesheet projection)
        {
            if (projection.Timesheet == null)
                return "Incomplete timesheet data";

            if (projection.Timesheet.STAFFNO == null)
                return "Staff number not populated";

            if (projection.Timesheet.COST_GROUP == null)
                return "Discipline not selected";

            if (projection.Timesheet.COST_TYPE == null)
                return "Deliverable not selected";

            return string.Empty;
        }

        private void initializeUnitOfWork()
        {
            isRemoteEXODb = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.EXO_DesignTimeSheetOfficeIsForeign));
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(IsRemoteEXODb);
            primeroEntitiesUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();

            IQueryable<USER> USERCollection = bluePrintsUnitOfWorkFactory.CreateUnitOfWork().USERS;
            currentUserSTAFFNO = BluePrintsUtils.GetUpdatedProjectLocaleUserExoId(BluePrintsResources.OfficePerth, USERCollection, LoginCredentials.CurrentUserGuid);
            currentUserJOBCOST_RESOURCE = primeroEntitiesUnitOfWork.JOBCOST_RESOURCE.FirstOrDefault(x => x.STAFFNO == currentUserSTAFFNO);
            userExoTimeAuthorisation = ExoQueries.GetExoLinesAuthorisations(primeroEntitiesUnitOfWork, null, currentUserSTAFFNO);
            userExoTimeAuthorisation.ForEach(x => x.OfficeName = BluePrintsResources.OfficePerth);
        }
        #endregion

        #region View Properties
        public override void FullRefresh()
        {
            authorisedJobCollection = null;
            costGroupCollection = null;
            initializeUnitOfWork();

            base.FullRefresh();
        }

        public bool IsRemoteEXODb
        {
            get => isRemoteEXODb;
            set
            {
                isRemoteEXODb = value;
                BluePrintsDataUtils.SaveUserPreference(DataUtils.GetNameOf(() => UserPreferences.EXO_DesignTimeSheetOfficeIsForeign), value ? UserPreferences.PreferenceTrueValue : UserPreferences.PreferenceFalseValue);
                FullRefresh();
            }
        }

        public void ToggleTimesheetSubmit(DesignTimesheet designTimesheet)
        {
            if (designTimesheet.Timesheet.X_SUBMITTED == true)
                designTimesheet.Timesheet.X_SUBMITTED = false;
            else
                designTimesheet.Timesheet.X_SUBMITTED = true;

            primeroEntitiesUnitOfWork.SaveChanges();
            designTimesheet.Update();
        }

        List<JOBCOST_HDR> authorisedJobCollection;
        public List<JOBCOST_HDR> AuthorisedJobCollection
        {
            get
            {
                if (authorisedJobCollection == null && userExoTimeAuthorisation != null)
                    authorisedJobCollection = DataUtils.DistinctBy(userExoTimeAuthorisation, x => x.MasterJobNo).Select(x => new JOBCOST_HDR() { JOBNO = x.MasterJobNo, JOBCODE = x.MasterJobCode }).OrderBy(x => x.JOBCODE).ToList();

                return authorisedJobCollection;
            }
        }

        List<JOB_COSTGROUPS> costGroupCollection;
        public List<JOB_COSTGROUPS> JOB_COSTGROUPCollection
        {
            get
            {
                if (costGroupCollection == null && userExoTimeAuthorisation != null)
                    costGroupCollection = DataUtils.DistinctBy(userExoTimeAuthorisation, x => x.DisciplineCode).Select(x => new JOB_COSTGROUPS() { SEQNO = (int)x.DisciplineId, SHORTCODE = x.DisciplineCode }).OrderBy(x => x.SHORTCODE).ToList();

                return costGroupCollection;
            }
        }

        DateTime weekBeginningDate;
        public DateTime WeekBeginningDate
        {
            get => weekBeginningDate;
            set
            {
                DateTime mondayDate = ChronologicalHelpers.StartOfWeek(value, DayOfWeek.Monday);
                weekBeginningDate = mondayDate;
                populateWeekDates();
                FullRefresh();
            }
        }

        private void populateWeekDates()
        {
            DateTime loopDate = WeekBeginningDate;
            for(int i = 1; i < 8; i++)
            {
                string weekDatePropertyName = weekDatePropertyPrefix + i.ToString();
                PropertyInfo propertyInfo = this.GetType().GetProperty(weekDatePropertyName);
                propertyInfo.SetValue(this, loopDate.ToShortDateString());
                loopDate = loopDate.AddDays(1);
            }
        }

        static string weekDatePropertyPrefix = "WeekDateDay";
        string weekDateDay1;
        public string WeekDateDay1 { get => weekDateDay1; set { weekDateDay1 = value; this.RaisePropertyChanged(x => x.WeekDateDay1); } }
        string weekDateDay2;
        public string WeekDateDay2 { get => weekDateDay2; set { weekDateDay2 = value; this.RaisePropertyChanged(x => x.WeekDateDay2); } }
        string weekDateDay3;
        public string WeekDateDay3 { get => weekDateDay3; set { weekDateDay3 = value; this.RaisePropertyChanged(x => x.WeekDateDay3); } }
        string weekDateDay4;
        public string WeekDateDay4 { get => weekDateDay4; set { weekDateDay4 = value; this.RaisePropertyChanged(x => x.WeekDateDay4); } }
        string weekDateDay5;
        public string WeekDateDay5 { get => weekDateDay5; set { weekDateDay5 = value; this.RaisePropertyChanged(x => x.WeekDateDay5); } }
        string weekDateDay6;
        public string WeekDateDay6 { get => weekDateDay6; set { weekDateDay6 = value; this.RaisePropertyChanged(x => x.WeekDateDay6); } }
        string weekDateDay7;
        public string WeekDateDay7 { get => weekDateDay7; set { weekDateDay7 = value; this.RaisePropertyChanged(x => x.WeekDateDay7); } }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "DesignTimesheetEntryCollectionViewModelWrapper_v1"; }
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }
        #endregion
    }
}