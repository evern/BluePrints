using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class PROJECTPlanViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>
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

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private List<DateTime> alignedDateCollection;
        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> specifyMainViewModelProjection()
        {
            return query => populatePROJECTPlanProject(query);
        }

        private IQueryable<PROJECT> populatePROJECTPlanProject(IQueryable<PROJECT> query)
        {
            List<PROJECT> PROJECTPlan = query.Where(x => x.STATUS == ProjectStatus.Lead || x.STATUS == ProjectStatus.Tender || x.STATUS == ProjectStatus.TenderSubmitted).ToList();
            return PROJECTPlan.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Helpers

        private List<DateTime> generateDates(IEnumerable<PROJECT> PROJECTS)
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            foreach (PROJECT PROJECT in PROJECTS)
            {
                if (PROJECT.TENDER_PROJECT_START != null)
                {
                    DateTime PROJECTPlanStartDate = ((DateTime)PROJECT.TENDER_PROJECT_START);
                    int duration = PROJECT.TENDER_PROJECT_DURATION == null ? 1 : (int)PROJECT.TENDER_PROJECT_DURATION;
                    DateTime PROJECTPlanEndDate = PROJECTPlanStartDate.AddDays(duration * 7);
                    if (startDate > PROJECTPlanStartDate)
                        startDate = new DateTime(PROJECTPlanStartDate.Year, PROJECTPlanStartDate.Month, 1);
                    if (endDate < PROJECTPlanEndDate)
                        endDate = PROJECTPlanEndDate;
                }
            }

            return ChronologicalHelpers.GenerateEndDatesCollection(startDate, endDate);
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(PROJECT entity, PROJECT projection, bool isNewEntity)
        {
            onAfterPROJECTPlanSaved(entity);
        }

        private void onAfterPROJECTPlanSaved(PROJECT entity)
        {

        }

        public void NewRowAddUndoAndSave(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();

                DataRowView row = (DataRowView)e.Row;

                //findExistingOrAddNewFORECAST_JOB(row.Row);
                //MainViewModel.EntitiesUndoRedoManager.AddUndo(updatedForecastJobFromDataRow(row.Row), null, null, null, EntityMessageType.Added);
                //focusNewlyAddedProjectionTimer.Start();
                ////added not working well atm because when row is removed from datatable its itemarray is cleared
                ////EntitiesUndoRedoManager.AddUndo(row.Row, null, null, null, EntityMessageType.Added);
                //MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            }
        }

        public override void ValidateRow(GridRowValidationEventArgs e)
        {
            DataRow dataRow = ((DataRowView)e.Row).Row;
            string errorMessage = UnifiedRowValidation((PROJECT)dataRow[columnEntity]);

            if (errorMessage != string.Empty)
            {
                e.IsValid = false;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                e.ErrorContent = errorMessage;
            }
        }

        /// <summary>
        /// Influence column(s) when changes happens in other column
        /// </summary>
        public void CellValueChangedUpdate(CellValueChangedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row;
            if (e.RowHandle == GridControl.AutoFilterRowHandle)
                return;

            //new item handling
            if (e.RowHandle == GridControl.NewItemRowHandle)
            {

                PROJECT newPROJECT;
                if (dataRowView[columnEntity] == DBNull.Value)
                {
                    newPROJECT = new PROJECT();
                    dataRowView[columnEntity] = newPROJECT;
                }
                else
                    newPROJECT = (PROJECT)dataRowView[columnEntity];

                return;
            }

            //existing item handling
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            string fieldName = e.Column.FieldName;

            //commitCellValue(fieldName, dataRowView.Row, e.Value);
            //MainViewModel.EntitiesUndoRedoManager.AddUndo(updatedForecastJobFromDataRow(dataRowView.Row), fieldName, e.OldValue, e.Value, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();

            e.Handled = true;
        }

        public override string UnifiedRowValidation(PROJECT projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PROJECT projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        DataTable dataPointsTable = null;
        string columnEntity = "Entity";

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

        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || Entities == null)
                    return null;

                if (dataPointsTable == null)
                {
                    GridControlService.BeginDataUpdate();
                    dataPointsTable = new DataTable();

                    if (alignedDateCollection == null)
                    {
                        alignedDateCollection = generateDates(Entities);
                        InitializeColumnSource(ColumnDescriptors, SummaryDescriptors, alignedDateCollection);
                    }

                    dataPointsTable.Columns.Add(columnEntity, typeof(PROJECT));
                    foreach (DateTime alignedDataDate in alignedDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach (PROJECT entity in Entities)
                    {
                        BuildRowStats(entity, false);
                    }

                    GridControlService.EndDataUpdate();
                }

                return dataPointsTable;
            }
        }

        private void BuildRowStats(PROJECT entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            DataRow newDataRow;
            if (!isUpdate)
                newDataRow = dataPointsTable.NewRow();
            else
            {
                newDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((PROJECT)dr[columnEntity]).GUID == entity.GUID
                              select dr).FirstOrDefault();
            }

            if (newDataRow == null)
                return;

            newDataRow[columnEntity] = entity;
            //format dates row to numbers
            for (int i = 0; i < newDataRow.ItemArray.Count(); i++)
            {
                string columnName = dataPointsTable.Columns[i].ColumnName;
                if (columnName != columnEntity)
                    newDataRow[columnName] = 0.00m;
            }

            if (!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }

        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".NUMBER", ReadOnly = true, Header = "Number", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = columnEntity + ".NUMBER", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".NAME", ReadOnly = true, Header = "Name", Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".STATUS", Header = "Status", Fixed = FixedStyle.Left, Width = 70, ItemsSource = ProjectStatusCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_TYPE", Header = "Type", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineTypeCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_DIVISION", Header = "Division", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineDivisionCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_COMMODITY", Header = "Commodity", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineCommodityCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_CONTRACT", Header = "Contract", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineContractCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_STATUS", Header = "Status", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineStatusCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".TENDER_PROJECT_START", Header = "Start Date", ReadOnly = false, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".TENDER_PROJECT_DURATION", ReadOnly = false, Visible = true, Header = "Duration", Mask = "###,##0 Weeks", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_GROSS_PROFIT", ReadOnly = false, Visible = true, Header = "Gross Profit", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_TOTAL_VALUE", ReadOnly = false, Visible = true, Header = "Total Value", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = columnEntity + ".PIPELINE_SCOPE_PCT", ReadOnly = false, Visible = true, Header = "Scope %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.Date });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPlanCollectionViewModelWrapper"; }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                if (collection == null)
                    return new List<PROJECT>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection.OrderBy(x => x.NUMBER).ToList();
            }
        }

        public IEnumerable<ProjectStatus> ProjectStatusCollection
        {
            get
            {
                return DataUtils.GetValuesOf(() => new ProjectStatus());
            }
        }

        public IEnumerable<PipelineType> PipelineTypeCollection
        {
            get
            {
                return DataUtils.GetValuesOf(() => new PipelineType());
            }
        }

        public IEnumerable<PipelineDivision> PipelineDivisionCollection
        {
            get
            {
                return DataUtils.GetValuesOf(() => new PipelineDivision());
            }
        }

        public IEnumerable<PipelineCommodity> PipelineCommodityCollection
        {
            get
            {
                return DataUtils.GetValuesOf(() => new PipelineCommodity());
            }
        }

        public IEnumerable<PipelineContract> PipelineContractCollection
        {
            get
            {
                return DataUtils.GetValuesOf(() => new PipelineContract());
            }
        }

        public IEnumerable<PipelineStatus> PipelineStatusCollection
        {
            get
            {
                return DataUtils.GetValuesOf(() => new PipelineStatus());
            }
        }
        #endregion
    }
}