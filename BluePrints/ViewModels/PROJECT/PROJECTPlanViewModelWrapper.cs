using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.Services;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static BaseModel.Data.Helpers.DataUtils;

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
        BackgroundWorker createProjectBackgroundWorker;
        protected override void resolveParameters(object parameter)
        {
            IsLoading = true;
            primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
            _bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            doNotApplyBestFit = true;
            focusNewlyAddedProjectionTimer = new DispatcherTimer();
            focusNewlyAddedProjectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            createProjectBackgroundWorker = new BackgroundWorker();
            createProjectBackgroundWorker.DoWork += CreateProjectBackgroundWorker_DoWork;
            createProjectBackgroundWorker.RunWorkerCompleted += CreateProjectBackgroundWorker_RunWorkerCompleted;
            createProjectBackgroundWorker.WorkerSupportsCancellation = true;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
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

        /// <summary>
        /// creates the projection
        /// </summary>
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
                PROJECTTenderProfile.Deliverables = _bluePrintsUnitOfWork.BASELINE_ITEMS.Where(x => x.GUID_BASELINE == projectLiveBaseline.GUID);
                PROJECTTenderProfile.ResetDeliverableList();
                return PROJECTTenderProfile;
            }

            return null;
        }
        #endregion

        #region Helpers
        //populate the view model
        private void populateDataPoints(PROJECTTenderProfile PROJECTTenderProfile)
        {
            if (PROJECTTenderProfile.Entity.TENDER_PROJECT_START == null || PROJECTTenderProfile.Entity.TENDER_PROJECT_DURATION == null)
                return;

            int totalDurationInDays = BluePrintsDataUtils.GetTenderDuration(PROJECTTenderProfile.Entity);

            Tuple<DateTime, DateTime> startEndDates = BluePrintsDataUtils.GetTenderStartEndDate(PROJECTTenderProfile.Entity);
            DateTime startDate = startEndDates.Item1;
            DateTime endDate = startEndDates.Item2;

            //always start from zero since we are generating forecast from the beginning
            double beginPercentage = 0;

            //when it's first used when form loads entities is empty
            dataPointsDateCollection = Entities.Count == 0 ? alignedDateCollection : generateDates(Entities.Select(x => x.Entity));
            foreach (TENDER_PROFILE_ITEM TENDER_PROFILE_ITEM in PROJECTTenderProfile.TENDER_PROFILE_ITEMS)
            {
                Tuple<DateTime, DateTime> profileStartEndDate = TENDER_PROFILE_ITEM.GetProRatedStartEndDate(totalDurationInDays, startDate, endDate);
                decimal assignHours = PROJECTTenderProfile.TenderProfile.TENDER_HOURS * TENDER_PROFILE_ITEM.HOURS_PERCENTAGE;
                //pro-rate the dates of the deliverable based on tender item
                DateTime proRatedStartDate = profileStartEndDate.Item1;
                DateTime proRatedEndDate = profileStartEndDate.Item2;

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

            PROJECTTenderProfile.ResetDeliverableList();
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
                if ((date > endOfStartDateMonth) && (date <= previousEndOfEndDateMonth))
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

        public void SyncDeliverables()
        {
            if (SelectedParentDataRow == null)
            {
                MessageBoxService.ShowMessage("Please select a project", "Select Project", MessageButton.OK);
                return;
            }

            PROJECTTenderProfile projectTenderProfile = ((PROJECTTenderProfile)SelectedParentDataRow[columnProject]);
            PROJECT project = _bluePrintsUnitOfWork.PROJECTS.FirstOrDefault(x => x.GUID == projectTenderProfile.Entity.GUID);
            if (project == null)
            {
                MessageBoxService.ShowMessage("Project doesn't exist", "Error", MessageButton.OK);
                return;
            }

            if (_bluePrintsUnitOfWork.DOCTYPES.Count() == 0)
            {
                MessageBoxService.ShowMessage("Please add document type in system", "Error", MessageButton.OK);
                return;
            }

            IEnumerable<AREA> projectAREACollection = _bluePrintsUnitOfWork.AREAS.Where(x => x.GUID_PROJECT == project.GUID);
            if (projectAREACollection.Count() == 0)
            {
                MessageBoxService.ShowMessage("Project doesn't have any area", "Error", MessageButton.OK);
                return;
            }

            IEnumerable<SUBJOB> projectSUBJOBCollection = _bluePrintsUnitOfWork.SUBJOBS.Where(x => x.GUID_PROJECT == project.GUID);
            if (projectSUBJOBCollection.Count() == 0)
            {
                MessageBoxService.ShowMessage("Project doesn't have any subjob", "Error", MessageButton.OK);
                return;
            }

            BASELINE projectLiveBASELINE = _bluePrintsUnitOfWork.BASELINES.FirstOrDefault(x => x.GUID_PROJECT == project.GUID && x.STATUS == BaselineStatus.Live);
            if (projectLiveBASELINE == null)
            {
                MessageBoxService.ShowMessage("Project doesn't have any live baseline", "Error", MessageButton.OK);
                return;
            }

            IEnumerable<PROGRESS> projectLivePROGRESSES = _bluePrintsUnitOfWork.PROGRESSES.Where(x => x.GUID_PROJECT == project.GUID && x.STATUS == ProgressStatus.Live);
            if (projectLivePROGRESSES.Count() == 0)
            {
                MessageBoxService.ShowMessage("Project doesn't have any live progress", "Error", MessageButton.OK);
                return;
            }

            if (projectTenderProfile.Entity.TENDER_PROJECT_START == null)
            {
                MessageBoxService.ShowMessage("Please set project start date", "Error", MessageButton.OK);
                return;
            }
            else if (projectTenderProfile.Entity.TENDER_PROJECT_DURATION == null)
            {
                MessageBoxService.ShowMessage("Please set project duration", "Error", MessageButton.OK);
                return;
            }

            IEnumerable<BASELINE_ITEM> projectDeliverables = _bluePrintsUnitOfWork.BASELINE_ITEMS.Where(x => x.GUID_BASELINE == projectLiveBASELINE.GUID);
            UICommand fromPlannedCommand = new UICommand()
            {
                Id = ProjectPlanSyncAction.UnitsFromPlanning,
                Caption = "Units from Plan",
                IsCancel = true,
                IsDefault = false,
            };

            UICommand fromDeliverablesCommand = new UICommand()
            {
                Id = ProjectPlanSyncAction.UnitsFromDeliverables,
                Caption = "Units from Deliverables",
                IsCancel = true,
                IsDefault = false,
            };

            string message = String.Format("Do you wish to use budgeted units from plan\nor from deliverable(s)");

            BasicMessageBoxViewModel viewModel = BasicMessageBoxViewModel.Create(message);
            viewModel.CheckboxVisibility = Visibility.Hidden;
            UICommand result = BasicMessageBoxDialogService.ShowDialog(new List<UICommand>() { fromPlannedCommand, fromDeliverablesCommand }, "Please choose which budgeted units to use", "BasicMessageBox", viewModel);
            if (result == fromPlannedCommand)
            {
                syncDeliverables(false, projectTenderProfile, projectDeliverables, projectAREACollection, projectSUBJOBCollection, projectLiveBASELINE, projectLivePROGRESSES);
            }
            else if (result == fromDeliverablesCommand)
            {
                syncDeliverables(true, projectTenderProfile, projectDeliverables, projectAREACollection, projectSUBJOBCollection, projectLiveBASELINE, projectLivePROGRESSES);
            }
        }

        private void syncDeliverables(bool useDeliverablesHours, PROJECTTenderProfile projectTenderProfile, IEnumerable<BASELINE_ITEM> projectDeliverables, IEnumerable<AREA> projectAREACollection, IEnumerable<SUBJOB> projectSUBJOBCollection, BASELINE projectLiveBASELINE, IEnumerable<PROGRESS> projectLivePROGRESSES)
        {
            List<DeliverableEditModel> deliverableEditModels = new List<DeliverableEditModel>();
            decimal totalDeliverablesHours = projectDeliverables.Sum(x => x.BUDGET_HOURS);
            if (useDeliverablesHours)
                projectTenderProfile.TenderProfile.TENDER_HOURS = totalDeliverablesHours;

            var deliverableGroup = projectDeliverables.GroupBy(x => new { x.GUID_DEPARTMENT, x.GUID_DISCIPLINE }).Select(group => new { group.Key.GUID_DEPARTMENT, group.Key.GUID_DISCIPLINE, BUDGET_HOURS = group.Sum(x => x.BUDGET_HOURS) });

            //edit tender profile items based on deliverables
            foreach (var deliverables in deliverableGroup)
            {
                if (deliverables.GUID_DEPARTMENT == null || deliverables.GUID_DISCIPLINE == null)
                    continue;

                //add tender profile item when department and discipline doesn't exist
                IEnumerable<TENDER_PROFILE_ITEM> findTENDER_PROFILE_ITEMS = projectTenderProfile.TENDER_PROFILE_ITEMS.Where(x => x.GUID_DEPARTMENT == deliverables.GUID_DEPARTMENT && x.GUID_DISCIPLINE == deliverables.GUID_DISCIPLINE);
                TENDER_PROFILE_ITEM findTENDER_PROFILE_ITEM;
                if (findTENDER_PROFILE_ITEMS.Count() == 0)
                {
                    findTENDER_PROFILE_ITEM = new TENDER_PROFILE_ITEM();
                    findTENDER_PROFILE_ITEM.GUID = Guid.Empty;
                    findTENDER_PROFILE_ITEM.GUID_TENDER_PROFILE = projectTenderProfile.TenderProfile.GUID;
                    findTENDER_PROFILE_ITEM.PROJECTTenderProfile = projectTenderProfile;
                    findTENDER_PROFILE_ITEM.GUID_DEPARTMENT = (Guid)deliverables.GUID_DEPARTMENT;
                    findTENDER_PROFILE_ITEM.GUID_DISCIPLINE = (Guid)deliverables.GUID_DISCIPLINE;
                    findTENDER_PROFILE_ITEM.SCHEDULE_START_PERCENTAGE = 0;
                    findTENDER_PROFILE_ITEM.SCHEDULE_FINISH_PERCENTAGE = 1;

                    if (useDeliverablesHours)
                    {
                        decimal hourPercentage = totalDeliverablesHours == 0 ? 0 : deliverables.BUDGET_HOURS / totalDeliverablesHours;
                        findTENDER_PROFILE_ITEM.HOURS_PERCENTAGE = hourPercentage;
                    }
                    else
                        findTENDER_PROFILE_ITEM.HOURS_PERCENTAGE = 0;

                    projectTenderProfile.TENDER_PROFILE_ITEMS.Add(findTENDER_PROFILE_ITEM);
                    _bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Add(findTENDER_PROFILE_ITEM);
                }
                else if (useDeliverablesHours && findTENDER_PROFILE_ITEMS.Count() > 0)
                {
                    findTENDER_PROFILE_ITEM = findTENDER_PROFILE_ITEMS.First();
                    decimal hourPercentage = totalDeliverablesHours == 0 ? 0 : deliverables.BUDGET_HOURS / totalDeliverablesHours;
                    findTENDER_PROFILE_ITEM.HOURS_PERCENTAGE = hourPercentage;
                    foreach (TENDER_PROFILE_ITEM tenderProfileItem in findTENDER_PROFILE_ITEMS.Where(x => x.GUID != findTENDER_PROFILE_ITEM.GUID))
                    {
                        if (tenderProfileItem.GUID != Guid.Empty)
                        {
                            projectTenderProfile.TENDER_PROFILE_ITEMS.Remove(tenderProfileItem);
                            _bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Remove(tenderProfileItem);
                        }
                    }
                }
            }

            //edit the deliverables based on tender profile items
            foreach (TENDER_PROFILE_ITEM tenderItem in projectTenderProfile.TENDER_PROFILE_ITEMS)
            {
                decimal assignHours = projectTenderProfile.TenderProfile.TENDER_HOURS * tenderItem.HOURS_PERCENTAGE;
                Guid tenderItemDepartmentGuid = tenderItem.GUID_DEPARTMENT;
                Guid tenderItemDisciplineGuid = tenderItem.GUID_DISCIPLINE;

                //User couldn't proceed to this stage without having the following property validated as not null from PROJECTCollectionView
                DateTime startDate = (DateTime)projectTenderProfile.Entity.TENDER_PROJECT_START;
                decimal tenderDuration = (decimal)projectTenderProfile.Entity.TENDER_PROJECT_DURATION;
                int totalDurationInDays = Convert.ToInt32(tenderDuration * 7);
                DateTime endDate = startDate.AddDays(totalDurationInDays);

                //pro-rate the dates of the deliverable based on tender item
                int startProrateDurationInDays = Convert.ToInt32(totalDurationInDays * tenderItem.SCHEDULE_START_PERCENTAGE);
                DateTime proRatedStartDate = startDate.AddDays(startProrateDurationInDays);
                int endProrateDurationInDays = Convert.ToInt32(totalDurationInDays * (1 - tenderItem.SCHEDULE_FINISH_PERCENTAGE));
                DateTime proRatedEndDate = endDate.AddDays(-1 * endProrateDurationInDays);

                BASELINE_ITEM baseline_item = projectDeliverables.FirstOrDefault(x => x.GUID_DEPARTMENT == tenderItemDepartmentGuid && x.GUID_DISCIPLINE == tenderItemDisciplineGuid);
                if (baseline_item == null)
                {
                    DeliverableEditModel deliverableEditModel = new DeliverableEditModel();

                    deliverableEditModel.Action = RowEditAction.Add;
                    deliverableEditModel.UnitsFrom = 0;
                    deliverableEditModel.UnitsTo = useDeliverablesHours ? 0 : assignHours;
                    deliverableEditModel.Name = "System Generated";


                    deliverableEditModel.DepartmentGuid = tenderItemDepartmentGuid;
                    deliverableEditModel.DisciplineGuid = tenderItemDisciplineGuid;


                    deliverableEditModel.StartDateTo = proRatedStartDate;
                    deliverableEditModel.EndDateTo = proRatedEndDate;
                    deliverableEditModel.BellCurveShape = tenderItem.BELLCURVESHAPE;
                    deliverableEditModels.Add(deliverableEditModel);
                }
                else
                {
                    IEnumerable<BASELINE_ITEM> sameDepartmentDisciplineDeliverables = projectDeliverables.Where(x => x.GUID_DEPARTMENT == tenderItemDepartmentGuid && x.GUID_DISCIPLINE == tenderItemDisciplineGuid);

                    decimal hoursPerDeliverable = assignHours / sameDepartmentDisciplineDeliverables.Count();
                    foreach (BASELINE_ITEM sameDepartmentDisciplineDeliverable in sameDepartmentDisciplineDeliverables)
                    {
                        decimal editDeliverableHours = 0;
                        editDeliverableHours = hoursPerDeliverable;

                        if (sameDepartmentDisciplineDeliverable.BUDGET_HOURS != editDeliverableHours || sameDepartmentDisciplineDeliverable.TENDER_START_DATE != proRatedStartDate || sameDepartmentDisciplineDeliverable.TENDER_END_DATE != proRatedEndDate || sameDepartmentDisciplineDeliverable.BELLCURVESHAPE != tenderItem.BELLCURVESHAPE)
                        {
                            DeliverableEditModel deliverableEditModel = new DeliverableEditModel();

                            deliverableEditModel.DeliverableGuid = sameDepartmentDisciplineDeliverable.GUID;
                            deliverableEditModel.Name = sameDepartmentDisciplineDeliverable.INTERNAL_NUM;
                            deliverableEditModel.StartDateFrom = sameDepartmentDisciplineDeliverable.TENDER_START_DATE;
                            deliverableEditModel.EndDateFrom = sameDepartmentDisciplineDeliverable.TENDER_END_DATE;
                            deliverableEditModel.UnitsFrom = sameDepartmentDisciplineDeliverable.Budget_Units;
                            deliverableEditModel.DepartmentGuid = tenderItemDepartmentGuid;
                            deliverableEditModel.DisciplineGuid = tenderItemDisciplineGuid;

                            deliverableEditModel.BellCurveShape = tenderItem.BELLCURVESHAPE;
                            deliverableEditModel.UnitsTo = useDeliverablesHours ? deliverableEditModel.UnitsFrom : editDeliverableHours;
                            deliverableEditModel.StartDateTo = proRatedStartDate;
                            deliverableEditModel.EndDateTo = proRatedEndDate;

                            deliverableEditModel.Action = RowEditAction.Edit;
                            deliverableEditModels.Add(deliverableEditModel);
                        }
                    }
                }

                LoadingScreenManager.Progress();
            }

            PHASE findPHASE = _bluePrintsUnitOfWork.PHASES.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.Default_Design_Phase);

            //set deliverable hours to zero when there's no tender profile item for it
            if (!useDeliverablesHours)
                foreach (BASELINE_ITEM deliverable in projectDeliverables)
                {
                    if (!projectTenderProfile.TENDER_PROFILE_ITEMS.Any(x => x.GUID_DEPARTMENT == deliverable.GUID_DEPARTMENT && x.GUID_DISCIPLINE == deliverable.GUID_DISCIPLINE))
                    {
                        if (deliverable.BUDGET_HOURS > 0)
                        {
                            DeliverableEditModel deliverableEditModel = new DeliverableEditModel();
                            deliverableEditModel.DeliverableGuid = deliverable.GUID;
                            deliverableEditModel.Action = RowEditAction.ZeroBudget;
                            deliverableEditModel.Name = deliverable.Deliverable_Name;
                            deliverableEditModel.UnitsFrom = deliverable.BUDGET_HOURS;

                            if (!useDeliverablesHours)
                                deliverableEditModel.UnitsTo = 0;

                            deliverableEditModel.StartDateFrom = deliverable.TENDER_START_DATE;
                            deliverableEditModel.EndDateFrom = deliverable.TENDER_END_DATE;
                            deliverableEditModel.StartDateTo = deliverable.TENDER_START_DATE;
                            deliverableEditModel.EndDateTo = deliverable.TENDER_END_DATE;
                            deliverableEditModel.DepartmentGuid = deliverable.Department_Guid;
                            deliverableEditModel.DisciplineGuid = deliverable.Discipline_Guid;
                            deliverableEditModel.BellCurveShape = deliverable.BELLCURVESHAPE;
                            deliverableEditModels.Add(deliverableEditModel);
                        }
                    }

                    populateDeliverableDefaults(deliverable, findPHASE, projectSUBJOBCollection);
                }

            bool isDialogConfirmed = false;
            if (deliverableEditModels.Count > 0)
            {
                DeliverableEditConfirmationViewModel deliverableEditConfirmationViewModel = DeliverableEditConfirmationViewModel.Create(deliverableEditModels, "Please review changes and confirm", DEPARTMENTCollection, DISCIPLINECollection);
                if (DeliverableEditDialogService.ShowDialog(MessageButton.OKCancel, "", "DeliverableEditConfirmation", deliverableEditConfirmationViewModel) == MessageResult.OK)
                {
                    isDialogConfirmed = true;
                    LoadingScreenManager.SetMessage("Writing time phase data to deliverables...");
                    LoadingScreenManager.ShowLoadingScreen(projectTenderProfile.TENDER_PROFILE_ITEMS.Count);
                    foreach (DeliverableEditModel deliverableEditModel in deliverableEditModels)
                    {
                        BASELINE_ITEM baseline_item;
                        if (deliverableEditModel.Action == RowEditAction.Add)
                        {
                            baseline_item = new BASELINE_ITEM();
                            //Default area has been validated before so it's safe to use First()
                            baseline_item.GUID_AREA = projectAREACollection.First().GUID;
                            baseline_item.GUID_DEPARTMENT = deliverableEditModel.DepartmentGuid;
                            baseline_item.GUID_DISCIPLINE = deliverableEditModel.DisciplineGuid;
                            baseline_item.GUID_BASELINE = projectLiveBASELINE.GUID;
                            baseline_item.INTERNAL_NUM = deliverableEditModel.Name;
                            //Doc type has been validated before and it doesn't matter which is used
                            baseline_item.GUID_DOCTYPE = _bluePrintsUnitOfWork.DOCTYPES.First().GUID;
                            _bluePrintsUnitOfWork.BASELINE_ITEMS.Add(baseline_item);
                        }
                        else
                        {
                            baseline_item = _bluePrintsUnitOfWork.BASELINE_ITEMS.First(x => x.GUID == deliverableEditModel.DeliverableGuid);
                        }

                        if (deliverableEditModel.Action == RowEditAction.ZeroBudget)
                            baseline_item.BUDGET_HOURS = 0;
                        else
                            baseline_item.BUDGET_HOURS = deliverableEditModel.UnitsTo;

                        populateDeliverableDefaults(baseline_item, findPHASE, projectSUBJOBCollection);
                        baseline_item.BELLCURVESHAPE = deliverableEditModel.BellCurveShape;
                        baseline_item.TENDER_START_DATE = deliverableEditModel.StartDateTo;
                        baseline_item.TENDER_END_DATE = deliverableEditModel.EndDateTo;

                        LoadingScreenManager.Progress();
                    }
                }
            }
            else
                isDialogConfirmed = true;

            if (isDialogConfirmed)
            {

                //final touches to ensure S-Curve plots from start date
                IEnumerable<BASELINE_ITEM> deliverableWithTenderStartDate = projectDeliverables.Where(x => x.TENDER_START_DATE != null);
                if (deliverableWithTenderStartDate.Count() > 0)
                {
                    foreach (PROGRESS progress in projectLivePROGRESSES)
                    {
                        DateTime progressStartDate = progress.PROGRESS_START;
                        DateTime earliestDeliverableStartDate = deliverableWithTenderStartDate.Min(x => (DateTime)x.TENDER_START_DATE);

                        progress.PROGRESS_START = earliestDeliverableStartDate;

                        //follow the same convention as progress view
                        progress.DATA_DATE = earliestDeliverableStartDate.AddDays(-6).AddSeconds(-1);
                        progress.REPORT_DATE = progress.DATA_DATE;
                        progress.DISABLE_AUTO_REPORT_DATE = true;
                    }
                }

                _bluePrintsUnitOfWork.SaveChanges();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BluePrintsContextHelper.AsyncRefreshDeliverablesDataPointsByProject(projectTenderProfile.Entity.NUMBER);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                reload(projectTenderProfile);

                LoadingScreenManager.CloseLoadingScreen();
                if (useDeliverablesHours)
                    MessageBoxService.ShowMessage("Deliverables sync with time phase from planned, and planned design hours sync with deliverables\n\nPlease note that S-Curve takes about a minute to refresh");
                else
                    MessageBoxService.ShowMessage("Deliverables sync with time phase and hours from planned\n\nPlease note that S-Curve takes about a minute to refresh");
            }
        }

        private void populateDeliverableDefaults(BASELINE_ITEM deliverable, PHASE defaultDesignPhase, IEnumerable<SUBJOB> projectSUBJOBS)
        {
            if (deliverable.GUID_PHASE == null)
            {
                if (defaultDesignPhase != null)
                    deliverable.GUID_PHASE = defaultDesignPhase.GUID;
            }

            bool shouldPopulateSubJob = false;
            if (deliverable.GUID_SUBJOB == null)
                shouldPopulateSubJob = true;
            else
            {
                SUBJOB findSUBJOBByGuid = projectSUBJOBS.FirstOrDefault(x => x.GUID == deliverable.GUID_SUBJOB);
                if (findSUBJOBByGuid == null)
                    shouldPopulateSubJob = true;
            }

            if (shouldPopulateSubJob)
            {
                SUBJOB findSUBJOB = projectSUBJOBS.FirstOrDefault(x => x.GUID_DPHASE == deliverable.GUID_PHASE && x.GUID_DAREA == deliverable.GUID_AREA && x.GUID_DSUBAREA == deliverable.GUID_SUBAREA);
                if (findSUBJOB != null)
                    deliverable.GUID_SUBJOB = findSUBJOB.GUID;
            }
        }

        private Tuple<double, double> getBellCurveProfile(BellCurveShape bellCurveShape)
        {
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
        //when new row is initiated a new instance of projection needs to be instantiated for cell update, if not cellvaluechanged won't have any values
        public void ParentInitNewRow(InitNewRowEventArgs e)
        {
            var gridView = (TableView)e.OriginalSource;
            var grid = gridView.Grid;
            DataRowView dataRowView = (DataRowView)grid.GetRow(e.RowHandle);

            if (dataRowView[columnProject] == DBNull.Value)
            {
                PROJECTTenderProfile newPROJECT = new PROJECTTenderProfile();
                newPROJECT.TENDER_PROFILE_ITEMS = new List<TENDER_PROFILE_ITEM>();
                newPROJECT.Entity.STATUS = ProjectStatus.Lead;
                dataRowView[columnProject] = newPROJECT;
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

                if (PROJECTTenderProfile.TenderProfile == null)
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
            //prevent save layout from setting null values
            if (IsLoading)
                return;

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
                LoadingScreenManager.ShowLoadingScreen(1, true);
                LoadingScreenManager.SetMessage("Creating project defaults, please wait...");

                _bluePrintsUnitOfWork.PROJECTS.Add(newPROJECT.Entity);
                _bluePrintsUnitOfWork.SaveChanges();
                newPROJECT.TenderProfile = findExistingOrAddTenderProfile(newPROJECT.Entity, _bluePrintsUnitOfWork);
                _bluePrintsUnitOfWork.SaveChanges();
                createProjectBackgroundWorker.RunWorkerAsync(newPROJECT);
            }
        }

        private void CreateProjectBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PROJECTTenderProfile newPROJECT = (PROJECTTenderProfile)e.Argument;
            BluePrintsDataUtils.CreateNewProjectDefaults(newPROJECT.Entity, _bluePrintsUnitOfWork);
            e.Result = newPROJECT;

            mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<PROJECT, Guid>(newPROJECT.GUID, Guid.NewGuid(), EntityMessageType.Added))));
        }

        private void CreateProjectBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PROJECTTenderProfile newPROJECT = (PROJECTTenderProfile)e.Result;
            BuildRowStats(newPROJECT, true);
            LoadingScreenManager.CloseLoadingScreen();
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
            if (TENDER_PROFILE == null)
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
            //prevent save layout from setting null values
            if (IsLoading)
                return;

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
                if (DataUtils.TrySetNestedValue(fieldName, PROJECTTenderProfile.Entity, newValue))
                {
                    _bluePrintsUnitOfWork.SaveChanges();
                    onDataPointsCalculated(PROJECTTenderProfile);
                }
            }

            if (fieldName == BindableBase.GetPropertyName(() => new PROJECT().STATUS))
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<PROJECT, Guid>(PROJECTTenderProfile.GUID, Guid.Empty, EntityMessageType.Changed))));
            }
            else
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<PROJECT, Guid>(PROJECTTenderProfile.GUID, Guid.NewGuid(), EntityMessageType.Changed))));
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
            if (DataUtils.TrySetNestedValue(formattedFieldName, tenderProfileItem, newValue))
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

            //because aligned date collection is generated for a particular project each time it refreshes, only do detection on dataPointsDateCollection
            //foreach (DateTime dataPointsDate in dataPointsDateCollection)
            //{
            //    if (!alignedDateCollection.Any(x => x.Year == dataPointsDate.Year && x.Month == dataPointsDate.Month))
            //    {
            //        shouldRefreshGrid = true;
            //        break;
            //    }
            //}

            foreach (DateTime alignDate in alignedDateCollection)
            {
                if (!dataPointsDateCollection.Any(x => x.Year == alignDate.Year && x.Month == alignDate.Month))
                {
                    shouldRefreshGrid = true;
                    break;
                }
            }

            if (shouldRefreshGrid)
            {
                //refresh the entire grid
                realignDateCollectionOnDataTable(dataPointsTable, dataPointsDateCollection);

                alignedDateCollection = dataPointsDateCollection;
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

        public override void FullRefresh()
        {
            resetDataPointsTable();
            base.FullRefresh();
        }

        private void resetDataPointsTable()
        {
            _bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            ParentColumns.Clear();
            ChildColumns.Clear();
            ParentSummaries.Clear();
            ChildSummaries.Clear();
            dataPointsTable = null;
        }

        private void realignDateCollectionOnDataTable(DataTable dataTable, IEnumerable<DateTime> alignedDataDates)
        {
            List<DataColumn> removeColumns = new List<DataColumn>();
            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                DateTime dateTime;
                if (DateTime.TryParseExact(dataColumn.ColumnName, "dd-MMM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    if (!alignedDataDates.Any(x => x.Date == dateTime.Date))
                    {
                        removeColumns.Add(dataColumn);
                    }
                }
            }

            foreach (DataColumn removeColumn in removeColumns)
            {
                dataTable.Columns.Remove(removeColumn);
            }

            foreach (DateTime alignedDataDate in alignedDataDates)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                DataColumn dataColumn = findDataColumn(dataTable, columnFieldName);
                if (dataColumn == null)
                {
                    if (!dataTable.Columns.Contains(columnFieldName))
                    {
                        dataTable.Columns.Add(columnFieldName, typeof(decimal));
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            DataTable tenderProfilesDataPointsTable = (DataTable)dataRow[columnTenderProfileDataTable];
                            if (!tenderProfilesDataPointsTable.Columns.Contains(columnFieldName))
                                tenderProfilesDataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                        }
                    }
                }
            }
        }

        private DataColumn findDataColumn(DataTable dataTable, string fieldName)
        {
            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                if (dataColumn.ColumnName == fieldName)
                    return dataColumn;
            }

            return null;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, PROJECTTenderProfile projection, bool isNew)
        {
            string fieldName = formatParentFieldName(field_name);
            if (fieldName == BindableBase.GetPropertyName(() => new PROJECT().STATUS))
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<PROJECT, Guid>(projection.GUID, Guid.Empty, EntityMessageType.Changed))));
            }
            else
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => Messenger.Default.Send(new EntityMessage<PROJECT, Guid>(projection.GUID, Guid.NewGuid(), EntityMessageType.Changed))));
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedValueValidation(PROJECTTenderProfile projection, string field_name, object new_value, bool isPaste)
        {
            string fieldName = formatParentFieldName(field_name);
            if (fieldName == BindableBase.GetPropertyName(() => new PROJECT().NUMBER))
            {
                if (new_value != null)
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
            else if (fieldName == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().SCHEDULE_START_PERCENTAGE))
            {
                if (new_value != null)
                {
                    //when it's new item and user just started entering start percentage, don't flag it as an error
                    if (TENDER_PROFILE_ITEM.SCHEDULE_FINISH_PERCENTAGE != 0)
                    {
                        decimal currentStartPercentage = (decimal)new_value;
                        if (currentStartPercentage > TENDER_PROFILE_ITEM.SCHEDULE_FINISH_PERCENTAGE)
                            return "Start percentage cannot be more than finish percentage";
                    }
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().SCHEDULE_FINISH_PERCENTAGE))
            {
                if (new_value != null)
                {
                    decimal currentFinishPercentage = (decimal)new_value;
                    if (currentFinishPercentage < TENDER_PROFILE_ITEM.SCHEDULE_START_PERCENTAGE)
                        return "Finish percentage cannot be less than start percentage";
                }
            }

            //custom error message is used
            //else if (fieldName == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().HOURS_PERCENTAGE))
            //{
            //    if(new_value != null)
            //    {
            //        decimal currentPercentage = (decimal)new_value;
            //        IEnumerable<TENDER_PROFILE_ITEM> otherTENDER_PROFILE_ITEM = TENDER_PROFILE_ITEM.PROJECTTenderProfile.TENDER_PROFILE_ITEMS.Where(x => x.GUID != TENDER_PROFILE_ITEM.GUID);

            //        decimal totalPercentage = otherTENDER_PROFILE_ITEM.Sum(x => x.HOURS_PERCENTAGE) + currentPercentage;
            //        if (totalPercentage > 1)
            //            return "Total percentage exceed 100%";
            //    }
            //}

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
        protected string columnTenderProfile = "TenderProfile";
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

        public void ParentPastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            //when detailed view is in focus this will be null
            //because copy paste is not implemented in master grid level, when this is null it indicates that detailed view is in focus
            if (SelectedParentDataRow != null)
                return;

            if (lastSelectedParentDataRow == null)
                return;

            var gridControl = (GridControl)e.Source;
            TableView tableView = gridControl.View as TableView;
            DataControlDetailDescriptor gridDetail = (DataControlDetailDescriptor)GridControlService.GridControl.DetailDescriptor;
            GridControl childGridControl = (GridControl)gridDetail.DataControl;
            TableView childGridControlTableView = (TableView)childGridControl.View;

            PROJECTTenderProfile PROJECTTenderProfile = forceRetrieveSelectedProject();
            if (PROJECTTenderProfile == null)
                return;

            //when cell is in editing mode, user might want to paste clipboard data into cell
            if (childGridControlTableView.ActiveEditor != null)
                return;

            if (childGridControlTableView != null && childGridControlTableView.FocusedRowHandle == GridControl.AutoFilterRowHandle)
                return;

            var PasteString = System.Windows.Clipboard.GetText();
            string[] RowData;

            RowData = DataUtils.ExcelSplit(PasteString).ToArray();
            for (int i = 0; i < RowData.Count(); i++)
            {
                //remove tab at the beginning of paste string
                if (RowData[i].Substring(0, 1) == "\t")
                    RowData[i] = RowData[i].Substring(1, RowData[i].Length - 1);

                if (RowData[i].Contains("\t"))
                {
                    MessageBoxService.ShowMessage("Please copy values from only a single column", "Multiple columns data not supported", MessageButton.OK);
                    return;
                }
            }


            List<TENDER_PROFILE_ITEM> pasteProjections = new List<TENDER_PROFILE_ITEM>();
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            if (SelectMode == MultiSelectMode.Cell)
            {
                pasteProjections = PastingFromClipboardCellLevel(childGridControl, RowData, out errorMessages);
                if (pasteProjections.Count > 0)
                {
                    _bluePrintsUnitOfWork.SaveChanges();
                    reload(PROJECTTenderProfile);
                }
            }
            else
            {
                pasteProjections = PastingFromClipboard(childGridControl, RowData, PROJECTTenderProfile, out errorMessages);
                if (pasteProjections.Count > 0)
                {
                    foreach (TENDER_PROFILE_ITEM pasteProjection in pasteProjections)
                    {
                        pasteProjection.GUID_TENDER_PROFILE = PROJECTTenderProfile.TenderProfile.GUID;
                        PROJECTTenderProfile.TENDER_PROFILE_ITEMS.Add(pasteProjection);
                        _bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Add(pasteProjection);
                    }

                    _bluePrintsUnitOfWork.SaveChanges();
                    reload(PROJECTTenderProfile);
                }
            }

            if (errorMessages.Count > 0)
            {
                if (ErrorMessagesDialogService != null)
                {
                    DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "Paste error");
                    ErrorMessagesDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListErrorMessages", viewModel);
                }
            }
        }

        public List<TENDER_PROFILE_ITEM> PastingFromClipboard(GridControl gridControl, string[] RowData, PROJECTTenderProfile project, out List<ErrorMessage> errorMessages)
        {
            var gridView = gridControl.View;
            errorMessages = new List<ErrorMessage>();
            List<TENDER_PROFILE_ITEM> pasteProjections = new List<TENDER_PROFILE_ITEM>();
            if (gridView.ActiveEditor == null)
            {
                TableView gridTableView = gridView as TableView;
                PasteResult result = PasteResult.Success;
                foreach (var Row in RowData)
                {
                    TENDER_PROFILE_ITEM projection = new TENDER_PROFILE_ITEM();
                    List<KeyValuePair<ColumnBase, string>> columnData = new List<KeyValuePair<ColumnBase, string>>();
                    var ColumnStrings = Row.Split('\t');

                    for (var i = 0; i < ColumnStrings.Count(); i++)
                    {
                        if (i > gridTableView.VisibleColumns.Count - 1)
                            continue;

                        ColumnBase copyColumn = gridTableView.VisibleColumns[i];
                        if (copyColumn.FieldName.Contains('%') || copyColumn.FieldName.ToUpper().Contains("PERCENT"))
                            ColumnStrings[i] = ColumnStrings[i].Replace("%", "");

                        string errorMessage = string.Empty;
                        string alternateFieldName = formatChildFieldName(copyColumn.FieldName);

                        result = pasteDataInProjectionColumn(projection, copyColumn, ColumnStrings[i], out errorMessage, null, null, alternateFieldName);

                        if (errorMessage != string.Empty)
                            errorMessages.Add(new ErrorMessage(gridTableView.VisibleColumns[i].Header.ToString(), errorMessage));

                        //When column has gone through unifiedCellValidation and have error
                        if (result == PasteResult.Failed)
                            break;

                        columnData.Add(new KeyValuePair<ColumnBase, string>(copyColumn, ColumnStrings[i]));
                    }

                    if (result != PasteResult.Failed)
                    {
                        pasteProjections.Add(projection);
                    }
                }
            }

            List<TENDER_PROFILE_ITEM> validatedProjections = new List<TENDER_PROFILE_ITEM>();
            foreach (TENDER_PROFILE_ITEM pasteProjection in pasteProjections)
            {
                pasteProjection.GUID_TENDER_PROFILE = project.TenderProfile.GUID;
                pasteProjection.PROJECTTenderProfile = project;

                List<ErrorMessage> currentErrorMessages = validateTENDER_PROFILE_ITEM(pasteProjection);
                if (currentErrorMessages.Count == 0)
                    validatedProjections.Add(pasteProjection);

                errorMessages.AddRange(currentErrorMessages);
            }

            return validatedProjections;
        }

        private List<TENDER_PROFILE_ITEM> PastingFromClipboardCellLevel(GridControl gridControl, string[] RowData, out List<ErrorMessage> errorMessages)
        {
            List<TENDER_PROFILE_ITEM> preValidatedProjections = new List<TENDER_PROFILE_ITEM>();
            errorMessages = new List<ErrorMessage>();

            TableView gridTableView = (TableView)gridControl.View;
            if (gridTableView.ActiveEditor == null)
            {
                List<List<string>> row_data = new List<List<string>>();
                foreach (var row in RowData)
                {
                    List<string> column_data = row.Split('\t').ToList();
                    row_data.Add(column_data);
                }

                var grouped_results = row_data
                    .SelectMany(inner => inner.Select((item, index) => new { item, index }))
                    .GroupBy(i => i.index, i => i.item)
                    .Select(g => g.ToList())
                    .ToList();

                var selected_cells = gridTableView.GetSelectedCells();
                if (selected_cells.Count == 0)
                    return preValidatedProjections;

                var selected_cells_groupby_columns = selected_cells.GroupBy(x => x.Column.FieldName).Select(group => new { FieldName = group.Key, Cells = group.ToList() });
                if (grouped_results.Count == 0)
                {
                    foreach (var selected_cell in selected_cells)
                    {

                        int row_handle = selected_cell.RowHandle;
                        TENDER_PROFILE_ITEM editing_row = (TENDER_PROFILE_ITEM)gridControl.GetRow(row_handle);
                        string errorMessage = string.Empty;
                        PasteResult result = pasteDataInProjectionColumn(editing_row, selected_cell.Column, string.Empty, out errorMessage);
                        if (result == PasteResult.FailOnRequired || errorMessage != string.Empty)
                        {
                            string errorString = errorMessage == string.Empty ? "Cannot set null in required cell, operation has been terminated" : errorMessage;
                            errorMessages.Add(new ErrorMessage(selected_cell.Column.Header.ToString(), errorString));
                            break;
                        }
                        if (result != PasteResult.Success)
                            continue;

                        if (!preValidatedProjections.Any(x => x.GetHashCode() == editing_row.GetHashCode()))
                            preValidatedProjections.Add(editing_row);
                    }
                }
                else
                {
                    GridCell first_selected_cell = selected_cells.First();
                    GridCell last_selected_cell = selected_cells.Last();

                    int first_row_visible_index = 0;
                    int last_row_visible_index = SelectedChildDataRows.Count - 1;

                    int numberOfSelectedRows = (last_row_visible_index - first_row_visible_index) + 1;
                    int numberOfCopiedRows = grouped_results.First().Count;

                    List<GridColumn> visible_columns = gridTableView.VisibleColumns.ToList();
                    //commented out because not accurate during banded view
                    //int first_column_visible_index = first_selected_cell.Column.VisibleIndex;
                    int first_column_visible_index = visible_columns.IndexOf(visible_columns.First(x => x.FieldName == first_selected_cell.Column.FieldName));
                    int last_column_visible_index = visible_columns.IndexOf(visible_columns.First(x => x.FieldName == last_selected_cell.Column.FieldName));

                    int numberOfSelectedColumns = (last_column_visible_index - first_column_visible_index) + 1;
                    int numberOfCopiedColumns = grouped_results.Count;

                    //commented out because not accurate during banded view
                    //int first_column_visible_index = first_selected_cell.Column.VisibleIndex;

                    int rowOffsetSelection = numberOfSelectedRows > numberOfCopiedRows ? numberOfSelectedRows : numberOfCopiedRows;
                    int columnOffsetSelection = numberOfSelectedColumns > numberOfCopiedColumns ? numberOfSelectedColumns : numberOfCopiedColumns;

                    int pasteValueRowOffset = 0;
                    TENDER_PROFILE_ITEM validate_row = null;
                    for (int rowOffset = 0; rowOffset < rowOffsetSelection; rowOffset++)
                    {
                        int pasteValueColumnOffset = 0;
                        for (int columnOffset = 0; columnOffset < columnOffsetSelection; columnOffset++)
                        {
                            int findVisibleIndex = first_column_visible_index + columnOffset;
                            if (findVisibleIndex >= visible_columns.Count)
                                continue;

                            GridColumn current_column = visible_columns[findVisibleIndex];
                            string columnValue = grouped_results[pasteValueColumnOffset][pasteValueRowOffset];
                            if (current_column.FieldName.Contains('%') || current_column.FieldName.ToUpper().Contains("PERCENT"))
                                columnValue = columnValue.Replace("%", "");

                            pasteValueColumnOffset += 1;
                            if (pasteValueColumnOffset >= grouped_results.Count)
                                pasteValueColumnOffset = 0;

                            int current_row_visible_index = first_row_visible_index + rowOffset;
                            int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);
                            if (current_row_handle < 1)
                                current_row_handle = current_row_visible_index;

                            if (SelectedChildDataRows.Count < current_row_handle + 1)
                                continue;

                            DataRowView rowDataRowView = SelectedChildDataRows[current_row_handle];
                            if (rowDataRowView == null)
                                continue;

                            TENDER_PROFILE_ITEM editing_row = (TENDER_PROFILE_ITEM)rowDataRowView[columnTenderProfile];
                            validate_row = editing_row;
                            if (editing_row == null)
                            {
                                errorMessages.Add(new ErrorMessage(current_column.Header.ToString(), "Please remove all line break from paste data or double click into cell to paste your data with line breaks"));
                                break;
                            }

                            string errorMessage = string.Empty;
                            string alternateFieldName = formatChildFieldName(current_column.FieldName);
                            PasteResult result = pasteDataInProjectionColumn(editing_row, current_column, columnValue, out errorMessage, null, null, alternateFieldName);
                            if (result == PasteResult.FailOnRequired || errorMessage != string.Empty)
                            {
                                string errorString = errorMessage == string.Empty ? "Cannot set null in required cell, operation has been terminated" : errorMessage;
                                errorMessages.Add(new ErrorMessage(current_column.Header.ToString(), errorString));
                                break;
                            }
                            if (result != PasteResult.Success)
                                continue;

                        }

                        if (validate_row != null)
                            preValidatedProjections.Add(validate_row);

                        pasteValueRowOffset += 1;
                        if (pasteValueRowOffset >= grouped_results[pasteValueColumnOffset].Count)
                            pasteValueRowOffset = 0;
                    }
                }

            }

            List<TENDER_PROFILE_ITEM> validatedProjections = new List<TENDER_PROFILE_ITEM>();
            foreach (TENDER_PROFILE_ITEM preValidatedProjection in preValidatedProjections)
            {
                List<ErrorMessage> currentErrorMessages = validateTENDER_PROFILE_ITEM(preValidatedProjection);
                if (currentErrorMessages.Count == 0)
                    validatedProjections.Add(preValidatedProjection);

                errorMessages.AddRange(currentErrorMessages);
            }

            return validatedProjections;
        }

        private List<ErrorMessage> validateTENDER_PROFILE_ITEM(TENDER_PROFILE_ITEM projection)
        {
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            PROJECTTenderProfile project = projection.PROJECTTenderProfile;
            IEnumerable<TENDER_PROFILE_ITEM> otherTENDER_PROFILE_ITEMS = project.TENDER_PROFILE_ITEMS.Where(x => x.GUID != projection.GUID);


            DEPARTMENT findDEPARTMENT = DEPARTMENTCollection.FirstOrDefault(x => x.GUID == projection.GUID_DEPARTMENT);
            DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.GUID_DISCIPLINE);
            string projectionName = string.Empty;
            if (findDEPARTMENT != null && findDISCIPLINE != null)
                projectionName = "Department: " + findDEPARTMENT.NAME + " and Discipline: " + findDISCIPLINE.NAME;
            else if (findDEPARTMENT != null)
                projectionName = "Department: " + findDEPARTMENT.NAME;
            else if (findDISCIPLINE != null)
                projectionName = "Discipline: " + findDISCIPLINE.NAME;

            if (otherTENDER_PROFILE_ITEMS.Any(x => x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE))
            {

                if (findDEPARTMENT != null && findDISCIPLINE != null)
                    errorMessages.Add(new ErrorMessage(projectionName, "Already exists for this project"));
                else if (findDEPARTMENT != null)
                    errorMessages.Add(new ErrorMessage(projectionName, "Already exists for this project"));
                else if (findDISCIPLINE != null)
                    errorMessages.Add(new ErrorMessage(projectionName, "Already exists for this project"));
            }

            if (projection.SCHEDULE_FINISH_PERCENTAGE < projection.SCHEDULE_START_PERCENTAGE)
                errorMessages.Add(new ErrorMessage(projectionName, "Finish percentage: " + projection.SCHEDULE_FINISH_PERCENTAGE + " is less than start percentage: " + projection.SCHEDULE_START_PERCENTAGE));

            return errorMessages;
        }

        protected DevExpress.Mvvm.IDialogService DefaultTenderProfileSelectionDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DefaultTenderProfileSelectionDialog"); }
        }

        public bool CanShowTenderProfileSelection()
        {
            return !IsLoading;
        }

        public void ShowTenderProfileSelection()
        {
            PROJECTTenderProfile projectTENDER_PROFILE = forceRetrieveSelectedProject();

            if (projectTENDER_PROFILE == null)
            {
                MessageBoxService.ShowMessage("Please select a project where you wish to replace tender profile", "Please select project", MessageButton.OK);
                return;
            }

            List<TENDER_PROFILE> TENDER_PROFILECollection = bluePrintsUnitOfWorkFactory.CreateUnitOfWork().TENDER_PROFILES.Where(x => x.GUID_PROJECT == null).ToList();
            if (projectTENDER_PROFILE.TENDER_PROFILE_ITEMS.Count == 0)
                ShowDefaultTenderProfileSelection(projectTENDER_PROFILE, TENDER_PROFILECollection);
            else
                ShowOptionalTenderProfileSelection(projectTENDER_PROFILE, TENDER_PROFILECollection);
        }

        public void ShowOptionalTenderProfileSelection(PROJECTTenderProfile projectTENDER_PROFILE, List<TENDER_PROFILE> TENDER_PROFILECollection)
        {
            UICommand allCommand = new UICommand()
            {
                Id = CopyProfileAction.All,
                Caption = "Replace All",
                IsCancel = true,
                IsDefault = false,
            };

            UICommand exceptBudgetCommand = new UICommand()
            {
                Id = CopyProfileAction.ExceptBudget,
                Caption = "Merge (Exclude Budget)",
                IsCancel = true,
                IsDefault = false,
            };

            UICommand cancelCommand = new UICommand()
            {
                Id = CopyProfileAction.Cancel,
                Caption = "Cancel",
                IsCancel = true,
                IsDefault = false,
            };

            string message = String.Format("Do you wish to use budgeted units from plan\nor from deliverable(s)");

            var bulkEditEnumsViewModel = BulkEditEnumsViewModel.Create(TENDER_PROFILECollection, "NAME");
            UICommand result = DefaultTenderProfileSelectionDialogService.ShowDialog(new List<UICommand>() { allCommand, exceptBudgetCommand, cancelCommand }, "Select Tender Profile and Copy Mode", "BulkEditEnums", bulkEditEnumsViewModel);
            if (result == allCommand)
            {
                confirmReplaceCurrentProfile(projectTENDER_PROFILE, bulkEditEnumsViewModel.SelectedItem, true);
            }
            else if (result == exceptBudgetCommand)
            {
                confirmReplaceCurrentProfile(projectTENDER_PROFILE, bulkEditEnumsViewModel.SelectedItem, false);
            }
        }

        public void ShowDefaultTenderProfileSelection(PROJECTTenderProfile projectTENDER_PROFILE, List<TENDER_PROFILE> TENDER_PROFILECollection)
        {
            var bulkEditEnumsViewModel = BulkEditEnumsViewModel.Create(TENDER_PROFILECollection, "NAME");
            if (DefaultTenderProfileSelectionDialogService.ShowDialog(MessageButton.OKCancel, "Select Default Tender Profile", "BulkEditEnums", bulkEditEnumsViewModel) == MessageResult.OK)
            {
                if (bulkEditEnumsViewModel.SelectedItem != null)
                {
                    TENDER_PROFILE selectedEntity = bulkEditEnumsViewModel.SelectedItem as TENDER_PROFILE;
                    if (projectTENDER_PROFILE == null || MessageBoxService.ShowMessage("Are you sure you want to replace current profile with " + selectedEntity.NAME + "?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
                    {
                        replaceExistingProfile(selectedEntity, projectTENDER_PROFILE, true);
                        reload(projectTENDER_PROFILE);
                    }
                }
            }
        }

        private void confirmReplaceCurrentProfile(PROJECTTenderProfile projectTENDER_PROFILE, object selectedEntity, bool replaceAll)
        {
            if (selectedEntity != null)
            {
                string copyModeStr = replaceAll ? "all" : "all except budget";
                TENDER_PROFILE selectedTENDER_PROFILE = selectedEntity as TENDER_PROFILE;
                if (projectTENDER_PROFILE == null || MessageBoxService.ShowMessage("Are you sure you want to replace all current profile with " + copyModeStr + " details from " + selectedTENDER_PROFILE.NAME + "?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
                {
                    replaceExistingProfile(selectedTENDER_PROFILE, projectTENDER_PROFILE, replaceAll);
                    reload(projectTENDER_PROFILE);
                }
            }
        }

        private void reload(PROJECTTenderProfile projectTENDER_PROFILE)
        {
            populateDataPoints(projectTENDER_PROFILE);
            BuildRowStats(projectTENDER_PROFILE, true);
        }

        /// <summary>
        /// Try to find project specific TENDER_PROFILE from data context and replace it
        /// </summary>
        /// <param name="profileName">Profile name to use</param>
        /// <returns>Return existing or new TENDER_PROFILE</returns>
        private void replaceExistingProfile(TENDER_PROFILE selectedTenderProfile, PROJECTTenderProfile projectTenderProfile, bool replaceAll)
        {
            TENDER_PROFILE existingOrNewTENDER_PROFILE;
            TENDER_PROFILE findCurrentTENDER_PROFILE = _bluePrintsUnitOfWork.TENDER_PROFILES.FirstOrDefault(x => x.GUID == projectTenderProfile.TenderProfile.GUID);

            if (replaceAll)
            {
                existingOrNewTENDER_PROFILE = new TENDER_PROFILE();
                //set new tender profile hours with existing tender profile hours and remove old tender profile
                if (findCurrentTENDER_PROFILE != null)
                {
                    existingOrNewTENDER_PROFILE.TENDER_HOURS = findCurrentTENDER_PROFILE.TENDER_HOURS;
                    _bluePrintsUnitOfWork.TENDER_PROFILES.Remove(findCurrentTENDER_PROFILE);
                }

                existingOrNewTENDER_PROFILE.NAME = selectedTenderProfile.NAME;
                existingOrNewTENDER_PROFILE.GUID_PROJECT = projectTenderProfile.Entity.GUID;
                _bluePrintsUnitOfWork.TENDER_PROFILES.Add(existingOrNewTENDER_PROFILE);

                projectTenderProfile.TenderProfile = existingOrNewTENDER_PROFILE;

                //save tender profile
                _bluePrintsUnitOfWork.SaveChanges();
            }
            else
                existingOrNewTENDER_PROFILE = projectTenderProfile.TenderProfile;

            List<TENDER_PROFILE_ITEM> addTENDER_PROFILE_ITEMS = new List<TENDER_PROFILE_ITEM>();
            foreach (TENDER_PROFILE_ITEM defaultTENDER_PROFILE_ITEM in selectedTenderProfile.TENDER_PROFILE_ITEM)
            {
                TENDER_PROFILE_ITEM findExistingTENDER_PROFILE_ITEM = projectTenderProfile.TENDER_PROFILE_ITEMS.FirstOrDefault(x => x.GUID_DEPARTMENT == defaultTENDER_PROFILE_ITEM.GUID_DEPARTMENT && x.GUID_DISCIPLINE == defaultTENDER_PROFILE_ITEM.GUID_DISCIPLINE);
                if (replaceAll || findExistingTENDER_PROFILE_ITEM == null)
                {
                    TENDER_PROFILE_ITEM copyTENDER_PROFILE_ITEM = new TENDER_PROFILE_ITEM();
                    DataUtils.ShallowCopy(copyTENDER_PROFILE_ITEM, defaultTENDER_PROFILE_ITEM);

                    //when it's not replace all but tender profile isn't found in current list, copy it as zero hours %
                    if (!replaceAll)
                        copyTENDER_PROFILE_ITEM.HOURS_PERCENTAGE = 0;

                    copyTENDER_PROFILE_ITEM.GUID = Guid.Empty;
                    copyTENDER_PROFILE_ITEM.GUID_TENDER_PROFILE = existingOrNewTENDER_PROFILE.GUID;
                    copyTENDER_PROFILE_ITEM.PROJECTTenderProfile = projectTenderProfile;
                    _bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Add(copyTENDER_PROFILE_ITEM);
                    addTENDER_PROFILE_ITEMS.Add(copyTENDER_PROFILE_ITEM);
                }
                else
                {
                    findExistingTENDER_PROFILE_ITEM.SCHEDULE_START_PERCENTAGE = defaultTENDER_PROFILE_ITEM.SCHEDULE_START_PERCENTAGE;
                    findExistingTENDER_PROFILE_ITEM.SCHEDULE_FINISH_PERCENTAGE = defaultTENDER_PROFILE_ITEM.SCHEDULE_FINISH_PERCENTAGE;
                    findExistingTENDER_PROFILE_ITEM.BELLCURVESHAPE = defaultTENDER_PROFILE_ITEM.BELLCURVESHAPE;
                }
            }

            //refresh project tender profile items with newly added items
            if(replaceAll)
                projectTenderProfile.TENDER_PROFILE_ITEMS.Clear();
            else
            {
                //logic to set tender profile hours % to zero for deliverable generate profile that isn't standard
                //List<TenderProfileItemEditModel> tenderProfileItemEditModels = new List<TenderProfileItemEditModel>();
                //foreach (TENDER_PROFILE_ITEM currentTENDER_PROFILE_ITEM in projectTenderProfile.TENDER_PROFILE_ITEMS)
                //{
                //    TENDER_PROFILE_ITEM findDefaultTENDER_PROFILE_ITEM = selectedTenderProfile.TENDER_PROFILE_ITEM.FirstOrDefault(x => x.GUID_DEPARTMENT == currentTENDER_PROFILE_ITEM.GUID_DEPARTMENT && x.GUID_DISCIPLINE == currentTENDER_PROFILE_ITEM.GUID_DISCIPLINE);
                //    if(findDefaultTENDER_PROFILE_ITEM != null)
                //    {
                //        if(currentTENDER_PROFILE_ITEM.HOURS_PERCENTAGE > 0)
                //        {
                //            TenderProfileItemEditModel tenderProfileItemEditModel = new TenderProfileItemEditModel();
                //            tenderProfileItemEditModel.TenderProfileItemGuid = currentTENDER_PROFILE_ITEM.GUID;
                //            tenderProfileItemEditModel.DepartmentGuid = currentTENDER_PROFILE_ITEM.GUID_DEPARTMENT;
                //            tenderProfileItemEditModel.DisciplineGuid = currentTENDER_PROFILE_ITEM.GUID_DISCIPLINE;
                //            tenderProfileItemEditModel.HoursPercentageFrom = currentTENDER_PROFILE_ITEM.HOURS_PERCENTAGE;
                //            tenderProfileItemEditModel.HoursPercentageTo = 0;
                //            tenderProfileItemEditModels.Add(tenderProfileItemEditModel);
                //        }
                //    }
                //}

                //TenderProfileItemEditConfirmationViewModel tenderProfileItemEditConfirmationViewModel = TenderProfileItemEditConfirmationViewModel.Create(tenderProfileItemEditModels, "Do you wish to set hours % to zero for items that doesn't exists in the copied profile?", DEPARTMENTCollection, DISCIPLINECollection);
                //if (DeliverableEditDialogService.ShowDialog(MessageButton.OKCancel, "", "TenderProfileItemEditConfirmation", tenderProfileItemEditConfirmationViewModel) == MessageResult.OK)
                //{
                //    foreach(TenderProfileItemEditModel tenderProfileItemEditModel in tenderProfileItemEditModels)
                //    {
                //        TENDER_PROFILE_ITEM findCurrentTENDER_PROFILE_ITEM = projectTenderProfile.TENDER_PROFILE_ITEMS.FirstOrDefault(x => x.GUID == tenderProfileItemEditModel.TenderProfileItemGuid);
                //        if(findCurrentTENDER_PROFILE_ITEM != null)
                //        {
                //            findCurrentTENDER_PROFILE_ITEM.HOURS_PERCENTAGE = tenderProfileItemEditModel.HoursPercentageTo;
                //        }
                //    }
                //}
            }

            projectTenderProfile.TENDER_PROFILE_ITEMS.AddRange(addTENDER_PROFILE_ITEMS);

            //save tender profile items
            _bluePrintsUnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Retrieve selected project when master table view is not on focus
        /// </summary>
        private PROJECTTenderProfile forceRetrieveSelectedProject()
        {
            if (SelectedParentDataRow != null)
                return (PROJECTTenderProfile)SelectedParentDataRow[columnProject];
            else
            {
                int rowHandle = ((TableView)GridControlService.GridControl.View.MasterRootRowsContainer.FocusedView).Grid.GetMasterRowHandle();
                if (rowHandle >= 0)
                    return (PROJECTTenderProfile)((DataRowView)GridControlService.GetRow(rowHandle))[columnProject];
                else
                    return null;
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
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private DataRowView lastSelectedParentDataRow { get; set; }
        DataRowView selectedParentDataRow;
        public DataRowView SelectedParentDataRow
        {
            get => selectedParentDataRow;
            set
            {
                //when child grid is chosen
                if (value != null)
                    lastSelectedParentDataRow = value;

                selectedParentDataRow = value;
            }
        }

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

        public DataRowView SelectedChildDataRow { get; set; }
        ObservableCollection<DataRowView> selectedChildDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedChildDataRows
        {
            get
            {
                if (selectedChildDataRows == null)
                    selectedChildDataRows = new ObservableCollection<DataRowView>();

                return selectedChildDataRows;
            }
            set
            {
                selectedChildDataRows = value;
            }
        }

        protected override bool loadDataPointsTable()
        {
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);

            dataPointsTable = null;

            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);

            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());
            return true;
        }

        private void updateDataPointsTable()
        {
            GridControlService.BeginDataUpdate();
            dataPointsTable = new DataTable();
            dataPointsTable.RowChanged += DataPointsTable_RowChanged;

            if (ParentColumns.Count() == 0)
                InitializeParentColumnSource(ParentColumns, ParentSummaries, alignedDateCollection);

            if (ChildColumns.Count() == 0)
                InitializeChildColumnSource(ChildColumns, ChildSummaries, alignedDateCollection);

            dataPointsTable.Columns.Add(columnProject, typeof(PROJECTTenderProfile));
            dataPointsTable.Columns.Add(columnTenderProfileDataTable, typeof(DataTable));
            populateAlignedDataDate(dataPointsTable, alignedDateCollection);

            foreach (PROJECTTenderProfile entity in Entities)
            {
                BuildRowStats(entity, false);
            }

            GridControlService.EndDataUpdate();
        }

        public DataTable DataPointsTable
        {
            get
            {
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

        private DataRow findPROJECTDataRowBy(Guid guid)
        {
            return (from DataRow dr in dataPointsTable.Rows
                    where ((PROJECTTenderProfile)dr[columnProject]).GUID == guid
                    select dr).FirstOrDefault();
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
                newDataRow = findPROJECTDataRowBy(entity.GUID);
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
                    DateTime columnDate = DateTime.ParseExact(columnName, "dd-MMM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None);
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
                populateAlignedDataDate(tenderProfilesDataPointsTable, dataPointsDateCollection);
            }

            tenderProfilesDataPointsTable.Clear();

            IEnumerable<TENDER_PROFILE_ITEM> tenderProfileItems = entity.TENDER_PROFILE_ITEMS.OrderBy(x => x.CREATED);
            bool isTotalPercentageError = tenderProfileItems.Sum(x => x.HOURS_PERCENTAGE) > 1.01m;


            if (entity.TENDER_PROFILE_ITEMS != null)
                foreach (TENDER_PROFILE_ITEM tenderProfileItem in tenderProfileItems)
                {
                    //set hours percentage error when total percentage is more than 1
                    tenderProfileItem.IsPercentageError = isTotalPercentageError;

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
                            DateTime columnDate = DateTime.ParseExact(columnName, "dd-MMM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None);
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
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.NUMBER", SortIndex = 1, VisibleIndex = visibleIndex, Header = "Number", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            visibleIndex += 10;
            summaries.Add(new SummaryDescriptor() { FieldName = columnProject + ".Entity.NUMBER", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.NAME", VisibleIndex = visibleIndex, SortIndex = -1, Header = "Name", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.GUID_OFFICE", VisibleIndex = visibleIndex, Header = "Office", Fixed = FixedStyle.Left, Width = 80, DisplayMember = "NAME", ValueMember = "GUID", ItemsSource = OFFICECollection, Settings = SettingsType.Collection });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.CLIENT", SortIndex = -1, VisibleIndex = visibleIndex, Header = "Client", Fixed = FixedStyle.Left, Width = 80, Settings = SettingsType.Default });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.STATUS", VisibleIndex = visibleIndex, Header = "Project Status", Fixed = FixedStyle.Left, Width = 120, Settings = SettingsType.Enum1 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_TYPE", VisibleIndex = visibleIndex, Header = "Type", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum2 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_DIVISION", VisibleIndex = visibleIndex, Header = "Division", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum3 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.PIPELINE_CONTRACT", VisibleIndex = visibleIndex, Header = "Contract", Fixed = FixedStyle.Left, Width = 120, Settings = SettingsType.Enum5 });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".TenderProfile.TENDER_HOURS", VisibleIndex = visibleIndex, Header = "Design Hours", MaxValue = 999999999, Fixed = FixedStyle.Left, Mask = "n", Increment = 1, Width = 70, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.TENDER_PROJECT_START", VisibleIndex = visibleIndex, Header = "Start Date", ReadOnly = false, Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Date });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".Entity.TENDER_PROJECT_DURATION", VisibleIndex = visibleIndex, ReadOnly = false, Visible = true, MaxValue = 999999999, Header = "Duration", Mask = "###,##0 Weeks", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnProject + ".IsSynced", VisibleIndex = visibleIndex, SortIndex = -1, ReadOnly = false, Visible = true, Header = "Synced", Fixed = FixedStyle.Left, Width = 90, Settings = SettingsType.Default });
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
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".GUID_DISCIPLINE", VisibleIndex = visibleIndex, Tag = "Contract", Header = "Discipline", DisplayMember = "NAME", ValueMember = "GUID", Fixed = FixedStyle.Left, Width = 70, ItemsSource = DISCIPLINECollection, Settings = SettingsType.Collection });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".HOURS_PERCENTAGE", VisibleIndex = visibleIndex, Tag = "Design Hours", Header = "Hours %", MaxValue = 1, Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Custom1 });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTenderProfile + ".HOURS_PERCENTAGE", DisplayFormat = "p2", Type = SummaryItemType.Sum });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".SCHEDULE_START_PERCENTAGE", VisibleIndex = visibleIndex, MinValue = 0, MaxValue = 1, Tag = "Start Date", Header = "Schedule Start %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".SCHEDULE_FINISH_PERCENTAGE", VisibleIndex = visibleIndex, MinValue = 0, MaxValue = 1, Tag = "Duration", Header = "Schedule Finish %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            visibleIndex += 10;
            columns.Add(new ColumnDescriptor() { FieldName = columnTenderProfile + ".BELLCURVESHAPE", VisibleIndex = visibleIndex, Header = "Bell Curve", Tag = "Synced", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Enum7 });
            visibleIndex += 10;

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Mask = "n2", VisibleIndex = visibleIndex, ColumnDate = alignedDate, ReadOnly = true, Header = columnFieldName, Tag = columnFieldName, Width = 60, Settings = SettingsType.Number });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n2", Type = SummaryItemType.Sum });
                visibleIndex += 10;
            }
        }

        public void DeleteSelectedTenderProfiles()
        {
            if (SelectedChildDataRows.Count == 0)
            {
                MessageBoxService.ShowMessage("Please select tender profile(s) to delete", "Confirmation", MessageButton.OK);
                return;
            }

            if (MessageBoxService.ShowMessage("Are you sure you want to delete " + selectedChildDataRows.Count + " selected tender profiles?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<DataRow> removeRows = new List<DataRow>();
            TENDER_PROFILE_ITEM TENDER_PROFILE_ITEM = (TENDER_PROFILE_ITEM)SelectedChildDataRow[columnTenderProfile];
            DataRow parentDataRow = findPROJECTDataRowBy(TENDER_PROFILE_ITEM.PROJECTTenderProfile.GUID);
            DataTable childDataTable = (DataTable)parentDataRow[columnTenderProfileDataTable];

            foreach (DataRowView selectedRow in SelectedChildDataRows)
            {
                deleteChildRow(selectedRow, _bluePrintsUnitOfWork);
                removeRows.Add(selectedRow.Row);
            }

            foreach (DataRow removeRow in removeRows)
            {
                childDataTable.Rows.Remove(removeRow);
            }

            onDataPointsCalculated(TENDER_PROFILE_ITEM.PROJECTTenderProfile);
        }

        private void deleteChildRow(DataRowView selectedRow, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork)
        {
            TENDER_PROFILE_ITEM TENDER_PROFILE_ITEM = (TENDER_PROFILE_ITEM)selectedRow[columnTenderProfile];
            if (TENDER_PROFILE_ITEM != null)
            {
                TENDER_PROFILE_ITEM.DELETED = DateTime.Now;
                TENDER_PROFILE_ITEM.DELETEDBY = LoginCredentials.CurrentUserGuid;
                bluePrintsEntitiesUnitOfWork.SaveChanges();
            }
        }

        public void DeleteSelectedProjects()
        {
            if (SelectedParentDataRows.Count == 0)
            {
                MessageBoxService.ShowMessage("Please select project(s) to delete", "Confirmation", MessageButton.OK);
                return;
            }

            if (MessageBoxService.ShowMessage("Are you sure you want to delete " + SelectedParentDataRows.Count + " selected projects?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<DataRow> removeRows = new List<DataRow>();
            foreach (DataRowView selectedRow in SelectedParentDataRows)
            {
                deleteParentRow(selectedRow, _bluePrintsUnitOfWork);
                removeRows.Add(selectedRow.Row);
            }

            foreach (DataRow removeRow in removeRows)
                DataPointsTable.Rows.Remove(removeRow);
        }

        private void deleteParentRow(DataRowView selectedRow, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork)
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

        public IEnumerable<OFFICE> OFFICECollection
        {
            get
            {
                var collection = GetEntities<OFFICE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.NAME);
                }

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

        [ServiceProperty(Key = "ChildTableViewService")]
        protected virtual ITableViewService ChildTableViewService { get { return null; } }

        private DevExpress.Mvvm.IDialogService BasicMessageBoxDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BasicMessageBoxDialogService"); }
        }

        private DevExpress.Mvvm.IDialogService DeliverableEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DeliverableEditDialogService"); }
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