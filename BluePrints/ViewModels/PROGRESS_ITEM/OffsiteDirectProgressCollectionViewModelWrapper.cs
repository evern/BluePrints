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
using BluePrints.Common.Filtering;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.DataAccess.Excel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Spreadsheet;
using DevExpress.SpreadsheetSource;
using DevExpress.Xpf.Editors;
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
    public partial class OffsiteDirectProgressCollectionViewModelWrapper : BluePrintsEntitiesProgressCollectionWrapper<BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<BASELINE_ITEMProgress>
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
        protected IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            AlwaysSkipMessage = true;
            is_load_p6_task = true;
            isUseReportDate = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_ProgressPreviousWeeksDate)) != LoginCredentials.PermissionStatus.None;
            canDateBackwardForward = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_CanDateBackwardForward)) != LoginCredentials.PermissionStatus.None;
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

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_CONTRACTORS, PROJECT_CONTRACTORProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DSTATUS_DOCTYPES, DSTATUS_DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);

            base.addEntitiesLoader();
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE != PhaseType.Construct && x.PHASE_TYPE != PhaseType.Procurement);
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

        protected virtual Func<IRepositoryQuery<PROJECT_CONTRACTOR>, IQueryable<PROJECT_CONTRACTOR>> PROJECT_CONTRACTORProjectionFunc()
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
        }

        public bool CanUpdateAllPercentagesByStatus()
        {
            if (IsLoading)
                return false;

            return LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_UpdateProgressByStatus)) != LoginCredentials.PermissionStatus.None;
        }

        public void UpdateAllPercentagesByStatus()
        {
            if (IsLoading)
                return;

            if (MessageBoxService.ShowMessage("Warning\nThis action will update or delete progresses based on deliverable status and is not reversible\nDo you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            MainViewModel.AlwaysSkipMessage = true;
            BluePrintsDataUtils.UpdatePercentagesByStatus(MessageBoxService, PROGRESS_ITEMSCollectionViewModel, MainViewModel.Entities);
            MainViewModel.AlwaysSkipMessage = false;
            FullRefresh();
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            //avoid crash when this is disposed
            if(PROGRESS_ITEMSCollectionViewModel != null)
            {
                //progress items needs to get notified for view to reflect update
                PROGRESS_ITEMSCollectionViewModel.AlwaysSkipMessage = false;

                //only respond to message from same key
                PROGRESS_ITEMSCollectionViewModel.RefreshOnlyOnSameSenderKey = true;

                if (PROJECT_REPORTCollectionViewModel != null)
                    PROJECT_REPORTCollectionViewModel.AlwaysSkipMessage = false;
            }

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, loadPROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, DeliverableInternalNumberMode.Default, isUseReportDate, P6TASKCollection, null, null, null, null, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection, null, null, null, PROGRESS_ETCCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.FuncManualCellPastingIsContinue = BluePrintsDataUtils.FuncManualCellPastingIsContinue;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #region Collection Call Backs

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DeliverableStatusProgressGuid))
                projection.ShouldSave = true;
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.DISCIPLINE_NUM)))
                projection.ShouldSave = true;
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.FORECAST_START_DATE)))
                projection.ShouldSave = true;
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.TARGET_DATE)))
                projection.ShouldSave = true;
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_PROJECT_CONTRACTOR)))
                projection.ShouldSave = true;

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(BASELINE_ITEMProgress projection, out bool isNew)
        {
            isNew = false;
            if (projection.ShouldSave)
            {
                projection.ShouldSave = false;
                return OperationInterceptMode.Continue;
            }
            else
                return OperationInterceptMode.SkipOneAndAllDbSaves;
        }

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
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// Show document type even when it is not valid
        /// </summary>
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DeliverableStatusProgressGuid)) && e.Row != null)
            {
                BASELINE_ITEMProgress projection = (BASELINE_ITEMProgress)e.Row;
                if (projection.Entity.Entity.DELIVERABLES_STATUS != null)
                    e.DisplayText = projection.Entity.Entity.DELIVERABLES_STATUS.NAME;
            }
        }

        public bool CanClearDeliverableStatus()
        {
            return !IsLoading && SelectedEntities.Count > 0;
        }


        public void ClearDeliverableStatus()
        {
            List<BASELINE_ITEMProgress> saveEntities = new List<BASELINE_ITEMProgress>();
            foreach (BASELINE_ITEMProgress entity in SelectedEntities)
            {
                entity.DeliverableStatusProgressGuid = null;
                //to prevent custom display text from showing it
                entity.Entity.Entity.DELIVERABLES_STATUS = null;
                saveEntities.Add(entity);
            }

            MainViewModel.BaseBulkSave(saveEntities);
        }


        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "OffsiteDirectProgressViewModelWrapper_v7"; }
        }

        public bool IsDataDateChangeVisible => canDateBackwardForward;

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

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<Data.PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>();
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

        protected override bool manuallySaveProgressOnAfterBaselineItemSaved => false;

        protected override bool isSingleProjectAndUserLocale => true;
        #endregion

        public bool CanFixVariation()
        {
            return !IsLoading;
        }

        public void FixVariation()
        {
            IBluePrintsEntitiesUnitOfWork unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            IQueryable<VARIATION> approvedVariations = unitOfWork.VARIATIONS.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.APPROVED != null && x.ADJUSTMENT_TO_BUDGET == false);

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

            MainViewModel.BaseBulkSave(saveEntities);
        }

        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Please check whether data date is correct before updating", null, null, null);
            notification1.ShowAsync();
        }

        public bool CanUpdateTargetDates()
        {
            return !IsLoading;
        }

        public void UpdateTargetDates()
        {
            if (MessageBoxService.ShowMessage("This will copy start dates and due dates to target dates, do you wish to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<BASELINE_ITEMProgress> saveProgresses = new List<BASELINE_ITEMProgress>();
            IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            foreach (BASELINE_ITEMProgress progress in MainViewModel.Entities)
            {
                BASELINE_ITEM repositoryBASELINE_ITEM = bluePrintsUnitOfWork.BASELINE_ITEMS.FirstOrDefault(x => x.GUID == progress.Entity.Entity.GUID);
                if(repositoryBASELINE_ITEM != null)
                {
                    repositoryBASELINE_ITEM.FORECAST_START_DATE = progress.StartDate;
                    repositoryBASELINE_ITEM.TARGET_DATE = progress.DueDate;
                }
            }

            bluePrintsUnitOfWork.SaveChanges();
            FullRefresh();
        }

        public bool CanExportContractorDeliverablesToExcel()
        {
            return !IsLoading;
        }

        private PROJECT_CONTRACTOR selectedContractor;
        public IEnumerable<BASELINE_ITEMProgress> ContractorDeliverableList
        {
            get
            {
                if (selectedContractor == null)
                    return null;
                else
                    return Entities.Where(x => x.Entity.Entity.GUID_PROJECT_CONTRACTOR == selectedContractor.GUID);
            }
        }

        private PROJECT_CONTRACTOR ContractorExportSelection()
        {
            var bulkEditEnumsViewModel = BulkEditEnumsViewModel.Create(PROJECT_CONTRACTORCollection, "NAME");
            if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Select contractor to export",
                    "BulkEditEnums", bulkEditEnumsViewModel) == MessageResult.OK)
            {
                if (bulkEditEnumsViewModel.SelectedItem != null)
                {
                    return (PROJECT_CONTRACTOR)bulkEditEnumsViewModel.SelectedItem;
                }
            }

            return null;
        }

        [ServiceProperty(Key = "ExportTableViewService")]
        protected virtual ITableViewService ExportTableViewService { get { return null; } }
        public int InternalNumSortIndex => 1;
        public void ExportContractorDeliverablesToExcel()
        {
            string ResultPath = string.Empty;
            selectedContractor = ContractorExportSelection();
            this.RaisePropertyChanged(x => x.ContractorDeliverableList);

            if(ContractorDeliverableList == null)
            {
                if(selectedContractor == null)
                    MessageBoxService.ShowMessage("Contractor is not selected", "Error", MessageButton.OK, MessageIcon.Warning);
                else
                    MessageBoxService.ShowMessage("There are no deliverable(s) for " + selectedContractor.NAME + " please assign contractor column on deliverable(s)", "Error", MessageButton.OK, MessageIcon.Warning);

                return;
            }

            this.RaisePropertyChanged(x => x.ContractorDeliverableList);
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                bool result = ExportTableViewService.ExportToXls(ResultPath + "\\" + loadPROJECT.NUMBER + "_ContractorExport_" + selectedContractor.NAME + "_" + DataDate.Date.ToString(BluePrintsResources.ColumnDateFormat) + ".xlsx", isExcelExportDataAware, selectedContractor.NAME);

                if (!result)
                    MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Warning);
            }
        }

        public Guid? ContractorOfficeGuid
        {
            get
            {
                OFFICE findOFFICE = OFFICECollection.FirstOrDefault(x => x.NAME.ToUpper() == BluePrintsResources.Deliverables_Contractor_Filter.ToUpper());
                if (findOFFICE != null)
                    return findOFFICE.GUID;

                return null;
            }
        }

        private DevExpress.Mvvm.IDialogService ImportDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ImportDialog"); }
        }

        public bool CanImportContractorDeliverableFromExcel()
        {
            return CanExportContractorDeliverablesToExcel();
        }

        public string AreaHeaderString => ColumnHeaderResources.AreaHeaderString;
        public string SubAreaHeaderString => ColumnHeaderResources.SubAreaHeaderString;
        public string DisciplineHeaderString => ColumnHeaderResources.DisciplineHeaderString;
        public string DisciplineNumberHeaderString => ColumnHeaderResources.DisciplineNumberHeaderString;
        public string DocumentTypeHeaderString => ColumnHeaderResources.DocumentTypeHeaderString;
        public string DeliverableTypeHeaderString => ColumnHeaderResources.DeliverableTypeHeaderString;
        public string DepartmentHeaderString => ColumnHeaderResources.DepartmentHeaderString;
        public string ClientNumberHeaderString => ColumnHeaderResources.ClientNumberHeaderString;
        public string PrimaryTitleHeaderString => ColumnHeaderResources.PrimaryTitleHeaderString;
        public string SecondaryTitleHeaderString => ColumnHeaderResources.SecondaryTitleHeaderString;
        public string CommentsHeaderString => ColumnHeaderResources.CommentsHeaderString;
        public string ResourceHeaderString => ColumnHeaderResources.ResourceHeaderString;
        public string SubJobHeaderString => ColumnHeaderResources.SubJobHeaderString;
        public string OfficeHeaderString => ColumnHeaderResources.OfficeHeaderString;
        public string PhaseHeaderString => ColumnHeaderResources.PhaseHeaderString;
        public string InternalNumberHeaderString => ColumnHeaderResources.InternalNumberHeaderString;
        public string CurrentPercentageHeaderString => ColumnHeaderResources.CurrentPercentageHeaderString;
        public string BudgetHourHeaderString => ColumnHeaderResources.BudgetHourHeaderString;

        private string GetWorkSheetNameByIndex(int p, string fileName)
        {
            string worksheetName = "";
            using (ISpreadsheetSource spreadsheetSource = SpreadsheetSourceFactory.CreateSource(fileName))
            {
                IWorksheetCollection worksheetCollection = spreadsheetSource.Worksheets;
                worksheetName = worksheetCollection[p].Name;
            }
            return worksheetName;
        }

        public void ImportContractorDeliverableFromExcel()
        {
            FileBrowserDialogService.Filter = "Excel Files (.xls)|*.xlsx|All Files (*.*)|*.*";
            if (FileBrowserDialogService.ShowDialog())
            {
                string fullFileName = FileBrowserDialogService.GetFullFileName();
                string[] fileNamesSplit = fullFileName.Split('_');
                string dateString = fileNamesSplit.Last();
                dateString = dateString.Split('.').First();
                DateTime importSheetDate;

                if(DateTime.TryParse(dateString, out importSheetDate))
                {
                    if(importSheetDate.Date == DataDate.Date)
                    {
                        ExcelDataSource excelDataSource = new ExcelDataSource();
                        excelDataSource.Name = "Excel Data Source";
                        excelDataSource.FileName = FileBrowserDialogService.GetFullFileName();
                        ExcelWorksheetSettings worksheetSettings = new ExcelWorksheetSettings();
                        worksheetSettings.WorksheetName = GetWorkSheetNameByIndex(0, fullFileName);
                        excelDataSource.SourceOptions = new ExcelSourceOptions(worksheetSettings);

                        PROJECT_CONTRACTOR findPROJECT_CONTRACTOR = PROJECT_CONTRACTORCollection.FirstOrDefault(x => x.NAME.ToUpper() == worksheetSettings.WorksheetName.ToUpper());

                        if(findPROJECT_CONTRACTOR == null)
                        {
                            MessageBoxService.ShowMessage("Contractor " + worksheetSettings.WorksheetName + " cannot be found", "Error", MessageButton.OK, MessageIcon.Warning);
                            return;
                        }

                        selectedContractor = findPROJECT_CONTRACTOR;
                        this.RaisePropertyChanged(x => x.ContractorDeliverableList);

                        excelDataSource.Fill();
                        DataTable excelSourceDataTable = excelDataSource.ToDataTable();
                        if (ContractorDeliverableList == null)
                        {
                            MessageBoxService.ShowMessage("Could not find any contractor deliverable to import to", "Error", MessageButton.OK, MessageIcon.Warning);
                            return;
                        }

                        List<PROGRESS_ITEM> updateProgress = new List<PROGRESS_ITEM>();
                        List<ErrorMessage> errorMessages = new List<ErrorMessage>();
                        List<BASELINE_ITEMProgressImportWrapper> importBaselineItems = new List<BASELINE_ITEMProgressImportWrapper>();
                        List<ErrorMessage> columnHeaderErrorMessages = new List<ErrorMessage>();
                        BASELINE_ITEMProgressImportWrapperHelper.IsColumnHeadersExists(excelSourceDataTable, out columnHeaderErrorMessages);
                        if(columnHeaderErrorMessages.Count > 0)
                        {
                            ShowErrorMessage("Missing columns in Excel", columnHeaderErrorMessages);
                            return;
                        }

                        foreach (DataRow dataRow in excelSourceDataTable.Rows)
                        {
                            if (dataRow[InternalNumberHeaderString] != DBNull.Value)
                            {
                                string internalNumber = dataRow[InternalNumberHeaderString].ToString();
                                decimal newPercentage;
                                if (dataRow[CurrentPercentageHeaderString] == DBNull.Value)
                                    dataRow[CurrentPercentageHeaderString] = 0.ToString();

                                if (decimal.TryParse(dataRow[CurrentPercentageHeaderString].ToString(), out newPercentage))
                                {
                                    BASELINE_ITEMProgress changeTrackingBaselineItemProgress = BASELINE_ITEMProgressImportWrapperHelper.ConvertDataRowToBASELINE_ITEMProgress(dataRow, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection);
                                    List<BASELINE_ITEMProgress> findDeliverable = ContractorDeliverableList.Where(x => x.Deliverable_Name == internalNumber).ToList();
                                    if (findDeliverable.Count == 1)
                                    {
                                        BASELINE_ITEMProgress findContractorDeliverable = findDeliverable.First();
                                        string totalEarnedPercentageString = string.Format("{0:P2}", findContractorDeliverable.Total_Earned_Percentage);
                                        string newPercentageString = string.Format("{0:P2}", newPercentage);

                                        BASELINE_ITEMProgressImportWrapper newBASELINE_ITEMProgressImportWrapper = BASELINE_ITEMProgressImportWrapper.Create(findContractorDeliverable, changeTrackingBaselineItemProgress, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection);

                                        if (findContractorDeliverable.Total_Earned_Percentage > newPercentage)
                                        {
                                            newBASELINE_ITEMProgressImportWrapper.Message = "Contractor update of " + newPercentageString + " is less than current of " + totalEarnedPercentageString;
                                            newBASELINE_ITEMProgressImportWrapper.IsError = true;
                                        }
                                        else if(!newBASELINE_ITEMProgressImportWrapper.IsSame)
                                            newBASELINE_ITEMProgressImportWrapper.Import = true;

                                        importBaselineItems.Add(newBASELINE_ITEMProgressImportWrapper);
                                        //IEnumerable<PROGRESS_ITEM> newPROGRESS_ITEMS = UpdateContractorDeliverableProgress(findContractorDeliverable, newPercentage);
                                        //updateProgress.AddRange(newPROGRESS_ITEMS);
                                        //errorMessages.Add(new ErrorMessage(internalNumber, "Updated from " + totalEarnedPercentageString + "% to " + newPercentageString + "%, update success"));
                                    }
                                    else if (findDeliverable.Count == 0)
                                    {
                                        BASELINE_ITEMProgressImportWrapper newBASELINE_ITEMProgressImportWrapper = BASELINE_ITEMProgressImportWrapper.Create(changeTrackingBaselineItemProgress, changeTrackingBaselineItemProgress, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection);
                                        if (MainViewModel.Entities.Where(x => x.Entity.Entity.GUID_PROJECT_CONTRACTOR != selectedContractor.GUID).Any(x => x.Deliverable_Name == newBASELINE_ITEMProgressImportWrapper.Deliverable_Name))
                                        {
                                            newBASELINE_ITEMProgressImportWrapper.Message = "Error: There are other deliverable(s) with the same internal number";
                                            newBASELINE_ITEMProgressImportWrapper.IsError = true;
                                        }

                                        newBASELINE_ITEMProgressImportWrapper.IsNew = true;
                                        importBaselineItems.Add(newBASELINE_ITEMProgressImportWrapper);
                                        //errorMessages.Add(new ErrorMessage(internalNumber, "Contractor deliverable with internal number: " + internalNumber + " is not found, updated skipped"));

                                    }
                                    else
                                    {
                                        BASELINE_ITEMProgressImportWrapper newBASELINE_ITEMProgressImportWrapper = BASELINE_ITEMProgressImportWrapper.Create(changeTrackingBaselineItemProgress, changeTrackingBaselineItemProgress, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection);
                                        newBASELINE_ITEMProgressImportWrapper.Message = "Multiple deliverable with internal number: " + internalNumber + " is found, please revise deliverable's list or import sheet";
                                        newBASELINE_ITEMProgressImportWrapper.IsError = true;

                                        importBaselineItems.Add(newBASELINE_ITEMProgressImportWrapper);
                                        //errorMessages.Add(new ErrorMessage(internalNumber, "More than a single contractor deliverable with internal number: " + internalNumber + " is found, updated skipped"));
                                    }
                                }
                            }
                        }


                        ListImportDeliverableViewModel<BASELINE_ITEMProgressImportWrapper> viewModel = ListImportDeliverableViewModel<BASELINE_ITEMProgressImportWrapper>.Create(importBaselineItems, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection);
                        if (ImportDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListImportDeliverableView", viewModel) == MessageResult.OK)
                        {
                            foreach (BASELINE_ITEMProgressImportWrapper sourceObject in viewModel.SourceObjects.Where(x => x.Import))
                            {
                                BASELINE_ITEMProgress findDeliverable = ContractorDeliverableList.FirstOrDefault(x => x.Deliverable_Name == sourceObject.Deliverable_Name);
                                if (findDeliverable != null)
                                {
                                    bool isProgressUpdated = false;
                                    string totalEarnedPercentageString = string.Format("{0:P2}", findDeliverable.Total_Earned_Percentage);
                                    string newPercentageString = string.Format("{0:P2}", sourceObject.Total_Earned_Percentage);

                                    if (sourceObject.Total_Earned_Percentage > findDeliverable.Total_Earned_Percentage)
                                    {
                                        IEnumerable<PROGRESS_ITEM> newPROGRESS_ITEMS = UpdateContractorDeliverableProgress(findDeliverable, sourceObject.Total_Earned_Percentage);
                                        updateProgress.AddRange(newPROGRESS_ITEMS);
                                        isProgressUpdated = true;
                                    }

                                    bool isPropertiesUpdated = false;
                                    if (sourceObject.IsAnyPropertyDifferent())
                                    {
                                        isPropertiesUpdated = true;
                                        findDeliverable.Phase_Guid = sourceObject.Phase_Guid;
                                        findDeliverable.Entity.Entity.GUID_AREA = sourceObject.Entity.Entity.GUID_AREA;
                                        findDeliverable.Entity.Entity.GUID_SUBAREA = sourceObject.Entity.Entity.GUID_SUBAREA;
                                        findDeliverable.Entity.Entity.GUID_DISCIPLINE = sourceObject.Entity.Entity.GUID_DISCIPLINE;
                                        findDeliverable.Entity.Entity.DISCIPLINE_NUM = sourceObject.Entity.Entity.DISCIPLINE_NUM;
                                        findDeliverable.Entity.Entity.DELIVERABLE_TYPE = sourceObject.Entity.Entity.DELIVERABLE_TYPE;
                                        findDeliverable.Entity.Entity.GUID_DEPARTMENT = sourceObject.Entity.Entity.GUID_DEPARTMENT;
                                        findDeliverable.Entity.Entity.INTERNAL_NUM = sourceObject.Entity.Entity.INTERNAL_NUM;
                                        findDeliverable.Entity.Entity.CLIENT_NUM = sourceObject.Entity.Entity.CLIENT_NUM;
                                        findDeliverable.Entity.Entity.PRIMARY_TITLE = sourceObject.Entity.Entity.PRIMARY_TITLE;
                                        findDeliverable.Entity.Entity.SECONDARY_TITLE = sourceObject.Entity.Entity.SECONDARY_TITLE;
                                        findDeliverable.Entity.Entity.COMMENTS = sourceObject.Entity.Entity.COMMENTS;
                                        findDeliverable.ShouldSave = true;
                                        MainViewModel.Save(findDeliverable);
                                    }

                                    if(isProgressUpdated && isPropertiesUpdated)
                                        errorMessages.Add(new ErrorMessage(sourceObject.Deliverable_Name, "Properties updated and progress updated from " + totalEarnedPercentageString + " to " + newPercentageString + ", update success"));
                                    else if(isProgressUpdated)
                                        errorMessages.Add(new ErrorMessage(sourceObject.Deliverable_Name, "Progress updated from " + totalEarnedPercentageString + " to " + newPercentageString + ", update success"));
                                    else if(isPropertiesUpdated)
                                        errorMessages.Add(new ErrorMessage(sourceObject.Deliverable_Name, "Properties updated"));
                                }
                                else
                                {
                                    sourceObject.Live_PROGRESS = loadPROGRESS;
                                    sourceObject.SetReportingDataDate(DataDate);
                                    sourceObject.Entity.Entity.GUID_OFFICE = ContractorOfficeGuid;
                                    sourceObject.Entity.Entity.GUID_PROJECT_CONTRACTOR = selectedContractor.GUID;
                                    sourceObject.ShouldSave = true;
                                    decimal totalEarnedPercentage = sourceObject.Total_Earned_Percentage;
                                    //mainly for generating sub job
                                    BluePrintsDataUtils.OnBeforeSavingBASELINE_ITEM(bluePrintsUnitOfWork, sourceObject, loadPROJECT, loadBASELINE, DeliverablesViewType.Both, PHASECollection, MainViewModel.Entities.Select(x => x.Entity.Entity), AREACollection, SUBAREACollection, DISCIPLINECollection, DOCTYPECollection, WORKPACKCollection, SUBJOBCollection, WORKPACKSCollectionViewModel, false, false);

                                    //restore total earned percentage because it's being resetted when projection.Update() is called in BluePrintsDataUtils.OnBeforeSavingBASELINE_ITEM
                                    sourceObject.Total_Earned_Percentage = totalEarnedPercentage;

                                    errorMessages.Add(new ErrorMessage(sourceObject.Deliverable_Name, "Deliverable added"));
                                    MainViewModel.Save(sourceObject);
                                    sourceObject.Update();
                                }
                            }
                        }

                        PROGRESS_ITEMSCollectionViewModel.BaseBulkSave(updateProgress);
                        if (errorMessages.Count > 0)
                            ShowErrorMessage("Contractor Deliverable Update Status", errorMessages);
                        else
                            MessageBoxService.ShowMessage("No update has been done", "Status", MessageButton.OK, MessageIcon.Information);
                    }
                    else
                    {
                        MessageBoxService.ShowMessage("Import excel workbook data date of " + importSheetDate.Date.ToShortDateString() + " doesn't match current data date of " + DataDate.Date.ToShortDateString(), "Import Error", MessageButton.OK, MessageIcon.Warning);
                    }
                }
                else
                {
                    MessageBoxService.ShowMessage("Data date cannot be determined from import spreadsheet file name, please rename it ending with " + loadPROGRESS.DATA_DATE.ToString(BluePrintsResources.ColumnDateFormat));
                }
            }
        }

        protected override bool OnBeforeApplyingProjectionPropertiesToEntityIsContinue(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity)
        {
            if(projectionEntity.GetType().BaseType == typeof(BASELINE_ITEMProgressImportWrapper))
            {
                projectionEntity.Entity.Entity.GUID_BASELINE = loadBASELINE.GUID;
                DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            }

            return base.OnBeforeApplyingProjectionPropertiesToEntityIsContinue(projectionEntity, entity);
        }

        protected override void OnAfterEntitySavedCallBack(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity, bool isNewEntity)
        {
            if (projectionEntity.GetType().BaseType == typeof(BASELINE_ITEMProgressImportWrapper))
            {
                projectionEntity.Entity.Entity = entity;
            }

            base.OnAfterEntitySavedCallBack(projectionEntity, entity, isNewEntity);
        }

        private IEnumerable<PROGRESS_ITEM> UpdateContractorDeliverableProgress(BASELINE_ITEMProgress findContractorDeliverable, decimal newPercentage)
        {
            if(findContractorDeliverable != null)
            {
                findContractorDeliverable.Total_Earned_Percentage = newPercentage;
                IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = findContractorDeliverable.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel);
                return newPRORESS_ITEMS;
            }

            return new List<PROGRESS_ITEM>();
        }

        public IEnumerable<PROJECT_CONTRACTOR> PROJECT_CONTRACTORCollection
        {
            get
            {
                var collection = GetEntities<PROJECT_CONTRACTOR>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public override IEnumerable<IReportable> ReportingEntities => Entities;
    }
}