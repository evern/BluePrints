using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class OffsiteDirectDistributionCollectionViewModelWrapper :
        BluePrintsEntitiesProgressCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<BASELINE_ITEMProgress>
    {

        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectDistributionCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectDistributionCollectionViewModelWrapper());
        }

        #region Database Operation
        protected override void resolveParameters(object parameter)
        {
            is_load_p6_task = true;
            base.resolveParameters(parameter);
        }

        private BASELINE loadBASELINE;
        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            //in user offsite direct view model wrapper baseline should not be loaded because query gets from navigational baseline
            if(is_single_project_mode)
            {
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => assign_baseline(x));
            }

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);

            base.initializeEntitiesLoadersDescription();
        }

        private void assign_baseline(BASELINE baseline)
        {
            if (baseline == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));

            loadBASELINE = baseline;
        }

        protected virtual Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID_PROJECT).OrderBy(x => x.NUMBER);
        }

        protected virtual Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        protected virtual Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        public bool CanUpdateAllPercentagesByStatus()
        {
            return LoginCredentials.hasPermission(PermissionResources.ProgressUpdatePercentageByStatus);
        }

        public void UpdateAllPercentagesByStatus()
        {
            if (MessageBoxService.ShowMessage("Warning\nThis action will update or delete progresses based on deliverable status and is not reversible\nDo you wish to continue?",
                         BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            IEnumerable<BASELINE_ITEMProgress> deliverables = MainViewModel.Entities.Where(x => x.Entity.Entity.GUID_STATUS != null);
            List<PROGRESS_ITEM> updateProgress = new List<PROGRESS_ITEM>();

            foreach (var deliverable in deliverables)
            {
                DELIVERABLES_STATUS deliverableStatus = deliverable.Entity.Deliverable_Status;

                //when this is null it means the deliverable status is no longer valid (e.g. deleted)
                if (deliverableStatus == null)
                    continue;

                //user are able to fill up/down on statuses that might result in assigned status isn't valid to doctype, so check if status is valid before continuing
                bool isValidStatus = deliverable.Entity.Entity.DOCTYPE.DELIVERABLES_STATUS.Any(x => x.GUID == deliverableStatus.GUID);
                if (!isValidStatus)
                    continue;

                decimal? autoPercentage = deliverableStatus.AUTO_PERCENTAGE;
                if (autoPercentage != null)
                {
                    if (deliverable.Total_Earned_Percentage < autoPercentage)
                    {
                        decimal oldPercentage = deliverable.Total_Earned_Percentage;
                        decimal newPercentage = (decimal)autoPercentage;

                        deliverable.Total_Earned_Percentage = newPercentage;
                        IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = deliverable.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                        updateProgress.AddRange(newPRORESS_ITEMS);
                    }
                }

                if (deliverable.Total_Earned_Percentage > deliverableStatus.MAX_PERCENTAGE)
                {
                    decimal totalDeliverableUnits = deliverable.Total_Units;
                    decimal maxAllowableEarnedUnit = totalDeliverableUnits * deliverableStatus.MAX_PERCENTAGE;
                    if (maxAllowableEarnedUnit > 0)
                    {
                        decimal iterateEarnedUnits = 0;
                        List<PROGRESS_ITEM> progressesByDate = deliverable.PROGRESS_ITEMS.OrderBy(x => x.EARNED_DATE).ToList();
                        foreach (PROGRESS_ITEM progressByDate in progressesByDate)
                        {
                            decimal postProgressEarnedUnit = (iterateEarnedUnits + progressByDate.EARNED_UNITS);
                            decimal oldProgressEarnUnit = progressByDate.EARNED_UNITS;
                            if (postProgressEarnedUnit > maxAllowableEarnedUnit)
                            {
                                decimal newProgressEarnUnit = (maxAllowableEarnedUnit - iterateEarnedUnits);
                                progressByDate.EARNED_UNITS = newProgressEarnUnit < 0 ? 0 : newProgressEarnUnit;
                                updateProgress.Add(progressByDate);
                            }

                            iterateEarnedUnits += oldProgressEarnUnit;
                        }
                    }
                }

            }

            PROGRESS_ITEMSCollectionViewModel.BulkSave(updateProgress);
            FullRefresh();
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, loadPROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, DeliverableInternalNumberMode.Default, false, P6TASKCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            OnDisplaySelectedEntityChangedCallBack = reselectRowInDataPointsTable;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #region Collection Call Backs

        public bool ValidateFillDownCallBack(BASELINE_ITEMProgress fillDownEntity, string fieldName, object fillValue)
        {
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage))
            {
                var newPercentage = (decimal)fillValue;
                if (newPercentage > fillDownEntity.MaxPercentage)
                    return false;
                else if (newPercentage < fillDownEntity.MinPercentage)
                    return false;
            }

            return true;
        }

        public override void FullRefresh()
        {
            base.FullRefresh();
            refreshDataPointsTable();
        }
        #endregion

        #endregion

        #region View Properties
        private void refreshDataPointsTable()
        {
            dataPointsTable = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
        }

        protected override void onAfterRefresh()
        {
            base.onAfterRefresh();
            refreshDataPointsTable();
        }

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (e.Column.FieldName != deliverableKeyFieldName)
            {
                SpinEditSettings spinEdit = new SpinEditSettings();
                spinEdit.DisplayFormat = "p2";
                spinEdit.MaskUseAsDisplayFormat = true;
                spinEdit.Increment = 0.05m;
                e.Column.EditSettings = spinEdit;
            }
            else
            {
                e.Column.Visible = false;
            }
        }

        private void reselectRowInDataPointsTable(BASELINE_ITEMProgress changedEntity)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.SelectedDataRow)));
        }

        private void reselectDeliverable()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplaySelectedEntity)));
        }


        string deliverableKeyFieldName = "OriginalEntityKey";
        DataTable dataPointsTable = null;
        private DataRow selectedDataRow { get; set; }
        public DataRow SelectedDataRow
        {
            get
            {
                if (dataPointsTable == null || DisplaySelectedEntity == null)
                    return null;

                DataRow selectedRow = (from DataRow dr in dataPointsTable.Rows
                            where (Guid)dr[deliverableKeyFieldName] == DisplaySelectedEntity.OriginalEntityKey
                            select dr).FirstOrDefault();

                return selectedRow;
            }
            set
            {
                selectedDataRow = value;
            }
        }



        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                if(dataPointsTable == null)
                {
                    //if (DisplayEntities.Any(x => x.Stats.Earned == null) || DisplayEntities.Any(x => x.Stats.Earned.CumulativeDataPoints == null))
                    //    return null;

                    dataPointsTable = new DataTable();
                    TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
                    DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
                    DateTime lastDataDate = DisplayEntities.Where(x => x.Progresses != null && x.Progresses.Count() > 0).Max(x => x.Progresses.Max(y => y.EARNED_DATE));

                    IEnumerable<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);
                    dataPointsTable.Columns.Add(deliverableKeyFieldName, typeof(Guid));
                    foreach(DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        dataPointsTable.Columns.Add(alignedDataDate.Date.ToShortDateString(), typeof(decimal));
                    }

                    foreach(BASELINE_ITEMProgress entity in DisplayEntities)
                    {
                        DataRow newDataRow = dataPointsTable.NewRow();
                        newDataRow[deliverableKeyFieldName] = entity.OriginalEntityKey;

                        string s;
                        if (entity.OriginalEntityKey.ToString() == "63fba242-f15d-4163-8f36-06b9ba6e9f54")
                            s = string.Empty;

                        for(int i = 0; i < newDataRow.ItemArray.Count(); i++)
                        {
                            string columnName = dataPointsTable.Columns[i].ColumnName;
                            if (columnName != deliverableKeyFieldName)
                                newDataRow[columnName] = 0.00m;
                        }

                        if(entity.Stats.Earned != null && entity.Stats.Earned.CumulativeDataPoints != null)
                            foreach(Common.ViewModel.Reporting.DataPoint progress in entity.Stats.Earned.CumulativeDataPoints)
                            {
                                string dateField = progress.ProgressDate.Date.ToShortDateString();
                                if(dataPointsTable.Columns.Contains(dateField))
                                {
                                    newDataRow[dateField] = progress.UnitsPercentage;
                                }
                            }

                        dataPointsTable.Rows.Add(newDataRow);
                    }
                }

                return dataPointsTable;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "OffsiteDirectProgressViewModelWrapper_v3"; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }

        public IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection
        {
            get
            {
                var collection = GetEntities<DELIVERABLES_STATUS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.MAX_PERCENTAGE);
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

        public IEnumerable<P6Data.TASK> P6TASKCollection
        {
            get
            {
                return GetEntities<P6Data.TASK>();
            }
        }

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                return GetEntities<P6_ASSIGNMENT>();
            }
        }

        protected override CostGroup cost_group => CostGroup.Offsite;

        protected override IEnumerable<IReportable> ReportableCollection => MainViewModel == null || MainViewModel.Entities == null ? new ObservableCollection<BASELINE_ITEMProgress>() : MainViewModel.Entities;

        private BASELINE_ITEMSchedulingViewModelWrapper baseline_item_scheduling_view_model;
        protected override IEntitiesSchedulingCollectionWrapper scheduling_view_model
        {
            get
            {
                if (baseline_item_scheduling_view_model == null)
                    baseline_item_scheduling_view_model = BASELINE_ITEMSchedulingViewModelWrapper.Create();

                return baseline_item_scheduling_view_model;
            }
        }

        protected override void dispose_scheduling_view_model()
        {
            baseline_item_scheduling_view_model = null;
        }

        protected override ProgressType progress_type => ProgressType.Design;

        protected override bool have_group_entity => false;
        #endregion

        #region Reporting
        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Progress_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public bool CanViewReport()
        {
            return true;
        }

        public async void ViewReport()
        {
            var progressReport = new XtraReportPROGRESS_ITEMS();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    progressReport.LoadLayout(sw.BaseStream);
                }
            }

            LoadingScreenManager.ShowLoadingScreen(1);
            await BluePrintsContextHelper.RefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
            LoadingScreenManager.Progress();

            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            List<VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONCollection.AsQueryable(), ReportableCollection);

            DateTime reporting_data_date = loadPROGRESS.DATA_DATE;
            TimeSpan reporting_interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            DeliverableSummaryStats projectSummary = new DeliverableSummaryStats(MainViewModel.Entities, reporting_data_date, reporting_interval, first_aligned_data_date, projectVariationAdjustment);
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT.NUMBER, loadPROJECT.CURRENCYCONVERSION, reporting_interval, first_aligned_data_date, SUBJOBCollection, reporting_data_date);
            fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, loadPROJECT.NUMBER);
            fullSummarizer.BuildBurnedDataPoints(ExoBurnedFilterType.Design);
            fullSummarizer.Build();

            progressReport.AssignProperties(projectSummary, loadPROGRESS.DATA_DATE, loadPROGRESS.PROJECT.NAME);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = progressReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            progressReport.RequestParameters = false;
            progressReport.CreateDocument(true);
            previewWindow.Show();
        }
        #endregion
    }
}