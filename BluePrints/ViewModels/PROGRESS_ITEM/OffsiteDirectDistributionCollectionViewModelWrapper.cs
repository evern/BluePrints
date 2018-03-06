using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
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
            defaultColumnFieldNames.Add(Id);
            defaultColumnFieldNames.Add(subJob);
            defaultColumnFieldNames.Add(workpack);
            defaultColumnFieldNames.Add(area);
            defaultColumnFieldNames.Add(subArea);
            defaultColumnFieldNames.Add(documentType);
            defaultColumnFieldNames.Add(discipline);
            defaultColumnFieldNames.Add(department);
            defaultColumnFieldNames.Add(internalNumber);
            defaultColumnFieldNames.Add(deliverableType);
            defaultColumnFieldNames.Add(primaryTitle);
            defaultColumnFieldNames.Add(secondaryTitle);
            defaultColumnFieldNames.Add(comments);

            hiddenColumnFieldNames.Add(Id);
            hiddenColumnFieldNames.Add(comments);
            hiddenColumnFieldNames.Add(secondaryTitle);
            hiddenColumnFieldNames.Add(deliverableType);
            hiddenColumnFieldNames.Add(subArea);
            hiddenColumnFieldNames.Add(workpack);

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

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, loadPROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, DeliverableInternalNumberMode.Default, false, P6TASKCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
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

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(PROGRESS_ITEM))
            {
                BASELINE_ITEMProgress deliverable = DisplayEntities.FirstOrDefault(x => x.PROGRESS_ITEMS.Any(y => y.GUID == (Guid)key));
                if (deliverable != null)
                {
                    deliverable.BuildStats();
                    BuildRowStats(deliverable, true);
                    mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
                }

                return;
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!defaultColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                SpinEditSettings spinEdit = new SpinEditSettings();
                spinEdit.MaskType = MaskType.Numeric;
                spinEdit.Mask = "p2";
                spinEdit.MaskUseAsDisplayFormat = true;
                spinEdit.Increment = 0.01m;
                spinEdit.MaxValue = 1;
                spinEdit.MinValue = 0;
                e.Column.EditSettings = spinEdit;
            }
            else
            {
                if (hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
                {
                    e.Column.Visible = false;
                }

                e.Column.Fixed = FixedStyle.Left;
                e.Column.ReadOnly = true;
            }
        }

        private void reselectDeliverable()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplaySelectedEntity)));
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedProgressUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            if (!e.Handled)
            {
                DateTime columnDate;
                if(DateTime.TryParse(e.Column.FieldName, out columnDate))
                {
                    string earnedUnitsFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEM().EARNED_UNITS);

                    PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    decimal oldValue = (decimal)e.OldValue;
                    decimal newValue = (decimal)e.Value;
                    DateTime currentProgressDate = columnDate.AddDays(1).AddSeconds(-1);

                    DataRowView dataRowView = (DataRowView)e.Row;
                    Guid id = (Guid)dataRowView.Row[Id];
                    BASELINE_ITEMProgress currentDeliverable = DisplayEntities.FirstOrDefault(x => x.GUID == id);

                    List<PROGRESS_ITEM> progressToSave = new List<PROGRESS_ITEM>();
                    
                    decimal currentProgressMaximumUnits = getDeliverableProgressMaximumUnits(currentDeliverable, currentProgressDate);
                    decimal percentageDifference = newValue - oldValue;

                    if(percentageDifference != 0)
                    {
                        decimal totalUnitsDifferences = percentageDifference * currentProgressMaximumUnits;
                        decimal maximumEarnUnits = currentProgressMaximumUnits;

                        IEnumerable<PROGRESS_ITEM> previousProgresses = currentDeliverable.PROGRESS_ITEMS.Where(x => x.EARNED_DATE < currentProgressDate).OrderByDescending(x => x.EARNED_DATE);
                        //decimal minimumEarnUnits = 0;
                        //if (previousProgresses.Count() > 0)
                        //    minimumEarnUnits = previousProgresses.Sum(x => x.EARNED_UNITS);

                        PROGRESS_ITEM currentPeriodPROGRESS_ITEM = currentDeliverable.PROGRESS_ITEMS.FirstOrDefault(x => x.EARNED_DATE == currentProgressDate);
                        List<PROGRESS_ITEM> futureProgressToEdit = currentDeliverable.PROGRESS_ITEMS.Where(x => x.EARNED_DATE > currentProgressDate).OrderBy(x => x.EARNED_DATE).ToList();

                        //maximum and minimum is controlled here by the spinedit ability to set max as 100% and min as 0%, and that includes variation validatation, so there is no need to validate here
                        if (currentPeriodPROGRESS_ITEM == null && totalUnitsDifferences > 0)
                        {
                            PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                            newPROGRESS_ITEM.GUID_ORIBASEITEM = currentDeliverable.OriginalEntityKey;
                            newPROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
                            newPROGRESS_ITEM.EARNED_DATE = currentProgressDate;
                            newPROGRESS_ITEM.EARNED_UNITS = totalUnitsDifferences;
                            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, null, null, null, EntityMessageType.Added);
                            progressToSave.Add(newPROGRESS_ITEM);
                        }
                        else
                        {
                            decimal postEditUnits = currentPeriodPROGRESS_ITEM.EARNED_UNITS + totalUnitsDifferences;
                            if (postEditUnits < 0)
                            {
                                totalUnitsDifferences = -1 * currentPeriodPROGRESS_ITEM.EARNED_UNITS;
                                postEditUnits = 0;
                            }

                            decimal oldProgressValue = currentPeriodPROGRESS_ITEM.EARNED_UNITS;
                            currentPeriodPROGRESS_ITEM.EARNED_UNITS = postEditUnits;
                            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, earnedUnitsFieldName, oldProgressValue, postEditUnits, EntityMessageType.Changed);
                            progressToSave.Add(currentPeriodPROGRESS_ITEM);
                        }
                        //The addition of removal of units from current data date needs to be balanced by progress in the future, starting from the next progress
                        totalUnitsDifferences = totalUnitsDifferences * -1;
                        for (int i = 0; i < futureProgressToEdit.Count; i++)
                        {
                            if (totalUnitsDifferences == 0)
                                break;

                            PROGRESS_ITEM progress = futureProgressToEdit[i];
                            decimal oldProgressValue = progress.EARNED_UNITS;
                            decimal postEditEarnUnits = progress.EARNED_UNITS + totalUnitsDifferences;
                            if (postEditEarnUnits < 0)
                            {
                                postEditEarnUnits = 0;
                                totalUnitsDifferences -= progress.EARNED_UNITS;
                            }
                            else if(postEditEarnUnits > currentProgressMaximumUnits)
                            {
                                postEditEarnUnits = currentProgressMaximumUnits;
                                totalUnitsDifferences -= currentProgressMaximumUnits;
                            }
                            else
                            {
                                totalUnitsDifferences = 0;
                            }

                            progress.EARNED_UNITS = postEditEarnUnits;

                            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(progress, earnedUnitsFieldName, oldProgressValue, postEditEarnUnits, EntityMessageType.Changed);
                            progressToSave.Add(progress);
                        }
                    }

                    foreach (PROGRESS_ITEM progress in progressToSave)
                    {
                        PROGRESS_ITEMSCollectionViewModel.Save(progress);
                    }

                    PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.UnpauseActionId();
                    //will be unpaused in existingrow or newrow save
                }
            }
        }

        private decimal getDeliverableProgressMaximumUnits(BASELINE_ITEMProgress deliverable, DateTime progressDate)
        {
            if (deliverable.Stats.Budgeted.CurrentPeriodDataPoint != null && deliverable.Stats.Budgeted.CurrentPeriodDataPoint.BudgetedUnits != 0)
                return deliverable.Stats.Budgeted.CurrentPeriodDataPoint.BudgetedUnits;
            else if (deliverable.Stats.Budgeted.CumulativeDataPoints.Count > 0)
                return deliverable.Stats.Budgeted.CumulativeDataPoints.Last().BudgetedUnits;
            else
                return deliverable.Total_Units;
        }

        private decimal getDeliverableProgressMinimumUnits(BASELINE_ITEMProgress deliverable, DateTime progressDate)
        {
            if (deliverable.Stats.Budgeted.CurrentPeriodDataPoint != null && deliverable.Stats.Budgeted.CurrentPeriodDataPoint.BudgetedUnits != 0)
                return deliverable.Stats.Budgeted.CurrentPeriodDataPoint.BudgetedUnits;
            else if (deliverable.Stats.Budgeted.CumulativeDataPoints.Count > 0)
                return deliverable.Stats.Budgeted.CumulativeDataPoints.Last().BudgetedUnits;
            else
                return deliverable.Total_Units;
        }

        List<string> defaultColumnFieldNames = new List<string>();
        List<string> hiddenColumnFieldNames = new List<string>();
        string subJob = "SubJob";
        string workpack = "Workpack";
        string area = "Area";
        string subArea = "Sub-Area";
        string documentType = "Document Type";
        string discipline = "Discipline";
        string department = "Department";
        string internalNumber = "Internal Number";
        string deliverableType = "Deliverable Type";
        string primaryTitle = "Primary Title";
        string secondaryTitle = "Secondary Title";
        string comments = "Comments";
        string Id = "Id";

        DataTable dataPointsTable = null;
        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || DisplayEntities == null)
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

                    dataPointsTable.Columns.Add(Id, typeof(Guid));
                    foreach(string defaultColumnFieldName in defaultColumnFieldNames)
                    {
                        if(defaultColumnFieldName != Id)
                            dataPointsTable.Columns.Add(defaultColumnFieldName, typeof(string));
                    }

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        dataPointsTable.Columns.Add(alignedDataDate.Date.ToShortDateString(), typeof(decimal));
                    }

                    string currentDataDateColumn = loadPROGRESS.DATA_DATE.ToShortDateString();
                    if (!dataPointsTable.Columns.Contains(currentDataDateColumn))
                        dataPointsTable.Columns.Add(currentDataDateColumn, typeof(decimal));

                    foreach(BASELINE_ITEMProgress entity in DisplayEntities)
                    {
                        BuildRowStats(entity, false);
                    }
                }

                return dataPointsTable;
            }
        }

        private void BuildRowStats(BASELINE_ITEMProgress entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            DataRow newDataRow;
            if(!isUpdate)
                newDataRow = dataPointsTable.NewRow();
            else
            {
                newDataRow = (from DataRow dr in dataPointsTable.Rows
                            where (Guid)dr[Id] == entity.GUID
                            select dr).FirstOrDefault();
            }

            if (newDataRow == null)
                return;

            newDataRow[Id] = entity.GUID;
            newDataRow[subJob] = entity.Subjob_Name;
            newDataRow[workpack] = entity.Entity.Entity.Workpack_Name;
            newDataRow[area] = entity.Entity.Entity.Area_Name;
            newDataRow[subArea] = entity.Entity.Entity.SubArea_Name;
            newDataRow[documentType] = entity.Entity.Entity.DocType_Name;
            newDataRow[discipline] = entity.Entity.Entity.Discipline_Name;
            newDataRow[department] = entity.Entity.Entity.Department_Name;
            newDataRow[internalNumber] = entity.Entity.Entity.INTERNAL_NUM;
            newDataRow[deliverableType] = entity.Entity.Entity.DELIVERABLE_TYPE;
            newDataRow[primaryTitle] = entity.Entity.Entity.PRIMARY_TITLE;
            newDataRow[secondaryTitle] = entity.Entity.Entity.SECONDARY_TITLE;
            newDataRow[comments] = entity.Entity.Entity.COMMENTS;

            for (int i = 0; i < newDataRow.ItemArray.Count(); i++)
            {
                string columnName = dataPointsTable.Columns[i].ColumnName;
                if (!defaultColumnFieldNames.Any(x => x == columnName))
                    newDataRow[columnName] = 0.00m;
            }

            if (entity.Stats.Earned != null && entity.Stats.Earned.CumulativeDataPoints != null)
                foreach (Common.ViewModel.Reporting.DataPoint progress in entity.Stats.Earned.CumulativeDataPoints)
                {
                    string dateField = progress.ProgressDate.Date.ToShortDateString();
                    if (dataPointsTable.Columns.Contains(dateField))
                    {
                        newDataRow[dateField] = progress.UnitsPercentage;
                    }
                }

            if(!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "OffsiteDirectProgressDistributionViewModelWrapper_v3"; }
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