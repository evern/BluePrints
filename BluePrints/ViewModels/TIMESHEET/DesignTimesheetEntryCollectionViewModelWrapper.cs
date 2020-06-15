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
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
        JOB_COSTGROUPS defaultTenderCostGroup;
        JOB_COSTTYPES defaultTenderCostType;
        public bool IsReview { get; set; }
        protected override void resolveParameters(object parameter)
        {
            IsReview = (bool)parameter;
            initializeUnitOfWork();
            WeekBeginningDate = DateTime.Now;
            canUnsubmit = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_UserTimesheet_Unsubmit)) == LoginCredentials.PermissionStatus.All;
            string defaultTenderDisciplineCode = BluePrintsResources.Default_TenderDisciplineCode + "01";
            defaultTenderCostGroup = primeroEntitiesUnitOfWork.JOB_COSTGROUPS.FirstOrDefault(x => x.SHORTCODE == defaultTenderDisciplineCode);
            defaultTenderCostType = primeroEntitiesUnitOfWork.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == BluePrintsResources.Default_TenderCommodityCode);
            NewItemRowPosition = IsReview ? NewItemRowPosition.None : NewItemRowPosition.Top;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        protected virtual Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid);
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
            List<int> subJobNos = new List<int>();
            if(IsReview)
            {
                foreach(PROJECT project in PROJECTCollection)
                {
                    JOBCOST_HDR masterJob = primeroEntitiesUnitOfWork.JOBCOST_HDR.FirstOrDefault(x => x.JOBCODE == project.NUMBER);
                    if(masterJob != null)
                    {
                        foreach(JOBCOST_HDR subJob in primeroEntitiesUnitOfWork.JOBCOST_HDR.Where(x => x.MASTER_JOBNO == masterJob.JOBNO))
                        {
                            subJobNos.Add(subJob.JOBNO);
                        }
                    }
                }
            }

            if (currentUserJOBCOST_RESOURCE != null)
            {
                if (IsReview)
                    timesheetsQueryable = primeroEntitiesUnitOfWork.JOB_TIMESHEETS.Where(x => x.JOBNO != null && x.X_SUBMITTED == true).Where(x => x.WEEK_START_DATE == WeekBeginningDate).Where(x => subJobNos.Contains((int)x.JOBNO));
                else
                    timesheetsQueryable = primeroEntitiesUnitOfWork.JOB_TIMESHEETS.Where(x => x.WEEK_START_DATE == WeekBeginningDate && x.STAFFNO == currentUserJOBCOST_RESOURCE.SEQNO);

                foreach (JOB_TIMESHEETS timesheet in timesheetsQueryable)
                {
                    DesignTimesheets.Add(new DesignTimesheet(userExoTimeAuthorisation, primeroEntitiesUnitOfWork, deliverablesQueryable, currentUserJOBCOST_RESOURCE, WeekBeginningDate, canUnsubmit, defaultTenderCostGroup.SEQNO, defaultTenderCostType.SEQNO, COMMODITY_CODECollection, IsReview, timesheet));
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

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, DesignTimesheet projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().MasterJobNo))
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().CostGroupNo), projection.CostGroupNo, null, EntityMessageType.Changed);
                projection.CostGroupNo = null;
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().AreaCode), projection.AreaCode, null, EntityMessageType.Changed);
                projection.AreaCode = null;
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().CostGroupNo))
            {
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().AreaCode), projection.AreaCode, null, EntityMessageType.Changed);
                projection.AreaCode = null;
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().DeliverableInternalName), projection.DeliverableInternalName, null, EntityMessageType.Changed);
                projection.DeliverableInternalName = null;
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().COST_TYPE), projection.COST_TYPE, null, EntityMessageType.Changed);
                projection.COST_TYPE = null;
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().AreaCode))
            {
                if (new_value == null || new_value.ToString() == string.Empty)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().PhaseCode), projection.PhaseCode, null, EntityMessageType.Changed);
                    projection.PhaseCode = null;

                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().DeliverableInternalName), projection.DeliverableInternalName, null, EntityMessageType.Changed);
                    projection.DeliverableInternalName = null;
                }
                else if (projection.PhaseCode == null || projection.PhaseCode == string.Empty)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().PhaseCode), projection.PhaseCode, BluePrintsResources.Default_Design_Phase, EntityMessageType.Changed);
                    projection.PhaseCode = BluePrintsResources.Default_Design_Phase;
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().PhaseCode))
            {
                if (new_value == null || new_value.ToString() == string.Empty)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().AreaCode), projection.AreaCode, null, EntityMessageType.Changed);
                    projection.AreaCode = null;

                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().DeliverableInternalName), projection.DeliverableInternalName, null, EntityMessageType.Changed);
                    projection.DeliverableInternalName = null;
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().DeliverableInternalName))
            {
                if (projection.DeliverableCollection != null)
                {
                    BASELINE_ITEM deliverable = projection.DeliverableCollection.FirstOrDefault(x => x.INTERNAL_NUM == projection.DeliverableInternalName);
                    if (deliverable != null && deliverable.DOCTYPE != null)
                    {
                        JOB_COSTTYPES costType = primeroEntitiesUnitOfWork.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == deliverable.DOCTYPE.CODE);
                        if (costType != null)
                        {
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().COST_TYPE), projection.COST_TYPE, costType.SEQNO, EntityMessageType.Changed);
                            projection.COST_TYPE = costType.SEQNO;
                        }
                    }
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().Day1) || field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().Day2) || field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().Day3) || field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().Day4) || field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().Day5) || field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().Day6) || field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().Day7))
            {
                GridControlService.RefreshSummary();
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedNewRowInitializationFromView(DesignTimesheet projection)
        {
            projection.SetInitProperties(userExoTimeAuthorisation, primeroEntitiesUnitOfWork, deliverablesQueryable, currentUserJOBCOST_RESOURCE, WeekBeginningDate, canUnsubmit, defaultTenderCostGroup.SEQNO, defaultTenderCostType.SEQNO, COMMODITY_CODECollection, IsReview);
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

        private string projectionDataValidation(DesignTimesheet projection, string field_name, object new_value)
        {
            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().MasterJobNo))
            {
                if (new_value != null)
                {
                    JOBCOST_HDR findJob = AuthorisedJobCollection.FirstOrDefault(x => x.JOBNO == (int)new_value);
                    if (findJob == null)
                        return "Unauthorised job";
                    else
                        projection.MasterJobNo = findJob.JOBNO;
                }
                else
                    return "Job name cannot be empty";
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().CostGroupNo))
            {
                if (new_value != null)
                {
                    JOB_COSTGROUPS findCostGroup = projection.CostGroupCollection.FirstOrDefault(x => x.SEQNO == (int)new_value);
                    if (findCostGroup == null)
                        return "Invalid cost group";
                    else
                        projection.CostGroupNo = findCostGroup.SEQNO;
                }
                else
                    return "Cost group cannot be empty";
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().AreaCode))
            {
                if (new_value != null && new_value.ToString() != string.Empty)
                {
                    string findArea = projection.AreaCollection.FirstOrDefault(x => x == new_value.ToString());
                    if (findArea == null)
                        return "Invalid area";
                    else
                        projection.AreaCode = findArea;
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().PhaseCode))
            {
                if (new_value != null && new_value.ToString() != string.Empty)
                {
                    if (projection.AreaCode == null || projection.AreaCode == string.Empty)
                        return "Area cannot be empty when phase exist";
                }
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().COST_TYPE))
            {
                if (new_value != null)
                {
                    JOB_COSTTYPES findCostType = projection.TaggedValidJOB_COSTTYPES.FirstOrDefault(x => x.SEQNO == (int)new_value);
                    if (findCostType == null)
                        return "Invalid cost type";
                    else
                        projection.COST_TYPE = findCostType.SEQNO;
                }
                else
                    return "Cost type cannot be empty";
            }

            if (field_name == BindableBase.GetPropertyName(() => new DesignTimesheet().DeliverableInternalName))
            {
                if (new_value != null && new_value.ToString() != string.Empty)
                {
                    BASELINE_ITEM findDeliverable = projection.DeliverableCollection.FirstOrDefault(x => x.INTERNAL_NUM == new_value.ToString());
                    if (findDeliverable == null)
                        return "Invalid deliverable";
                    else
                        projection.DeliverableInternalName = findDeliverable.INTERNAL_NUM;
                }
            }

            return string.Empty;
        }

        public override string UnifiedRowValidation(DesignTimesheet projection)
        {
            if (projection.Timesheet == null)
                return "Incomplete timesheet data";

            if (projection.Timesheet.STAFFNO == null)
                return "Staff number not populated";

            string dataErrorString = "Data Error: ";

            string errorString = string.Empty;
            string errorMessage = projectionDataValidation(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().MasterJobNo), projection.MasterJobNo);
            if (errorMessage != string.Empty)
                errorString += errorMessage + ", ";

            errorMessage = projectionDataValidation(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().AreaCode), projection.AreaCode);
            if (errorMessage != string.Empty)
                errorString += errorMessage + ", ";

            errorMessage = projectionDataValidation(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().PhaseCode), projection.PhaseCode);
            if (errorMessage != string.Empty)
                errorString += errorMessage + ", ";

            errorMessage = projectionDataValidation(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().CostGroupNo), projection.CostGroupNo);
            if (errorMessage != string.Empty)
                errorString += errorMessage + ", ";

            errorMessage = projectionDataValidation(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().DeliverableInternalName), projection.DeliverableInternalName);
            if (errorMessage != string.Empty)
                errorString += errorMessage + ", ";

            errorMessage = projectionDataValidation(projection, BindableBase.GetPropertyName(() => new DesignTimesheet().COST_TYPE), projection.COST_TYPE);
            if (errorMessage != string.Empty)
                errorString += errorMessage + ", ";

            if (errorString != string.Empty)
            {
                errorString = errorString.Substring(0, errorString.Length - 2);
                return dataErrorString + errorString;
            }

            if (projection.Timesheet.COST_GROUP == null)
                return "Invalid discipline";

            if (projection.Timesheet.COST_TYPE == null)
                return "Invalid cost type";

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
            {
                if(MessageBoxService.ShowMessage("Are you sure you want to unsubmit this line?", "Confirmation", MessageButton.YesNo) == MessageResult.No)
                    return;

                designTimesheet.Timesheet.X_SUBMITTED = false;
                designTimesheet.Timesheet.DAY1_NARRATIVE = null;
                designTimesheet.Timesheet.DAY2_NARRATIVE = null;
                designTimesheet.Timesheet.DAY3_NARRATIVE = null;
                designTimesheet.Timesheet.DAY4_NARRATIVE = null;
                designTimesheet.Timesheet.DAY5_NARRATIVE = null;
                designTimesheet.Timesheet.DAY6_NARRATIVE = null;
                designTimesheet.Timesheet.DAY7_NARRATIVE = null;
            }
            else
            {
                if (MessageBoxService.ShowMessage("Are you sure you want to submit this line for review?", "Confirmation", MessageButton.YesNo) == MessageResult.No)
                    return;

                designTimesheet.Timesheet.X_SUBMITTED = true;
                if (designTimesheet.DeliverableInternalName != null && designTimesheet.DeliverableInternalName != string.Empty)
                {
                    int narrativeId = BluePrintsUtils.FindExistingOrAddNewNarrative(designTimesheet.DeliverableInternalName, primeroEntitiesUnitOfWork);
                    if (designTimesheet.Timesheet.DAY1 != null)
                        designTimesheet.Timesheet.DAY1_NARRATIVE = narrativeId;
                    if (designTimesheet.Timesheet.DAY2 != null)
                        designTimesheet.Timesheet.DAY2_NARRATIVE = narrativeId;
                    if (designTimesheet.Timesheet.DAY3 != null)
                        designTimesheet.Timesheet.DAY3_NARRATIVE = narrativeId;
                    if (designTimesheet.Timesheet.DAY4 != null)
                        designTimesheet.Timesheet.DAY4_NARRATIVE = narrativeId;
                    if (designTimesheet.Timesheet.DAY5 != null)
                        designTimesheet.Timesheet.DAY5_NARRATIVE = narrativeId;
                    if (designTimesheet.Timesheet.DAY6 != null)
                        designTimesheet.Timesheet.DAY6_NARRATIVE = narrativeId;
                    if (designTimesheet.Timesheet.DAY7 != null)
                        designTimesheet.Timesheet.DAY7_NARRATIVE = narrativeId;
                }

                designTimesheet.Update();
            }

            designTimesheet.RefreshSubmitStatus();
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

        public NewItemRowPosition NewItemRowPosition { get; set; }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "DesignTimesheetEntryCollectionViewModelWrapper_v1"; }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public IEnumerable<JOBCOST_RESOURCE> JOBCOST_RESOURCECollection
        {
            get
            {
                if (primeroEntitiesUnitOfWork == null)
                    return new List<JOBCOST_RESOURCE>();

                return primeroEntitiesUnitOfWork.JOBCOST_RESOURCE;
            }
        }

        public IEnumerable<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                if (primeroEntitiesUnitOfWork == null)
                    return new List<JOBCOST_HDR>();

                return primeroEntitiesUnitOfWork.JOBCOST_HDR;
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                if (primeroEntitiesUnitOfWork == null)
                    return new List<JOB_COSTTYPES>();

                return primeroEntitiesUnitOfWork.JOB_COSTTYPES;
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                if (primeroEntitiesUnitOfWork == null)
                    return new List<JOB_COSTGROUPS>();

                return primeroEntitiesUnitOfWork.JOB_COSTGROUPS;
            }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                return GetEntities<PROJECT>();
            }
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

        List<string> phaseCollection;
        public List<string> PHASECollection
        {
            get
            {
                if(phaseCollection == null)
                {
                    phaseCollection = new List<string>();
                    phaseCollection.Add(BluePrintsResources.Default_Design_Phase);
                    phaseCollection.Add(BluePrintsResources.Default_Indirect_Phase);
                }

                return phaseCollection;
            }
        }
        #endregion
    }

    public class DesignTimesheetEditorTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string resourceName = string.Empty;
            EditGridCellData editGridCellData = (EditGridCellData)item;

            GridCellData data = (GridCellData)item;
            var dataItem = data.RowData.Row as DesignTimesheet;

            //dataItem == null is new row
            if (editGridCellData.Column.FieldName == BindableBase.GetPropertyName(() => new DesignTimesheet().MasterJobNo))
                resourceName = dataItem == null || !dataItem.IsReview ? "MasterJobNoAuthorisedCollection" : null;
            else if (editGridCellData.Column.FieldName == BindableBase.GetPropertyName(() => new DesignTimesheet().CostGroupNo))
                resourceName = dataItem == null || !dataItem.IsReview ? "CostGroupAuthorisedCollection" : null;
            else if (editGridCellData.Column.FieldName == BindableBase.GetPropertyName(() => new DesignTimesheet().AreaCode))
                resourceName = dataItem == null || !dataItem.IsReview ? "AreaAuthorisedCollection" : null;
            else if (editGridCellData.Column.FieldName == BindableBase.GetPropertyName(() => new DesignTimesheet().COST_TYPE))
                resourceName = dataItem == null || !dataItem.IsReview ? "CostTypeAuthorisedCollection" : null;
            else if (editGridCellData.Column.FieldName == BindableBase.GetPropertyName(() => new DesignTimesheet().DeliverableInternalName))
                resourceName = dataItem == null || !dataItem.IsReview ? "DeliverableCollectionEditor" : "TextEditor";
            if (resourceName == null)
                return null;
            else
                return (DataTemplate)((FrameworkElement)container).FindResource(resourceName);
        }
    }
}