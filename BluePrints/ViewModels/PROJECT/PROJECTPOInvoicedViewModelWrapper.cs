using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.Services;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
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
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using BluePrints.Common.Misc;

namespace BluePrints.ViewModels
{
    public class PROJECTPOInvoicedViewModelWrapper  : PROJECTFlatPOSnapshotForecastViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECTPOInvoicedViewModelWrapper  as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTPOInvoicedViewModelWrapper  CreatePOInvoiced(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTPOInvoicedViewModelWrapper (unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTPOInvoicedViewModelWrapper  class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTPOInvoicedViewModelWrapper  type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPOInvoicedViewModelWrapper (
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            Messenger.Default.Register<POFilterMessage>(this, x => OnPOFilterMessage(x));
        }

        #region Database Operations
        protected override void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PONO", Header = "PO Number", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, GroupIndex = 1, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PONO", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.VariationCode", Header = "Variation", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.StockCode", Header = "Stock Code", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Description", Header = "Description", ReadOnly = true, Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Narrative", Header = "Narrative", ReadOnly = true, Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Supplier", Header = "Supplier", ReadOnly = true, Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.FirstActualDate", Header = "First Raised", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.LastUpdated", Header = "PO Hdr Last Updated", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_OrderQuantity", Header = "Total Qty", Mask = "n", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PO_OrderQuantity", DisplayFormat = "n", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_TotalPrice", Header = "Total Cost", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PO_TotalPrice", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_SuppliedQty", Header = "Cut Off Invoiced Qty", Mask = "n", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PO_SuppliedQty", DisplayFormat = "n", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_Invoiced", Header = "Cut Off Invoiced", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PO_Invoiced", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_Quantity", Header = "Cut Off Outstanding Qty", Mask = "n", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PO_Quantity", DisplayFormat = "n", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_RemainingPrice", Header = "Cut Off Outstanding", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PO_RemainingPrice", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = columnComments, Header = "Comments", ReadOnly = false, Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = true, Header = columnFieldName, Mask = "c0", Increment = 1, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }

        protected override bool generateAlignedDataDates()
        {
            if (MainViewModel == null || ForecastStartDate == null)
                return false;

            //since displayentities comes from mainviewmodel it should be populated by now
            DateTime earliestDate = CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection.Count() == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1) : CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.FORECAST_DATE != null).Min(x => (DateTime)x.FORECAST_DATE);
            ForecastEndDate = ActualsCutOffDate;

            DateTime earliestDateBeginningOfMonth = new DateTime(((DateTime)earliestDate).Year, ((DateTime)earliestDate).Month, 1);
            alignedDataDateCollection = ChronologicalHelpers.GenerateEndDatesCollection(earliestDateBeginningOfMonth, ForecastEndDate);

            return true;
        }

        protected override List<POFlatSnapshotLine> getPOFlatLines()
        {
            if (CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection == null)
                return new List<POFlatSnapshotLine>();

            return CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.PO_NUMBER == "106783" && x.STOCK_CODE == "EH-SI-011" && x.VARIATION_CODE == "VAR-005 REMOVAL OF SITE WASTE").GroupBy(x => new { x.PO_NUMBER, x.VARIATION_CODE, x.STOCK_CODE }).Select(group => new POFlatSnapshotLine { PONumber = group.Key.PO_NUMBER, VariationCode = group.Key.VARIATION_CODE, StockCode = group.Key.STOCK_CODE, DataPoints = group.ToList() }).ToList();
        }

        protected override void updateRowPOForecast(List<DateTime> alignedDates, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> cutOffActuals, DateTime cutOffDate, string POno = "", string stockCode = "", string variationCode = "", DataRow PORow = null)
        {
            if(PORow == null && POno != string.Empty)
                PORow = findPORow(POno, stockCode, variationCode);

            if (PORow != null)
            {
                POFlatForecastSnapshotProjection forecast = (POFlatForecastSnapshotProjection)PORow[columnEntity];
                forecast.UpdateForecastPayments(FORECAST_POCollection, cutOffActuals, cutOffDate);

                //reset datarow dates
                foreach (DateTime alignedDate in alignedDates)
                {
                    PORow[alignedDate.ToString(BluePrintsResources.ColumnDateFormat)] = 0;
                }

                foreach (ExoDataPoint forecastActual in forecast.ForecastActuals)
                {
                    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= forecastActual.ActualDate);
                    if (alignedDataDate == null || ((DateTime)alignedDataDate).Year == 1)
                    {
                        refreshDataTable();
                        return;
                    }
                    else
                    {
                        string alignedDateField = ((DateTime)alignedDataDate).ToString(BluePrintsResources.ColumnDateFormat);
                        PORow[alignedDateField] = forecastActual.Costs;
                    }
                }
            }
        }

        protected override void setFilter(DataRowView dataRowView, GridColumn gridColumn)
        {
            if (gridColumn == null || SelectedDataRows == null || SelectedDataRows.Count == 0)
                return;

            DateTime parseEndDate;
            if (DateTime.TryParse(gridColumn.ActualColumnChooserHeaderCaption.ToString(), out parseEndDate))
            {
                POFlatForecastSnapshotProjection entity = (POFlatForecastSnapshotProjection)dataRowView[columnEntity];
                DateTime monthStartDate = new DateTime(parseEndDate.Year, parseEndDate.Month, 1);
                ActualFilterCriteria = CriteriaOperator.Parse("[PO_NUMBER] = " + entity.PONO + " AND [VARIATION_CODE] = '" + entity.VariationCode + "' AND [StockCode] = '" + entity.StockCode + "' AND [TRANSDATE] >= #" + monthStartDate.Year + "-" + monthStartDate.Month + "-" + monthStartDate.Day + "# AND [TRANSDATE] <= #" + parseEndDate.Year + "-" + parseEndDate.Month + "-" + parseEndDate.Day + "#");
                IsHidden = false;
                IsPoDetailsVisible = false;

                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("PO_REMAININGPRICE") || gridColumn.FieldName.ToUpper().Contains("PO_QUANTITY"))
            {
                POFlatForecastSnapshotProjection entity = (POFlatForecastSnapshotProjection)dataRowView[columnEntity];
                POFilterCriteria = CriteriaOperator.Parse("[PO_NUMBER] = '" + entity.PONO + "' AND [VARIATION_CODE] = '" + entity.VariationCode + "' AND [StockCode] = '" + entity.StockCode + "'");
                IsHidden = false;
                IsPoDetailsVisible = true;

                this.RaisePropertyChanged(x => x.PODetail);
                this.RaisePropertyChanged(x => x.POFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("PO_TOTALPRICE") || gridColumn.FieldName.ToUpper().Contains("PO_ORDERQUANTITY"))
            {
                POFlatForecastSnapshotProjection entity = (POFlatForecastSnapshotProjection)dataRowView[columnEntity];
                POFilterCriteria = CriteriaOperator.Parse("[PO_NUMBER] = '" + entity.PONO + "' AND [VARIATION_CODE] = '" + entity.VariationCode + "' AND [StockCode] = '" + entity.StockCode + "'");
                IsHidden = false;
                IsPoDetailsVisible = true;

                this.RaisePropertyChanged(x => x.PODetail);
                this.RaisePropertyChanged(x => x.POFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("PO_INVOICED") || gridColumn.FieldName.ToUpper().Contains("PO_SUPPLIEDQTY"))
            {
                POFlatForecastSnapshotProjection entity = (POFlatForecastSnapshotProjection)dataRowView[columnEntity];
                ActualFilterCriteria = CriteriaOperator.Parse("[PO_NUMBER] = " + entity.PONO + " AND [VARIATION_CODE] = '" + entity.VariationCode + "' AND [StockCode] = '" + entity.StockCode + "' AND [TRANSDATE] <= #" + ActualsCutOffDate.Year + "-" + ActualsCutOffDate.Month + "-" + ActualsCutOffDate.Day + "#");
                IsHidden = false;
                IsPoDetailsVisible = false;

                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }

            this.RaisePropertyChanged(x => x.ActualDetailsVisibility);
            this.RaisePropertyChanged(x => x.PODetailsVisibility);
            this.RaisePropertyChanged(x => x.DateSortIndex);
        }

        protected override void OnClose(CancelEventArgs e)
        {
            try
            {
                Messenger.Default.Unregister(this);
            }
            catch
            {

            }

            base.OnClose(e);
        }

        private void OnPOFilterMessage(POFilterMessage pOFilterMessage)
        {
            if(pOFilterMessage.StockCode == string.Empty)
                MainGridFilterCriteria = CriteriaOperator.Parse("[Entity.PONO] = '" + pOFilterMessage.PONumber + "' AND [Entity.VariationCode] = '" + pOFilterMessage.VariationCode + "'");
            else
                MainGridFilterCriteria = CriteriaOperator.Parse("[Entity.PONO] = '" + pOFilterMessage.PONumber + "' AND [Entity.VariationCode] = '" + pOFilterMessage.VariationCode + "' AND [Entity.StockCode] = '" + pOFilterMessage.StockCode + "'");

            this.RaisePropertyChanged(x => x.MainGridFilterCriteria);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPOInvoicedViewModelWrapper _v1"; }
        }
        #endregion
    }
}