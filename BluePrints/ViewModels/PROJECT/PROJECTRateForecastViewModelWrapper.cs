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
            mainThreadDispatcher.BeginInvoke(new Action(() => loadDataPointsTable()));
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

        public IEnumerable<ExoDataPoint> ActualsDetail => AllActuals;
        protected override string strTotalCosts => "Total Sell $ Excl. Variation";
        protected override string strTotalQty => "Total Sell Qty";

        protected override void raiseSummaryChanges()
        {
            this.RaisePropertyChanged(x => x.TotalRevenue);
            this.RaisePropertyChanged(x => x.RevisedRevenue);
            this.RaisePropertyChanged(x => x.EACRevenue);
            this.RaisePropertyChanged(x => x.UnapprovedVariationSell);
            this.RaisePropertyChanged(x => x.ApprovedVariationSell);
            this.RaisePropertyChanged(x => x.TotalSellActual);
            this.RaisePropertyChanged(x => x.TotalCostActual);
            this.RaisePropertyChanged(x => x.TotalSellForecast);
            this.RaisePropertyChanged(x => x.TotalCostForecast);
            this.RaisePropertyChanged(x => x.EACSell);
            this.RaisePropertyChanged(x => x.EACCost);
            this.RaisePropertyChanged(x => x.Margin);
            this.RaisePropertyChanged(x => x.MarginPercentage);

            base.raiseSummaryChanges();
        }

        //summaries
        public decimal TotalRevenue
        {
            get
            {
                decimal totalRevenue = 0;
                if(DataPointsTable != null)
                    foreach(DataRow row in DataPointsTable.Rows)
                    {
                        if (!row.IsNull(columnTotalForecastSellFromProjectStart))
                            totalRevenue += (decimal)row[columnTotalForecastSellFromProjectStart];
                    }

                return totalRevenue;
            }
        }

        public decimal RevisedRevenue => TotalRevenue + ApprovedVariationSell;

        public decimal EACRevenue => RevisedRevenue + UnapprovedVariationSell;

        public decimal TotalSellActual
        {
            get
            {
                decimal totalSellActual = 0;
                if (DataPointsTable != null)
                    foreach (DataRow row in DataPointsTable.Rows)
                    {
                        if (!row.IsNull(columnTotalActualSellCosts))
                            totalSellActual += (decimal)row[columnTotalActualSellCosts];
                    }

                return totalSellActual;
            }
        }

        public decimal TotalCostActual
        {
            get
            {
                decimal totalCostActual = 0;
                if (DataPointsTable != null)
                    foreach (DataRow row in DataPointsTable.Rows)
                    {
                        if (!row.IsNull(columnTotalActualCosts))
                            totalCostActual += (decimal)row[columnTotalActualCosts];
                    }

                return totalCostActual;
            }
        }

        public decimal TotalSellForecast
        {
            get
            {
                decimal totalSellForecast = 0;
                if (DataPointsTable != null)
                    foreach (DataRow row in DataPointsTable.Rows)
                    {
                        if (!row.IsNull(columnTotalForecastSellCosts))
                            totalSellForecast += (decimal)row[columnTotalForecastSellCosts];
                    }

                return totalSellForecast;
            }
        }

        public decimal TotalCostForecast
        {
            get
            {
                decimal totalCostForecast = 0;
                if (DataPointsTable != null)
                    foreach (DataRow row in DataPointsTable.Rows)
                    {
                        if (!row.IsNull(columnTotalForecastCosts))
                            totalCostForecast += (decimal)row[columnTotalForecastCosts];
                    }

                return totalCostForecast;
            }
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

        public decimal EACSell => TotalSellActual + TotalSellForecast;

        public decimal EACCost => TotalCostActual + TotalCostForecast;

        public decimal Margin => EACRevenue - TotalCostActual;

        public decimal MarginPercentage => EACRevenue == 0 ? 0 : Margin / EACRevenue;
    }
}