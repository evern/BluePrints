using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
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
using BluePrints.Common.ViewModel.Misc;
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
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraGrid.Views.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class OffsiteDirectDistributionCollectionViewModelWrapper : BluePrintsEntitiesProgressCollectionWrapper<BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<BASELINE_ITEMProgress>
    {
        IBluePrintsEntitiesUnitOfWork bluePrintsUOW;
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectDistributionCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectDistributionCollectionViewModelWrapper());
        }

        #region Database Operation

        List<string> defaultColumnFieldNames = new List<string>();
        List<string> hiddenColumnFieldNames = new List<string>();
        List<string> systemColumnFieldNames = new List<string>();

        string unboundProgressIdFieldname = "ProgressId";
        string columnEntity = "Entity";
        string columnPrimaryTitle = "Entity.Entity.Entity.PRIMARY_TITLE";
        string columnDeliverableStatus = "Entity.DeliverableStatusProgressGuid";
        Timer focusLastColumnTimer = new Timer();
        BluePrintsNativeEntities nativeDataContext = new BluePrintsNativeEntities();
        protected override void resolveParameters(object parameter)
        {
            defaultColumnFieldNames.Add(columnEntity);

            systemColumnFieldNames.Add(columnEntity);
            systemColumnFieldNames.Add(columnPrimaryTitle);
            systemColumnFieldNames.Add(columnDeliverableStatus);

            is_load_p6_task = true;
            bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            extrapolateDataDate = true;
            focusLastColumnTimer.Interval = 1000;
            focusLastColumnTimer.Elapsed += FocusLastColumnTimer_Elapsed;
            GlobalMethods.SetAccordionExpandedState?.Invoke(false);
            base.resolveParameters(parameter);
        }

        private void FocusLastColumnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            focusLastColumnTimer.Stop();
            mainThreadDispatcher.BeginInvoke(new Action(() => TableViewService.ScrollToLast()));
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
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, loadPROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, DeliverableInternalNumberMode.Default, false, P6TASKCollection, null, null, DateTime.Now.Date, null, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection);
        }

        bool isBestFitApplied;
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            //placed before current routine because AlwaysSkipMessage should be set to false
            base.AssignCallBacksAndRaisePropertyChange(entities);
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            MainViewModel.ValidateFillDownCallBack = ValidateFillDownCallBack;
            MainViewModel.IsPasteCellLevel = false;
            MainViewModel.AlwaysSkipMessage = false;
            MainViewModel.RefreshOnSameSenderKey = true;
            MainViewModel.DisableEntitiesPauseUnpause = true;
            PROGRESS_ITEMSCollectionViewModel.AlwaysSkipMessage = false;
            PROGRESS_ITEMSCollectionViewModel.RefreshOnSameSenderKey = true;
            PROGRESS_ITEMSCollectionViewModel.DisableEntitiesPauseUnpause = true;
            doNotApplyBestFit = true;
        }

        private void applyBestFit()
        {
            if (TableViewService != null && !isBestFitApplied)
            {
                if (Entities != null && Entities.Count > 0)
                    TableViewService.ApplyBestFit();

                TableViewService.ScrollToLast();
                isBestFitApplied = true;
            }
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

        protected override void delayedPROGRESSSavingDispatcher_Tick(object sender, EventArgs e)
        {
            //base delayed progress saving will call FullRefresh()
            alignedDataDateCollection = null;
            base.delayedPROGRESSSavingDispatcher_Tick(sender, e);
        }

        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            IsCalculationCompleted = false;

            //set datapoints table to empty so user cannot edit anything whilst it's refreshing
            refreshDataPointsTable();
            this.RaisePropertyChanged(x => x.IsCalculationCompleted);
            base.FullRefresh();
        }
        #endregion

        #endregion

        #region View Properties
        private void refreshDataPointsTable()
        {
            alignedDataDateCollection = null;
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }
        
        protected override void onAfterRefresh()
        {
            refreshDataPointsTable();
            base.onAfterRefresh();
        }
        
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(BASELINE_ITEM))
            {
                BASELINE_ITEMProgress deliverable = Entities.FirstOrDefault(x => x.GUID.ToString() == key.ToString());
                if(deliverable != null)
                {
                    IEnumerable<PROGRESS_ITEM> progressItems = PROGRESS_ITEMCollection.Where(x => x.GUID_ORIBASEITEM == deliverable.OriginalEntityKey);
                    deliverable.SetProgressItems(progressItems.ToList());
                    List<StatsCalculationType> calcTypes = new List<StatsCalculationType>();
                    calcTypes.Add(StatsCalculationType.Earned);
                    deliverable.BuildStats(1, calcTypes);
                    BuildRowStats(deliverable, true);
                    deliverable.Update();
                    mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
                }
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        public bool CanProgressUndo()
        {
            if (!IsCalculationCompleted)
                return false;

            return PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.CanUndo() || MainViewModel.EntitiesUndoRedoManager.CanUndo();
        }

        public bool CanProgressRedo()
        {
            if (!IsCalculationCompleted)
                return false;

            return PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.CanRedo() || MainViewModel.EntitiesUndoRedoManager.CanRedo();
        }


        public void ProgressUndo()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Undo();

            //mainviewmodel must be undone last so that auxiliary message event can handle progress_item changes
            MainViewModel.EntitiesUndoRedoManager.Undo();
        }

        public void ProgressRedo()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Redo();

            //mainviewmodel must be redone last so that auxiliary message event can handle progress_item changes
            MainViewModel.EntitiesUndoRedoManager.Redo();
        }

        private void pauseUndoRedoAction()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
        }

        private void unPauseUndoRedoAction()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        private void clearUndoRedo()
        {
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Clear();
            MainViewModel.EntitiesUndoRedoManager.Clear();
        }

        private void reselectDeliverable()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.SelectedEntity)));
        }

        public override bool CanFillDown(object button)
        {
            if (!IsCalculationCompleted)
                return false;

            BarButtonItem barButtonItem = button as BarButtonItem;
            if(barButtonItem != null)
            {
                GridMenuInfo info = barButtonItem.DataContext as GridMenuInfo;
                if (info != null)
                    if (info.Column.FieldName.Contains(unboundProgressIdFieldname) && SelectedDataRows.Count > 1)
                        return true;
            }

            return false;
        }

        public override bool CanFillUp(object button)
        {
            return CanFillDown(button);
        }

        public override void FillDown(object button)
        {
            Fill(button, false);
        }

        public override void FillUp(object button)
        {
            Fill(button, true);
        }

        public void Fill(object button, bool isUp)
        {
            GridMenuInfo info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            Guid? valueToFill;

            DataRowView copyRow;
            if (isUp)
                copyRow = SelectedDataRows[SelectedDataRows.Count - 1];
            else
                copyRow = SelectedDataRows[0];

            BASELINE_ITEMProgress copyEntity = (BASELINE_ITEMProgress)(copyRow[columnEntity]);
            valueToFill = copyEntity.DeliverableStatusProgressGuid;
            clearUndoRedo();
            pauseUndoRedoAction();
            var bulkSaveEntities = new List<BASELINE_ITEMProgress>();

            for (int i = 0; i < SelectedDataRows.Count; i++)
            {
                BASELINE_ITEMProgress selectedEntity = (BASELINE_ITEMProgress)((SelectedDataRows[i])[columnEntity]);

                Guid? oldValue = selectedEntity.DeliverableStatusProgressGuid;
                string newValueString = string.Empty;
                if (valueToFill != null)
                {
                    DELIVERABLES_STATUS currentDELIVERABLE_STATUS = DELIVERABLES_STATUSCollection.FirstOrDefault(x => x.GUID == (Guid)valueToFill);
                    if (currentDELIVERABLE_STATUS != null)
                        newValueString = currentDELIVERABLE_STATUS.NAME;

                    selectedEntity.Entity.Entity.SetDeliverableStatusByName(newValueString);
                }
                else
                    selectedEntity.DeliverableStatusProgressGuid = null;

                onDeliverableStatusChange(selectedEntity, oldValue, selectedEntity.DeliverableStatusProgressGuid);
            }

            unPauseUndoRedoAction();
        }

        public void CustomUnboundColumnData(GridColumnDataEventArgs e)
        {
            if (Entities != null && Entities.Count > 0 && DataPointsTable != null)
            {
                if(DataPointsTable.Rows.Count > 0)
                {
                    if (e.Column.FieldName == unboundProgressIdFieldname)
                    {
                        BASELINE_ITEMProgress selectedEntity = (BASELINE_ITEMProgress)(DataPointsTable.Rows[e.ListSourceRowIndex])[columnEntity];
                        if (e.IsGetData)
                            e.Value = selectedEntity.DeliverableStatusProgressGuid;
                        else if (e.IsSetData)
                        {
                            if (e.Value == DBNull.Value)
                            {
                                selectedEntity.DeliverableStatusProgressGuid = null;
                            }
                        }
                    }
                }
            }
        }

        public void ClearDeliverableStatus()
        {
            foreach(var selectedDataRow in SelectedDataRows)
            {
                BASELINE_ITEMProgress entity = (BASELINE_ITEMProgress)(selectedDataRow[columnEntity]);
                entity.DeliverableStatusProgressGuid = null;
                MainViewModel.Save(entity);
            }
        }

        public override void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;

            var selected_cells = gridTableView.GetSelectedCells();
            if (selected_cells.Count == 0)
                return;

            string newValueString = Clipboard.GetText().ToString().Replace("%", "");
            List<string> newValueArr = newValueString.Split('\r').ToList();
            if (newValueString.Contains("\t") || newValueArr.Where(x => x == "\n").Count() > 1)
            {
                MessageBoxService.ShowMessage("Grid doesn't support pasting from multiple cells, sorry for the inconvenience");
                return;
            }

            newValueString = newValueArr[0];
            decimal newValueDecimal = 0;

            clearUndoRedo();
            pauseUndoRedoAction();
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            foreach (var selected_cell in selected_cells)
            {
                DataRowView editing_row = (DataRowView)gridControl.GetRow(selected_cell.RowHandle);
                BASELINE_ITEMProgress entity = (BASELINE_ITEMProgress)editing_row[columnEntity];
                if (DataUtils.FormatColumnFieldname(selected_cell.Column.FieldName) == unboundProgressIdFieldname)
                {
                    Guid? oldValue = entity.Entity.Entity.GUID_STATUS;
                    if (newValueString == string.Empty)
                    {
                        entity.Entity.Entity.GUID_STATUS = null;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, selected_cell.Column.FieldName, oldValue, null, EntityMessageType.Changed);
                        MainViewModel.Save(entity);
                    }
                    else if (entity.Entity.Entity.SetDeliverableStatusByName(newValueString))
                    {
                        Guid? newValue = entity.DeliverableStatusProgressGuid;
                        onDeliverableStatusChange(entity, oldValue, newValue);
                        //do this so that deliverable goes through the projection refresh
                        //Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(entity.GUID, MainViewModel.Key, EntityMessageType.Changed, MainViewModel, MainViewModel.CurrentHWID));
                    }
                }
                else if (decimal.TryParse(newValueString, out newValueDecimal))
                {
                    decimal oldValue = (decimal)editing_row[selected_cell.Column.FieldName];
                    if (newValueDecimal > 1)
                        newValueDecimal *= 0.01m;

                    ErrorMessage errorMessage;
                    updatePercentage(entity, selected_cell.Column.FieldName, oldValue, newValueDecimal, out errorMessage);
                    if(errorMessage != null)
                        errorMessages.Add(errorMessage);
                }
            }
            unPauseUndoRedoAction();

            showErrorMessages(errorMessages);
            e.Handled = true;
        }

        public void FixProgressItem()
        {
            if (MessageBoxService.ShowMessage("Fixing data will cause some progress to be changed, it is recommended to copy and paste a currency copy of progress to excel before continuing, do you wish to continue?", "Warning", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            //update progress items so that it is accurate at run time
            IBluePrintsEntitiesUnitOfWork uow = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            IQueryable<PROGRESS_ITEM> allProgress = uow.PROGRESS_ITEMS.Where(x => x.PROGRESS.GUID == loadPROGRESS.GUID);
            List<PROGRESS_ITEM> progressItemToDelete = new List<PROGRESS_ITEM>();
            int fixedCount = 0;
            foreach (PROGRESS_ITEM progress_item in allProgress)
            {
                List<PROGRESS_ITEM> deliverablePROGRESS = allProgress.Where(x => x.GUID_ORIBASEITEM == progress_item.GUID_ORIBASEITEM).ToList();
                List<PROGRESS_ITEM> progressOnSameDate = deliverablePROGRESS.Where(x => x.EARNED_DATE.Date == progress_item.EARNED_DATE.Date).ToList();
                if (progressOnSameDate.Count > 1)
                {
                    bool firstSelected = false;
                    bool deleteAll = progressOnSameDate.Sum(x => x.EARNED_UNITS) == 0;
                    foreach (PROGRESS_ITEM progressItem in progressOnSameDate)
                    {
                        if (deleteAll)
                        {
                            progressItemToDelete.Add(progressItem);

                        }
                        else if (firstSelected)
                            progressItemToDelete.Add(progressItem);
                        else
                        {
                            progressItem.EARNED_UNITS = progressOnSameDate.Sum(x => x.EARNED_UNITS);
                            firstSelected = true;
                        }

                        fixedCount += 1;
                    }
                }

                if (progress_item.EARNED_DATE.Hour == 0)
                {
                    progress_item.EARNED_DATE = progress_item.EARNED_DATE.AddDays(1).AddSeconds(-1);
                }
            }

            foreach (PROGRESS_ITEM progressItemDelete in progressItemToDelete)
            {
                uow.PROGRESS_ITEMS.Remove(progressItemDelete);
            }

            uow.SaveChanges();
            FullRefresh();
            MessageBoxService.ShowMessage(fixedCount + " progress fixed");
        }

        private void onDeliverableStatusChange(BASELINE_ITEMProgress entity, Guid? oldDeliverableStatusGuid, Guid? newDeliverableStatusGuid)
        {
            entity.DeliverableStatusProgressGuid = newDeliverableStatusGuid;
            MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DeliverableStatusProgressGuid), oldDeliverableStatusGuid, newDeliverableStatusGuid, EntityMessageType.Changed);

            DELIVERABLES_STATUS currentDELIVERABLE_STATUS = entity.Entity.Entity.DeliverableStatusCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_STATUS);
            if (currentDELIVERABLE_STATUS != null && currentDELIVERABLE_STATUS.AUTO_PERCENTAGE != null)
            {
                decimal oldTotalPercentage = entity.Total_Percentage;
                decimal auto_percentage = (decimal)currentDELIVERABLE_STATUS.AUTO_PERCENTAGE;
                if (auto_percentage > entity.Total_Percentage)
                {
                    ErrorMessage errorMessage;
                    //entity will be saved within this method
                    updatePercentage(entity, loadPROGRESS.DATA_DATE.ToShortDateString(), oldTotalPercentage, auto_percentage, out errorMessage);
                    showErrorMessage(errorMessage);
                }
                else
                    MainViewModel.Save(entity);
            }
            else //save deliverables status only
                MainViewModel.Save(entity);
        }

        private void showErrorMessage(ErrorMessage errorMessage)
        {
            if (errorMessage == null)
                return;

            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            errorMessages.Add(errorMessage);
            showErrorMessages(errorMessages);
        }

        private void showErrorMessages(IEnumerable<ErrorMessage> errorMessages)
        {
            if (errorMessages.Count() > 0)
            {
                DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "Cannot create assignments due to the following error");
                ErrorMessagesDialogService.ShowDialog(MessageButton.OK, string.Empty, "ListErrorMessages", viewModel);
            }
        }

        private void updatePercentage(BASELINE_ITEMProgress entity, string fieldName, object oldValue, object newValue, out ErrorMessage errorMessage)
        {
            if (entity.Total_Units == 0)
            {
                errorMessage = new ErrorMessage(entity.Deliverable_Name, "Deliverable doesn't have any units to progress");
                return;
            }

            errorMessage = null;
            DateTime columnDate;
            if (DateTime.TryParse(fieldName, out columnDate))
            {
                string earnedUnitsFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEM().EARNED_UNITS);
                DateTime currentProgressDate = columnDate.AddDays(1).AddSeconds(-1);

                List<PROGRESS_ITEM> progressToSave = new List<PROGRESS_ITEM>();

                decimal currentProgressMaximumUnits = getDeliverableProgressMaximumUnits(entity, currentProgressDate);
                decimal oldPercentage = (decimal)oldValue;
                decimal newPercentage = (decimal)newValue;
                decimal percentageDifference = newPercentage - oldPercentage;

                if (percentageDifference != 0)
                {
                    decimal totalUnitsDifferences = percentageDifference * currentProgressMaximumUnits;
                    decimal maximumEarnUnits = currentProgressMaximumUnits;

                    IEnumerable<PROGRESS_ITEM> unalignedDataDatePROGRESS_ITEMS = entity.PROGRESS_ITEMS.Where(x => !alignedDataDateCollection.Any(y => y.Date == x.EARNED_DATE.Date));
                    foreach(PROGRESS_ITEM unalignedPROGRESS_ITEM in unalignedDataDatePROGRESS_ITEMS)
                    {
                        if(alignedDataDateCollection.Count > 0)
                        {
                            DateTime? alignDateTime = alignedDataDateCollection.OrderByDescending(x => x).FirstOrDefault(x => unalignedPROGRESS_ITEM.EARNED_DATE > x.Date);
                            if (alignDateTime != null)
                                unalignedPROGRESS_ITEM.EARNED_DATE = (DateTime)alignDateTime;

                            progressToSave.Add(unalignedPROGRESS_ITEM);
                        }
                    }

                    IEnumerable<PROGRESS_ITEM> previousProgresses = entity.PROGRESS_ITEMS.Where(x => x.EARNED_DATE < currentProgressDate).OrderByDescending(x => x.EARNED_DATE);
                    PROGRESS_ITEM currentPeriodPROGRESS_ITEM = entity.PROGRESS_ITEMS.FirstOrDefault(x => x.EARNED_DATE.Date == currentProgressDate.Date);
                    List<PROGRESS_ITEM> futureProgressToEdit = entity.PROGRESS_ITEMS.Where(x => x.EARNED_DATE > currentProgressDate).OrderBy(x => x.EARNED_DATE).ToList();
                    
                    //maximum and minimum is controlled here by the spinedit ability to set max as 100% and min as 0%, and that includes variation validatation, so there is no need to validate here
                    if (currentPeriodPROGRESS_ITEM == null && totalUnitsDifferences > 0)
                    {
                        PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                        newPROGRESS_ITEM.GUID_ORIBASEITEM = entity.OriginalEntityKey;
                        newPROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
                        newPROGRESS_ITEM.EARNED_DATE = currentProgressDate;
                        newPROGRESS_ITEM.EARNED_UNITS = totalUnitsDifferences;
                        newPROGRESS_ITEM.CREATED = DateTime.Now;
                        newPROGRESS_ITEM.CREATEDBY = LoginCredentials.CurrentUserGuid;
                        PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(newPROGRESS_ITEM, null, null, null, EntityMessageType.Added);
                        progressToSave.Add(newPROGRESS_ITEM);
                    }
                    else if (currentPeriodPROGRESS_ITEM != null)
                    {
                        decimal postEditUnits = currentPeriodPROGRESS_ITEM.EARNED_UNITS + totalUnitsDifferences;
                        if (postEditUnits < 0)
                        {
                            totalUnitsDifferences = -1 * currentPeriodPROGRESS_ITEM.EARNED_UNITS;
                            postEditUnits = 0;
                            errorMessage = new ErrorMessage(entity.Deliverable_Name, "Cannot go below currently assigned units, hence current % is set to lowest possible %. Please check past progress to reduce % further");
                        }
                        else
                        {
                            decimal oldProgressValue = currentPeriodPROGRESS_ITEM.EARNED_UNITS;
                            currentPeriodPROGRESS_ITEM.EARNED_UNITS = postEditUnits;
                            //use this to fix time issue
                            currentPeriodPROGRESS_ITEM.EARNED_DATE = currentProgressDate;
                            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, earnedUnitsFieldName, oldProgressValue, postEditUnits, EntityMessageType.Changed);
                            progressToSave.Add(currentPeriodPROGRESS_ITEM);
                        }
                    }
                    else
                    {
                        decimal oldValueDecimal;
                        if(oldValue != null && Decimal.TryParse(oldValue.ToString(), out oldValueDecimal))
                        {
                            errorMessage = new ErrorMessage(entity.Deliverable_Name, "There is no datapoint to edit on this date, if you wish to reduce it please do so on the first instance of " + string.Format(oldValueDecimal.ToString("P")));
                            Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(entity.GUID, MainViewModel.Key, EntityMessageType.Changed, PROGRESS_ITEMSCollectionViewModel));
                        }

                        return;
                    }


                    //The addition of removal of units from current data date needs to be balanced by progress in the future, starting from the next progress
                    totalUnitsDifferences = totalUnitsDifferences * -1;
                    for (int i = 0; i < futureProgressToEdit.Count; i++)
                    {
                        if (totalUnitsDifferences == 0)
                            break;

                        PROGRESS_ITEM progress = futureProgressToEdit[i];
                        if (progress.EARNED_UNITS == 0 && totalUnitsDifferences < 0)
                            continue;

                        decimal oldProgressValue = progress.EARNED_UNITS;
                        decimal postEditEarnUnits = progress.EARNED_UNITS + totalUnitsDifferences;
                        decimal remainingUnitsToIncrease = currentProgressMaximumUnits - (entity.PROGRESS_ITEMS.Sum(x => x.EARNED_UNITS) - progress.EARNED_UNITS);
                        if (postEditEarnUnits < 0)
                        {
                            postEditEarnUnits = 0;
                            totalUnitsDifferences -= progress.EARNED_UNITS;
                        }
                        else if (postEditEarnUnits > remainingUnitsToIncrease)
                        {
                            postEditEarnUnits = remainingUnitsToIncrease;
                            totalUnitsDifferences -= remainingUnitsToIncrease;
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

                PROGRESS_ITEMSCollectionViewModel.BaseBulkSave(progressToSave);
                //add a dummy undo so that during undo/redo operation a baseline item message will be sent
                MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_ORIGINAL), entity.GUID_ORIGINAL, entity.GUID_ORIGINAL, EntityMessageType.Changed);
            }

            //save baseline_item here so that auxiliary message can respond to progress item changes
            MainViewModel.Save(entity);
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public virtual void FixedCellValueChanging(DevExpress.Xpf.Grid.CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            if (!e.Handled)
            {
                if(e.Column.FieldName.Contains(unboundProgressIdFieldname))
                {
                    DataRowView dataRowView = (DataRowView)e.Row;
                    BASELINE_ITEMProgress entity = (BASELINE_ITEMProgress)dataRowView.Row[columnEntity];
                    entity.DeliverableStatusProgressGuid = (Guid?)e.Value;
                    clearUndoRedo();
                    pauseUndoRedoAction();
                    onDeliverableStatusChange(entity, (Guid?)e.OldValue, (Guid?)e.Value);
                    unPauseUndoRedoAction();
                }
            }
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedProgressUpdate(DevExpress.Xpf.Grid.CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            DataRowView dataRowView = (DataRowView)e.Row;
            BASELINE_ITEMProgress entity = (BASELINE_ITEMProgress)dataRowView.Row[columnEntity];

            if(!e.Column.FieldName.ToUpper().Contains("ENTITY") && e.Column.FieldName != unboundProgressIdFieldname)
            {
                //only clear undo redo before update percentage here because this is the only event called from grid
                clearUndoRedo();
                pauseUndoRedoAction();
                ErrorMessage errorMessage;
                updatePercentage(entity, e.Column.FieldName, e.OldValue, e.Value, out errorMessage);
                showErrorMessage(errorMessage);
                unPauseUndoRedoAction();
            }

            e.Handled = true;
        }

        private decimal getDeliverableProgressMaximumUnits(BASELINE_ITEMProgress deliverable, DateTime progressDate)
        {
            //if (deliverable.Stats.Budgeted.CurrentPeriodDataPoint != null && deliverable.Stats.Budgeted.CurrentPeriodDataPoint.BudgetedUnits != 0)
            //    return deliverable.Stats.Budgeted.CurrentPeriodDataPoint.BudgetedUnits;
            //else if (deliverable.Stats.Budgeted.CurrentPeriodDataPoint != null && deliverable.Stats.Budgeted.CumulativeDataPoints.Count > 0)
            //    return deliverable.Stats.Budgeted.CumulativeDataPoints.Last().BudgetedUnits;
            //else
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

        DataRowView selectedDataRow { get; set; }
        public DataRowView SelectedDataRow
        {
            get
            {
                return selectedDataRow;
            }
            set
            {
                if (dataPointsTable == null || value == null)
                    return;

                int rowIndex = dataPointsTable.Rows.IndexOf(value.Row);
                if (rowIndex == -1)
                    return;

                DataRowView dataRowView = dataPointsTable.DefaultView[rowIndex];
                selectedDataRow = dataRowView;
            }
        }

        ObservableCollection<DataRowView> selectedDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedDataRows
        {
            get
            {
                if (selectedDataRows == null)
                    selectedDataRows = new ObservableCollection<DataRowView>();

                return selectedDataRows;
            }
            set
            {
                selectedDataRows = value;
            }
        }

        protected ObservableCollection<ColumnDescriptor> columnDescriptors;
        public ObservableCollection<ColumnDescriptor> ColumnDescriptors
        {
            get
            {
                if (columnDescriptors == null)
                {
                    columnDescriptors = new ObservableCollection<ColumnDescriptor>();
                }
                return columnDescriptors;
            }
        }

        protected ObservableCollection<SummaryDescriptor> summaryDescriptors;
        public ObservableCollection<SummaryDescriptor> SummaryDescriptors
        {
            get
            {
                if (summaryDescriptors == null)
                {
                    summaryDescriptors = new ObservableCollection<SummaryDescriptor>();
                }
                return summaryDescriptors;
            }
        }

        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.AREA.INTERNAL_NUM", ReadOnly = true, Header = "Area", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Entity.Entity.AREA.INTERNAL_NUM", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.DOCTYPE.NAME", ReadOnly = true, Header = "Document Type", Fixed = FixedStyle.Left, Width = 90, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.DISCIPLINE.NAME", ReadOnly = true, Header = "Discipline", Fixed = FixedStyle.Left, Width = 90, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.DEPARTMENT.NAME", ReadOnly = true, Header = "Department", Fixed = FixedStyle.Left, Width = 90, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.INTERNAL_NUM", ReadOnly = true, Header = "Internal Number", Fixed = FixedStyle.Left, Width = 140, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.CLIENT_NUM", ReadOnly = true, Header = "Client Number", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.PRIMARY_TITLE", ReadOnly = true, Header = "Primary Title", Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Budget_Units", Mask = "###,##0 hrs", ReadOnly = true, Header = "Budget Hours", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Budget_Units", DisplayFormat = "###,##0 hrs", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Variation_Units", Mask = "###,##0 hrs", ReadOnly = true, Header = "Variation Hours", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Variation_Units", DisplayFormat = "###,##0 hrs", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Total_Units", Mask = "###,##0 hrs", ReadOnly = true, Header = "Total Hours", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Total_Units", DisplayFormat = "###,##0 hrs", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Earned_Units_ToDate", Mask= "###,##0 hrs", ReadOnly = true, Header = "Total Earn", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Earned_Units_ToDate", DisplayFormat = "###,##0 hrs", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Earned_Units_OnDataDate", Mask = "###,##0 hrs", ReadOnly = true, Header = "Period Earn", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Earned_Units_OnDataDate", DisplayFormat = "###,##0 hrs", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = unboundProgressIdFieldname, ReadOnly = false, DataContext = this, Header = "Gates", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Gates, HeaderToolTip = "Changing this value will increase current % only if the gate has a higher percentage than previously earned %" });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.SUBJOB.INTERNAL_NAME1", ReadOnly = true, Header = "Sub Job", Fixed = FixedStyle.Left, Width = 110, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.DELIVERABLE_TYPE", ReadOnly = true, Header = "Deliverable Type", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.SECONDARY_TITLE", ReadOnly = true, Visible = false, Header = "Comments", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Entity.Entity.COMMENTS", ReadOnly = true, Visible = false, Header = "Comments", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Total_Earned_Percentage", Mask="p2", ReadOnly = true, Header = "Total Earned %", Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToShortDateString();
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 100, Settings = SettingsType.Percent });
            }
        }

        DataTable dataPointsTable = null;
        List<DateTime> alignedDataDateCollection = null;
        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || Entities == null)
                    return null;

                if (!IsCalculationCompleted)
                    return null;

                if(dataPointsTable == null)
                {
                    GridControlService.BeginDataUpdate();
                    dataPointsTable = new DataTable();
                    TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
                    DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);

                    DateTime? lastEarnedDate = null;
                    if(Entities.Where(x => x.LastDataDate != null).Count() > 0)
                        lastEarnedDate = Entities.Where(x => x.LastDataDate != null).Max(x => x.LastDataDate);

                    DateTime lastDataDate = lastEarnedDate == null ? loadPROGRESS.DATA_DATE : ((DateTime)lastEarnedDate > loadPROGRESS.DATA_DATE) ? (DateTime)lastEarnedDate : loadPROGRESS.DATA_DATE;
                    if(alignedDataDateCollection == null)
                    {
                        alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);
                        InitializeColumnSource(ColumnDescriptors, SummaryDescriptors, alignedDataDateCollection);
                    }

                    dataPointsTable.Columns.Add(columnEntity, typeof(BASELINE_ITEMProgress));
                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        //ColorScaleFormatCondition colorScaleFormatCondition = new ColorScaleFormatCondition();
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        //colorScaleFormatCondition.FieldName = columnFieldName;
                        //colorScaleFormatCondition.Format = new ColorScaleFormat() { ColorMin = Colors.LightSalmon, ColorMiddle = Colors.LemonChiffon, ColorMax = Colors.Lime };
                        //colorScaleFormatCondition.MinValue = 0;
                        //colorScaleFormatCondition.MaxValue = 1;
                        //TableViewService.AddFormatCondition(colorScaleFormatCondition);

                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach(BASELINE_ITEMProgress entity in Entities)
                    {
                        BuildRowStats(entity, false);
                    }

                    focusLastColumnTimer.Start();
                    GridControlService.EndDataUpdate();
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
                            where ((BASELINE_ITEMProgress)dr[columnEntity]).GUID == entity.GUID
                            select dr).FirstOrDefault();
            }

            if (newDataRow == null)
                return;

            newDataRow[columnEntity] = entity;

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

            if (!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }

        public bool CanUpdateAllPercentagesByStatus()
        {
            if (!IsCalculationCompleted)
                return false;

            return LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_UpdateProgressByStatus)) != LoginCredentials.PermissionStatus.None;
        }

        public void UpdateAllPercentagesByStatus()
        {
            if (MessageBoxService.ShowMessage("Warning\nThis action will update or delete progresses based on deliverable status and is not reversible\nDo you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            MainViewModel.AlwaysSkipMessage = true;
            BluePrintsDataUtils.UpdatePercentagesByStatus(MessageBoxService, PROGRESS_ITEMSCollectionViewModel, MainViewModel.Entities);
            MainViewModel.AlwaysSkipMessage = false;

            //need to perform full refresh to reload stats due to support for % reduction by gates
            FullRefresh();
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "OffsiteDirectProgressDistributionViewModelWrapper_v5"; }
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

        protected override bool manuallySaveProgressOnAfterBaselineItemSaved => true;

        protected override void OnClose(CancelEventArgs e)
        {
            GlobalMethods.SetAccordionExpandedState?.Invoke(true);
            base.OnClose(e);
        }
        #endregion

        #region Reporting
        public bool CanEditReport()
        {
            return IsCalculationCompleted;
        }

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
            return IsCalculationCompleted;
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

            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            List<VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONCollection.AsQueryable(), ReportableCollection);

            DateTime reporting_data_date = loadPROGRESS.DATA_DATE;
            TimeSpan reporting_interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
            DeliverableSummaryStats projectSummary = new DeliverableSummaryStats(MainViewModel.Entities, reporting_data_date, reporting_interval, first_aligned_data_date, projectVariationAdjustment);
            FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(loadPROJECT.NUMBER, loadPROJECT.CURRENCYCONVERSION, reporting_interval, first_aligned_data_date, SUBJOBCollection, reporting_data_date, primeroUnitOfWork);
            fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, loadPROJECT.NUMBER);
            fullSummarizer.BuildBurnedDataPoints(false, false);
            fullSummarizer.Build();

            progressReport.AssignProperties(projectSummary, loadPROGRESS.DATA_DATE, loadPROGRESS.PROJECT.NAME);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = progressReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            progressReport.RequestParameters = false;
            progressReport.CreateDocument(true);
            previewWindow.Show();

            LoadingScreenManager.CloseLoadingScreen();
        }
        #endregion
    }
}