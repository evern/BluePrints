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

namespace BluePrints.ViewModels
{
    public class PROJECTFlatPOSnapshotForecastViewModelWrapper : PROJECTPOSnapshotForecastViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECTFlatPOSnapshotForecastViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTFlatPOSnapshotForecastViewModelWrapper CreateFlatPOForecast(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTFlatPOSnapshotForecastViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTFlatPOSnapshotForecastViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTFlatPOSnapshotForecastViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTFlatPOSnapshotForecastViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        protected override void loadExoData(IPrimeroEntitiesUnitOfWork primeroUOW)
        {
            isExoDataLoaded = false;
            Common.LoadingScreenManager.SetMessage("Loading PO Details...");
            X_PURCHORD_LINE_DETAILS = PrimeroEntities.GetPurchaseOrdersDetail(primeroUnitOfWork, loadPROJECT.NUMBER, ActualsCutOffDate);
            Common.LoadingScreenManager.CloseLoadingScreen();

            generateAlignedDataDates();
            isExoDataLoaded = true;
        }

        protected override void updateDataPointsTable()
        {
            GridControlService.BeginDataUpdate();

            //generate aligned dates
            if (alignedDataDateCollection == null || alignedDataDateCollection.Count == 0)
                return;

            //initialize view source
            InitializeColumnSource(ColumnDescriptors, SummaryDescriptors, alignedDataDateCollection);

            //initialize datatable schema
            dataPointsTable = new DataTable();
            dataPointsTable.Columns.Add(columnEntity, typeof(POFlatForecastSnapshotProjection));
            dataPointsTable.Columns.Add(columnComments, typeof(string));

            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            //construction projection from grouped po lines
            List<POFlatSnapshotLine> POFlatLines = getPOFlatLines();
            List<POFlatForecastSnapshotProjection> projections = new List<POFlatForecastSnapshotProjection>();
            LoadingScreenManager.ShowLoadingScreen(POFlatLines.Count() + 1);
            LoadingScreenManager.SetMessage("Loading PO Snapshot...");
            foreach (POFlatSnapshotLine POFlatLine in POFlatLines.OrderBy(x => x.PONumber))
            {
                POFlatForecastSnapshotProjection newFlatForecastProjection = new POFlatForecastSnapshotProjection();
                newFlatForecastProjection.PONO = POFlatLine.PONumber;
                newFlatForecastProjection.CurrentPOSnapshots = POFlatLine.DataPoints;
                newFlatForecastProjection.VariationCode = POFlatLine.VariationCode;
                newFlatForecastProjection.StockCode = POFlatLine.StockCode;

                IEnumerable<X_PURCHORD_LINE_DETAIL> purchaseOrderDetails = PODetail.Where(x => x.PO_NUMBER.ToString() == POFlatLine.PONumber && x.STOCKCODE == POFlatLine.StockCode);
                if (purchaseOrderDetails.Count() > 0)
                {
                    newFlatForecastProjection.Description = purchaseOrderDetails.First().DESCRIPTION;
                    newFlatForecastProjection.Narrative = purchaseOrderDetails.First().NARRATIVE;
                    newFlatForecastProjection.LastUpdated = purchaseOrderDetails.First().LAST_UPDATED;
                    newFlatForecastProjection.Supplier = purchaseOrderDetails.First().SUPPLIER_NAME;
                    newFlatForecastProjection.FirstActualDate = purchaseOrderDetails.Min(x => x.ORDERDATE);
                }

                projections.Add(newFlatForecastProjection);
                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.ResetCurrentProgress();
            LoadingScreenManager.SetMaxProgress(projections.Count());
            LoadingScreenManager.SetMessage("Loading PO Forecast...");
            //gets the forecasted data into dates bucket in the row and adds to datatable
            foreach (POFlatForecastSnapshotProjection projection in projections)
            {
                DataRow newRow = DataPointsTable.NewRow();
                newRow[columnEntity] = projection;
                updateRowPOForecast(alignedDataDateCollection, Entities, CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection, ActualsCutOffDate, projection.PONO, projection.StockCode, projection.VariationCode, newRow);

                //populate comment
                FORECAST_PO_SETTING forecastPOSetting = FORECAST_PO_SETTINGCollection.FirstOrDefault(x => x.PONO == projection.PONO && x.VARIATION_CODE == projection.VariationCode && x.STOCK_CODE == projection.StockCode);
                if (forecastPOSetting != null)
                    newRow[columnComments] = forecastPOSetting.PO_COMMENTS;

                dataPointsTable.Rows.Add(newRow);
                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            //TableViewService.ScrollToLast();
            GridControlService.EndDataUpdate();
        }

        protected override void InitializeColumnSource(ObservableCollection<ColumnDescriptor> columns, ObservableCollection<SummaryDescriptor> summaries, List<DateTime> alignedDates)
        {
            columns.Clear();
            summaries.Clear();

            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PONO", Header = "PO Number", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.PONO", DisplayFormat = "{0} Record(s)", Type = SummaryItemType.Count });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.VariationCode", Header = "Variation", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.StockCode", Header = "Stock Code", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Description", Header = "Description", ReadOnly = true, Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Narrative", Header = "Narrative", ReadOnly = true, Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Supplier", Header = "Supplier", ReadOnly = true, Fixed = FixedStyle.Left, Width = 150, Settings = SettingsType.Default });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.InvoicedLastMonth", Header = "Invoiced Prior Month", HeaderToolTip = "Invoice amount of month before cut off", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.FirstActualDate", Header = "First Raised", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.LastUpdated", Header = "PO Hdr Last Updated", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            //columns.Add(new ColumnDescriptor() { FieldName = "Entity.FirstInvoiceDate", Header = "First Invoiced", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Date });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_OrderQuantity", Header = "Total Qty", Mask = "n", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_TotalPrice", Header = "Total Cost", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_SuppliedQty", Header = "Cut Off Invoiced Qty", Mask = "n", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_Invoiced", Header = "Cut Off Invoiced", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_Quantity", Header = "Cut Off Outstanding Qty", Mask = "n", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.PO_RemainingPrice", Header = "Cut Off Outstanding", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.TotalForecast", Header = "Forecasted", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Number });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.TotalForecast", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = "Entity.Unforecasted", Header = "Not Forecasted", Mask = "c", ReadOnly = true, Fixed = FixedStyle.Left, Width = 70, Settings = SettingsType.Unforecasted });
            summaries.Add(new SummaryDescriptor() { FieldName = "Entity.Unforecasted", DisplayFormat = "c", Type = SummaryItemType.Sum });
            columns.Add(new ColumnDescriptor() { FieldName = columnComments, Header = "Comments", ReadOnly = false, Fixed = FixedStyle.Left, Width = 200, Settings = SettingsType.Default });

            foreach (DateTime alignedDate in alignedDates.OrderBy(x => x))
            {
                string columnFieldName = alignedDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                columns.Add(new ColumnDescriptor() { FieldName = columnFieldName, ReadOnly = false, Header = columnFieldName, Mask = "c0", Increment = 1, Fixed = FixedStyle.None, Width = 60, Settings = SettingsType.ForecastFuture });
                summaries.Add(new SummaryDescriptor() { FieldName = columnFieldName, DisplayFormat = "c0", Type = SummaryItemType.Sum });
            }
        }

        protected override void findExistingOrAddNewFORECAST_JOB_SETTING(DataRow updateRow, string comments)
        {
            POFlatForecastSnapshotProjection forecast = ((POFlatForecastSnapshotProjection)updateRow[columnEntity]);    
            FORECAST_PO_SETTING relevantFORECAST_PO_SETTING = FORECAST_PO_SETTINGCollection.FirstOrDefault(x => x.PONO == forecast.PONO && x.STOCK_CODE == forecast.StockCode && x.VARIATION_CODE == forecast.VariationCode);
            if (relevantFORECAST_PO_SETTING == null)
            {
                FORECAST_PO_SETTING newFORECAST_PO_SETTING = new FORECAST_PO_SETTING();
                newFORECAST_PO_SETTING.GUID_PROJECT = loadPROJECT.GUID;
                newFORECAST_PO_SETTING.PONO = forecast.PONO;
                newFORECAST_PO_SETTING.VARIATION_CODE = forecast.VariationCode;
                newFORECAST_PO_SETTING.STOCK_CODE = forecast.StockCode;

                if (forecast.VariationCode != null && forecast.VariationCode != string.Empty)
                    newFORECAST_PO_SETTING.VARIATION_CODE = forecast.VariationCode;
                else
                    newFORECAST_PO_SETTING.VARIATION_CODE = string.Empty;

                relevantFORECAST_PO_SETTING = newFORECAST_PO_SETTING;
            }

            relevantFORECAST_PO_SETTING.PO_COMMENTS = comments;
            FORECAST_PO_SETTINGCollectionViewModel.Save(relevantFORECAST_PO_SETTING);
            forecast.Comments = comments;
        }

        private bool generateAlignedDataDates()
        {
            if (MainViewModel == null || ForecastStartDate == null)
                return false;

            //since displayentities comes from mainviewmodel it should be populated by now
            DateTime latestDate = Entities.Count == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1) : Entities.Max(x => x.FORECAST_DATE);
            if (latestDate > ForecastEndDate)
                ForecastEndDate = latestDate;

            DateTime earliestDateBeginningOfMonth = new DateTime(((DateTime)ForecastStartDate).Year, ((DateTime)ForecastStartDate).Month, 1);
            alignedDataDateCollection = ChronologicalHelpers.GenerateEndDatesCollection(earliestDateBeginningOfMonth, ForecastEndDate);

            return true;
        }

        private List<POFlatSnapshotLine> getPOFlatLines()
        {
            if (CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection == null)
                return new List<POFlatSnapshotLine>();

            string equipmentHireStockCodeInitials = BluePrintsResources.EquipmentHireStockCodeInitials;
            return CurrentPO_FORECAST_JOB_HOUR_SNAPSHOTCollection.Where(x => x.STOCK_CODE.StartsWith(equipmentHireStockCodeInitials)).GroupBy(x => new { x.PO_NUMBER, x.VARIATION_CODE, x.STOCK_CODE }).Select(group => new POFlatSnapshotLine { PONumber = group.Key.PO_NUMBER, VariationCode = group.Key.VARIATION_CODE, StockCode = group.Key.STOCK_CODE, DataPoints = group.ToList() }).ToList();
        }

        protected override void clearPOForecast(string poNo, string stockCode, string variationCode)
        {
            List<FORECAST_PO> removePOForecasts = Entities.Where(x => x.PONO == poNo && x.STOCK_CODE == stockCode && x.VARIATION_CODE == variationCode).ToList();
            MainViewModel.BaseBulkDelete(removePOForecasts);
            shouldPromptForSavingSnapshot = true;
        }

        protected override void findExistingOrAddNewFORECAST_PO(DataRow dataRow, DateTime forecastDate, decimal? viewCosts, bool skipUpdating = false)
        {
            POFlatForecastSnapshotProjection entity = (POFlatForecastSnapshotProjection)dataRow[columnEntity];

            //each PO have multiple items, so we need to store the pro-rated value per PO items in the database
            decimal proRateOnPOItem = 1;
            if (entity.PO_RemainingPrice > 0)
                proRateOnPOItem = (decimal)viewCosts / entity.PO_RemainingPrice;

            var groupByCodesPOItems = entity.CurrentPOSnapshots.GroupBy(g => new { PONumber = g.PO_NUMBER, JobCode = g.SUBJOB_CODE, DisciplineCode = g.DISCIPLINE_CODE, CommodityCode = g.COMMODITY_CODE, g.STOCK_CODE, VariationCode = g.VARIATION_CODE }).Select(g => new { g.Key.PONumber, g.Key.JobCode, g.Key.DisciplineCode, g.Key.CommodityCode, g.Key.STOCK_CODE, g.Key.VariationCode, RemainingCosts = g.Sum(x => x.FORECAST_COST) }).ToList();
            decimal cumulativeTrueProRateValue = 0;

            for(int i = 0;i < groupByCodesPOItems.Count;i++)
            {
                var groupByCodesPOItem = groupByCodesPOItems[i];
                FORECAST_PO findFORECAST_PO = Entities.FirstOrDefault(x => x.FORECAST_DATE == forecastDate.Date && x.PONO == groupByCodesPOItem.PONumber && x.COMMODITY_CODE == groupByCodesPOItem.CommodityCode && x.DISCIPLINE_CODE == groupByCodesPOItem.DisciplineCode && x.STOCK_CODE == groupByCodesPOItem.STOCK_CODE && x.VARIATION_CODE == groupByCodesPOItem.VariationCode && x.JOB_CODE == groupByCodesPOItem.JobCode);

                if (findFORECAST_PO == null)
                {
                    findFORECAST_PO = new FORECAST_PO();
                    findFORECAST_PO.GUID = Guid.Empty;
                }

                findFORECAST_PO.GUID_PROJECT = loadPROJECT.GUID;
                findFORECAST_PO.PONO = groupByCodesPOItem.PONumber;
                findFORECAST_PO.JOB_CODE = groupByCodesPOItem.JobCode;
                findFORECAST_PO.DISCIPLINE_CODE = groupByCodesPOItem.DisciplineCode;
                findFORECAST_PO.COMMODITY_CODE = groupByCodesPOItem.CommodityCode;
                findFORECAST_PO.STOCK_CODE = groupByCodesPOItem.STOCK_CODE == null ? "" : groupByCodesPOItem.STOCK_CODE;
                findFORECAST_PO.VARIATION_CODE = groupByCodesPOItem.VariationCode;
                findFORECAST_PO.FORECAST_DATE = new DateTime(forecastDate.Year, forecastDate.Month, forecastDate.Day);
                if (viewCosts == null || ((decimal)viewCosts) == 0.00m)
                    findFORECAST_PO.FORECAST_VALUE = null;
                else
                {
                    decimal trueProRateValue = groupByCodesPOItem.RemainingCosts * proRateOnPOItem;
                    decimal viewCostDecimal = (decimal)viewCosts;
                    cumulativeTrueProRateValue += trueProRateValue;
                    //when it's the last item but the figures doesn't match what user's has keyed in
                    if (i == groupByCodesPOItems.Count - 1)
                    {
                        if (cumulativeTrueProRateValue < viewCostDecimal)
                            trueProRateValue += (viewCostDecimal - cumulativeTrueProRateValue);
                    }

                    findFORECAST_PO.FORECAST_VALUE = trueProRateValue;
                }

                MainViewModel.Save(findFORECAST_PO);
                shouldPromptForSavingSnapshot = true;
            }

            if(!skipUpdating)
                updateRowPOForecast(alignedDataDateCollection, Entities, CutoffActual_FORECAST_JOB_HOUR_SNAPSHOTCollection, ActualsCutOffDate, string.Empty, string.Empty, string.Empty, dataRow);
        }

        private void updateRowPOForecast(List<DateTime> alignedDates, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<FORECAST_JOB_HOUR_SNAPSHOT> cutOffActuals, DateTime cutOffDate, string POno = "", string stockCode = "", string variationCode = "", DataRow PORow = null)
        {
            if(PORow == null && POno != string.Empty)
                PORow = findPORow(POno, stockCode, variationCode);

            if (PORow != null)
            {
                POFlatForecastSnapshotProjection forecast = (POFlatForecastSnapshotProjection)PORow[columnEntity];
                forecast.UpdateForecastPayments(FORECAST_POCollection, cutOffActuals, cutOffDate);

                //reset datarow dates
                foreach (DateTime alignedDate in alignedDataDateCollection)
                {
                    PORow[alignedDate.ToString(BluePrintsResources.ColumnDateFormat)] = 0;
                }

                foreach (ExoDataPoint forecastPayment in forecast.ForecastPayments)
                {
                    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= forecastPayment.ActualDate);
                    if (alignedDataDate == null || ((DateTime)alignedDataDate).Year == 1)
                    {
                        refreshDataTable();
                        return;
                    }
                    else
                    {
                        string alignedDateField = ((DateTime)alignedDataDate).ToString(BluePrintsResources.ColumnDateFormat);
                        PORow[alignedDateField] = forecastPayment.Costs;
                    }
                }
            }
        }

        public override void AlignPOsWithActuals()
        {
            EntitiesUndoRedoManager.Clear();
            IEnumerable<POFlatForecastSnapshotProjection> projections = from DataRow dr in dataPointsTable.Rows
                                                            select (POFlatForecastSnapshotProjection)dr[columnEntity];

            List<FORECAST_PO> saveFORECAST_POs = new List<FORECAST_PO>();
            //fix codes mis-alignment
            LoadingScreenManager.ShowLoadingScreen(projections.Count());
            LoadingScreenManager.SetMessage("Aligning Actuals...");
            //fix dates mis-alignment
            foreach (POFlatForecastSnapshotProjection projection in projections)
            {
                LoadingScreenManager.Progress();
                DataRow editing_row = findPORow(projection.PONO, projection.StockCode, projection.VariationCode);
                decimal totalForecastValue = projection.FORECAST_POs.Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                if (totalForecastValue == 0)
                {
                    if (editing_row != null)
                    {
                        findExistingOrAddNewFORECAST_PO(editing_row, (DateTime)ForecastStartDate, projection.PO_RemainingPrice);
                    }
                }
                else
                {
                    foreach (FORECAST_PO FORECAST_PO in projection.FORECAST_POs.OrderBy(x => x.FORECAST_DATE))
                    {
                        //need to pro-rate costs by WBS
                        decimal wbsRemainingCosts = projection.CurrentPOSnapshots.Where(x => x.SUBJOB_CODE == FORECAST_PO.JOB_CODE && x.DISCIPLINE_CODE == FORECAST_PO.DISCIPLINE_CODE && x.COMMODITY_CODE == FORECAST_PO.COMMODITY_CODE && x.STOCK_CODE == FORECAST_PO.STOCK_CODE).Sum(x => x.FORECAST_COST);
                        //forecast POs already filtered by variation code
                        decimal wbsForecastCosts = projection.FORECAST_POs.Where(x => x.JOB_CODE == FORECAST_PO.JOB_CODE && x.DISCIPLINE_CODE == FORECAST_PO.DISCIPLINE_CODE && x.COMMODITY_CODE == FORECAST_PO.COMMODITY_CODE && x.STOCK_CODE == FORECAST_PO.STOCK_CODE).Where(x => x.FORECAST_DATE.Date > ActualsCutOffDate.Date && x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                        decimal wbsCostDifference = wbsRemainingCosts - wbsForecastCosts;

                        if (FORECAST_PO.FORECAST_DATE.Date <= ActualsCutOffDate.Date)
                        {
                            //store as 0 so that when we rewind and adjust actuals again this point will actually be used
                            FORECAST_PO.FORECAST_VALUE = 0.00m;
                            saveFORECAST_POs.Add(FORECAST_PO);

                            //when the previous date is adjusted as 0 and no existing record to move unforecasted amount anymore, default to adding forecast amount to forecast start date
                            if (projection.FORECAST_POs.Where(x => x.FORECAST_VALUE != null).Sum(x => x.FORECAST_VALUE) == 0)
                            {
                                findExistingOrAddNewFORECAST_PO(editing_row, (DateTime)ForecastStartDate, projection.PO_RemainingPrice);
                                //no point to continue since the rest will be zero
                                break;
                            }

                            continue;
                        }

                        //cost adjustment
                        if (wbsCostDifference > 0)
                        {
                            FORECAST_PO.FORECAST_VALUE += wbsCostDifference;
                            saveFORECAST_POs.Add(FORECAST_PO);
                        }
                        else if (wbsCostDifference < 0)
                        {
                            decimal forecastValue = FORECAST_PO.FORECAST_VALUE == null ? 0 : (decimal)FORECAST_PO.FORECAST_VALUE;
                            decimal postAdjustmentCosts = forecastValue + wbsCostDifference;
                            FORECAST_PO.FORECAST_VALUE += wbsCostDifference;
                            saveFORECAST_POs.Add(FORECAST_PO);
                        }
                    }
                }
            }

            LoadingScreenManager.CloseLoadingScreen();
            LastAlignedDate = DateTime.Now;
            MainViewModel.BaseBulkSave(saveFORECAST_POs);
            SaveSnapshot();
            shouldPromptForSavingSnapshot = false;
            refreshDataTable();
        }

        private DataRow findPORow(string PONumber, string stockCode, string variationCode)
        {
            return (from DataRow dr in dataPointsTable.Rows
                    where ((POFlatForecastSnapshotProjection)dr[columnEntity]).PONO == PONumber && ((POFlatForecastSnapshotProjection)dr[columnEntity]).VariationCode == variationCode && ((POFlatForecastSnapshotProjection)dr[columnEntity]).StockCode == stockCode
                    select dr).FirstOrDefault();
        }
        
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTFlatPOSnapshotForecastViewModelWrapper_v2"; }
        }
        #endregion
    }
}