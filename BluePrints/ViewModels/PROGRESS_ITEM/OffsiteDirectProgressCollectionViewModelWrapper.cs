using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
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
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
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
        protected override void resolveParameters(object parameter)
        {
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

        public IEnumerable<BASELINE_ITEMProgress> ContractorDeliverableList
        {
            get
            {
                if (ContractorOfficeGuid == null)
                    return null;
                else
                    return Entities.Where(x => x.Entity.Entity.GUID_OFFICE == ContractorOfficeGuid);
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

        [ServiceProperty(Key = "ExportTableViewService")]
        protected virtual ITableViewService ExportTableViewService { get { return null; } }
        public int InternalNumSortIndex => 1;
        public void ExportContractorDeliverablesToExcel()
        {
            string ResultPath = string.Empty; 
            if(ContractorDeliverableList == null)
            {
                MessageBoxService.ShowMessage("There are no contractor deliverables, please assign 'Office' column in deliverables list to " + BluePrintsResources.Deliverables_Contractor_Filter, "Error", MessageButton.OK, MessageIcon.Warning);
                return;
            }

            this.RaisePropertyChanged(x => x.ContractorDeliverableList);
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                bool result = ExportTableViewService.ExportToXls(ResultPath + "\\" + loadPROJECT.NUMBER + "_ContractorExport_" + loadPROGRESS.DATA_DATE.ToString("dd-MMM-yy") + ".xlsx", isExcelExportDataAware);

                if (!result)
                    MessageBoxService.ShowMessage("Export failed because the file is in use", "Warning", MessageButton.OK, MessageIcon.Warning);
            }
        }

        public bool CanImportContractorDeliverableFromExcel()
        {
            return CanExportContractorDeliverablesToExcel();
        }

        public string InternalNumberHeaderString => "Internal Number";
        public string CurrentPercentageHeaderString => "Current %";
        public void ImportContractorDeliverableFromExcel()
        {
            if(FileBrowserDialogService.ShowDialog())
            {
                ExcelDataSource excelDataSource = new ExcelDataSource();
                excelDataSource.Name = "Excel Data Source";
                excelDataSource.FileName = FileBrowserDialogService.GetFullFileName();
                ExcelWorksheetSettings worksheetSettings = new ExcelWorksheetSettings("Sheet");
                excelDataSource.SourceOptions = new ExcelSourceOptions(worksheetSettings);
                excelDataSource.Fill();

                DataTable excelSourceDataTable = excelDataSource.ToDataTable();
                if (ContractorDeliverableList == null)
                {
                    MessageBoxService.ShowMessage("Could not find any contractor deliverable to import to", "Error", MessageButton.OK, MessageIcon.Warning);
                    return;
                }

                List<PROGRESS_ITEM> updateProgress = new List<PROGRESS_ITEM>();
                List<ErrorMessage> errorMessages = new List<ErrorMessage>();
                foreach(DataRow dataRow in excelSourceDataTable.Rows)
                {
                    if(dataRow[InternalNumberHeaderString] != DBNull.Value && dataRow[CurrentPercentageHeaderString] != DBNull.Value)
                    {
                        string internalNumber = dataRow[InternalNumberHeaderString].ToString();
                        decimal newPercentage;
                        if (decimal.TryParse(dataRow[CurrentPercentageHeaderString].ToString(), out newPercentage))
                        {
                            List<BASELINE_ITEMProgress> findContractorDeliverables = ContractorDeliverableList.Where(x => x.Deliverable_Name == internalNumber).ToList();
                            if (findContractorDeliverables.Count == 1)
                            {
                                BASELINE_ITEMProgress findContractorDeliverable = findContractorDeliverables.First();
                                string totalEarnedPercentageString = string.Format("{0:P2}.", findContractorDeliverable.Total_Earned_Percentage);
                                string newPercentageString = string.Format("{0:P2}.", newPercentage);
                                if (findContractorDeliverable.Total_Earned_Percentage == newPercentage)
                                    continue;

                                if (findContractorDeliverable.Total_Earned_Percentage > newPercentage)
                                {
                                    errorMessages.Add(new ErrorMessage(findContractorDeliverable.Deliverable_Name, "Contractor update % of " + newPercentageString + " is less than current % of " + totalEarnedPercentageString + ", update skipped"));
                                    continue;
                                }

                                IEnumerable<PROGRESS_ITEM> newPROGRESS_ITEMS = UpdateContractorDeliverableProgress(findContractorDeliverable, newPercentage);
                                updateProgress.AddRange(newPROGRESS_ITEMS);
                                errorMessages.Add(new ErrorMessage(internalNumber, "Updated from " + totalEarnedPercentageString + "% to " + newPercentageString + "%, update success"));
                            }
                            else if(findContractorDeliverables.Count == 0)
                                errorMessages.Add(new ErrorMessage(internalNumber, "Contractor deliverable with internal number: " + internalNumber + " is not found, updated skipped"));
                            else
                                errorMessages.Add(new ErrorMessage(internalNumber, "More than a single contractor deliverable with internal number: " + internalNumber + " is found, updated skipped"));

                        }
                    }
                }

                PROGRESS_ITEMSCollectionViewModel.BaseBulkSave(updateProgress);
                if (errorMessages.Count > 0)
                    ShowErrorMessage("Contractor Deliverable Update Status", errorMessages);
                else
                    MessageBoxService.ShowMessage("No contractor deliverable has been progressed, nothing to import", "Status", MessageButton.OK, MessageIcon.Information);
            }
        }

        private IEnumerable<PROGRESS_ITEM> UpdateContractorDeliverableProgress(BASELINE_ITEMProgress findContractorDeliverable, decimal newPercentage)
        {
            if(findContractorDeliverable != null)
            {
                findContractorDeliverable.Total_Earned_Percentage = newPercentage;
                IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = findContractorDeliverable.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                return newPRORESS_ITEMS;
            }

            return new List<PROGRESS_ITEM>();
        }

        public override IEnumerable<IReportable> ReportingEntities => Entities;
    }
}