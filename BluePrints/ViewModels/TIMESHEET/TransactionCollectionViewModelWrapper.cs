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
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
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
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class TransactionCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TransactionCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new TransactionCollectionViewModelWrapper());
        }

        #region Database Operation
        private Data.PROJECT loadPROJECT;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        public ObservableCollection<string> VARIATIONCODECollection { get; set; }
        string valueNotFoundError = "Value not found";
        public string ColumnEntity = "Entity";
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            JOB_TRANSACTIONS = new ObservableCollection<JOB_TRANSACTIONS>(ExoQueries.GetJOB_TRANSACTIONS(primeroUnitOfWork, loadPROJECT.NUMBER));
            VARIATIONCODECollection = new ObservableCollection<string>(ExoQueries.GetVariationCodes(primeroUnitOfWork, loadPROJECT.NUMBER));
        }

        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc);
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query.Where(x => x.JOBCODE.Contains(loadPROJECT.NUMBER.ToString()));
        }

        public ObservableCollection<JOB_TRANSACTIONS> JOB_TRANSACTIONS = new ObservableCollection<JOB_TRANSACTIONS>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> specifyMainViewModelProjection()
        {
            return query => new List<BASELINE_ITEM>().AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEM> entities)
        {
            MainViewModel.SetParentViewModel(this);
            MainViewModel.IsPasteCellLevel = false;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #endregion

        #region View Properties
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        private void reselectDeliverable()
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.DisplaySelectedEntity)));
        }

        public ExoTimeAuthorisation SelectedDataRow { get; set; }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "TransactionEntryViewModelWrapper_v1" + view_project_specific_affix; }
        }

        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
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


        DataTable dataPointsTable = null;
        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || DisplayEntities == null)
                    return null;

                if (dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    dataPointsTable.Columns.Add(ColumnEntity, typeof(JOB_TRANSACTIONS));

                    foreach (JOB_TRANSACTIONS transaction in JOB_TRANSACTIONS)
                    {
                        DataRow newRow = dataPointsTable.NewRow();
                        newRow[ColumnEntity] = transaction;
                        dataPointsTable.Rows.Add(newRow);
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
            set
            {
                dataPointsTable = value;
            }
        }
        #endregion

        public override void FullRefresh()
        {
            EntitiesUndoRedoManager.Clear();
            base.FullRefresh();
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            EntitiesUndoRedoManager.Clear();
            DataRowView dataRowView = (DataRowView)e.Row;
            JOB_TRANSACTIONS entity = (JOB_TRANSACTIONS)dataRowView.Row[ColumnEntity];
            EntitiesUndoRedoManager.PauseActionId();
            if (e.Column.FieldName.ToUpper().Contains("ENTITY"))
            {
                EntitiesUndoRedoManager.AddUndo(dataRowView.Row, e.Column.FieldName, e.OldValue, e.Value, EntityMessageType.Changed);
                SaveEntity();
            }

            EntitiesUndoRedoManager.UnpauseActionId();

            e.Handled = true;
        }

        private void SaveEntity()
        {
            primeroUnitOfWork.SaveChanges();
        }

        public virtual void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            TableView gridTableView = (TableView)gridControl.View;
            string newValueString = Clipboard.GetText().ToString();
            string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();
            pasteCellData(gridControl, gridTableView, RowData);

            SaveEntity();
            GridControlService.RefreshData();
            e.Handled = true;
        }


        private void pasteCellData(GridControl gridControl, TableView gridTableView, string[] RowData)
        {
            EntitiesUndoRedoManager.Clear();
            EntitiesUndoRedoManager.PauseActionId();
            var selected_cells = gridTableView.GetSelectedCells();
            if (selected_cells.Count == 0)
                return;

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

            var selected_cells_groupby_columns = selected_cells.GroupBy(x => x.Column.FieldName).Select(group => new { FieldName = group.Key, Cells = group.ToList() });
            if (grouped_results.Count == 0)
            {
                foreach (var selected_cell in selected_cells)
                {
                    int row_handle = selected_cell.RowHandle;
                    DataRowView editing_row_view = (DataRowView)gridControl.GetRow(row_handle);
                    DataRow editing_row = editing_row_view.Row;
                    basePasteData(editing_row, selected_cell.Column, string.Empty, false);
                }
            }
            else
            {
                GridCell first_selected_cell = selected_cells.First();
                GridCell last_selected_cell = selected_cells.Last();

                int first_row_handle = selected_cells.Min(x => x.RowHandle);
                int last_row_handle = selected_cells.Max(x => x.RowHandle);
                int first_row_visible_index = gridControl.GetRowVisibleIndexByHandle(first_row_handle);
                int last_row_visible_index = gridControl.GetRowVisibleIndexByHandle(last_row_handle);
                int numberOfSelectedRows = (last_row_visible_index - first_row_visible_index) + 1;
                int numberOfCopiedRows = grouped_results.First().Count;

                List<GridColumn> visible_columns = gridTableView.VisibleColumns.ToList();
                int first_column_visible_index = visible_columns.First(x => x.FieldName == first_selected_cell.Column.FieldName).VisibleIndex;
                int last_column_visible_index = visible_columns.First(x => x.FieldName == last_selected_cell.Column.FieldName).VisibleIndex;

                int numberOfSelectedColumns = (last_column_visible_index - first_column_visible_index) + 1;
                int numberOfCopiedColumns = grouped_results.Count;

                //commented out because not accurate during banded view
                //int first_column_visible_index = first_selected_cell.Column.VisibleIndex;

                int rowOffsetSelection = numberOfSelectedRows > numberOfCopiedRows ? numberOfSelectedRows : numberOfCopiedRows;
                int columnOffsetSelection = numberOfSelectedColumns > numberOfCopiedColumns ? numberOfSelectedColumns : numberOfCopiedColumns;

                int pasteValueRowOffset = 0;
                for (int rowOffset = 0; rowOffset < rowOffsetSelection; rowOffset++)
                {
                    int pasteValueColumnOffset = 0;
                    for (int columnOffset = 0; columnOffset < columnOffsetSelection; columnOffset++)
                    {
                        if (!visible_columns.Any(x => x.VisibleIndex == (first_column_visible_index + columnOffset)))
                            continue;

                        GridColumn current_column = visible_columns.First(x => x.VisibleIndex == (first_column_visible_index + columnOffset));
                        string columnValue = grouped_results[pasteValueColumnOffset][pasteValueRowOffset];

                        int current_row_visible_index = first_row_visible_index + rowOffset;
                        int current_row_handle = gridControl.GetRowHandleByVisibleIndex(current_row_visible_index);

                        object rowObject = gridControl.GetRow(current_row_handle);
                        if (rowObject == null)
                            continue;

                        DataRowView editing_row_view = (DataRowView)rowObject;
                        DataRow editing_row = editing_row_view.Row;
                        var gg = from c in editing_row.Table.Columns.Cast<DataColumn>()
                                 select c.ColumnName;

                        pasteValueColumnOffset += 1;
                        if (pasteValueColumnOffset >= grouped_results.Count)
                            pasteValueColumnOffset = 0;

                        basePasteData(editing_row, current_column, columnValue, false);
                    }

                    pasteValueRowOffset += 1;
                    if (pasteValueRowOffset >= grouped_results[pasteValueColumnOffset].Count)
                        pasteValueRowOffset = 0;
                }
            }

            EntitiesUndoRedoManager.UnpauseActionId();
        }


        private bool basePasteData(DataRow newRow, ColumnBase copyColumn, string pasteData, bool isNewRow)
        {
            JOB_TRANSACTIONS entity = (JOB_TRANSACTIONS)newRow[ColumnEntity];
            string editFieldname = formatFieldName(copyColumn.FieldName);

            if (copyColumn.FieldType == typeof(int?))
            {
                ComboBoxEditSettings editSettings = copyColumn.ActualEditSettings as ComboBoxEditSettings;
                if (editSettings != null)
                {
                    var copyColumnValueMember = (string)editSettings.GetType().GetProperty("ValueMember").GetValue(editSettings);
                    var copyColumnDisplayMember = (string)editSettings.GetType().GetProperty("DisplayMember").GetValue(editSettings);
                    var copyColumnItemsSource = (IEnumerable<object>)editSettings.GetType().GetProperty("ItemsSource").GetValue(editSettings);
                    int? int_value = null;

                    if (copyColumnItemsSource == null || copyColumnValueMember == null || copyColumnDisplayMember == null)
                        return false;

                    foreach (var copyColumnItem in copyColumnItemsSource)
                    {
                        var itemDisplayMemberPropertyInfo =
                            copyColumnItem.GetType().GetProperty(copyColumnDisplayMember);
                        var itemValueMemberPropertyInfo =
                            copyColumnItem.GetType().GetProperty(copyColumnValueMember);
                        if (itemDisplayMemberPropertyInfo.GetValue(copyColumnItem).ToString().ToUpper() == pasteData.ToUpper())
                        {
                            int_value = (int)itemValueMemberPropertyInfo.GetValue(copyColumnItem);
                            break;
                        }
                    }

                    if (int_value != null)
                    {
                        if (!isNewRow)
                        {
                            int? oldValue = (int?)entity.GetType().GetProperty(editFieldname).GetValue(entity);
                            EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldValue, int_value, EntityMessageType.Changed);
                        }

                        entity.GetType().GetProperty(editFieldname).SetValue(entity, int_value);
                    }
                }
            }
            else if (copyColumn.FieldType == typeof(string))
            {
                string oldValue = (string)entity.GetType().GetProperty(editFieldname).GetValue(entity);
                EntitiesUndoRedoManager.AddUndo(newRow, copyColumn.FieldName, oldValue, pasteData, EntityMessageType.Changed);
                entity.GetType().GetProperty(editFieldname).SetValue(entity, pasteData);
            }

            return true;
        }

        private string formatFieldName(string fieldName)
        {
            return fieldName.Replace(ColumnEntity + ".", "");
        }

        public override string UnifiedValueValidation(BASELINE_ITEM projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(BASELINE_ITEM projection)
        {
            return string.Empty;
        }


        public bool CanUndo()
        {
            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanUndo();
        }

        public bool CanRedo()
        {
            if (EntitiesUndoRedoManager == null)
                return false;

            return EntitiesUndoRedoManager.CanRedo();
        }

        public void Undo()
        {
            EntitiesUndoRedoManager.Undo();
        }

        public void Redo()
        {
            EntitiesUndoRedoManager.Redo();
        }

        /// <summary>
        /// Manages all undo and redo operation
        /// </summary>
        private EntitiesUndoRedoManager<DataRow> entitiesundoredomanager { get; set; }

        public EntitiesUndoRedoManager<DataRow> EntitiesUndoRedoManager
        {
            get
            {
                if (entitiesundoredomanager == null)
                    entitiesundoredomanager = new EntitiesUndoRedoManager<DataRow>(BulkPropertyUndo, BulkPropertyRedo);

                return entitiesundoredomanager;
            }
        }

        bool isBackgroundEdit = false;
        /// <summary>
        /// Function to undo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyUndo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);

            //use ignore refresh here because it'll be refreshed in basebulksave

            EntitiesUndoRedoManager.PauseActionId();
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                JOB_TRANSACTIONS entity = (JOB_TRANSACTIONS)entityProperty.ChangedEntity[ColumnEntity];
                string formattedFieldName = formatFieldName(entityProperty.PropertyName);
                object newValue = entityProperty.OldValue;
                object oldValue = entity.GetType().GetProperty(formattedFieldName).GetValue(entity);
                entity.GetType().GetProperty(formattedFieldName).SetValue(entity, newValue);
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            SaveEntity();
            GridControlService.RefreshData();
            isBackgroundEdit = false;
        }

        /// <summary>
        /// Function to redo the entity changes
        /// Must be used in conjunction of EntitiesUndoManager
        /// </summary>
        /// <param name="entityProperty">Entity passed over from EntitiesUndoRedo</param>
        public virtual void BulkPropertyRedo(IEnumerable<UndoRedoEntityInfo<DataRow>> entityProperties)
        {
            isBackgroundEdit = true;
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkSaveProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Changed);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkAddProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Added);
            IEnumerable<UndoRedoEntityInfo<DataRow>> bulkDeleteProperties = entityProperties.Where(x => x.MessageType == EntityMessageType.Deleted);

            EntitiesUndoRedoManager.PauseActionId();
            foreach (UndoRedoEntityInfo<DataRow> entityProperty in bulkSaveProperties)
            {
                JOB_TRANSACTIONS entity = (JOB_TRANSACTIONS)entityProperty.ChangedEntity[ColumnEntity];
                string formattedFieldName = formatFieldName(entityProperty.PropertyName);
                object newValue = entityProperty.NewValue;
                object oldValue = entity.GetType().GetProperty(formattedFieldName).GetValue(entity);
                entity.GetType().GetProperty(formattedFieldName).SetValue(entity, newValue);
            }

            EntitiesUndoRedoManager.UnpauseActionId();
            SaveEntity();
            GridControlService.RefreshData();
            isBackgroundEdit = false;
        }

        public IEnumerable<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_HDR>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.JOBCODE);
                return collection;
            }
        }

        public IEnumerable<JOBCOST_RESOURCE> JOBCOST_RESOURCECollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_RESOURCE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.RESOURCENAME);
                return collection;
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }
    }
}

