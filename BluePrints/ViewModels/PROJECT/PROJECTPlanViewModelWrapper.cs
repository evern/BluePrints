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
using System.Windows.Threading;

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
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory;
        private IBluePrintsEntitiesUnitOfWork _bluePrintsUnitOfWork;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        private List<DateTime> alignedDateCollection;
        private List<DateTime> dataPointsDateCollection;
        DispatcherTimer focusNewlyAddedProjectionTimer = new DispatcherTimer();
        protected override void resolveParameters(object parameter)
        {
            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
            _bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            doNotApplyBestFit = true;
            focusNewlyAddedProjectionTimer = new DispatcherTimer();
            focusNewlyAddedProjectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
        }
        
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECTTenderProfile>> specifyMainViewModelProjection()
        {
            return query => populatePROJECTPlanProject(query);
        }

        private IQueryable<PROJECTTenderProfile> populatePROJECTPlanProject(IQueryable<PROJECT> query)
        {
            List<PROJECT> PROJECTS = query.ToList();
            List<PROJECTTenderProfile> returnPROJECTS = new List<PROJECTTenderProfile>();

            IEnumerable<PROJECT> tenderPROJECTS = PROJECTS.Where(x => x.STATUS == ProjectStatus.Lead || x.STATUS == ProjectStatus.Tender || x.STATUS == ProjectStatus.TenderSubmitted);
            alignedDateCollection = generateDates(tenderPROJECTS);
            foreach (PROJECT tenderPROJECT in tenderPROJECTS)
            {
                PROJECTTenderProfile projectDashboard = populateTenderProfiles(tenderPROJECT, _bluePrintsUnitOfWork);
                if (projectDashboard != null)
                    returnPROJECTS.Add(projectDashboard);
            }

            return returnPROJECTS.AsQueryable();
        }

        private PROJECTTenderProfile populateTenderProfiles(PROJECT tenderPROJECT, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork)
        {
            //use unit of work entry so it's faster when saving
            PROJECT PROJECT = bluePrintsEntitiesUnitOfWork.PROJECTS.FirstOrDefault(x => x.GUID == tenderPROJECT.GUID);
            BASELINE projectLiveBaseline = bluePrintsEntitiesUnitOfWork.BASELINES.FirstOrDefault(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == tenderPROJECT.GUID);
            PROGRESS projectDesignLiveProgress = bluePrintsEntitiesUnitOfWork.PROGRESSES.FirstOrDefault(x => x.GUID_PROJECT == tenderPROJECT.GUID && x.TYPE == PhaseType.Design && x.STATUS == ProgressStatus.Live);
            if (PROJECT != null && projectLiveBaseline != null && projectDesignLiveProgress != null)
            {
                //because grid detail is binded, reinstantiation will cause detail grid to show blanks
                PROJECTTenderProfile PROJECTTenderProfile = Entities.FirstOrDefault(x => x.GUID == PROJECT.GUID);
                if (PROJECTTenderProfile == null)
                {
                    PROJECTTenderProfile = new PROJECTTenderProfile();
                    PROJECTTenderProfile.Entity = PROJECT;
                    PROJECTTenderProfile.GUID = tenderPROJECT.GUID;
                }

                //populate tender profile items
                TENDER_PROFILE tenderPROFILE = bluePrintsEntitiesUnitOfWork.TENDER_PROFILES.FirstOrDefault(x => x.GUID_PROJECT == tenderPROJECT.GUID);

                if (tenderPROFILE != null)
                    PROJECTTenderProfile.TenderProfile = tenderPROFILE;
                else
                    PROJECTTenderProfile.TenderProfile = findExistingOrAddTenderProfile(PROJECTTenderProfile.Entity, _bluePrintsUnitOfWork);

                List<TENDER_PROFILE_ITEM> PROJECTTenderProfileItems = bluePrintsEntitiesUnitOfWork.TENDER_PROFILE_ITEMS.Where(x => x.GUID_TENDER_PROFILE == PROJECTTenderProfile.TenderProfile.GUID).ToList();
                if (PROJECTTenderProfile.TENDER_PROFILE_ITEMS == null)
                    PROJECTTenderProfile.TENDER_PROFILE_ITEMS = PROJECTTenderProfileItems;
                else
                {
                    //because grid detail is binded, reinstantiation will cause detail grid to show blanks
                    PROJECTTenderProfile.TENDER_PROFILE_ITEMS.Clear();
                    PROJECTTenderProfile.TENDER_PROFILE_ITEMS.AddRange(PROJECTTenderProfileItems);
                }

                PROJECTTenderProfile.TENDER_PROFILE_ITEMS.ForEach(x => x.PROJECTTenderProfile = PROJECTTenderProfile);
                populateDataPoints(PROJECTTenderProfile);

                return PROJECTTenderProfile;
            }

            return null;
        }
        #endregion

        #region Helpers
        private void populateDataPoints(PROJECTTenderProfile PROJECTTenderProfile)
        {
            if (PROJECTTenderProfile.Entity.TENDER_PROJECT_START == null || PROJECTTenderProfile.Entity.TENDER_PROJECT_DURATION == null || PROJECTTenderProfile.Entity.TENDER_PROJECT_DURATION == 0)
                return;

            DateTime startDate = PROJECTTenderProfile.Entity.TENDER_PROJECT_START == null ? DateTime.Now : (DateTime)PROJECTTenderProfile.Entity.TENDER_PROJECT_START;

            decimal tenderDuration = PROJECTTenderProfile.Entity.TENDER_PROJECT_DURATION == null ? 0 : (decimal)PROJECTTenderProfile.Entity.TENDER_PROJECT_DURATION;
            int totalDurationInDays = Convert.ToInt32(tenderDuration * 7);
            DateTime endDate = startDate.AddDays(totalDurationInDays);

            //always start from zero since we are generating forecast from the beginning
            double beginPercentage = 0;

            //when it's first used when form loads entities is empty
            dataPointsDateCollection = Entities.Count == 0 ? alignedDateCollection : generateDates(Entities.Select(x => x.Entity));
            foreach (TENDER_PROFILE_ITEM TENDER_PROFILE_ITEM in PROJECTTenderProfile.TENDER_PROFILE_ITEMS)
            {
                decimal assignHours = PROJECTTenderProfile.TenderProfile.TENDER_HOURS * TENDER_PROFILE_ITEM.HOURS_PERCENTAGE;
                //pro-rate the dates of the deliverable based on tender item
                int startProrateDurationInDays = Convert.ToInt32(totalDurationInDays * TENDER_PROFILE_ITEM.SCHEDULE_START_PERCENTAGE);
                DateTime proRatedStartDate = startDate.AddDays(startProrateDurationInDays);
                int endProrateDurationInDays = Convert.ToInt32(totalDurationInDays * (1 - TENDER_PROFILE_ITEM.SCHEDULE_FINISH_PERCENTAGE));
                DateTime proRatedEndDate = endDate.AddDays(-1 * endProrateDurationInDays);

                Tuple<double, double> bellCurveProfile = getBellCurveProfile((BellCurveShape)TENDER_PROFILE_ITEM.BELLCURVESHAPE);
                List<BellCurvePeriodDate> bellCurvePeriodDates = getBellCurvePeriodDates(proRatedStartDate, proRatedEndDate, dataPointsDateCollection);
                double totalPeriod = bellCurvePeriodDates.Count;

                //calculate inflation for dates without pro rate
                List<WorkingDaysDate> workingDaysDates = bellCurvePeriodDates.Select(x => x.PeriodWorkingDate).ToList();

                TENDER_PROFILE_ITEM.DataPoints = new List<Common.ViewModel.Reporting.DataPoint>();
                //calculate bellcurve factor
                foreach (BellCurvePeriodDate bellCurvePeriodDate in bellCurvePeriodDates)
                {
                    decimal workingDaysFactor;
                    //when there is only one period, don't use any factor
                    if (bellCurvePeriodDates.Count == 1)
                        workingDaysFactor = 1;
                    //when there is only two periods, pro rate between them
                    else
                    {
                        workingDaysFactor = bellCurvePeriodDate.PeriodWorkingDate.Prorate;
                    }

                    double bellCurveProRate = betaPer(bellCurveProfile.Item1, bellCurveProfile.Item2, bellCurvePeriodDate.PeriodNumber, totalPeriod, beginPercentage);
                    decimal bellCurveAdjustment = Convert.ToDecimal(bellCurveProRate);
                    decimal weightedBellCurve = bellCurveAdjustment * workingDaysFactor;

                    bellCurvePeriodDate.BellCurveFactor = Convert.ToDecimal(weightedBellCurve);
                }

                decimal totalBellCurveFactor = bellCurvePeriodDates.Sum(x => x.BellCurveFactor);
                foreach (BellCurvePeriodDate bellCurvePeriodDate in bellCurvePeriodDates)
                {
                    Common.ViewModel.Reporting.DataPoint dataPoint = new Common.ViewModel.Reporting.DataPoint();
                    dataPoint.ProgressDate = bellCurvePeriodDate.PeriodWorkingDate.Date;
                    dataPoint.Units = assignHours * (bellCurvePeriodDate.BellCurveFactor / totalBellCurveFactor);
                    TENDER_PROFILE_ITEM.DataPoints.Add(dataPoint);
                }
            }
        }

        private List<BellCurvePeriodDate> getBellCurvePeriodDates(DateTime startDate, DateTime endDate, List<DateTime> datesCollection)
        {
            List<BellCurvePeriodDate> bellCurvePeriodDates = new List<BellCurvePeriodDate>();
            double period = 1;

            DateTime endOfStartDateMonth = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);
            DateTime previousEndOfEndDateMonth = new DateTime(endDate.Year, endDate.Month, 1).AddDays(-1);
            List<DateTime> dateBetweenStartEndDate = new List<DateTime>();

            BellCurvePeriodDate bellCurveStartDate = new BellCurvePeriodDate() { PeriodWorkingDate = new WorkingDaysDate(startDate, true), PeriodNumber = period };
            bellCurvePeriodDates.Add(bellCurveStartDate);

            period += 1;
            //in between dates
            foreach (DateTime date in datesCollection)
            {
                if((date > endOfStartDateMonth) && (date <= previousEndOfEndDateMonth))
                {
                    BellCurvePeriodDate bellCurvePeriodDate = new BellCurvePeriodDate() { PeriodWorkingDate = new WorkingDaysDate(date, false), PeriodNumber = period };
                    bellCurvePeriodDates.Add(bellCurvePeriodDate);
                    period += 1;
                }
            }

            BellCurvePeriodDate bellCurveEndDate = new BellCurvePeriodDate() { PeriodWorkingDate = new WorkingDaysDate(endDate, false), PeriodNumber = period };
            bellCurvePeriodDates.Add(bellCurveEndDate);

            return bellCurvePeriodDates;
        }

        private Tuple<double, double> getBellCurveProfile(BellCurveShape bellCurveShape)
        {
            double A = 0;
            double B = 0;
            if (bellCurveShape == BellCurveShape.FrontLoaded2)
                return new Tuple<double, double>(0.75, 0.25);
            else if (bellCurveShape == BellCurveShape.FrontLoaded1)
                return new Tuple<double, double>(0.5, 0.5);
            else if (bellCurveShape == BellCurveShape.Balanced)
                return new Tuple<double, double>(0.5, 0);
            else if (bellCurveShape == BellCurveShape.BackLoaded1)
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
                    return 10 * (Math.Pow(T, 2)) * (Math.Pow(1 - T, 2)) * (A + B * T) + Math.Pow(T, 4) * (5 - 4 * T);
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
                    Tuple<DateTime, DateTime> projectStartEndDate = BluePrintsDataUtils.GetTenderStartEndDate(PROJECT);
                    if (startDate > projectStartEndDate.Item1)
                        startDate = new DateTime(projectStartEndDate.Item1.Year, projectStartEndDate.Item1.Month, 1);
                    if (endDate < projectStartEndDate.Item2)
                        endDate = projectStartEndDate.Item2;
                }
            }

            return ChronologicalHelpers.GenerateEndDatesCollection(startDate, endDate);
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(PROJECTTenderProfile projection, PROJECT entity, bool isNewEntity)
        {
        }

        //when new row is initiated a new instance of projection needs to be instantiated for cell update, if not cellvaluechanged won't have any values
        public void ParentInitNewRow(InitNewRowEventArgs e)
        {
            var gridView = (TableView)e.OriginalSource;
            var grid = gridView.Grid;
            DataRowView dataRowView = (DataRowView)grid.GetRow(e.RowHandle);

            if (dataRowView[columnTenderProfile] == DBNull.Value)
            {
                PROJECTTenderProfile newPROJECT = new PROJECTTenderProfile();
                newPROJECT.Entity.STATUS = ProjectStatus.Tender;
                dataRowView[columnTenderProfile] = newPROJECT;
            }
        }

        public void ChildInitNewRow(InitNewRowEventArgs e)
        {
            var detailGridView = (TableView)e.OriginalSource;
            GridControl detailGrid = detailGridView.Grid;
            PROJECTTenderProfile PROJECTTenderProfile = getPROJECTTenderProfileFromParentGrid(detailGridView);
            DataRowView detailDataRowView = (DataRowView)detailGrid.GetRow(e.RowHandle);

            if (detailDataRowView[columnTenderProfile] == DBNull.Value && PROJECTTenderProfile != null)
            {
                TENDER_PROFILE_ITEM newTENDER_PROFILE_ITEM = new TENDER_PROFILE_ITEM();

                if(PROJECTTenderProfile.TenderProfile == null)
                    PROJECTTenderProfile.TenderProfile = findExistingOrAddTenderProfile(PROJECTTenderProfile.Entity, _bluePrintsUnitOfWork);

                newTENDER_PROFILE_ITEM.GUID_TENDER_PROFILE = PROJECTTenderProfile.TenderProfile.GUID;
                detailDataRowView[columnTenderProfile] = newTENDER_PROFILE_ITEM;
            }
        }

        private PROJECTTenderProfile getPROJECTTenderProfileFromParentGrid(TableView detailGridView)
        {
            GridControl detailGrid = detailGridView.Grid;
            var masterGrid = (detailGrid.OwnerDetailDescriptor as DataControlDetailDescriptor).Parent as GridControl;
            int masterRowHandle = detailGridView.Grid.GetMasterRowHandle();
            if (masterGrid != null)
            {
                DataRowView masterDataRowView = (DataRowView)masterGrid.GetRow(masterRowHandle);
                PROJECTTenderProfile PROJECTTenderProfile = (PROJECTTenderProfile)masterDataRowView[columnProject];
                return PROJECTTenderProfile;
            }

            return null;
        }

        public void ParentValidateCell(GridCellValidationEventArgs e)
        {
            DataRow dataRow = ((DataRowView)e.Row).Row;
            string errorMessage = UnifiedValueValidation((PROJECTTenderProfile)dataRow[columnProject], e.Column.FieldName, e.Value, false);

            if (errorMessage != string.Empty)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = errorMessage;
            }
        }

        public void ChildValidateCell(GridCellValidationEventArgs e)
        {
            DataRow dataRow = ((DataRowView)e.Row).Row;
            string errorMessage = UnifiedChildValueValidation((TENDER_PROFILE_ITEM)dataRow[columnTenderProfile], e.Column.FieldName, e.Value, false);

            if (errorMessage != string.Empty)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = errorMessage;
            }
        }

        public void ParentValidateRow(GridRowValidationEventArgs e)
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
            if (e.RowHandle != DataControlBase.NewItemRowHandle)
            {
                DataRowView dataRowView = (DataRowView)e.Row;
                if (e.RowHandle == GridControl.AutoFilterRowHandle)
                    return;

                string fieldName = e.Column.FieldName;
                commitParentCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value);
            }

            e.Handled = true;
        }

        public void ParentNewRowAddUndoAndSave(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

                DataRowView row = (DataRowView)e.Row;
                PROJECTTenderProfile newPROJECT = (PROJECTTenderProfile)row[columnProject];
                _bluePrintsUnitOfWork.PROJECTS.Add(newPROJECT.Entity);
                _bluePrintsUnitOfWork.SaveChanges();
                newPROJECT.TenderProfile = findExistingOrAddTenderProfile(newPROJECT.Entity, _bluePrintsUnitOfWork);
                BluePrintsDataUtils.CreateNewProjectDefaults(newPROJECT.Entity, _bluePrintsUnitOfWork);
                BuildRowStats(newPROJECT, true);
            }
        }

        public void ChildNewRowAddUndoAndSave(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

                DataRowView detailRow = (DataRowView)e.Row;
                TENDER_PROFILE_ITEM newTENDER_PROFILE_ITEM = (TENDER_PROFILE_ITEM)detailRow[columnTenderProfile];
                _bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Add(newTENDER_PROFILE_ITEM);
                _bluePrintsUnitOfWork.SaveChanges();

                var detailGridView = (TableView)e.OriginalSource;
                PROJECTTenderProfile PROJECTTenderProfile = getPROJECTTenderProfileFromParentGrid(detailGridView);
                if (PROJECTTenderProfile != null)
                {
                    //reconstruct the project because new tender profile item has been added
                    PROJECTTenderProfile = populateTenderProfiles(PROJECTTenderProfile.Entity, _bluePrintsUnitOfWork);
                    BuildRowStats(PROJECTTenderProfile, true);
                }
            }
        }

        private TENDER_PROFILE findExistingOrAddTenderProfile(PROJECT project, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork)
        {
            TENDER_PROFILE TENDER_PROFILE = _bluePrintsUnitOfWork.TENDER_PROFILES.FirstOrDefault(x => x.GUID_PROJECT == project.GUID);
            if(TENDER_PROFILE == null)
            {
                TENDER_PROFILE = new TENDER_PROFILE();
                TENDER_PROFILE.NAME = BluePrintsResources.Default_TenderProfile_Name;
                TENDER_PROFILE.GUID_PROJECT = project.GUID;
                bluePrintsEntitiesUnitOfWork.TENDER_PROFILES.Add(TENDER_PROFILE);
                bluePrintsEntitiesUnitOfWork.SaveChanges();
            }

            return TENDER_PROFILE;
        }

        public void ChildCellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle != DataControlBase.NewItemRowHandle)
            {
                DataRowView dataRowView = (DataRowView)e.Row;
                if (e.RowHandle == GridControl.AutoFilterRowHandle)
                    return;

                string fieldName = e.Column.FieldName;
                commitChildCellValue(e.Column.FieldName, dataRowView.Row, e.OldValue, e.Value);
            }

            e.Handled = true;
        }

        protected virtual void commitParentCellValue(string fieldName, DataRow row, object oldValue, object newValue, bool skipSaveChangesAndRowUpdate = false)
        {
            PROJECTTenderProfile PROJECTTenderProfile = ((PROJECTTenderProfile)row[columnProject]);
            IEnumerable<TENDER_PROFILE_ITEM> tenderProfileItems = PROJECTTenderProfile.TENDER_PROFILE_ITEMS;

            fieldName = formatParentFieldName(fieldName);
            string tenderHoursFieldName = BindableBase.GetPropertyName(() => new PROJECTTenderProfile().TenderProfile.TENDER_HOURS);

            if (fieldName.Contains(tenderHoursFieldName))
            {
                if (DataUtils.TrySetNestedValue(tenderHoursFieldName, PROJECTTenderProfile.TenderProfile, newValue))
                {
                    _bluePrintsUnitOfWork.SaveChanges();
                    onDataPointsCalculated(PROJECTTenderProfile);
                }
            }
            else
            {
                if(DataUtils.TrySetNestedValue(fieldName, PROJECTTenderProfile.Entity, newValue))
                {
                    _bluePrintsUnitOfWork.SaveChanges();
                    onDataPointsCalculated(PROJECTTenderProfile);
                }
            }
        }

        /// <summary>
        /// return the actual field name
        /// </summary>
        private string formatParentFieldName(string fieldName)
        {
            return fieldName.Replace(columnProject + ".Entity.", "");
        }

        /// <summary>
        /// return the actual field name
        /// </summary>
        private string formatChildFieldName(string fieldName)
        {
            return fieldName.Replace(columnTenderProfile + ".", "");
        }

        protected virtual void commitChildCellValue(string fieldName, DataRow row, object oldValue, object newValue, bool skipSaveChangesAndRowUpdate = false)
        {
            TENDER_PROFILE_ITEM tenderProfileItem = ((TENDER_PROFILE_ITEM)row[columnTenderProfile]);
            PROJECTTenderProfile PROJECTTenderProfile = tenderProfileItem.PROJECTTenderProfile;
            string formattedFieldName = fieldName.Replace(columnTenderProfile + ".", "");
            if(DataUtils.TrySetNestedValue(formattedFieldName, tenderProfileItem, newValue))
            {
                _bluePrintsUnitOfWork.SaveChanges();
                onDataPointsCalculated(tenderProfileItem.PROJECTTenderProfile);
            }
        }

        private void onTenderProfileSelectionViewModelWrapperLoaded(IEnumerable<TENDER_PROFILE_ITEM> tenderProfileItems, object invocationParent)
        {
            TENDER_PROFILE_ITEMSelectionViewModelWrapper tenderProfileSelectionViewModelWrapper = (TENDER_PROFILE_ITEMSelectionViewModelWrapper)invocationParent;
            tenderProfileSelectionViewModelWrapper.PopulateTenderDeliverables();
        }

        private void onDataPointsCalculated(PROJECTTenderProfile project)
        {
            project = populateTenderProfiles(project.Entity, _bluePrintsUnitOfWork);
            //detect whether datatable needs to be reinstantiated
            Tuple<DateTime, DateTime> projectStartEndDate = BluePrintsDataUtils.GetTenderStartEndDate(project.Entity);

            bool shouldRefreshGrid = false;
            foreach(DateTime dataPointsDate in dataPointsDateCollection)
            {
                if (!alignedDateCollection.Any(x => x.Year == dataPointsDate.Year && x.Month == dataPointsDate.Month))
                {
                    shouldRefreshGrid = true;
                    break;
                }
            }

            foreach(DateTime alignDate in alignedDateCollection)
            {
                if (!dataPointsDateCollection.Any(x => x.Year == alignDate.Year && x.Month == alignDate.Month))
                {
                    shouldRefreshGrid = true;
                    break;
                }
            }

            if(shouldRefreshGrid)
            {
                //refresh the entire grid
                realignDateCollectionOnDataTable(dataPointsTable, dataPointsDateCollection);

                InitializeParentColumnSource(ParentColumns, ParentSummaries, dataPointsDateCollection);
                InitializeChildColumnSource(ChildColumns, ChildSummaries, dataPointsDateCollection);

                this.RaisePropertyChanged(x => x.ChildColumns);
                this.RaisePropertyChanged(x => x.ChildSummaries);
                this.RaisePropertyChanged(x => x.ParentColumns);
                this.RaisePropertyChanged(x => x.ParentSummaries);
            }

            //refresh only the row
            BuildRowStats(project, true);
        }

        private void refreshDataPointsTable()
        {
            alignedDateCollection = generateDates(Entities.Select(x => x.Entity));
            ParentColumns.Clear();
            ChildColumns.Clear();
            ParentSummaries.Clear();
            ChildSummaries.Clear();
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        private void realignColumnDescription(List<DateTime> alignedDataDates, ObservableCollection<ColumnDescriptor> columnDescriptors, ObservableCollection<SummaryDescriptor> summaryDescriptors)
        {
            //List<ColumnDescriptor> removeColumns = new List<ColumnDescriptor>();
            //foreach (ColumnDescriptor columnDescriptor in columnDescriptors)
            //{
            //    DateTime dateTime;
            //    if (DateTime.TryParse(columnDescriptor.FieldName, out dateTime))
            //    {
            //        if (!alignedDataDates.Any(x => x.Date == dateTime.Date))
            //        {
            //            removeColumns.Add(columnDescriptor);
            //        }
            //    }
            //}

            //foreach (ColumnDescriptor removeColumn in removeColumns)
            //{
            //    SummaryDescriptor summaryDescriptor = summaryDescriptors.FirstOrDefault(x => x.FieldName == removeColumn.FieldName);
            //    if (summaryDescriptor != null)
            //        summaryDescriptors.Remove(summaryDescriptor);
            //    columnDescriptors.Remove(removeColumn);
            //}


            //foreach (DateTime alignedDate in alignedDataDates.OrderByDescending(x => x))
            //{
            //    string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
            //    if(!columnDescriptors.Where(x => x.ColumnDate != null).Any(x => x.FieldName == columnFieldName))
            //    {
            //        ColumnDescriptor earliestColumnDescriptor = columnDescriptors.Where(x => x.ColumnDate != null).OrderBy(x => x.ColumnDate).FirstOrDefault(x => x.ColumnDate > alignedDate);
            //        if(earliestColumnDescriptor != null)
            //        {
            //            columnDescriptors.Add(new ColumnDescriptor() { FieldName = columnFieldName, Mask = "n2", ColumnDate = alignedDate, VisibleIndex = earliestColumnDescriptor.VisibleIndex -1, ReadOnly = true, Header = columnFieldName, Width = 60, Settings = SettingsType.Number });
            //            summaryDescriptors.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n2", Type = SummaryItemType.Sum });
            //        }
            //    }
            //}
        }

        private void realignDateCollectionOnDataTable(DataTable dataTable, IEnumerable<DateTime> alignedDataDates)
        {
            List<DataColumn> removeColumns = new List<DataColumn>();
            foreach(DataColumn dataColumn in dataTable.Columns)
            {
                DateTime dateTime; 
                if(DateTime.TryParse(dataColumn.ColumnName, out dateTime))
                {
                    if(!alignedDataDates.Any(x => x.Date == dateTime.Date))
                    {
                        removeColumns.Add(dataColumn);
                    }
                }
            }

            foreach(DataColumn removeColumn in removeColumns)
            {
                dataTable.Columns.Remove(removeColumn);
            }

            foreach(DateTime alignedDataDate in alignedDataDates)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                DataColumn dataColumn = findDataColumn(dataTable, columnFieldName);
                if(dataColumn == null)
                {
                    if(!dataTable.Columns.Contains(columnFieldName))
                    {
                        dataTable.Columns.Add(columnFieldName, typeof(decimal));
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            DataTable tenderProfilesDataPointsTable = (DataTable)dataRow[columnTenderProfileDataTable];
                            tenderProfilesDataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                        }
                    }
                }
            }
        }

        private DataColumn findDataColumn(DataTable dataTable, string fieldName)
        {
            foreach(DataColumn dataColumn in dataTable.Columns)
            {
                if (dataColumn.ColumnName == fieldName)
                    return dataColumn;
            }

            return null;
        }

        public override string UnifiedValueValidation(PROJECTTenderProfile projection, string field_name, object new_value, bool isPaste)
        {
            string fieldName = formatParentFieldName(field_name);
            if (fieldName == BindableBase.GetPropertyName(() => new PROJECT().NUMBER))
            {
                if(new_value != null)
                    if (_bluePrintsUnitOfWork.PROJECTS.Any(x => x.NUMBER == new_value.ToString()))
                    {
                        return "Project number already exists";
                    }
            }

            return string.Empty;
        }

        public string UnifiedChildValueValidation(TENDER_PROFILE_ITEM TENDER_PROFILE_ITEM, string field_name, object new_value, bool isPaste)
        {
            string fieldName = formatChildFieldName(field_name);
            if (fieldName == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().GUID_DEPARTMENT) || fieldName == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().GUID_DISCIPLINE))
            {
                if (new_value != null)
                {
                    if (fieldName == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().GUID_DEPARTMENT))
                    {
                        Guid Guid_Department = (Guid)new_value;
                        DEPARTMENT DEPARTMENT = DEPARTMENTCollection.FirstOrDefault(x => x.GUID == Guid_Department);
                        if (DEPARTMENT != null)
                        {
                            if (_bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Any(x => x.GUID_TENDER_PROFILE == TENDER_PROFILE_ITEM.GUID_TENDER_PROFILE && x.GUID_DEPARTMENT == DEPARTMENT.GUID && x.GUID_DISCIPLINE == TENDER_PROFILE_ITEM.GUID_DISCIPLINE))
                            {
                                if (TENDER_PROFILE_ITEM.GUID_DISCIPLINE != null)
                                {
                                    DISCIPLINE DISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == TENDER_PROFILE_ITEM.GUID_DISCIPLINE);
                                    if (DISCIPLINE != null)
                                        return "Department: " + DEPARTMENT.NAME + " and Discipline: " + DISCIPLINE.NAME + " already exists for this project";
                                }

                                return "Department: " + DEPARTMENT.NAME + " already exists for this project";
                            }
                        }
                    }
                    else if (fieldName == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().GUID_DISCIPLINE))
                    {
                        Guid Guid_DISCIPLINE = (Guid)new_value;
                        DISCIPLINE DISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == Guid_DISCIPLINE);
                        if (DISCIPLINE != null)
                        {
                            if (_bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Any(x => x.GUID_TENDER_PROFILE == TENDER_PROFILE_ITEM.GUID_TENDER_PROFILE && x.GUID_DISCIPLINE == DISCIPLINE.GUID && x.GUID_DEPARTMENT == TENDER_PROFILE_ITEM.GUID_DEPARTMENT))
                            {
                                if (TENDER_PROFILE_ITEM.GUID_DEPARTMENT != null)
                                {
                                    DEPARTMENT DEPARTMENT = DEPARTMENTCollection.FirstOrDefault(x => x.GUID == TENDER_PROFILE_ITEM.GUID_DEPARTMENT);
                                    if (DISCIPLINE != null)
                                        return "Department: " + DEPARTMENT.NAME + " and Discipline: " + DISCIPLINE.NAME + " already exists for this project";
                                }

                                return "Discipline: " + DISCIPLINE.NAME + " already exists for this project";
                            }
                        }
                    }
                }
            }

            return string.Empty;
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

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECTTenderProfile> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public DataRowView SelectedParentDataRow { get; set; }
        ObservableCollection<DataRowView> selectedParentDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedParentDataRows
        {
            get
            {
                if (selectedParentDataRows == null)
                    selectedParentDataRows = new ObservableCollection<DataRowView>();

                return selectedParentDataRows;
            }
            set
            {
                selectedParentDataRows = value;
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
                    dataPointsTable.RowChanged += DataPointsTable_RowChanged;

                    if (ParentColumns.Count() == 0)
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

        List<DataRowView> newlyAddedRows;
        private void DataPointsTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                if (IsLoading)
                    return;

                int rowIndex = DataPointsTable.Rows.IndexOf(e.Row);
                if (rowIndex >= 0)
                {
                    DataRowView dataRowView = DataPointsTable.DefaultView[DataPointsTable.Rows.IndexOf(e.Row)];
                    OnAfterNewProjectionsAdded(dataRowView);
                }
            }
        }

        protected virtual void OnAfterNewProjectionsAdded(DataRowView newRow)
        {
            if (newRow != null)
            {
                if (newlyAddedRows == null)
                    newlyAddedRows = new List<DataRowView>();

                newlyAddedRows.Add(newRow);
                //Uncomment this to allow grid to focus on new row
                focusNewlyAddedProjectionTimer.Tick -= FocusNewlyAddedProjectionTimer_Tick;
                focusNewlyAddedProjectionTimer.Tick += FocusNewlyAddedProjectionTimer_Tick;
                focusNewlyAddedProjectionTimer.Start();
            }
        }

        private void FocusNewlyAddedProjectionTimer_Tick(object sender, EventArgs e)
        {
            focusNewlyAddedProjectionTimer.Stop();
            if (Entities == null || newlyAddedRows == null || newlyAddedRows.Count() == 0)
                return;

            List<DataRowView> selectedRows = new List<DataRowView>();
            foreach (DataRowView newlyAddedRow in newlyAddedRows)
            {
                selectedRows.Add(newlyAddedRow);
            }

            newlyAddedRows.Clear();
            SelectedParentDataRows?.Clear();
            foreach (DataRowView selectedRow in selectedRows)
            {
                SelectedParentDataRows?.Add(selectedRow);
            }

            if (selectedRows.Count > 0)
            {
                SelectedParentDataRow = selectedRows.Last();
                this.RaisePropertyChanged(x => x.SelectedParentDataRows);
                this.RaisePropertyChanged(x => x.SelectedParentDataRow);
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
            List<Common.ViewModel.Reporting.DataPoint> projectDataPoints = entity.TENDER_PROFILE_ITEMS == null ? new List<Common.ViewModel.Reporting.DataPoint>() : entity.TENDER_PROFILE_ITEMS.Where(x => x.DataPoints != null).SelectMany(x => x.DataPoints).ToList();
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
                        newDataRow[columnName] = DBNull.Value;
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
                                tenderProfileDataRow[columnName] = DBNull.Value;
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

            int visibleIndex = 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.NUMBER", VisibleIndex = visibleIndex, Header = "Number", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            visibleIndex += 10;
            summaries.Add(new SummaryDescriptor() { FieldName = columnProject + ".Entity.NUMBER", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.NAME", VisibleIndex = visibleIndex, Header = "Name", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.STATUS", VisibleIndex = visibleIndex, Header = "Status", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum1 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_TYPE", VisibleIndex = visibleIndex, Header = "Type", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum2 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_DIVISION", VisibleIndex = visibleIndex, Header = "Division", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum3 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_COMMODITY", VisibleIndex = visibleIndex, Header = "Commodity", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum4 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_CONTRACT", VisibleIndex = visibleIndex, Header = "Contract", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum5 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_STATUS", VisibleIndex = visibleIndex, Header = "Status", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum6 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".TenderProfile.TENDER_HOURS", VisibleIndex = visibleIndex, Header = "Tender Hours", Fixed = FixedStyle.Left, Mask = "n", Increment = 1, Width = 70, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.TENDER_PROJECT_START", VisibleIndex = visibleIndex, Header = "Start Date", ReadOnly = false, Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Date });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.TENDER_PROJECT_DURATION", VisibleIndex = visibleIndex, ReadOnly = false, Visible = true, Header = "Duration", Mask = "###,##0 Weeks", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_GROSS_PROFIT", VisibleIndex = visibleIndex, ReadOnly = false, Visible = true, Header = "Gross Profit", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_TOTAL_VALUE", VisibleIndex = visibleIndex, ReadOnly = false, Visible = true, Header = "Total Value", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_SCOPE_PCT", VisibleIndex = visibleIndex, ReadOnly = false, Visible = true, Header = "Scope %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Mask = "n2", VisibleIndex = visibleIndex, ColumnDate = alignedDate, ReadOnly = true, Header = columnFieldName, Width = 60, Settings = SettingsType.Number });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n2", Type = SummaryItemType.Sum });
                visibleIndex += 10;
            }
        }

        private void InitializeChildColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            int visibleIndex = 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".GUID_DEPARTMENT", VisibleIndex = visibleIndex, Header = "Department", DisplayMember = "NAME", ValueMember = "GUID", Fixed = FixedStyle.Left, Width = 70, ItemsSource = DEPARTMENTCollection, Settings = SettingsType.Collection });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".GUID_DISCIPLINE", VisibleIndex = visibleIndex, Tag = "Start Date", Header = "Discipline", DisplayMember = "NAME", ValueMember = "GUID", Fixed = FixedStyle.Left, Width = 70, ItemsSource = DISCIPLINECollection, Settings = SettingsType.Collection });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".HOURS_PERCENTAGE", VisibleIndex = visibleIndex, Tag = "Duration", Header = "Hours %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".SCHEDULE_START_PERCENTAGE", VisibleIndex = visibleIndex, Tag = "Gross Profit", Header = "Schedule Start %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".SCHEDULE_FINISH_PERCENTAGE", VisibleIndex = visibleIndex, Tag = "Total Value", Header = "Schedule Finish %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".BELLCURVESHAPE", VisibleIndex = visibleIndex, Tag = "Scope %", Header = "Bell Curve", Fixed = FixedStyle.Left, Width = 70, ItemsSource = BellCurveShapeCollection, Settings = SettingsType.Collection });
            visibleIndex += 10;

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Mask = "n2", VisibleIndex = visibleIndex, ColumnDate = alignedDate, ReadOnly = true, Header = columnFieldName, Tag = columnFieldName, Width = 60, Settings = SettingsType.Number });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n2", Type = SummaryItemType.Sum });
                visibleIndex += 10;
            }
        }

        public void DeleteSelectedProjects()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to delete " + SelectedParentDataRows.Count + " selected entries?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<DataRow> removeRows = new List<DataRow>();
            foreach (DataRowView selectedRow in SelectedParentDataRows)
            {
                deleteRow(selectedRow, _bluePrintsUnitOfWork);
                removeRows.Add(selectedRow.Row);
            }

            foreach (DataRow removeRow in removeRows)
                DataPointsTable.Rows.Remove(removeRow);
        }

        private void deleteRow(DataRowView selectedRow, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork)
        {
            PROJECTTenderProfile findPROJECT = (PROJECTTenderProfile)selectedRow[columnProject];
            if (findPROJECT != null && findPROJECT.Entity != null)
            {
                findPROJECT.Entity.DELETED = DateTime.Now;
                findPROJECT.Entity.DELETEDBY = LoginCredentials.CurrentUserGuid;
                bluePrintsEntitiesUnitOfWork.SaveChanges();
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
        public WorkingDaysDate PeriodWorkingDate { get; set; }
        public double PeriodNumber { get; set; }
        public decimal BellCurveFactor { get; set; }
    }

    public class WorkingDaysDateList : List<WorkingDaysDate>
    {
        decimal workingDaysDeficit = 0;
        public void AddWorkingDays(WorkingDaysDate workingDays)
        {
            this.Add(workingDays);
            if (workingDays.Prorate < 1)
                workingDaysDeficit = 1 - workingDays.Prorate;

            List<WorkingDaysDate> workingDaysWithoutDeficit = this.Where(x => x.Prorate >= 1).ToList();
            decimal inflationFactor = workingDaysDeficit / workingDaysWithoutDeficit.Count();
            workingDaysWithoutDeficit.ForEach(x => x.Prorate = 1 + inflationFactor);
        }
    }

    //dates are always generated as start of the month, when the actual date is middle of the month prorate will be applied
    public class WorkingDaysDate
    {
        public WorkingDaysDate(DateTime date, bool isStartDate)
        {
            DateTime monthBeginningDate = new DateTime(date.Year, date.Month, 1);
            DateTime monthEndDate = monthBeginningDate.AddMonths(1).AddDays(-1);
            Date = date;

            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            double workingDays = 0;
            if (isStartDate)
                workingDays = (monthEndDate - date).TotalDays;
            else
                workingDays = (date - monthBeginningDate).TotalDays;

            //include adjustment for subtraction that doesn't include the entire day
            workingDays += 1;
            double dblDaysInMonth = Convert.ToDouble(daysInMonth);
            Prorate = Convert.ToDecimal(workingDays / daysInMonth);
        }

        public DateTime Date { get; set; }
        public decimal Prorate { get; set; }
    }
}