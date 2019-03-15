using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class OffsiteDirectProgressCollectionViewModelWrapper :
        BluePrintsEntitiesProgressCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<BASELINE_ITEMProgress>
    {

        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectProgressCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectProgressCollectionViewModelWrapper());
        }

        #region Database Operation
        protected override void resolveParameters(object parameter)
        {
            is_load_p6_task = true;
            base.resolveParameters(parameter);
        }

        private BASELINE loadBASELINE;
        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            //in user offsite direct view model wrapper baseline should not be loaded because query gets from navigational baseline
            if(is_single_project_mode)
            {
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
                loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, x => assign_baseline(x));
            }

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DSTATUS_DOCTYPES, DSTATUS_DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);

            base.addEntitiesLoader();
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

        protected virtual Func<IRepositoryQuery<DSTATUS_DOCTYPE>, IQueryable<DSTATUS_DOCTYPE>> DSTATUS_DOCTYPEProjectionFunc()
        {
            if (is_single_project_mode)
                return query => query.Where(x => x.DELIVERABLES_STATUS.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.DELIVERABLES_STATUS.PROJECT.STATUS == ProjectStatus.Active);
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
            BluePrintsDataUtils.UpdateAllPercentagesByStatus(MessageBoxService, PROGRESS_ITEMSCollectionViewModel, MainViewModel.Entities);
            FullRefresh();
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, loadPROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, DeliverableInternalNumberMode.Default, false, P6TASKCollection, null, null, false, null, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.FuncManualCellPastingIsContinue = BluePrintsDataUtils.FuncManualCellPastingIsContinue;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        //protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        //{
        //    string fieldName = DataUtils.FormatColumnFieldname(e.Column.FieldName);
        //    if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.DeliverableStatusGuid))
        //    {
        //        BASELINE_ITEMProgress deliverable = (BASELINE_ITEMProgress)e.Row;

        //        if(e.Value != null)
        //        {
        //            Guid current_deliverable_status_guid = (Guid)e.Value;
        //            DELIVERABLES_STATUS current_deliverable_status = deliverable.Entity.Entity.DeliverableStatusCollection.FirstOrDefault(x => x.GUID == current_deliverable_status_guid);
        //            if (current_deliverable_status != null && current_deliverable_status.AUTO_PERCENTAGE != null)
        //            {
        //                decimal auto_percentage = (decimal)current_deliverable_status.AUTO_PERCENTAGE;
        //                if (current_deliverable_status.AUTO_PERCENTAGE > deliverable.Total_Earned_Percentage)
        //                {
        //                    decimal oldValue = deliverable.Total_Earned_Percentage;
        //                    deliverable.Total_Earned_Percentage = auto_percentage;

        //                    string undo_redo_fieldname = BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage);

        //                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
        //                    MainViewModel.EntitiesUndoRedoManager.AddUndo(deliverable, undo_redo_fieldname, oldValue, auto_percentage, EntityMessageType.Changed);
        //                }
        //            }
        //        }
        //    }

        //    base.CellValueExistingRowChanging(e);
        //}
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
            ReloadEntitiesCollection();
        }
        #endregion

        #endregion

        #region View Properties
        //public void OnCustomColumnSort(CustomColumnSortEventArgs e)
        //{
        //    string baseEntityString = "Entity.Entity.";
        //    if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.BUDGET_HOURS) ||
        //        e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.DC_HOURS) ||
        //        e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.Total_Units) ||
        //        e.Column.FieldName == "Entity." + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.ItemRate) || 
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Baseline_Percentage) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Percentage_ToDate) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().SchedulePercentage) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Earned_Units_ToDate) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Earned_Costs_ToDate) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().ScheduleCurrentPeriodPercentage) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Earned_Percentage_OnDataDate) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Earned_Units_OnDataDate) ||
        //        e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Earned_Costs_OnDataDate))
        //    {
        //        decimal decimal_value1 = e.Value1 == null ? 0 : (decimal)e.Value1;
        //        decimal decimal_value2 = e.Value2 == null ? 0 : (decimal)e.Value2;

        //        e.Result = decimal_value1.CompareTo(decimal_value2);
        //        e.Handled = true;
        //    }
        //    else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().ForecastDate) ||
        //            e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.TARGET_DATE) ||
        //            e.Column.FieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DueDate))
        //    {
        //        DateTime datetime_value1 = e.Value1 == null ? new DateTime() : (DateTime)e.Value1;
        //        DateTime datetime_value2 = e.Value2 == null ? new DateTime() : (DateTime)e.Value2;

        //        e.Result = datetime_value1.CompareTo(datetime_value2);
        //        e.Handled = true;
        //    }
        //}

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

        public IEnumerable<DSTATUS_DOCTYPE> DSTATUS_DOCTYPECollection
        {
            get
            {
                return GetEntities<DSTATUS_DOCTYPE>();
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

        protected override PhaseType progress_type => PhaseType.Design;

        protected override bool haveGroupEntity => false;
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

        public void FixVariation()
        {
            IBluePrintsEntitiesUnitOfWork unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            IQueryable<VARIATION> approvedVariations = unitOfWork.VARIATIONS.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.APPROVED != null);

            List<BASELINE_ITEMProgress> saveEntities = new List<BASELINE_ITEMProgress>();
            foreach (var entity in MainViewModel.Entities)
            {
                IQueryable<VARIATION_ITEM> variationItems = approvedVariations.SelectMany(x => x.VARIATION_ITEM);
                IQueryable<VARIATION_ITEM> currentEntityVariationItems = variationItems.Where(x => x.GUID_ORIBASEITEM == entity.OriginalEntityKey);
                if(currentEntityVariationItems.Count() > 0)
                {
                    decimal calculatedVariationUnits = currentEntityVariationItems.Sum(x => x.VARIATION_UNITS);
                    if (entity.Entity.Entity.DC_HOURS != calculatedVariationUnits)
                    {
                        entity.Entity.Entity.DC_HOURS = calculatedVariationUnits;
                        saveEntities.Add(entity);
                    }
                }
            }

            MainViewModel.BulkSave(saveEntities);
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
            fullSummarizer.BuildBurnedDataPoints(false);
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