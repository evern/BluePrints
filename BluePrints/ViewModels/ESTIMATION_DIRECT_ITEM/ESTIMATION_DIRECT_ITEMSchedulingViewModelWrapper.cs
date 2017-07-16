using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.Views;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BluePrints.ViewModels
{
    public class ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper :
        BluePrintsEntitiesSchedulingCollectionWrapper<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>, IHaveCanvasWidth
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper());
        }

        #region Database Operation
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        private DEPARTMENT defaultConstructionDEPARTMENT;
        private Data.PHASE defaultConstructionPHASE;

        protected override ProgressType progress_type => ProgressType.Construct;

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, x => defaultConstructionDEPARTMENT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, x => defaultConstructionPHASE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => loadESTIMATION_DIRECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS, ESTIMATION_DIRECT_ITEMProjectionFunc);

            base.InitializeAndLoadEntitiesLoaderDescription();
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == p6_baseline_entity.project_guid);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == p6_baseline_entity.EntityKey);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.SiteDirect);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Where(x => x.STOCK_CODE_TYPE == StockCodeType.Direct).Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == BluePrintsResources.DefaultConstructionDepartment);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.INTERNAL_NUM == BluePrintsResources.WorkpackDefaultConstructionPhase);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID && x.GUID_DEPARTMENT == defaultConstructionDEPARTMENT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID));
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEM>> ESTIMATION_DIRECT_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_ESTIMATION_DIRECT == loadESTIMATION_DIRECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = GetEntities<P6_ASSIGNMENT>();
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Rates_Transformation(query, loaderCollection.GetCollection<RATE>(), STOCK_CODECollection, loaderCollection.GetCollection<COMMODITY_CODE>(), P6_ASSIGNMENTS);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper"; }
        }

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<Data.PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
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
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
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

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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

        public IEnumerable<STOCK_CODE> GlobalSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> ProjectSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                var collection = GetEntities<P6_ASSIGNMENT>();
                return collection;
            }
        }

        public override IEnumerable<ICanAssignP6> Deliverables_Source => DisplayEntities;
        #endregion
    }
}