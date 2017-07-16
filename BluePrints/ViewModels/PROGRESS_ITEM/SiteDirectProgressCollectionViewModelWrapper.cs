using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace BluePrints.ViewModels
{
    public class SiteDirectProgressCollectionViewModelWrapper :
        BluePrintsEntitiesProgressCollectionWrapper
        <ESTIMATION_DIRECT_ITEM, ReportablesDisplay, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of SiteDirectProgressCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SiteDirectProgressCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new SiteDirectProgressCollectionViewModelWrapper());
        }

        #region Database Operations
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        protected override void InitializeParameters(object parameter)
        {
            delayedPROGRESSSavingDispatcher = new DispatcherTimer();
            delayedPROGRESSSavingDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 10);
            delayedPROGRESSSavingDispatcher.Tick += delayedPROGRESSSavingDispatcher_Tick;
            var receiveParameter =
                (OptionalEntitiesParameter<Data.PROJECT, PROGRESS>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadPROGRESS = receiveParameter.GetSecondEntity();

            if (loadPROJECT != null)
                isQueryForLiveStatus = true;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => loadESTIMATION_DIRECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS, ESTIMATION_DIRECT_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);

            base.InitializeAndLoadEntitiesLoaderDescription();
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID_PROJECT).OrderBy(x => x.NUMBER);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEM>> ESTIMATION_DIRECT_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.ESTIMATION_DIRECT.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ReportablesDisplay>> ConstructMainViewModelProjection()
        {
            return query => ProgressQueries.SiteDirectProgressItemTransformation(query.Where(x => x.GUID_ESTIMATION_DIRECT == loadESTIMATION_DIRECT.GUID), loadPROGRESS, PROGRESS_ITEMCollection, COMMODITY_CODECollection, STOCK_CODECollection, RATECollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ReportablesDisplay> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Behavior
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if(changedType == typeof(PROGRESS_ITEM))
            {
                PROGRESS_ITEM newPROGRESSITEM = PROGRESS_ITEMCollection.FirstOrDefault(x => x.GUID == (Guid)key);
                if(newPROGRESSITEM != null)
                {
                    ReportablesDisplay affectedDisplayEntity = getAffectedDisplayEntity(newPROGRESSITEM);
                    if (affectedDisplayEntity != null)
                        affectedDisplayEntity.Update();
                }

                return true;
            }

            return false;
        }

        private ReportablesDisplay getAffectedDisplayEntity(PROGRESS_ITEM newPROGRESS_ITEM)
        {
            foreach (ReportablesDisplay entity in MainViewModel.Entities)
            {
                if(entity.Reportables != null)
                {
                    foreach(DisplayQuantityReportable reportable in entity.Reportables)
                    {
                        if (reportable.OriginalEntityKey == newPROGRESS_ITEM.GUID_ORIBASEITEM)
                        {
                            setReportableNewProgress(reportable, newPROGRESS_ITEM);
                            return entity;
                        }
                    }
                }
                else
                {
                    if (entity.ProgressItem.OriginalEntityKey == newPROGRESS_ITEM.GUID_ORIBASEITEM)
                    {
                        setReportableNewProgress(entity.ProgressItem, newPROGRESS_ITEM);
                        return entity;
                    }
                }
            }

            return null;
        }

        private void setReportableNewProgress(IDeliverable updateEntity, PROGRESS_ITEM newPROGRESS_ITEM)
        {
            IReportable reportableProjection = updateEntity as IReportable;
            ICanSetProgresses setProgressEntity = updateEntity as ICanSetProgresses;
            if(reportableProjection != null && setProgressEntity != null)
            {
                if (reportableProjection.PROGRESS_ITEM_Current == null)
                {
                    setProgressEntity.AppendProgressItem(newPROGRESS_ITEM);
                }
            }
        }

        public override ObservableCollection<ReportablesDisplay> DisplayEntities => base.DisplayEntities;

        /// <summary>
        /// Intercept MainViewModel Saving because bulk or single selective saving is required
        /// </summary>
        public bool OnBeforeEntitySaved(ReportablesDisplay entity)
        {
            if(entity.ProgressItem.ShouldSaveProgress)
            {
                IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = entity.ProgressItem.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                PROGRESS_ITEMSCollectionViewModel.BulkSave(newPRORESS_ITEMS);
            }

            return false;
        }

        private PROGRESS_ITEM createNewPROGRESS_ITEM(Guid originalEntityKey)
        {
            PROGRESS_ITEM savePROGRESS_ITEM = new PROGRESS_ITEM();
            savePROGRESS_ITEM.GUID_ORIBASEITEM = originalEntityKey;
            savePROGRESS_ITEM.GUID_PROGRESS = loadPROGRESS.GUID;
            savePROGRESS_ITEM.EARNED_DATE = loadPROGRESS.DATA_DATE;
            savePROGRESS_ITEM.CREATED = DateTime.Now;

            return savePROGRESS_ITEM;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "SiteDirectProgressCollectionViewModelWrapper"; }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> STOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMCollection
        {
            get
            {
                var collection = GetEntities<ESTIMATION_DIRECT_ITEM>();
                return collection;
            }
        }

        protected override CostGroup cost_group => CostGroup.Site;

        protected override IEnumerable<IReportable> ReportableCollection => MainViewModel == null || MainViewModel.Entities == null ? new ObservableCollection<ESTIMATION_DIRECT_ITEMProgress>() : MainViewModel.Entities.Select(x => x.ProgressItem.Reportable);

        private ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper ESTIMATION_DIRECT_ITEM_scheduling_view_model;
        protected override IEntitiesSchedulingCollectionWrapper scheduling_view_model
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEM_scheduling_view_model == null)
                    ESTIMATION_DIRECT_ITEM_scheduling_view_model = ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper.Create();

                return ESTIMATION_DIRECT_ITEM_scheduling_view_model;
            }
            set => ESTIMATION_DIRECT_ITEM_scheduling_view_model = (ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper)value;
        }

        protected override ProgressType progress_type => ProgressType.Construct;
        #endregion

        #region Disposing
        private void CancelBackgroundWorker()
        {
            if (calculatePlannedBackgroundWorker != null)
                calculatePlannedBackgroundWorker.CancelAsync();
        }
        #endregion
    }
}