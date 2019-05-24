using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class PROJECTPOForecastViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <FORECAST_PO, FORECAST_PO, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTPOForecastViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTPOForecastViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTPOForecastViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTPOForecastViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTPOForecastViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPOForecastViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected PROJECT loadPROJECT;
        List<DateTime> alignedDataDateCollection;
        List<ExoDataPoint> exoPOs = new List<ExoDataPoint>();
        //List<ExoDataPoint> exoMaterials = new List<ExoDataPoint>();
        List<string> hiddenColumnFieldNames = new List<string>();
        protected string columnEntity = "Entity";
        DispatcherTimer selectedItemsChangedDispatcher;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            hiddenColumnFieldNames.Add(columnEntity);

            selectedItemsChangedDispatcher = new DispatcherTimer();
            selectedItemsChangedDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PO_CUSTOMDATES, PO_CUSTOMDATEProjectionFunc);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProgressStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.STATUS);
        }

        private Func<IRepositoryQuery<PO_CUSTOMDATE>, IQueryable<PO_CUSTOMDATE>> PO_CUSTOMDATEProjectionFunc()
        {
            return query => query.Where(x => x.FORECAST_PO.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.FORECAST_POS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<FORECAST_PO>, IQueryable<FORECAST_PO>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_PO> entities)
        {
            exoPOs = BluePrintsDataUtils.GetEXOPO(loadPROJECT.NUMBER);
            //exoMaterials = BluePrintsDataUtils.GetMaterials(loadPROJECT.NUMBER);
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(FORECAST_PO projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(FORECAST_PO projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        DataTable dataPointsTable = null;
        public virtual DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || exoPOs == null)
                    return null;

                if (dataPointsTable == null)
                {
                    dataPointsTable = new DataTable();
                    dataPointsTable.Columns.Add(columnEntity, typeof(POForecastProjection));
                    var groupedPOs = exoPOs.GroupBy(x => x.PONumber).Select(group => new { PONumber = group.Key, DataPoints = group.ToList() });

                    List<POForecastProjection> projections = new List<POForecastProjection>();
                    foreach(var groupedPO in groupedPOs.OrderBy(x => x.PONumber))
                    {
                        POForecastProjection newForecast = new POForecastProjection();
                        newForecast.PONO = groupedPO.PONumber;
                        ExoDataPoint dataPoint = groupedPO.DataPoints.FirstOrDefault();
                        if (dataPoint != null)
                            newForecast.Description = dataPoint.Description;

                        newForecast.ExoPOs = groupedPO.DataPoints;
                        //newForecast.ExoActuals = exoMaterials.Where(x => x.PONumber == groupedPO.PONumber).ToList();

                        FORECAST_PO forecastConfig = DisplayEntities.FirstOrDefault(x => x.PONO == groupedPO.PONumber);
                        if(forecastConfig != null)
                        {
                            newForecast.SetForecastConfig(forecastConfig);
                            if (forecastConfig.MODE == POPaymentTerms.Custom)
                            {
                                newForecast.CustomPaymentDates = PO_CUSTOMDATECollection.Where(x => x.FORECAST_PO_GUID == forecastConfig.GUID).ToList();
                            }
                        }

                        projections.Add(newForecast);
                    }

                    DateTime earliestDate = DateTime.Now;
                    IEnumerable<ExoDataPoint> forecastPayments = projections.SelectMany(x => x.ForecastPayments);
                    DateTime latestDate = earliestDate;
                    if (forecastPayments.Count() > 0)
                        latestDate = forecastPayments.Max(x => x.ActualDate);

                    DateTime earliestDateBeginningOfMonth = new DateTime(earliestDate.Year, earliestDate.Month, 1);
                    alignedDataDateCollection = ChronologicalHelpers.GenerateMonthEndDatesCollection(earliestDateBeginningOfMonth, latestDate);

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach (POForecastProjection projection in projections)
                    {
                        DataRow newRow = DataPointsTable.NewRow();
                        newRow[columnEntity] = projection;
                        generatePOForecast(projection, alignedDataDateCollection, newRow);
                        dataPointsTable.Rows.Add(newRow);
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }

        public void AutoGeneratingColumns(AutoGeneratingColumnEventArgs e)
        {
            if (hiddenColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                e.Cancel = true;
            }
            else
            {
                GridControl gridControl = (GridControl)e.Source;
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;
                    GridControlService.AddSummary(e.Column.FieldName, SummaryItemType.Sum, "c0");
                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
            }
        }

        public void ModeCellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new POForecastProjection().PaymentTerms)))
            {
                TableView tableView = e.Source as TableView;
                tableView.PostEditor();
            }
        }

        public void ValidateCell(GridCellValidationEventArgs e)
        {
            if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new POForecastProjection().PaymentTerms)))
            {
                DataRowView dataRowView = (DataRowView)e.Row;
                POForecastProjection projection = (POForecastProjection)dataRowView[columnEntity];

                //create empty entry so that custom payment date have something to attach to
                if (projection.ForecastConfig == null)
                {
                    FORECAST_PO newFORECAST_PO = new FORECAST_PO();
                    newFORECAST_PO.PONO = projection.PONO;
                    newFORECAST_PO.MODE = POPaymentTerms.None;
                    newFORECAST_PO.GUID_PROJECT = loadPROJECT.GUID;
                    projection.SetForecastConfig(newFORECAST_PO);
                    MainViewModel.Save(projection.ForecastConfig);
                }

                if (((POPaymentTerms)e.Value) == POPaymentTerms.Custom)
                {
                    //don't change the value if custom payment isn't populated
                    if (!showCustomPaymentDialog(projection))
                    {
                        e.IsValid = false;
                        e.ErrorContent = "Edit payment cancelled";
                        return;
                    }
                }

                projection.ForecastConfig.MODE = (POPaymentTerms)e.Value;
                projection.PaymentTerms = (POPaymentTerms)e.Value;

                MainViewModel.Save(projection.ForecastConfig);
                generatePOForecast(projection, alignedDataDateCollection);
                GridControlService.RefreshData();
                TableView tableView = e.Source as TableView;
                tableView.CloseEditor();
            }
        }

        /// <summary>
        /// Show dialog to allow user to input custom dates and percentage for a PO
        /// </summary>
        /// <param name="projection">Custom dates</param>
        /// <returns>User clicks ok</returns>
        private bool showCustomPaymentDialog(POForecastProjection projection)
        {
            PO_CUSTOMDATECollectionViewModelWrapper POCustomDateViewModel = PO_CUSTOMDATECollectionViewModelWrapper.Create();
            POCustomDateViewModel.OnParameterChange(projection.ForecastConfig);
            if (CustomPODialogService.ShowDialog(MessageButton.OKCancel, "Assign payment dates and percentages", "PO_CUSTOMDATECollectionView", POCustomDateViewModel) == MessageResult.OK)
            {
                projection.CustomPaymentDates = PO_CUSTOMDATECollection.Where(x => x.FORECAST_PO_GUID == projection.ForecastConfig.GUID).ToList();
                decimal totalPercent = projection.CustomPaymentDates.Sum(x => x.PAYMENT_PERCENT);
                if (totalPercent < 1 || totalPercent > 1)
                {
                    MessageBoxService.ShowMessage("Please make sure % is 100%");
                    return showCustomPaymentDialog(projection);
                }
                else
                {
                    POCustomDateViewModel.Dispose();
                    return true;
                }
            }
            else
            {
                POCustomDateViewModel.Dispose();
                return false;
            }
        }

        private void generatePOForecast(POForecastProjection forecast, List<DateTime> alignedDates, DataRow PORow = null)
        {
            if(PORow == null)
                PORow = findPORow(forecast.PONO);

            if (PORow != null)
            {
                forecast.ResetPaymentDates();
                //reset datarow dates
                foreach(DateTime alignedDate in alignedDataDateCollection)
                {
                    PORow[alignedDate.ToShortDateString()] = 0;
                }

                foreach (ExoDataPoint forecastPayment in forecast.ForecastPayments)
                {
                    DateTime? alignedDataDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x.Date >= forecastPayment.ActualDate);
                    if (alignedDataDate == null || ((DateTime)alignedDataDate).Year == 1)
                        refreshDataTable();
                    else
                    {
                        string alignedDateField = ((DateTime)alignedDataDate).ToShortDateString();
                        PORow[alignedDateField] = forecastPayment.Costs;
                    }
                }
            }
        }

        private void refreshDataTable()
        {
            dataPointsTable = null;
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        private DataRow findPORow(string PONumber)
        {
                return (from DataRow dr in dataPointsTable.Rows
                        where ((POForecastProjection)dr[columnEntity]).PONO == PONumber
                        select dr).FirstOrDefault();
        }

        public IEnumerable<PO_CUSTOMDATE> PO_CUSTOMDATECollection
        {
            get
            {
                var collection = GetEntities<PO_CUSTOMDATE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.PAYMENT_DATE);
                }

                return collection;
            }
        }

        public override void FullRefresh()
        {
            dataPointsTable = null;
            base.FullRefresh();
        }

        ObservableCollection<DataRowView> selectedDataRows { get; set; }
        public ObservableCollection<DataRowView> SelectedDataRows
        {
            get
            {
                if (selectedDataRows == null)
                {
                    selectedDataRows = new ObservableCollection<DataRowView>();
                    selectedDataRows.CollectionChanged += SelectedDataRows_CollectionChanged;
                }

                return selectedDataRows;
            }
            set
            {
                selectedDataRows = value;
            }
        }

        private void SelectedDataRows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(x => x.PODetails);
            //selectedItemsChangedDispatcher.Tick -= SelectedItemsChangedDispatcher_Tick;
            //selectedItemsChangedDispatcher.Tick += SelectedItemsChangedDispatcher_Tick;
            //selectedItemsChangedDispatcher.Start();
        }

        private void SelectedItemsChangedDispatcher_Tick(object sender, EventArgs e)
        {
            selectedItemsChangedDispatcher.Stop();
            this.RaisePropertyChanged(x => x.PODetails);
        }

        public IEnumerable<ExoDataPoint> PODetails
        {
            get
            {
                foreach(var selectedDataRow in SelectedDataRows)
                {
                    POForecastProjection projection = (POForecastProjection)selectedDataRow[columnEntity];
                    foreach(var po in projection.ExoPOs)
                    {
                        yield return po;
                    }
                }
            }
        }

        protected IDialogService CustomPODialogService
        {
            get { return this.GetRequiredService<IDialogService>("CustomPODialogService"); }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPOForecastViewModelWrapper"; }
        }

        #endregion
    }
}