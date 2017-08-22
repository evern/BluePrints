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
        BluePrintsEntitiesSchedulingCollectionWrapper<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, IHaveCanvasWidth
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
        private DEPARTMENT defaultConstructionDEPARTMENT;
        private Data.PHASE defaultConstructionPHASE;

        protected override ProgressType progress_type => ProgressType.Construct;

        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.cleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, assign_default_construction_department);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, assign_default_construction_phase);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, assign_estimation);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_GROUPS, STOCK_GROUPProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);

            base.initializeEntitiesLoadersDescription();
        }

        private void assign_default_construction_department(DEPARTMENT entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Construction department not found, please add " + BluePrintsResources.Default_Construction_Department.ToString() + " in department view")));

            defaultConstructionDEPARTMENT = entity;
        }

        private void assign_default_construction_phase(BluePrints.Data.PHASE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Site phase not found, please add " + BluePrintsResources.Default_Construction_Phase.ToString() + " in phase view")));

            defaultConstructionPHASE = entity;
        }

        private void assign_estimation(ESTIMATION_DIRECT entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live estimation not found")));

            p6_baseline_entity = entity;
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            if (isFromPROGRESS)
                return query => query.Where(x => x.GUID == live_PROGRESS.GUID_PROJECT);
            else
                return query => query.Where(x => x.GUID == p6_baseline_entity.project_guid);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            if (isFromPROGRESS)
                return query => query.Where(x => x.GUID_PROJECT == live_PROGRESS.GUID_PROJECT && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == p6_baseline_entity.EntityKey);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Where(x => x.STOCK_CODE_TYPE == StockCodeType.Direct).Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == BluePrintsResources.Default_Construction_Department);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.INTERNAL_NUM == BluePrintsResources.Default_Construction_Phase);
        }

        private Func<IRepositoryQuery<STOCK_GROUP>, IQueryable<STOCK_GROUP>> STOCK_GROUPProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID));
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.GUID_DEPARTMENT == defaultConstructionDEPARTMENT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = GetEntities<P6_ASSIGNMENT>();
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Progress_Transformation(query.Where(x => x.GUID_ESTIMATION_DIRECT == p6_baseline_entity.EntityKey), loadPROJECT, loaderCollection.GetCollection<RATE>(), live_PROGRESS, PROGRESS_ITEMCollection, STOCK_CODECollection, loaderCollection.GetCollection<STOCK_GROUP>(), null, true, P6_ASSIGNMENTS);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper" + view_project_specific_affix; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<STOCK_GROUP> STOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUP> ProjectSTOCK_GROUPCollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return STOCK_GROUPCollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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