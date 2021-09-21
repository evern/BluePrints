using BaseModel.DataModel;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class PROJECTPLCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <JOBCOST_HDR, JOBCOST_HDR, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTPLCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTPLCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTPLCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the PROJECTPLCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTPLCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPLCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<CLIENT_PROJECT, CLIENT_PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.CLIENT_PROJECTS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR);
        }

        protected override Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.JOBCODE.Length == 5 && x.STATUS == "C");
        }


        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<JOBCOST_HDR> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Saving Behavior
        public override string UnifiedRowValidation(JOBCOST_HDR projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(JOBCOST_HDR projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties

        //protected override bool loadDataPointsTable()
        //{
        //    IsLoading = true;
        //    this.RaisePropertyChanged(x => x.IsLoading);

        //    dataPointsTable = null;
        //    commodityJobs = null;

        //    updateDataPointsTable();
        //    this.RaisePropertyChanged(x => x.DataPointsTable);

        //    IsLoading = false;
        //    this.RaisePropertyChanged(x => x.IsLoading);

        //    //so filters will show transactions, as it is not shown during load, RaisePropertyChanged on ActualDetails will allow the grid to start showing data
        //    //instantFeedbackActualDetailViewModel.OnParameterChange(LoadPROJECT);
        //    //CommonMethods.AddSaveLayoutHandler(GridControlService.GetGridColumns());

        //    return true;
        //}

        //DataTable dataPointsTable = null;
        //List<ForecastJobData> commodityJobs = null;
        //public virtual DataTable DataPointsTable
        //{
        //    get
        //    {
        //        return dataPointsTable;
        //    }
        //}

        //private void updateDataPointsTable()
        //{
        //    dataPointsTable = new DataTable();
        //    GridControlService.GridControl.BeginDataUpdate();

        //    //get immutable data
        //    List<ExoDataPoint> allDataPoints = new List<ExoDataPoint>();
        //    if (AllProjectDashboards != null)
        //    {
        //        IEnumerable<Stats> actualStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
        //        IEnumerable<Stats> materialStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
        //        IEnumerable<Stats> poStats = AllProjectDashboards.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);

        //        allDataPoints.AddRange(actualStats.SelectMany(x => x.ExoDataPoints));
        //        allDataPoints.AddRange(materialStats.SelectMany(x => x.ExoDataPoints));
        //        allDataPoints.AddRange(poStats.SelectMany(x => x.ExoDataPoints));
        //    }

        //    if (allDataPoints.Count == 0)
        //        firstDataPointsDate = DateTime.Now;
        //    else
        //    {
        //        DateTime fixedDate = (DateTime)FixedDataDate;
        //        firstDataPointsDate = allDataPoints.Where(x => x.ActualDate.Year > fixedDate.Year - 10).Min(x => x.ActualDate);
        //    }

        //    alignedDataDateCollection = generateDates();
        //    InitializeColumnSource(ParentViewColumns, ParentSummaries, alignedDataDateCollection, false);
        //    InitializeColumnSource(ChildViewColumns, ChildSummaries, alignedDataDateCollection, true);

        //    bool isNewData = false;
        //    if (commodityJobs == null)
        //    {
        //        List<ExoSubJobProjection> unifiedJobList = ForecastHelper.ConstructUnifiedJobList(queryJobLines, COMMODITY_CODECollection, allDataPoints, JOB_COSTTYPESCollection, ShowLoadingScreen, AllProjectDashboards);
        //        //update discipline description a.k.a package no
        //        foreach (ExoDataPoint dataPoint in allDataPoints)
        //        {
        //            dataPoint.PopulateDisciplineDesc(DISCIPLINE_DESCCollection, JOB_COSTGROUPCollection);
        //        }

        //        DetailedData.AddRange(allDataPoints);

        //        commodityJobs = ForecastHelper.CreateCommodityProjections(unifiedJobList, queryJobLines, AllProjectDashboards, FORECAST_POCollection, FORECAST_EACCollection, FORECAST_EACPreviousCommitmentCollection, FORECAST_JOBCollection, FORECAST_JOB_SETTINGCollection, COMMODITY_CODECollection, alignedDataDateCollection, (DateTime)FixedDataDate, isWeeks, ShowLoadingScreen);
        //        isNewData = true;
        //    }

        //    //construct data points table
        //    dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobData));
        //    dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));
        //    foreach (DateTime alignedDataDate in alignedDataDateCollection)
        //    {
        //        string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
        //        dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
        //    }

        //    if (ShowLoadingScreen)
        //    {
        //        LoadingScreenManager.ShowLoadingScreen(1);
        //        LoadingScreenManager.SetMessage("Caching Forecast...");
        //    }

        //    //loads the forecast upfront so that data table doesn't have to query the database for each commodity
        //    List<FORECAST> cacheFORECAST = FORECASTCollection.ToList();
        //    LoadingScreenManager.CloseLoadingScreen();

        //    if (ShowLoadingScreen)
        //    {
        //        LoadingScreenManager.ShowLoadingScreen(commodityJobs.Count);
        //        LoadingScreenManager.SetMessage("Preparing View...");
        //    }

        //    dataPointsTable.BeginInit();
        //    //child data table is used to record original value of actuals + committed + remaining values before it is overridden by forecasts
        //    foreach (ForecastJobData commodityJob in commodityJobs)
        //    {
        //        ForecastHelper.PopulateEAC(commodityJob, FORECAST_EACCollection, PreviousEACDataDate);
        //        ForecastHelper.PopulateTenderBudget(commodityJob, FORECAST_EACTenderBudgetCollection);
        //        updateAdditionalJobInfo(commodityJob);

        //        DataRow commodityRow = updateDataTable(commodityJob, isNewData, cacheFORECAST);
        //        LoadingScreenManager.Progress();
        //    }
        //    dataPointsTable.EndInit();

        //    GridControlService.GridControl.EndDataUpdate();
        //    LoadingScreenManager.CloseLoadingScreen();

        //    ForecastSummary.Reset();

        //    //calculate project summary, needs to be done after uncommitted is calculated
        //    ForecastSummary.Budget_Cost = commodityJobs.Sum(x => x.Budget);
        //    ForecastSummary.Current_Cost = commodityJobs.Sum(x => x.ActualCosts);
        //    ForecastSummary.Commitments = commodityJobs.Sum(x => x.Outstanding);
        //    ForecastSummary.Uncommitted_Forecast = commodityJobs.Sum(x => x.Uncommitted);
        //    ForecastSummary.OriginalEstimateAtCompletion = commodityJobs.Sum(x => x.OriginalEstimateAtCompletion);
        //    ForecastSummary.EstimateAtCompletion = commodityJobs.Sum(x => x.EstimateAtCompletion);
        //    ForecastSummary.CurrentEstimateAtCompletion = commodityJobs.Sum(x => x.CurrentEstimateAtCompletion);
        //    ForecastSummary.Contingency = commodityJobs.Where(x => x.IsContingency).Sum(x => x.EstimateAtCompletion);

        //    this.RaisePropertyChanged(x => x.ForecastSummary);
        //    this.RaisePropertyChanged(x => x.ExportTable);
        //}
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPLCollectionViewModelWrapper_v2"; }
        }

        public IEnumerable<CLIENT_PROJECT> CLIENT_PROJECTCollection
        {
            get
            {
                var collection = GetEntities<CLIENT_PROJECT>();
                if (collection == null)
                    return new List<CLIENT_PROJECT>();

                return collection;
            }
        }
        #endregion
    }
}