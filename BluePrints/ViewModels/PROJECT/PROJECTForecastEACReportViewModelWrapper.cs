using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class PROJECTForecastEACReportViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <FORECAST_EAC, FORECAST_EAC, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of FORECASTCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTForecastEACReportViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTForecastEACReportViewModelWrapper(unitOfWorkFactory));
        }


        /// Initializes a new instance of the FORECASTCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the FORECASTCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTForecastEACReportViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private PROJECT loadPROJECT;
        List<ExoTimeAuthorisation> exoLines;
        private static string notAvailableStr = "Doesn't Exists in Exo";
        public bool IsWeeks => false;
        protected override void resolveParameters(object parameter)
        {
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.FORECAST_EACS);
        }

        protected override bool loadDataPointsTable()
        {
            dataPointsTable = null;
            updateDataPointsTable();
            this.RaisePropertyChanged(x => x.DataPointsTable);
            CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());
            IsLoading = false;
            this.RaisePropertyChanged(x => x.IsLoading);
            return true;
        }

        DataTable dataPointsTable = null;
        public virtual DataTable DataPointsTable
        {
            get
            {
                return dataPointsTable;
            }
        }

        protected List<DateTime> alignedDataDateCollection;
        protected string columnEntity = "Entity";
        private void updateDataPointsTable()
        {
            dataPointsTable = new DataTable();
            GridControlService.GridControl.BeginDataUpdate();
            //get immutable data
            alignedDataDateCollection = generateDates();
            exoLines = ExoQueries.GetProjectLines(primeroUnitOfWorkFactory.CreateUnitOfWork(), loadPROJECT.NUMBER);
            InitializeColumnSource(ViewColumns, ViewSummaries, alignedDataDateCollection);

            LoadingScreenManager.ShowLoadingScreen(Entities.Count);
            LoadingScreenManager.SetMessage("Preparing View...");

            //construct data points table
            dataPointsTable.Columns.Add(columnEntity, typeof(ForecastEACProjection));
            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            var groupedJobs = Entities.GroupBy(x => new { SubJobCode = x.SUBJOB_CODE, DisciplineCode = x.DISCIPLINE_CODE, CommodityCode = x.COMMODITY_CODE, VariationCode = x.VARIATION_CODE })
              .Select(group => new { group.Key.SubJobCode, group.Key.DisciplineCode, group.Key.CommodityCode, group.Key.VariationCode, DateCosts = group.ToList() }).OrderBy(x => x.SubJobCode).ThenBy(x => x.DisciplineCode).ThenBy(x => x.CommodityCode).ThenBy(x => x.VariationCode);

            foreach (var groupedJob in groupedJobs)
            {
                ExoTimeAuthorisation exoLine = exoLines.FirstOrDefault(x => x.SubJobCode == groupedJob.SubJobCode);
                JOB_COSTTYPES costType = JOB_COSTTYPESCollection.FirstOrDefault(x => x.SHORTCODE == groupedJob.CommodityCode);

                ExoSubJobProjection jobProjection = new ExoSubJobProjection();
                jobProjection.SubJobCode = groupedJob.SubJobCode;
                jobProjection.SubJobTitle = exoLine == null ? notAvailableStr : exoLine.SubJobTitle;
                jobProjection.DisciplineCode = groupedJob.DisciplineCode;
                jobProjection.CommodityCode = groupedJob.CommodityCode;
                jobProjection.CommodityName = costType == null ? notAvailableStr : costType.COSTDESC;
                jobProjection.VariationCode = groupedJob.VariationCode;

                ForecastEACProjection jobCostsProjection = new ForecastEACProjection();
                jobCostsProjection.Job = jobProjection;
                jobCostsProjection.DateCosts = new List<DatatableDateCost>();

                foreach (var dateCost in groupedJob.DateCosts)
                {
                    DatatableDateCost jobDateCost = new DatatableDateCost();
                    jobDateCost.Cost = dateCost.FORECAST_COSTS == null ? 0 : (decimal)dateCost.FORECAST_COSTS;
                    jobDateCost.Date = dateCost.FORECAST_DATE;
                    jobCostsProjection.DateCosts.Add(jobDateCost);
                }

                updateDataTable(jobCostsProjection);
                LoadingScreenManager.Progress();
            }

            GridControlService.GridControl.EndDataUpdate();
            LoadingScreenManager.CloseLoadingScreen();
        }

        private void updateDataTable(ForecastEACProjection job)
        {
            DataRow forecastRow = dataPointsTable.NewRow();
            forecastRow[columnEntity] = job;

            foreach (DatatableDateCost dateCost in job.DateCosts)
            {
                string dateColumnStr = dateCost.Date.ToString(BluePrintsResources.ColumnDateFormat);
                if (dataPointsTable.Columns.Contains(dateColumnStr))
                {
                    forecastRow[dateColumnStr] = dateCost.Cost;
                }
            }

            dataPointsTable.Rows.Add(forecastRow);
        }

        private List<DateTime> generateDates()
        {
            DateTime firstDataDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1);
            DateTime lastDataDate = firstDataDate;
            
            if(Entities.Count != 0)
            {
                firstDataDate = Entities.Min(x => x.FORECAST_DATE);
                lastDataDate = Entities.Max(x => x.FORECAST_DATE);
            }

            return ChronologicalHelpers.GenerateEndDatesCollection(firstDataDate, lastDataDate);
        }

        private void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Job.PhaseCode", ReadOnly = true, Header = "Phase", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Job.SubJobCode", ReadOnly = true, Header = "Subjob", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Job.SubJobTitle", ReadOnly = true, Header = "Subjob Title", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Job.SubJobCode", DisplayFormat = "Total {0} Records", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Job.DisciplineCode", ReadOnly = true, Header = "Discipline", Fixed = FixedStyle.Left, Width = 80, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Job.CommodityCode", ReadOnly = true, Header = "Commodity", Fixed = FixedStyle.Left, Width = 80, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Job.CommodityName", ReadOnly = true, Header = "Commodity Name", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Job.VariationCode", ReadOnly = true, Header = "Variation", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Fixed = FixedStyle.None, Width = 80, Settings = SettingsType.ForecastPast });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }

        public void AutoGeneratingColumns(AutoGeneratingColumnEventArgs e)
        {
            GridControl gridControl = (GridControl)e.Source;
            DateTime parsedate;
            if (DateTime.TryParse(e.Column.FieldName, out parsedate))
            {
                e.Column.HeaderTemplate = Application.Current.Resources["ForecastHeaderTemplate"] as DataTemplate;
                e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                e.Column.AllowEditing = DevExpress.Utils.DefaultBoolean.False;
                e.Column.ReadOnly = true;
                GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c0");
                e.Column.FilterPopupMode = FilterPopupMode.Excel;
                e.Column.Width = 60;
                e.Column.AllowBestFit = DevExpress.Utils.DefaultBoolean.False;
                e.Column.AddHandler(DXSerializer.AllowPropertyEvent, new AllowPropertyEventHandler(column_AllowProperty));
            }
            else
            {
                if (e.Column.FieldType == typeof(decimal))
                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, e.Column.FieldName + ": {0:c0}");

                e.Column.ReadOnly = true;
                e.Column.Fixed = FixedStyle.Left;
            }
        }

        void column_AllowProperty(object sender, AllowPropertyEventArgs e)
        {
            e.Allow = false;
        }

        protected override Func<IRepositoryQuery<FORECAST_EAC>, IQueryable<FORECAST_EAC>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == Common.ForecastEACType.EAC);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_EAC> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(FORECAST_EAC projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(FORECAST_EAC projection)
        {
            return string.Empty;
        }

        public override bool CanFullRefresh()
        {
            if (dataPointsTable == null)
                return false;

            return base.CanFullRefresh();
        }

        public override void FullRefresh()
        {
            IsLoading = true;
            this.RaisePropertyChanged(x => x.IsLoading);
            alignedDataDateCollection.Clear();
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
            base.FullRefresh();
        }
        #endregion

        #region View Properties
        protected ObservableCollection<ColumnDescriptor> viewColumns;
        public ObservableCollection<ColumnDescriptor> ViewColumns
        {
            get
            {
                if (viewColumns == null)
                {
                    viewColumns = new ObservableCollection<ColumnDescriptor>();
                }
                return viewColumns;
            }
        }

        protected ObservableCollection<SummaryDescriptor> viewSummaries;
        public ObservableCollection<SummaryDescriptor> ViewSummaries
        {
            get
            {
                if (viewSummaries == null)
                {
                    viewSummaries = new ObservableCollection<SummaryDescriptor>();
                }
                return viewSummaries;
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                return GetEntities<JOB_COSTTYPES>();
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "FORECASTEACReportCollectionViewModelWrapper_v2"; }
        }
        #endregion
    }
}