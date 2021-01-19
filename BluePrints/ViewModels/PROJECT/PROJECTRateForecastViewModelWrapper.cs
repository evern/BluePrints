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
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
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
    public class PROJECTRateForecastViewModelWrapper : PROJECTIndirectForecastViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of FORECAST_JOBCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTRateForecastViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTRateForecastViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the FORECAST_JOBCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the FORECAST_JOBCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTRateForecastViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        BackgroundWorker exoLoadingBackgroundWorker = new BackgroundWorker();
        protected override void resolveParameters(object parameter)
        {
            showAllJobs = true;
            exoLoadingBackgroundWorker.DoWork += ExoLoadingBackgroundWorker_DoWork;
            exoLoadingBackgroundWorker.WorkerSupportsCancellation = true;
            base.resolveParameters(parameter);
        }

        private void ExoLoadingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(LoadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            loadExoData(primeroUnitOfWork);
        }

        private void ProjectSavingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //when view is closed halfway
            if (PROJECTCollectionViewModel != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => PROJECTCollectionViewModel.Save(LoadPROJECT)));
        }

        bool isExoDataLoaded = false;
        private void loadExoData(IPrimeroEntitiesUnitOfWork primeroUOW)
        {
            isExoDataLoaded = false;
            exoActuals = BluePrintsDataUtils.GetBurned(primeroUOW, LoadPROJECT.NUMBER, ActualsCutOffDate);
            exoMaterials = BluePrintsDataUtils.GetMaterials(primeroUOW, LoadPROJECT.NUMBER, ActualsCutOffDate, null, 1, true);
            AllActuals = new List<ExoDataPoint>();
            AllActuals.AddRange(exoActuals);
            AllActuals.AddRange(exoMaterials);
            isExoDataLoaded = true;
            mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            exoLoadingBackgroundWorker.RunWorkerAsync();
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
            columns.Add(new ColumnDescriptor() { FieldName = columnReference, ReadOnly = false, Header = "Reference", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnNote, ReadOnly = false, Header = "Note", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnUOM, ReadOnly = false, Header = "UOM", Fixed = FixedStyle.Left, Width = 50, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnRecommendedForecastRate, ReadOnly = true, Header = "Recommended Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnForecastRate, ReadOnly = false, Header = "Sell Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            columns.Add(new ColumnDescriptor() { FieldName = columnForecastJob + "." + BindableBase.GetPropertyName(() => new FORECAST_JOB().IS_FLOATING_RATE), ReadOnly = false, Header = "Floating Rate", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalHours, ReadOnly = true, Header = "Total Hrs", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "n0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalHours, DisplayFormat = "n0", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalCosts, ReadOnly = false, Header = "Total $ Sell", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalCosts, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = columnTotalActuals, ReadOnly = false, Header = "Total $ Cost", Fixed = FixedStyle.Left, Width = 75, Settings = SettingsType.Number, Mask = "c0" });
            summaries.Add(new SummaryDescriptor() { FieldName = columnTotalCosts, DisplayFormat = "c0", Type = SummaryItemType.Sum });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);

                if (alignedDate > FixedDataDateMonthEnd)
                {
                    columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, Header = columnFieldName, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                    summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "n0", Type = SummaryItemType.Sum });
                }
            }
        }

        public DateTime ActualsCutOffDate
        {
            get
            {
                if (FixedDataDate == null)
                    return DateTime.Now;
                else
                {
                    DateTime forecastStartDate = (DateTime)FixedDataDate;
                    return new DateTime(forecastStartDate.Year, forecastStartDate.Month, 1).AddDays(-1);
                }
            }
        }

        public IEnumerable<ExoDataPoint> ActualsDetail => AllActuals;
    }
}