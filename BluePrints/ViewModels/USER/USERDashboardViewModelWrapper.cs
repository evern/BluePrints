using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class USERDashboardViewModelWrapper :
        DashboardViewModelWrapper<BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static USERDashboardViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new USERDashboardViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected USERDashboardViewModelWrapper()
        {
        }

        #region Database Operation
        private USER _loadUSER;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        ActionObject actionObject;
        protected override void resolveParameters(object parameter)
        {
            actionObject = parameter as ActionObject;
            if(actionObject == null)
            {
                var USERParameter = (EntitiesParameter<USER>)parameter;
                _loadUSER = USERParameter.GetEntity();
            }
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProjectStatus.Active);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            if (actionObject != null)
            {
                actionObject.ExecuteAction();
                return;
            }

            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.User_OffsiteDirectProgressItemTransformation(query, PROGRESS_ITEMCollection, _loadUSER);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            MainViewModel =
                (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>)
                mainEntityLoaderDescription.GetViewModel();
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);

            return base.OnMainViewModelLoaded(entities);
        }
        #endregion

        #region View Behavior
        public override void FullRefresh()
        {
            ReloadEntitiesCollection();
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProgress projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        #endregion

        #region View Properties
        public IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection
        {
            get
            {
                var collection = GetEntities<DELIVERABLES_STATUS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.MAX_PERCENTAGE);
                return collection;
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "USERDashboardViewModelWrapper"; }
        }
        #endregion
    }
}