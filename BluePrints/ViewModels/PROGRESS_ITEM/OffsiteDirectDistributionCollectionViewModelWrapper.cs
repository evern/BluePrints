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
    public partial class OffsiteDirectDistributionCollectionViewModelWrapper :
        BluePrintsEntitiesProgressCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<BASELINE_ITEMProgress>
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

        protected override void resolveParameters(object parameter)
        {
            defaultColumnFieldNames.Add(columnEntity);

            systemColumnFieldNames.Add(columnEntity);
            systemColumnFieldNames.Add(columnPrimaryTitle);
            systemColumnFieldNames.Add(columnDeliverableStatus);

            is_load_p6_task = true;
            bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            IsCalculating = true;
            this.RaisePropertyChanged(x => x.IsCalculating);
            extrapolateDataDate = true;
            GlobalMethods.SetAccordionExpandedState?.Invoke(false);

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

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID), loadPROJECT, loadPROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, DeliverableInternalNumberMode.Default, false, P6TASKCollection, null, null, true, null, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection);
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
            PROGRESS_ITEMSCollectionViewModel.AlwaysSkipMessage = false;
            PROGRESS_ITEMSCollectionViewModel.RefreshOnSameSenderKey = true;
            doNotApplyBestFit = true;
        }

        private void applyBestFit()
        {
            if (TableViewService != null && !isBestFitApplied)
            {
                if (DisplayEntities != null && DisplayEntities.Count > 0)
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

        public override void FullRefresh()
         {
            IsCalculating = true;
            this.RaisePropertyChanged(x => x.IsCalculating);
            base.FullRefresh();
            refreshDataPointsTable();
        }
        #endregion

        #endregion

        #region View Properties
        private void refreshDataPointsTable()
        {
            alignedDataDateCollection = null;
            dataPointsTable = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
        }

        private DataColumn lastColumn;
        public DataColumn LastColumn
        {
            get
            {
                return lastColumn;
            }
        }

        public bool IsCalculating { get; set; }
        //because stats aren't fully built yet in progress base class on CalculatePlannedBackgroundWorker, do not calculate dataPointsTable
        bool canCalculateDataPointsTable;
        protected override void onAfterRefresh()
        {
            base.onAfterRefresh();
            canCalculateDataPointsTable = true;
            refreshDataPointsTable();

            if(isCalculationCompleted)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => calculated()));
                //mainThreadDispatcher.BeginInvoke(new Action(() => applyBestFit()));
            }
        }

        private void calculated()
        {
            IsCalculating = false;
            this.RaisePropertyChanged(x => x.IsCalculating);
        }

        bool isUndoRedoOperation;
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            BASELINE_ITEMProgress deliverable = null;
            //only recalculate on progress item when undo/redo operation is being performed, because normal operation will be recalculated on Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>
            if (isUndoRedoOperation && changedType == typeof(PROGRESS_ITEM))
                deliverable = DisplayEntities.FirstOrDefault(x => x.PROGRESS_ITEMS.Any(y => y.GUID == (Guid)key));
            else if (changedType == typeof(BASELINE_ITEM))
                deliverable = DisplayEntities.FirstOrDefault(x => x.GUID == (Guid)key);

            if (deliverable != null)
            {
                List<StatsCalculationType> calcTypes = new List<StatsCalculationType>();
                calcTypes.Add(StatsCalculationType.Earned);
                deliverable.BuildStats(1, calcTypes);
                BuildRowStats(deliverable, true);
                mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DataPointsTable)));
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!defaultColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                //SpinEditSettings spinEdit = new SpinEditSettings();
                //spinEdit.MaskType = MaskType.Numeric;
                //spinEdit.Mask = "p2";
                //spinEdit.MaskUseAsDisplayFormat = true;
                //spinEdit.Increment = 0.01m;
                //spinEdit.MaxValue = 1;
                //spinEdit.MinValue = 0;
                //e.Column.EditSettings = spinEdit;
                e.Column.Width = 90;
                if(e.Column.FieldName.Contains(loadPROGRESS.DATA_DATE.Date.ToString()))
                    e.Column.CellTemplate = Application.Current.Resources["percentageTemplateLastDate"] as DataTemplate;
                else
                    e.Column.CellTemplate = Application.Current.Resources["percentageTemplate"] as DataTemplate;
            }
            else
            {
                if (hiddenColumnFieldNames.Any(x => x == e.Column.FieldName) || systemColumnFieldNames.Any(x => x == e.Column.FieldName))
                {
                    e.Column.Visible = false;
                }

                if(!systemColumnFieldNames.Any(x => x == e.Column.FieldName))
                {
                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;

                    if (e.Column.FieldName.ToUpper().Contains("PERCENT"))
                    {
                        SpinEditSettings spinEdit = new SpinEditSettings();
                        spinEdit.MaskType = MaskType.Numeric;
                        spinEdit.Mask = "p";
                        spinEdit.MaskUseAsDisplayFormat = true;
                        e.Column.EditSettings = spinEdit;
                    }
                    else if (e.Column.FieldName.ToUpper().Contains("COST"))
                    {
                        SpinEditSettings spinEdit = new SpinEditSettings();
                        spinEdit.MaskType = MaskType.Numeric;
                        spinEdit.Mask = "c";
                        spinEdit.MaskUseAsDisplayFormat = true;
                        e.Column.EditSettings = spinEdit;
                    }
                    else if (e.Column.FieldName.ToUpper().Contains("UNIT"))
                    {
                        SpinEditSettings spinEdit = new SpinEditSettings();
                        spinEdit.MaskType = MaskType.Numeric;
                        spinEdit.Mask = "n";
                        spinEdit.MaskUseAsDisplayFormat = true;
                        e.Column.EditSettings = spinEdit;
                    }

                    e.Column.ReadOnly = true;
                }

                e.Column.Fixed = FixedStyle.Left;
            }
        }

        public bool CanProgressUndo()
        {
            if (PROGRESS_ITEMSCollectionViewModel == null || PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager == null)
                return false;

            return PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.CanUndo();
        }

        public bool CanProgressRedo()
        {
            if (PROGRESS_ITEMSCollectionViewModel == null || PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager == null)
                return false;

            return PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.CanRedo();
        }


        public void ProgressUndo()
        {
            isUndoRedoOperation = true;
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Undo();
            isUndoRedoOperation = false;
        }

        public void ProgressRedo()
        {
            isUndoRedoOperation = true;
            PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.Redo();
            isUndoRedoOperation = false;
        }

        private void reselectDeliverable()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplaySelectedEntity)));
        }

        public bool CanFillDown(object button)
        {
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            return !IsLoading && info != null && info.Column != null && !info.Column.ReadOnly && info.Column.FieldName.Contains(unboundProgressIdFieldname) && SelectedDataRows.Count > 1;
        }

        public bool CanFillUp(object button)
        {
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            return !IsLoading && info != null && info.Column != null && !info.Column.ReadOnly && info.Column.FieldName.Contains(unboundProgressIdFieldname) && SelectedDataRows.Count > 1;
        }

        public void FillDown(object button)
        {
            Fill(button, false);
        }

        public void FillUp(object button)
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
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
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

                updateAutoPercentage(selectedEntity);
                MainViewModel.EntitiesUndoRedoManager.AddUndo(selectedEntity, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DeliverableStatusProgressGuid), oldValue, selectedEntity.DeliverableStatusProgressGuid, EntityMessageType.Changed);
                MainViewModel.Save(selectedEntity);
            }

            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        public void CustomUnboundColumnData(GridColumnDataEventArgs e)
        {
            if (DisplayEntities != null && DisplayEntities.Count > 0)
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
                                MainViewModel.EntitiesUndoRedoManager.AddUndo(selectedEntity, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DeliverableStatusProgressGuid), selectedEntity.DeliverableStatusProgressGuid, null, EntityMessageType.Changed);
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

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
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
                        Guid? newValue = entity.Entity.Entity.GUID_STATUS;
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, selected_cell.Column.FieldName, oldValue, newValue, EntityMessageType.Changed);

                        updateAutoPercentage(entity);
                        MainViewModel.Save(entity);
                        //do this so that deliverable goes through the projection refresh
                        //Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(entity.GUID, MainViewModel.Key, EntityMessageType.Changed, MainViewModel, MainViewModel.CurrentHWID));
                    }
                }
                else if (decimal.TryParse(newValueString, out newValueDecimal))
                {
                    decimal oldValue = (decimal)editing_row[selected_cell.Column.FieldName];
                    if (newValueDecimal > 1)
                        newValueDecimal *= 0.01m;

                    updatePercentage(entity, selected_cell.Column.FieldName, oldValue, newValueDecimal);
                }
            }

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

        private void updateAutoPercentage(BASELINE_ITEMProgress entity)
        {
            DELIVERABLES_STATUS currentDELIVERABLE_STATUS = entity.Entity.Entity.DeliverableStatusCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_STATUS);
            if (currentDELIVERABLE_STATUS != null && currentDELIVERABLE_STATUS.AUTO_PERCENTAGE != null)
            {
                decimal oldTotalPercentage = entity.Total_Percentage;
                decimal auto_percentage = (decimal)currentDELIVERABLE_STATUS.AUTO_PERCENTAGE;
                if (auto_percentage > entity.Total_Percentage)
                {
                    entity.Total_Earned_Percentage = auto_percentage;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage), oldTotalPercentage, auto_percentage, EntityMessageType.Changed);
                }
            }
        }

        private void updatePercentage(BASELINE_ITEMProgress entity, string fieldName, object oldValue, object newValue)
        {
            DateTime columnDate;
            if (DateTime.TryParse(fieldName, out columnDate))
            {
                string earnedUnitsFieldName = BindableBase.GetPropertyName(() => new PROGRESS_ITEM().EARNED_UNITS);

                PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.PauseActionId();
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
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

                    //update progress items so that it is accurate at run time
                    IBluePrintsEntitiesUnitOfWork uow = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
                    //refresh from database routine
                    List<PROGRESS_ITEM> dbPROGRESSES = uow.PROGRESS_ITEMS.Where(x => x.PROGRESS.GUID == loadPROGRESS.GUID && x.GUID_ORIBASEITEM == entity.OriginalEntityKey).OrderBy(x => x.EARNED_DATE).ToList();
                    IEnumerable<PROGRESS_ITEM> unalignedDataDatePROGRESS_ITEMS = dbPROGRESSES.Where(x => !alignedDataDateCollection.Any(y => y.Date == x.EARNED_DATE.Date));
                    foreach(PROGRESS_ITEM unalignedPROGRESS_ITEM in unalignedDataDatePROGRESS_ITEMS)
                    {
                        unalignedPROGRESS_ITEM.EARNED_UNITS = 0;
                    }

                    uow.SaveChanges();
                    entity.SetProgressItems(dbPROGRESSES);

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
                            MessageBoxService.ShowMessage("Cannot go below currently assigned units, hence current % is set to lowest possible %. Please check past progress to reduce % further");
                        }

                        decimal oldProgressValue = currentPeriodPROGRESS_ITEM.EARNED_UNITS;
                        currentPeriodPROGRESS_ITEM.EARNED_UNITS = postEditUnits;
                        //use this to fix time issue
                        currentPeriodPROGRESS_ITEM.EARNED_DATE = currentProgressDate;
                        PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(currentPeriodPROGRESS_ITEM, earnedUnitsFieldName, oldProgressValue, postEditUnits, EntityMessageType.Changed);
                        progressToSave.Add(currentPeriodPROGRESS_ITEM);
                    }
                    else
                    {
                        MessageBoxService.ShowMessage("There is no datapoint to edit on this date, if you wish to reduce it please do so on the last highest % datapoint");
                        Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(entity.GUID, MainViewModel.Key, EntityMessageType.Changed, PROGRESS_ITEMSCollectionViewModel));
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

                foreach (PROGRESS_ITEM progress in progressToSave)
                {
                    PROGRESS_ITEMSCollectionViewModel.Save(progress);
                }

                //do this so that deliverable goes through the projection refresh
                Messenger.Default.Send(new EntityMessage<BASELINE_ITEM, Guid>(entity.GUID, MainViewModel.Key, EntityMessageType.Changed, MainViewModel, MainViewModel.CurrentHWID));
                PROGRESS_ITEMSCollectionViewModel.EntitiesUndoRedoManager.UnpauseActionId();
                //will be unpaused in existingrow or newrow save
            }
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
                    if (entity.DeliverableStatusProgressGuid != null)
                        updateAutoPercentage(entity);
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

            if(e.Column.FieldName.ToUpper().Contains("ENTITY") || e.Column.FieldName == unboundProgressIdFieldname)
            {
                //entity.Entity.Entity.PRIMARY_TITLE = e.Value.ToString();
                ///MainViewModel.EntitiesUndoRedoManager.AddUndo(entity, columnPrimaryTitle, e.OldValue, e.Value, EntityMessageType.Changed);
                MainViewModel.Save(entity);
            }
            else
            {
                updatePercentage(entity, e.Column.FieldName, e.OldValue, e.Value);
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

        DataTable dataPointsTable = null;
        List<DateTime> alignedDataDateCollection = null;
        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || DisplayEntities == null)
                    return null;

                if (!canCalculateDataPointsTable)
                    return null;

                if(dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
                    DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(loadPROGRESS);
                    DateTime lastDataDate = loadPROGRESS.DATA_DATE;
                    if(alignedDataDateCollection == null)
                        alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);

                    dataPointsTable.Columns.Add(columnEntity, typeof(BASELINE_ITEMProgress));

                    //bool conditionalFormattingAdded = false;
                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        ColorScaleFormatCondition colorScaleFormatCondition = new ColorScaleFormatCondition();
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        //if (!conditionalFormattingAdded)
                        //{
                            colorScaleFormatCondition.FieldName = columnFieldName;
                            colorScaleFormatCondition.Format = new ColorScaleFormat() { ColorMin = Colors.LightSalmon, ColorMiddle = Colors.LemonChiffon, ColorMax = Colors.Lime };
                            colorScaleFormatCondition.MinValue = 0;
                            colorScaleFormatCondition.MaxValue = 1;
                            TableViewService.AddFormatCondition(colorScaleFormatCondition);
                        //    conditionalFormattingAdded = true;
                        //}

                        if(alignedDataDate == loadPROGRESS.DATA_DATE)
                        {
                            lastColumn = new DataColumn();
                            lastColumn.ColumnName = columnFieldName;
                            lastColumn.DataType = typeof(decimal);
                            dataPointsTable.Columns.Add(lastColumn);
                        }
                        else
                            dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

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
            return LoginCredentials.hasPermission(PermissionResources.ProgressUpdatePercentageByStatus);
        }

        public void UpdateAllPercentagesByStatus()
        {
            BluePrintsDataUtils.UpdateAllPercentagesByStatus(MessageBoxService, PROGRESS_ITEMSCollectionViewModel, MainViewModel.Entities);
            FullRefresh();
        }
        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Please check whether data date is correct before updating", null, null, null);
            notification1.ShowAsync();
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
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

        protected override void OnClose(CancelEventArgs e)
        {
            GlobalMethods.SetAccordionExpandedState?.Invoke(true);
            base.OnClose(e);
        }
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