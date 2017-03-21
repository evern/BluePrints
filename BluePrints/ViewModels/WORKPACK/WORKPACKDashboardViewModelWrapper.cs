using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Xpf.Grid;
using BluePrints.Common.ViewModel.Filtering;
using DevExpress.Mvvm;
using System.Linq.Expressions;
using BluePrints.Common;
using BluePrints.Data.Helpers;
using BluePrints.P6EntitiesDataModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using BluePrints.Common.ViewModel.Reporting;
using System.Windows.Threading;
using DevExpress.Xpf.Bars;
using System.ComponentModel;
using BluePrints.Common.Projections;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class WORKPACKDashboardViewModelWrapper :
        DashboardViewModelWrapper<WORKPACK, WORKPACK_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of WORKPACK_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACKDashboardViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new WORKPACKDashboardViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the WORKPACKViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACKViewModel type without the POCO proxy factory.
        /// </summary>
        protected WORKPACKDashboardViewModelWrapper()
        {
        }

        #region Database Operation

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private PROJECT_Dashboard projectDashboard;

        protected override void InitializeParameters(object parameter)
        {
            projectDashboard = (PROJECT_Dashboard) parameter;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == projectDashboard.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == projectDashboard.PROJECT.GUID);
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK_Dashboard>>
            ConstructMainViewModelProjection()
        {
            return query => WORKPACK_DashboardQueries.SummarizeWORKPACKDashboard(query, projectDashboard);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<WORKPACK_Dashboard> entities)
        {
            MainViewModel =
                (CollectionViewModel<WORKPACK, WORKPACK_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)
                mainEntityLoaderDescription.GetViewModel();
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            base.OnMainViewModelLoaded(entities);
            return true;
        }

        public override void OnAfterCompulsoryEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
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
            get { return "WORKPACKDashboardViewModelWrapper"; }
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
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
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