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
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class SUBJOBDashboardViewModelWrapper :
        DashboardViewModelWrapper<SUBJOB, SUBJOB_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of SUBJOB_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SUBJOBDashboardViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new SUBJOBDashboardViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the SUBJOBViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the SUBJOBViewModel type without the POCO proxy factory.
        /// </summary>
        protected SUBJOBDashboardViewModelWrapper()
        {

        }

        #region Database Operation

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private PROJECT_Dashboard projectDashboard;
        protected override void resolveParameters(object parameter)
        {
            projectDashboard = (PROJECT_Dashboard)parameter;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query;
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == projectDashboard.Entity.GUID);
        }

        protected override Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB_Dashboard>>
            specifyMainViewModelProjection()
        {
            return query => SUBJOB_DashboardQueries.Subjob_Dashboard_Summary(query, projectDashboard, SUBAREACollection);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<SUBJOB_Dashboard> entities)
        {
            MainViewModel =
                (CollectionViewModel<SUBJOB, SUBJOB_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)
                mainEntityLoaderDescription.GetViewModel();
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            base.OnMainViewModelLoaded(entities);
            return true;
        }

        public override void OnAfterCompulsoryEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender, bool isBulkRefresh)
        {
            return;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "SUBJOBDashboardViewModelWrapper"; }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                return GetSUBAREACollection();
            }
        }

        public IEnumerable<AREA> GetSUBAREACollection()
        {
            var collection = GetEntities<AREA>();
            if (collection != null)
                collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
            return collection;
        }

        public override string UnifiedValueValidation(SUBJOB_Dashboard projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        #endregion
    }
}