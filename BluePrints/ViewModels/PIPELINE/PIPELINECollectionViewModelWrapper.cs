using BaseModel.Data.Helpers;
using BaseModel.DataModel;
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
    public class PIPELINECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PIPELINE, PIPELINE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PIPELINECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PIPELINECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PIPELINECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PIPELINECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PIPELINECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PIPELINECollectionViewModelWrapper(
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
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<PIPELINE_REVENUE, PIPELINE_REVENUE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PIPELINE_REVENUES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PIPELINES);
        }

        protected override Func<IRepositoryQuery<PIPELINE>, IQueryable<PIPELINE>> specifyMainViewModelProjection()
        {
            return query => populatePipelineProject(query);
        }

        private IQueryable<PIPELINE> populatePipelineProject(IQueryable<PIPELINE> query)
        {
            List<PIPELINE> pipeline = query.ToList();
            return pipeline.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PIPELINE> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Helpers

        private List<DateTime> generateDates(IEnumerable<PIPELINE> pipelines)
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            foreach(PIPELINE pipeline in pipelines)
            {
                if(pipeline.START_DATE != null)
                {
                    DateTime pipelineStartDate = ((DateTime)pipeline.START_DATE);
                    DateTime pipelineEndDate = pipelineStartDate.AddMonths(pipeline.DURATION);
                    if(startDate > pipelineStartDate)
                        startDate = new DateTime(pipelineStartDate.Year, pipelineStartDate.Month, 1);
                    if (endDate < pipelineEndDate)
                        endDate = pipelineEndDate;
                }
            }

            return ChronologicalHelpers.GenerateEndDatesCollection(startDate, endDate);
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(PIPELINE entity, PIPELINE projection, bool isNewEntity)
        {
            onAfterPipelineSaved(entity);
        }

        private void onAfterPipelineSaved(PIPELINE entity)
        {

        }

        public override string UnifiedRowValidation(PIPELINE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PIPELINE projection, string field_name, object new_value, bool isPaste)
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

                    dataPointsTable.Columns.Add(columnEntity, typeof(PIPELINE));
                    foreach (DateTime alignedDataDate in alignedDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach (PIPELINE entity in Entities)
                    {
                        BuildRowStats(entity, false);
                    }

                    GridControlService.EndDataUpdate();
                }

                return dataPointsTable;
            }
        }

        private void BuildRowStats(PIPELINE entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            DataRow newDataRow;
            if (!isUpdate)
                newDataRow = dataPointsTable.NewRow();
            else
            {
                newDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((PIPELINE)dr[columnEntity]).GUID == entity.GUID
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

            columns.Add(new ColumnDescriptor() { FieldName = "Number", ReadOnly = true, Header = "Number", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = "Number", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = "Name", ReadOnly = true, Header = "Name", Fixed = FixedStyle.Left, Width = 110, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "TYPE", Header = "Type", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineTypeCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = "DIVISION", Header = "Division", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineDivisionCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = "COMMODITY", Header = "Commodity", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineCommodityCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = "CONTRACT", Header = "Contract", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineContractCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = "STATUS", Header = "Status", Fixed = FixedStyle.Left, Width = 70, ItemsSource = PipelineStatusCollection, Settings = SettingsType.Collection });
            columns.Add(new ColumnDescriptor() { FieldName = "START_DATE", Header = "Start Date", ReadOnly = false, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = "DURATION", ReadOnly = false, Visible = true, Header = "Duration", Mask = "###,##0 Months", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "GROSS_PROFIT", ReadOnly = false, Visible = true, Header = "Gross Profit", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "TOTAL_VALUE", ReadOnly = false, Visible = true, Header = "Total Value", Mask = "c2", Increment = 1, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "SCOPE_PCT", ReadOnly = false, Visible = true, Header = "Scope %", Mask = "p2", Increment = 0.1m, Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastChild });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PIPELINECollectionViewModelWrapper"; }
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