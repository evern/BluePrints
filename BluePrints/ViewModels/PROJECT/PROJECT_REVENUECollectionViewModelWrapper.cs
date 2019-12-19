using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class PROJECT_REVENUECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PROJECT_REVENUE, PROJECT_REVENUEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECT_REVENUECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECT_REVENUECollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECT_REVENUECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECT_REVENUECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECT_REVENUECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECT_REVENUECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT loadPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;

        List<ExoDataPoint> actualDataPoints;
        List<ExoDataPoint> materialDataPoints;
        List<ExoDataPoint> revenueDataPoints;

        //indicate whether projection transformation should run first loaded or refreshed
        bool isFirstLoaded;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();

            loadExoData();
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REVENUES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT_REVENUE>, IQueryable<PROJECT_REVENUEProjection>> specifyMainViewModelProjection()
        {
            return query => projectRevenueProjectionTransformation(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
        }

        private void loadExoData()
        {
            actualDataPoints = BluePrintsDataUtils.GetBurned(primeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, null, 1, true);
            materialDataPoints = BluePrintsDataUtils.GetMaterials(primeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, 1, true);
            revenueDataPoints = BluePrintsDataUtils.GetRevenue(primeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, 1, true);
        }

        private IQueryable<PROJECT_REVENUEProjection> projectRevenueProjectionTransformation(IQueryable<PROJECT_REVENUE> query)
        {
            List<PROJECT_REVENUEProjection> projections = new List<PROJECT_REVENUEProjection>();

            if(!isFirstLoaded)
            {
                List<DateTime> earliestDates = new List<DateTime>();
                DateTime firstRecordedActualDate = actualDataPoints.Min(x => x.ActualDate);
                DateTime firstRecordedMaterialDate = materialDataPoints.Min(x => x.ActualDate);
                DateTime firstRecordedRevenueDate = revenueDataPoints.Min(x => x.ActualDate);
                earliestDates.Add(firstRecordedActualDate);
                earliestDates.Add(firstRecordedMaterialDate);
                earliestDates.Add(firstRecordedRevenueDate);
                DateTime earliestDate = earliestDates.Min(x => x);
                earliestDate = new DateTime(earliestDate.Year, earliestDate.Month, 1);

                DateTime latestDate = loadPROJECT.FORECAST_END_DATE == null ? DateTime.Now : (DateTime)loadPROJECT.FORECAST_END_DATE;
                latestDate = new DateTime(latestDate.Year, latestDate.Month, 1);

                DateTime loopDate = earliestDate;
                do
                {
                    PROJECT_REVENUE findRevenue = query.FirstOrDefault(x => x.REVENUE_MONTH.Year == loopDate.Year && x.REVENUE_MONTH.Month == loopDate.Month);
                    PROJECT_REVENUEProjection newPROJECT_REVENUEProjection = new PROJECT_REVENUEProjection(loopDate, revenueDataPoints, actualDataPoints, materialDataPoints, findRevenue);
                    projections.Add(newPROJECT_REVENUEProjection);
                    loopDate = loopDate.AddMonths(1);
                } while (loopDate <= latestDate);
            }
            else
            {
                PROJECT_REVENUE editedRevenue = query.FirstOrDefault();
                if(editedRevenue != null)
                {
                    PROJECT_REVENUEProjection findProjection = DisplayEntities.FirstOrDefault(x => x.DisplayMonth.Year == editedRevenue.REVENUE_MONTH.Year && x.DisplayMonth.Month == editedRevenue.REVENUE_MONTH.Month);
                    if (findProjection != null)
                    {
                        findProjection.Entity = editedRevenue;
                        findProjection.Update();
                        projections.Add(findProjection);
                    }
                    else
                    {
                        //when message comes from another machine (Needs to be tested)
                        findProjection = new PROJECT_REVENUEProjection(editedRevenue.REVENUE_MONTH, revenueDataPoints, actualDataPoints, materialDataPoints, editedRevenue);
                        DisplayEntities.Add(findProjection);
                    }
                }
            }

            return projections.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT_REVENUEProjection> entities)
        {
            isFirstLoaded = true;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(PROJECT_REVENUEProjection projection)
        {
            projection.Entity.REVENUE_MONTH = new DateTime(projection.DisplayMonth.Year, projection.DisplayMonth.Month, 1);
            projection.Entity.REVENUE_PRICE = projection.GetNewEntityRevenuePrice();
            projection.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }

        public override string UnifiedValueValidation(PROJECT_REVENUEProjection projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(PROJECT_REVENUEProjection projection)
        {
            return string.Empty;
        }

        public void CustomSummary(CustomSummaryEventArgs e)
        {
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                decimal totalRevenue = 0;
                decimal eacRevenue = 0;
                if (revenueDataPoints != null)
                    totalRevenue = revenueDataPoints.Sum(x => x.Costs);

                if (loadPROJECT != null && loadPROJECT.EAC_REVENUE != null)
                    eacRevenue = (decimal)loadPROJECT.EAC_REVENUE;

                GridSummaryItem gridSummaryItem = e.Item as GridSummaryItem;
                bool isEAC = gridSummaryItem.ShowInColumn == BindableBase.GetPropertyName(() => new PROJECT_REVENUEProjection().DisplayMonth);
                if (!isEAC)
                    e.TotalValue = totalRevenue - eacRevenue;
                else
                    e.TotalValue = eacRevenue;
            }
        }

        public override void FullRefresh()
        {
            loadExoData();
            isFirstLoaded = false;
            base.FullRefresh();
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECT_REVENUECollectionViewModelWrapper"; }
        }

        #endregion
    }
}