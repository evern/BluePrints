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
using BluePrints.Common;
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
using System.Globalization;
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

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
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
            mainThreadDispatcher.BeginInvoke(new Action(() => LoadSummaryStats(QueryJobs, QueryJobLines)));
            mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_JOB> entities)
        {
            MainViewModel.SetParentViewModel(this);
            MainViewModel.IsPasteCellLevel = true;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            exoLoadingBackgroundWorker.RunWorkerAsync();
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

        private DevExpress.Mvvm.IDialogService DistributionDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DistributionDialogService"); }
        }

        public bool CanDistributeUnits(object parameter)
        {
            if (IsLoading)
                return false;

            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return false;
                else
                {
                    if (selected_cells.First().Column == null)
                        return false;

                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            return true;
        }

        public void DistributeUnits(object parameter)
        {
            GridControl gridControl = (GridControl)parameter;
            TableView tableView = gridControl.View as TableView;
            var selected_cells = tableView.GetSelectedCells();
            if (selected_cells.Count == 0)
            {
                selected_cells = Enumerable.Range(0, gridControl.VisibleRowCount)
                .Select(x => (GridControl)gridControl.GetDetail(x))
                .Where(x => x != null).
                SelectMany(x => ((TableView)(x).View).GetSelectedCells()).ToList();

                if (selected_cells.Count == 0)
                    return;
                else
                {
                    tableView = (TableView)selected_cells.First().Column.View;
                    gridControl = tableView.Grid;
                }
            }

            foreach (var selectedCell in selected_cells)
            {
                var gridColumn = gridControl.Columns[selectedCell.Column.FieldName];
                if (gridColumn == null || gridColumn.ReadOnly)
                {
                    MessageBoxService.ShowMessage("Your selection contains read only cell, please revise your selection");
                    return;
                }
            }

            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            var distributionSelectViewModel = DistributionSelectViewModel<FORECAST_JOB>.Create(gridControl, selected_cells, columnForecastJob);
            if (DistributionDialogService.ShowDialog(MessageButton.OKCancel, "Select distribution method", "DistributionSelect", distributionSelectViewModel) == MessageResult.OK)
            {
                string newValueString = distributionSelectViewModel.ConvertToPasteData();
                string[] RowData = DataUtils.ExcelSplit(newValueString).ToArray();

                pasteCellData(gridControl, tableView, RowData, out errorMessages);
            }

            ShowErrorMessage("Errors", errorMessages);
        }

        public IEnumerable<ExoDataPoint> ActualsDetail => AllActuals;
        protected override string strTotalCosts => "Total Sell $";
        protected override string strTotalQty => "Total Sell Qty";

        protected override void raiseSummaryChanges()
        {
            this.RaisePropertyChanged(x => x.ForecastSummary);
            this.RaisePropertyChanged(x => x.TotalRevenue);
            this.RaisePropertyChanged(x => x.UnapprovedVariationSell);
            this.RaisePropertyChanged(x => x.ApprovedVariationSell);
            this.RaisePropertyChanged(x => x.TotalSellActualBeforeDataDate);
            this.RaisePropertyChanged(x => x.TotalCostActual);
            this.RaisePropertyChanged(x => x.TotalSellForecastAfterDataDate);
            this.RaisePropertyChanged(x => x.TotalCostForecast);
            this.RaisePropertyChanged(x => x.EACSell);
            this.RaisePropertyChanged(x => x.EACCost);

            base.raiseSummaryChanges();
        }

        public decimal UnapprovedVariationSell
        {
            get
            {
                decimal UnapprovedVariationSell = VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Submitted).Sum(x => x.ManualApprovedEstimatedValue);
                return UnapprovedVariationSell;
            }
        }

        public decimal ApprovedVariationSell
        {
            get
            {
                decimal approvedVariationSell = VARIATION_CONSTRUCTIONCollection.Where(x => x.STATUS == VariationConstructionStatus.Approved).Sum(x => x.ManualApprovedEstimatedValue);
                return approvedVariationSell;
            }
        }
    }
}