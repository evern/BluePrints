using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
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

namespace BluePrints.ViewModels
{
    public class PROJECTPlanViewModelWrapper : BluePrintsEntitiesCollectionWrapper<PROJECT, PROJECTTenderProfile, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTPlanCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTPlanViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTPlanViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTPlanCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTPlanCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPlanViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> perthBluePrintsUnitOfWorkFactory;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> montrealBluePrintsUnitOfWorkFactory;
        private IBluePrintsEntitiesUnitOfWork perthBluePrintsUnitOfWork;
        private IBluePrintsEntitiesUnitOfWork montrealBluePrintsUnitOfWork;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        private List<DateTime> alignedDateCollection;
        protected override void resolveParameters(object parameter)
        {
            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            perthBluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
            montrealBluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(true);
            perthBluePrintsUnitOfWork = perthBluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            montrealBluePrintsUnitOfWork = montrealBluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            doNotApplyBestFit = true;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(perthBluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(perthBluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
        }
        
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(perthBluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECTTenderProfile>> specifyMainViewModelProjection()
        {
            return query => populatePROJECTPlanProject(query);
        }

        private IQueryable<PROJECTTenderProfile> populatePROJECTPlanProject(IQueryable<PROJECT> query)
        {
            List<PROJECT> tenderPROJECTS = query.Where(x => x.NUMBER == "09602" && (x.STATUS == ProjectStatus.Lead || x.STATUS == ProjectStatus.Tender || x.STATUS == ProjectStatus.TenderSubmitted)).ToList();
            List<PROJECTTenderProfile> returnPROJECTS = new List<PROJECTTenderProfile>();

            alignedDateCollection = generateDates(tenderPROJECTS);
            foreach (PROJECT tenderPROJECT in tenderPROJECTS)
            {
                PROJECTTenderProfile projectDashboard = populateTenderProfiles(tenderPROJECT);
                if (projectDashboard != null)
                    returnPROJECTS.Add(projectDashboard);
            }

            return returnPROJECTS.AsQueryable();
        }

        private PROJECTTenderProfile populateTenderProfiles(PROJECT tenderPROJECT)
        {
            //select database locale
            IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork;
            if (tenderPROJECT.OfficeName == BluePrintsResources.OfficeMontreal)
                bluePrintsEntitiesUnitOfWork = montrealBluePrintsUnitOfWork;
            else
                bluePrintsEntitiesUnitOfWork = perthBluePrintsUnitOfWork;
            BASELINE projectLiveBaseline = bluePrintsEntitiesUnitOfWork.BASELINES.FirstOrDefault(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == tenderPROJECT.GUID);
            PROGRESS projectDesignLiveProgress = bluePrintsEntitiesUnitOfWork.PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == tenderPROJECT.GUID && x.TYPE == PhaseType.Design && x.STATUS == ProgressStatus.Live);
            if (projectLiveBaseline != null && projectDesignLiveProgress != null)
            {
                PROJECTTenderProfile PROJECTTenderProfile = new PROJECTTenderProfile();
                PROJECTTenderProfile.BluePrintsEntitiesUnitOfWork = bluePrintsEntitiesUnitOfWork;
                PROJECTTenderProfile.Entity = tenderPROJECT;
                PROJECTTenderProfile.GUID = tenderPROJECT.GUID;

                //populate tender profile items
                TENDER_PROFILE tenderPROFILE = bluePrintsEntitiesUnitOfWork.TENDER_PROFILES.FirstOrDefault(x => x.GUID_PROJECT == tenderPROJECT.GUID);

                if (tenderPROFILE != null)
                {
                    PROJECTTenderProfile.TenderProfile = tenderPROFILE;
                    PROJECTTenderProfile.TENDER_PROFILE_ITEMS = tenderPROFILE.TENDER_PROFILE_ITEM.ToList();
                    PROJECTTenderProfile.TENDER_PROFILE_ITEMS.ForEach(x => x.PROJECTTenderProfile = PROJECTTenderProfile);
                    populateDataPoints(PROJECTTenderProfile);
                }

                return PROJECTTenderProfile;
            }

            return null;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECTTenderProfile> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Helpers
        private void populateDataPoints(PROJECTTenderProfile PROJECTTenderProfile)
        {                
            DateTime startDate = (DateTime)PROJECTTenderProfile.Entity.TENDER_PROJECT_START;
            decimal tenderDuration = (decimal)PROJECTTenderProfile.Entity.TENDER_PROJECT_DURATION;
            int totalDurationInDays = Convert.ToInt32(tenderDuration * 7);
            DateTime endDate = startDate.AddDays(totalDurationInDays);

            //always start from zero since we are generating forecast from the beginning
            double beginPercentage = 0;
            foreach (TENDER_PROFILE_ITEM TENDER_PROFILE_ITEM in PROJECTTenderProfile.TENDER_PROFILE_ITEMS)
            {
                decimal assignHours = PROJECTTenderProfile.TenderProfile.TENDER_HOURS * TENDER_PROFILE_ITEM.HOURS_PERCENTAGE;
                //pro-rate the dates of the deliverable based on tender item
                int startProrateDurationInDays = Convert.ToInt32(totalDurationInDays * TENDER_PROFILE_ITEM.SCHEDULE_START_PERCENTAGE);
                DateTime proRatedStartDate = startDate.AddDays(startProrateDurationInDays);
                proRatedStartDate = new DateTime(proRatedStartDate.Year, proRatedStartDate.Month, 1);
                int endProrateDurationInDays = Convert.ToInt32(totalDurationInDays * (1 - TENDER_PROFILE_ITEM.SCHEDULE_FINISH_PERCENTAGE));
                DateTime proRatedEndDate = endDate.AddDays(-1 * endProrateDurationInDays);
                proRatedEndDate = new DateTime(proRatedEndDate.Year, proRatedEndDate.Month, 1);
                proRatedEndDate = proRatedEndDate.AddMonths(1).AddDays(-1);

                Tuple<double, double> bellCurveProfile = getBellCurveProfile((BellCurveShape)TENDER_PROFILE_ITEM.BELLCURVESHAPE);
                List<BellCurvePeriodDate> bellCurvePeriodDates = getBellCurvePeriodDates(proRatedStartDate, proRatedEndDate);
                double totalPeriod = bellCurvePeriodDates.Count;
                decimal numberOfMonths = getNumberOfMonths(startDate, endDate);
                decimal hoursPerPeriod = assignHours / numberOfMonths;

                TENDER_PROFILE_ITEM.DataPoints = new List<Common.ViewModel.Reporting.DataPoint>();
                foreach (BellCurvePeriodDate bellCurvePeriodDate in bellCurvePeriodDates)
                {
                    double bellCurveProRate = betaPer(bellCurveProfile.Item1, bellCurveProfile.Item2, bellCurvePeriodDate.PeriodNumber, totalPeriod, beginPercentage);
                    decimal bellCurveProRateDecimal = Convert.ToDecimal(bellCurveProRate);
                    Common.ViewModel.Reporting.DataPoint dataPoint = new Common.ViewModel.Reporting.DataPoint();
                    dataPoint.ProgressDate = bellCurvePeriodDate.PeriodDate;
                    dataPoint.Units = hoursPerPeriod * bellCurveProRateDecimal;
                    TENDER_PROFILE_ITEM.DataPoints.Add(dataPoint);
                }
            }
        }

        private List<BellCurvePeriodDate> getBellCurvePeriodDates(DateTime startDate, DateTime endDate)
        {
            List<BellCurvePeriodDate> bellCurvePeriodDates = new List<BellCurvePeriodDate>();
            double period = 1;
            foreach (DateTime alignedDateTime in alignedDateCollection)
            {
                if((alignedDateTime >= startDate) && (alignedDateTime < endDate))
                {
                    BellCurvePeriodDate bellCurvePeriodDate = new BellCurvePeriodDate() { PeriodDate = alignedDateTime, PeriodNumber = period };
                    bellCurvePeriodDates.Add(bellCurvePeriodDate);
                    period += 1;
                }
            }

            return bellCurvePeriodDates;
        }

        private decimal getNumberOfMonths(DateTime startDate, DateTime endDate)
        {
            return ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
        }

        private Tuple<double, double> getBellCurveProfile(BellCurveShape bellCurveShape)
        {
            double A = 0;
            double B = 0;
            if (bellCurveShape == BellCurveShape.BackLoaded1)
                return new Tuple<double, double>(0.75, 0.25);
            else if (bellCurveShape == BellCurveShape.BackLoaded2)
                return new Tuple<double, double>(0.5, 0.5);
            else if (bellCurveShape == BellCurveShape.Balanced)
                return new Tuple<double, double>(0.5, 0);
            else if (bellCurveShape == BellCurveShape.FrontLoaded1)
                return new Tuple<double, double>(0, 0.5);
            else
                return new Tuple<double, double>(0, 0.25);
        }

        private double betaPer(double A, double B, double periodNum, double totalPeriod, double beginPercentage)
        {
            double remainingPercentage = 1 - beginPercentage;
            if (remainingPercentage == 0)
                return 0;

            double inflatedTotalPeriod = totalPeriod / remainingPercentage;
            double absoluteStartPeriod = beginPercentage * inflatedTotalPeriod;
            double currentStartPeriod = absoluteStartPeriod + periodNum;

            if (inflatedTotalPeriod == 0)
                return 0;

            double betaTotal = betaCum(A, B, 1) - betaCum(A, B, (absoluteStartPeriod / inflatedTotalPeriod));
            double betaThisPeriod = betaCum(A, B, currentStartPeriod / inflatedTotalPeriod);
            double betaPreviousPeriod = betaCum(A, B, (currentStartPeriod - 1) / inflatedTotalPeriod);
            double returnValue = betaThisPeriod - betaPreviousPeriod;

            return returnValue / betaTotal;
        }

        private double betaCum(double A, double B, double T)
        {
            if (T < 0)
                return 0;
            else
            {
                if (T >= 1)
                    return 1;
                else
                {
                    double minusT = 1 - T;
                    return 10 * (Math.Pow(T, 2)) * (Math.Pow(1 - T, 2)) * (A + B * T) + Math.Pow(T, 4) * (5 - (4 * T));
                }
            }
        }

        private List<DateTime> generateDates(IEnumerable<PROJECT> PROJECTS)
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            foreach (PROJECT PROJECT in PROJECTS)
            {
                if (PROJECT.TENDER_PROJECT_START != null)
                {
                    DateTime PROJECTPlanStartDate = ((DateTime)PROJECT.TENDER_PROJECT_START);
                    int duration = PROJECT.TENDER_PROJECT_DURATION == null ? 1 : (int)PROJECT.TENDER_PROJECT_DURATION;
                    DateTime PROJECTPlanEndDate = PROJECTPlanStartDate.AddDays(duration * 7);
                    if (startDate > PROJECTPlanStartDate)
                        startDate = new DateTime(PROJECTPlanStartDate.Year, PROJECTPlanStartDate.Month, 1);
                    if (endDate < PROJECTPlanEndDate)
                        endDate = PROJECTPlanEndDate;
                }
            }

            return ChronologicalHelpers.GenerateEndDatesCollection(startDate, endDate);
        }


        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(PROJECTTenderProfile projection, PROJECT entity, bool isNewEntity)
        {
            onAfterPROJECTPlanSaved(entity);
        }

        private void onAfterPROJECTPlanSaved(PROJECT entity)
        {

        }

        public void NewRowAddUndoAndSave(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

                DataRowView row = (DataRowView)e.Row;

                //findExistingOrAddNewFORECAST_JOB(row.Row);
                //MainViewModel.EntitiesUndoRedoManager.AddUndo(updatedForecastJobFromDataRow(row.Row), null, null, null, EntityMessageType.Added);
                //focusNewlyAddedProjectionTimer.Start();
                ////added not working well atm because when row is removed from datatable its itemarray is cleared
                ////EntitiesUndoRedoManager.AddUndo(row.Row, null, null, null, EntityMessageType.Added);
                //MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            }
        }

        public override void ValidateRow(GridRowValidationEventArgs e)
        {
            DataRow dataRow = ((DataRowView)e.Row).Row;
            string errorMessage = UnifiedRowValidation((PROJECTTenderProfile)dataRow[columnProject]);

            if (errorMessage != string.Empty)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = errorMessage;
            }
        }

        public void ParentCellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            //new item handling
            if (e.RowHandle == GridControl.NewItemRowHandle)
            {
                //PROJECT_Dashboard newPROJECT;
                //if (dataRowView[columnProject] == DBNull.Value)
                //{
                //    newPROJECT = new PROJECT_Dashboard();
                //    dataRowView[columnProject] = newPROJECT;
                //}
                //else
                //    newPROJECT = (PROJECT_Dashboard)dataRowView[columnProject];

                //return;
            }

            //existing item handling
            //MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            string fieldName = e.Column.FieldName;

            commitParentCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value);
            //MainViewModel.EntitiesUndoRedoManager.AddUndo(updatedForecastJobFromDataRow(dataRowView.Row), fieldName, e.OldValue, e.Value, EntityMessageType.Changed);
            //MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();

            e.Handled = true;
        }

        public void ChildCellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            //new item handling
            if (e.RowHandle == GridControl.NewItemRowHandle)
            {
                //PROJECT_Dashboard newPROJECT;
                //if (dataRowView[columnProject] == DBNull.Value)
                //{
                //    newPROJECT = new PROJECT_Dashboard();
                //    dataRowView[columnProject] = newPROJECT;
                //}
                //else
                //    newPROJECT = (PROJECT_Dashboard)dataRowView[columnProject];

                //return;
            }

            //existing item handling
            //MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            string fieldName = e.Column.FieldName;

            commitChildCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value);
            //MainViewModel.EntitiesUndoRedoManager.AddUndo(updatedForecastJobFromDataRow(dataRowView.Row), fieldName, e.OldValue, e.Value, EntityMessageType.Changed);
            //MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();

            e.Handled = true;
        }

        protected virtual void commitParentCellValue(string fieldName, DataRow row, object oldValue, object newValue, bool skipSaveChangesAndRowUpdate = false)
        {
            PROJECTTenderProfile project = ((PROJECTTenderProfile)row[columnProject]);
            IEnumerable<TENDER_PROFILE_ITEM> tenderProfileItems = project.TENDER_PROFILE_ITEMS;

            fieldName = fieldName.Replace(columnProject + ".Entity.", "");
        }

        protected virtual void commitChildCellValue(string fieldName, DataRow row, object oldValue, object newValue, bool skipSaveChangesAndRowUpdate = false)
        {
            TENDER_PROFILE_ITEM tenderProfileItem = ((TENDER_PROFILE_ITEM)row[columnTenderProfile]);
            PROJECTTenderProfile PROJECTTenderProfile = tenderProfileItem.PROJECTTenderProfile;
            string formattedFieldName = fieldName.Replace(columnTenderProfile + ".", "");
            DataUtils.TrySetNestedValue(formattedFieldName, tenderProfileItem, newValue);
            PROJECTTenderProfile.BluePrintsEntitiesUnitOfWork.SaveChanges();

            onDataPointsCalculated(tenderProfileItem.PROJECTTenderProfile);
            //TENDER_PROFILE_ITEMSelectionViewModelWrapper tenderProfileSelectionViewModelWrapper = TENDER_PROFILE_ITEMSelectionViewModelWrapper.Create();
            //EntitiesParameter<PROJECT> entitiesParameter = new EntitiesParameter<PROJECT>(tenderProfileItem.PROJECTTenderProfile.Entity);
            //tenderProfileSelectionViewModelWrapper.SetParentViewModel(this);
            //tenderProfileSelectionViewModelWrapper.IsUsedAsPersistentViewModel = true;
            //tenderProfileSelectionViewModelWrapper.OnDataPointsCalculated = onDataPointsCalculated;
            //tenderProfileSelectionViewModelWrapper.OnParameterChange(entitiesParameter);
            //tenderProfileSelectionViewModelWrapper.OnEntitiesLoadedCallBack = onTenderProfileSelectionViewModelWrapperLoaded;
            //tenderProfileSelectionViewModelWrapper.OnEntitiesLoadedCallBackRelateParam = () => tenderProfileSelectionViewModelWrapper;
        }

        private void onTenderProfileSelectionViewModelWrapperLoaded(IEnumerable<TENDER_PROFILE_ITEM> tenderProfileItems, object invocationParent)
        {
            TENDER_PROFILE_ITEMSelectionViewModelWrapper tenderProfileSelectionViewModelWrapper = (TENDER_PROFILE_ITEMSelectionViewModelWrapper)invocationParent;
            tenderProfileSelectionViewModelWrapper.PopulateTenderDeliverables();
        }

        private void onDataPointsCalculated(PROJECTTenderProfile project)
        {
            project = populateTenderProfiles(project.Entity);
            BuildRowStats(project, true);
        }

        public override string UnifiedValueValidation(PROJECTTenderProfile projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
            //throw new NotImplementedException();
        }

        public override string UnifiedRowValidation(PROJECTTenderProfile projection)
        {
            return string.Empty;
            //throw new NotImplementedException();
        }
        #endregion

        #region View Properties
        public bool ShowDayOnDate { get; set; }
        DataTable dataPointsTable = null;
        protected string columnProject = "Project";
        protected string columnTenderProfile = "TenderProfiles";
        protected string columnTenderProfileDataTable = "TenderProfileDataTable";
        protected ObservableCollection<ColumnDescriptor> parentColumns;
        public ObservableCollection<ColumnDescriptor> ParentColumns
        {
            get
            {
                if (parentColumns == null)
                {
                    parentColumns = new ObservableCollection<ColumnDescriptor>();
                }
                return parentColumns;
            }
        }

        protected ObservableCollection<ColumnDescriptor> childColumns;
        public ObservableCollection<ColumnDescriptor> ChildColumns
        {
            get
            {
                if (childColumns == null)
                {
                    childColumns = new ObservableCollection<ColumnDescriptor>();
                }
                return childColumns;
            }
        }

        protected ObservableCollection<SummaryDescriptor> parentSummaries;
        public ObservableCollection<SummaryDescriptor> ParentSummaries
        {
            get
            {
                if (parentSummaries == null)
                {
                    parentSummaries = new ObservableCollection<SummaryDescriptor>();
                }
                return parentSummaries;
            }
        }

        protected ObservableCollection<SummaryDescriptor> childSummaries;
        public ObservableCollection<SummaryDescriptor> ChildSummaries
        {
            get
            {
                if (childSummaries == null)
                {
                    childSummaries = new ObservableCollection<SummaryDescriptor>();
                }
                return childSummaries;
            }
        }

        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || Entities == null)
                    return null;

                if (dataPointsTable == null)
                {
                    IsLoading = true;
                    this.RaisePropertyChanged(x => x.IsLoading);
                    GridControlService.BeginDataUpdate();
                    dataPointsTable = new DataTable();

                    if(ParentColumns.Count() == 0)
                        InitializeParentColumnSource(ParentColumns, ParentSummaries, alignedDateCollection);

                    if(ChildColumns.Count() == 0)
                        InitializeChildColumnSource(ChildColumns, ChildSummaries, alignedDateCollection);

                    dataPointsTable.Columns.Add(columnProject, typeof(PROJECTTenderProfile));
                    dataPointsTable.Columns.Add(columnTenderProfileDataTable, typeof(DataTable));
                    populateAlignedDataDate(dataPointsTable, alignedDateCollection);

                    foreach (PROJECTTenderProfile entity in Entities)
                    {
                        BuildRowStats(entity, false);
                    }

                    GridControlService.EndDataUpdate();
                    IsLoading = false;
                    this.RaisePropertyChanged(x => x.IsLoading);
                }

                return dataPointsTable;
            }
        }

        private void populateAlignedDataDate(DataTable dataTable, List<DateTime> alignedDataDates)
        {
            foreach (DateTime alignedDataDate in alignedDataDates)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataTable.Columns.Add(columnFieldName, typeof(decimal));
            }
        }

        private void BuildRowStats(PROJECTTenderProfile entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            string s;
            if (entity.Entity.NUMBER == "09602")
                s = string.Empty;

            DataRow newDataRow;
            if (!isUpdate)
            {
                newDataRow = dataPointsTable.NewRow();
            }
            else
            {
                newDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((PROJECTTenderProfile)dr[columnProject]).GUID == entity.GUID
                              select dr).FirstOrDefault();
            }

            if (newDataRow == null)
                return;

            newDataRow[columnProject] = entity;
            List<Common.ViewModel.Reporting.DataPoint> projectDataPoints = entity.TENDER_PROFILE_ITEMS.Where(x => x.DataPoints != null).SelectMany(x => x.DataPoints).ToList();
            //format dates row to numbers
            for (int i = 0; i < newDataRow.ItemArray.Count(); i++)
            {
                string columnName = dataPointsTable.Columns[i].ColumnName;
                if (columnName != columnProject && columnName != columnTenderProfileDataTable)
                {
                    DateTime columnDate = DateTime.Parse(columnName);
                    IEnumerable<Common.ViewModel.Reporting.DataPoint> currentPeriodDataPoints;
                    currentPeriodDataPoints = projectDataPoints.Where(x => x.ProgressDate.Year == columnDate.Year && x.ProgressDate.Month == columnDate.Month);

                    if (currentPeriodDataPoints.Count() > 0)
                        newDataRow[columnName] = currentPeriodDataPoints.Sum(x => x.Units);
                    else
                        newDataRow[columnName] = 0.00m;
                }
            }

            //populate tender profiles
            DataTable tenderProfilesDataPointsTable = null;
            if (newDataRow[columnTenderProfileDataTable] != DBNull.Value)
                tenderProfilesDataPointsTable = (DataTable)newDataRow[columnTenderProfileDataTable];
            else
            {
                tenderProfilesDataPointsTable = new DataTable();
                tenderProfilesDataPointsTable.Columns.Add(columnTenderProfile, typeof(TENDER_PROFILE_ITEM));
                populateAlignedDataDate(tenderProfilesDataPointsTable, alignedDateCollection);
            }

            tenderProfilesDataPointsTable.Clear();
            if(entity.TENDER_PROFILE_ITEMS != null)
                foreach (TENDER_PROFILE_ITEM tenderProfileItem in entity.TENDER_PROFILE_ITEMS)
                {
                    DataRow tenderProfileDataRow = tenderProfilesDataPointsTable.NewRow();

                    tenderProfileDataRow[columnTenderProfile] = tenderProfileItem;
                    tenderProfilesDataPointsTable.Rows.Add(tenderProfileDataRow);

                    List<Common.ViewModel.Reporting.DataPoint> profileItemDataPoints = tenderProfileItem.DataPoints == null ? new List<Common.ViewModel.Reporting.DataPoint>() : tenderProfileItem.DataPoints;
                    //format dates row to numbers
                    for (int i = 0; i < tenderProfileDataRow.ItemArray.Count(); i++)
                    {
                        string columnName = tenderProfilesDataPointsTable.Columns[i].ColumnName;
                        if (columnName != columnTenderProfile)
                        {
                            DateTime columnDate = DateTime.Parse(columnName);
                            IEnumerable<Common.ViewModel.Reporting.DataPoint> currentPeriodDataPoints = profileItemDataPoints.Where(x => x.ProgressDate.Year == columnDate.Year && x.ProgressDate.Month == columnDate.Month);

                            if (currentPeriodDataPoints.Count() > 0)
                                tenderProfileDataRow[columnName] = currentPeriodDataPoints.Sum(x => x.Units);
                            else
                                tenderProfileDataRow[columnName] = 0.00m;
                        }
                    }
                }

            newDataRow[columnTenderProfileDataTable] = tenderProfilesDataPointsTable;

            if (!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }

        private void InitializeParentColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.NUMBER", ReadOnly = true, Header = "Number", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = columnProject + ".Entity.NUMBER", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.NAME", ReadOnly = true, Header = "Name", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.STATUS", Header = "Status", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum1 });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_TYPE", Header = "Type", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum2 });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_DIVISION", Header = "Division", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum3 });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_COMMODITY", Header = "Commodity", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum4 });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_CONTRACT", Header = "Contract", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum5 });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_STATUS", Header = "Status", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum6 });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.TENDER_PROJECT_START", Header = "Start Date", ReadOnly = false, Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.TENDER_PROJECT_DURATION", ReadOnly = false, Visible = true, Header = "Duration", Mask = "###,##0 Weeks", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_GROSS_PROFIT", ReadOnly = false, Visible = true, Header = "Gross Profit", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_TOTAL_VALUE", ReadOnly = false, Visible = true, Header = "Total Value", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_SCOPE_PCT", ReadOnly = false, Visible = true, Header = "Scope %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Mask = "n0", ReadOnly = true, Header = columnFieldName, Width = 60, Settings = SettingsType.Number });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n0", Type = SummaryItemType.Sum });
            }
        }

        private void InitializeChildColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".GUID_DEPARTMENT", Header = "Department", DisplayMember = "NAME", ValueMember = "GUID", Fixed = FixedStyle.Left, Width = 70, ItemsSource = DEPARTMENTCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".GUID_DISCIPLINE", Tag = "Start Date", Header = "Discipline", DisplayMember = "NAME", ValueMember = "GUID", Fixed = FixedStyle.Left, Width = 70, ItemsSource = DISCIPLINECollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".HOURS_PERCENTAGE", Tag = "Duration", Header = "Hours %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".SCHEDULE_START_PERCENTAGE", Tag = "Gross Profit", Header = "Schedule Start %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".SCHEDULE_FINISH_PERCENTAGE", Tag = "Total Value", Header = "Schedule Finish %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".BELLCURVESHAPE", Tag = "Scope %", Header = "Bell Curve", Fixed = FixedStyle.Left, Width = 70, ItemsSource = BellCurveShapeCollection, Settings = SettingsType.Collection });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Mask = "n0", ReadOnly = true, Header = columnFieldName, Tag = columnFieldName, Width = 60, Settings = SettingsType.Number });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n0", Type = SummaryItemType.Sum });
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPlanCollectionViewModelWrapper"; }
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                return collection;
            }
        }

        public IEnumerable<BellCurveShape> BellCurveShapeCollection
        {
            get
            {
                return DataUtils.GetValuesOf(() => new BellCurveShape());
            }
        }

        public CollectionViewModel<TENDER_PROFILE_ITEM, TENDER_PROFILE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> TENDER_PROFILE_ITEMViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<TENDER_PROFILE_ITEM, TENDER_PROFILE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<TENDER_PROFILE_ITEM>();
            }
        }
        #endregion
    }

    public class BellCurvePeriodDate
    {
        public DateTime PeriodDate { get; set; }
        public double PeriodNumber { get; set; }
    }
}