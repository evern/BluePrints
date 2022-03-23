using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.View;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class PROJECTIndirectForecastHistoryViewModelWrapper : PROJECTIndirectForecastViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of FORECAST_JOBCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static new PROJECTIndirectForecastHistoryViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTIndirectForecastHistoryViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the FORECAST_JOBCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the FORECAST_JOBCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTIndirectForecastHistoryViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOB_HOUR_SNAPSHOTS, FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc);
        }

        private Func<IRepositoryQuery<FORECAST_JOB_HOUR_SNAPSHOT>, IQueryable<FORECAST_JOB_HOUR_SNAPSHOT>> FORECAST_JOB_HOUR_SNAPSHOTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID && x.DATA_DATE == FixedDataDate && x.SNAPSHOT_TYPE == Common.ForecastSnapshotValueType.Actual);
        }

        protected override List<DateTime> generateDates()
        {
            IQueryable<FORECAST_JOB_HOUR> projectForecastJobHours = MainEntityUnitOfWork.FORECAST_JOB_HOURS.Where(x => x.FORECAST_JOB.GUID_PROJECT == LoadPROJECT.GUID);
            DateTime forecastMinDate = projectForecastJobHours.Min(x => x.FORECAST_DATE);
            int collectionCount = FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.FORECAST_DATE != null).Count();
            DateTime actualsMinDate = collectionCount == 0 ? DateTime.Now : FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.FORECAST_DATE != null).Min(x => (DateTime)x.FORECAST_DATE);
            DateTime minDate = forecastMinDate < actualsMinDate ? forecastMinDate : actualsMinDate;

            return ChronologicalHelpers.GenerateEndDatesCollection(forecastMinDate, (DateTime)FixedDataDate, true);
        }

        protected override void updateDataPointsTable()
        {
            GridControlService.GridControl.BeginDataUpdate();
            dataPointsTable = new DataTable();
            InitializeColumnSource(ParentViewColumns, ParentSummaries, alignedDataDateCollection, false);
            LoadingScreenManager.ShowLoadingScreen(MainViewModel.Entities.Count);
            LoadingScreenManager.SetMessage("Preparing View...");

            //construct data points table
            dataPointsTable.Columns.Add(columnFullCode, typeof(string));
            dataPointsTable.Columns.Add(columnCommodityName, typeof(string));
            dataPointsTable.Columns.Add(columnDescription, typeof(string));
            dataPointsTable.Columns.Add(columnStockItem, typeof(string));
            dataPointsTable.Columns.Add(columnReference, typeof(string));
            dataPointsTable.Columns.Add(columnNote, typeof(string));
            dataPointsTable.Columns.Add(columnUOM, typeof(string));
            dataPointsTable.Columns.Add(columnProjection, typeof(ExoSubJobProjection));
            dataPointsTable.Columns.Add(columnForecastJob, typeof(FORECAST_JOB));
            dataPointsTable.Columns.Add(columnStockItemName, typeof(string));
            dataPointsTable.Columns.Add(columnStockItemGroup2, typeof(string));
            dataPointsTable.Columns.Add(columnStockItemErrorImageWidth, typeof(decimal));
            dataPointsTable.Columns.Add(columnRecommendedForecastRate, typeof(decimal));
            dataPointsTable.Columns.Add(columnTotalHours, typeof(decimal));
            dataPointsTable.Columns.Add(columnTotalCosts, typeof(decimal));

            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            List<ExoDataPoint> allDataPoints = new List<ExoDataPoint>();
            List<FORECAST_JOB> deleteJobs = new List<FORECAST_JOB>();
            foreach (FORECAST_JOB job in MainViewModel.Entities)
            {
                //when job doesn't exist in EXO delete it
                ExoSubJobProjection projection = QueryJobs.Where(x => x.CommodityId != null && x.DisciplineId != null && x.SubJobId != null).FirstOrDefault(x => x.CommodityCode == job.COMMODITY_CODE && x.DisciplineCode == job.DISCIPLINE_CODE && x.SubJobCode == job.SUBJOB_CODE && x.VariationCode == job.VARIATION_CODE);
                if (projection == null)
                {
                    job.DELETE_REASON = "Removed from Budget Input";
                    deleteJobs.Add(job);
                    continue;
                }

                DataRow newRow = dataPointsTable.NewRow();
                newRow[columnFullCode] = projection.FullCode;
                newRow[columnProjection] = projection;
                newRow[columnForecastJob] = job;
                mapJobDataToDatatable(newRow);
                dataPointsTable.Rows.Add(newRow);

                job.ForecastJobHours = MainEntityUnitOfWork.FORECAST_JOB_HOURS.Where(x => x.GUID_FORECAST_JOB == job.GUID).ToList();
                job.BluePrintsEntitiesUnitOfWork = MainEntityUnitOfWork;
                foreach (DateTime alignedDate in alignedDataDateCollection)
                {
                    string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                    FORECAST_JOB_HOUR jobHourOnAlignedDate = job.ForecastJobHours.FirstOrDefault(x => x.GUID_FORECAST_JOB == job.GUID && x.FORECAST_DATE.Date == alignedDate.Date);
                    if (jobHourOnAlignedDate != null && jobHourOnAlignedDate.FORECAST_HOUR != null)
                        newRow[columnFieldName] = jobHourOnAlignedDate.FORECAST_HOUR;
                    else
                        newRow[columnFieldName] = DBNull.Value;
                }

                updateRowReadOnlyAttributes(newRow);
                LoadingScreenManager.Progress();
            }

            GridControlService.GridControl.EndDataUpdate();
            LoadingScreenManager.SetMessage("Deleting deprecated indirect forecasts");
            if (deleteJobs.Count() > 0)
            {
                //saves the delete reason
                MainViewModel.UnitOfWork.SaveChanges();

                foreach (FORECAST_JOB deleteJob in deleteJobs)
                    MainViewModel.UnitOfWork.FORECAST_JOBS.Remove(deleteJob);

                MainViewModel.UnitOfWork.SaveChanges();
            }

            LoadingScreenManager.CloseLoadingScreen();
        }

        protected override void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates, bool isChild)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = columnFullCode, ReadOnly = false, Header = "Full Code", ItemsSource = QueryJobs, Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.FullCode });
            summaries.Add(new SummaryDescriptor() { FieldName = columnFullCode, DisplayFormat = "Total {0} Records", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = columnCommodityName, ReadOnly = true, Header = "Commodity Name (AutoFilled)", Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnDescription, ReadOnly = false, Header = "Description", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItem, ReadOnly = false, Header = "Stock Code", ItemsSource = STOCK_ITEMCollection, HeaderToolTip = "Changing this value will automatically populate rate", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.StockItem });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItemName, ReadOnly = true, Header = "Stock Name (AutoFilled)", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnStockItemGroup2, ReadOnly = true, Header = "Stock Group 2 (AutoFilled)", Fixed = FixedStyle.Left, Width = 100, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnReference, ReadOnly = false, Header = "Reference", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnNote, ReadOnly = false, Header = "Note", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnUOM, ReadOnly = false, Header = "UOM", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnRecommendedForecastRate, ReadOnly = true, Header = "Recommended Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnForecastRate, ReadOnly = false, Header = "Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnForecastJob + "." + BindableBase.GetPropertyName(() => new FORECAST_JOB().IS_FLOATING_RATE), ReadOnly = false, Header = "Floating Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalHours, ReadOnly = true, Header = "Total Hrs", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "n0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalHours, DisplayFormat = "n0", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalCosts, ReadOnly = false, Header = "Total $", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalCosts, DisplayFormat = "c0", Type = SummaryItemType.Sum });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n0", Type = SummaryItemType.Sum });
            }
        }

        public IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> FORECAST_JOB_HOUR_SNAPSHOTCollection
        {
            get
            {
                return GetEntities<FORECAST_JOB_HOUR_SNAPSHOT>();
            }
        }

        public override string ViewName
        {
            get { return "PROEJCTIndirectForecastHistoryView_V1"; }
        }
    }
}