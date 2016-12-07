using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Scheduler;
using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using BluePrints.Common;
using DevExpress.Mvvm.POCO;
using BluePrints.Data.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Forms.Integration;
using BluePrints.Views;
using System.Windows.Threading;
using System.ComponentModel;
using BluePrints.Common.Projections;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;

namespace BluePrints.ViewModels
{
    public class PROJECTWORKPACKSMappingViewModelWrapper : CollectionViewModelsWrapper<WORKPACK, WORKPACK_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<WORKPACK, WORKPACK_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTWORKPACKSMappingViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTWORKPACKSMappingViewModelWrapper());
        }

        #region Used as Dependency Delegate
        public Action<IEnumerable<WORKPACK_Dashboard>> OnPROJECTWORKPACKSMappingViewModelLoaded { get; set; }
        bool isFromPROGRESS
        {
            get { return OnPROJECTWORKPACKSMappingViewModelLoaded != null; }
        }
        #endregion

        #region Database Operation
        BluePrints.Data.PROJECT loadPROJECT;
        BluePrints.P6Data.PROJECT loadP6PROJECT;
        PROGRESS loadPROGRESS;
        BASELINE loadBASELINE;
        BaselineMappingSelectionType mappingType;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            object[] obj = (object[])parameter;

            if (isFromPROGRESS)
                loadPROGRESS = (PROGRESS)obj[0];
            else
                loadBASELINE = (BASELINE)obj[0];

            mappingType = (BaselineMappingSelectionType)obj[1];
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, null, isContinueLoadingAfterBASELINE, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BluePrints.Data.PROJECT, BluePrints.Data.PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, typeof(BASELINE), isContinueLoadingAfterPROJECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, typeof(BASELINE), isContinueLoadingAfterPROGRESS, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc, typeof(BASELINE));
            loaderCollection.AddEntitiesLoader<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.WORKPACK_ASSIGNMENTS, WORKPACK_ASSIGNMENTProjectionFunc, typeof(WORKPACK));
            loaderCollection.AddEntitiesLoader<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc, typeof(BASELINE), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(6, bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, typeof(PROGRESS), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(7, bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, typeof(PROGRESS), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BluePrints.P6Data.PROJECT, BluePrints.P6Data.PROJECT, int, IP6EntitiesUnitOfWork>(8, p6UnitOfWorkFactory, x => x.PROJECT, P6PROJECTProjectionFunc, null, isContinueLoadingAfterP6PROJECT, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<TASK, TASK, int, IP6EntitiesUnitOfWork>(9, p6UnitOfWorkFactory, x => x.TASK, P6TASKProjectionFunc, typeof(BluePrints.P6Data.PROJECT), null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROJWBS, PROJWBS, int, IP6EntitiesUnitOfWork>(10, p6UnitOfWorkFactory, x => x.PROJWBS, PROJWBSProjectionFunc, typeof(BluePrints.P6Data.PROJECT));
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "BASELINE"))));
                return false;
            }

            this.loadBASELINE = entities.First();
            return true;
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<BluePrints.Data.PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        bool isContinueLoadingAfterP6PROJECT(IEnumerable<BluePrints.P6Data.PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "P6PROJECT"))));
                return false;
            }

            this.loadP6PROJECT = entities.First();
            return true;
        }

        bool isContinueLoadingAfterPROGRESS(IEnumerable<PROGRESS> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROGRESS"))));
                return false;
            }

            this.loadPROGRESS = entities.First();
            return true;
        }

        Func<IRepositoryQuery<BluePrints.Data.PROJECT>, IQueryable<BluePrints.Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == this.loadBASELINE.GUID_PROJECT);
        }

        Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            if (isFromPROGRESS)
                return query => query.Where(x => x.GUID == loadPROGRESS.GUID);
            else
                return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            if (isFromPROGRESS)
                return query => query.Where(x => x.GUID_PROJECT == loadPROGRESS.GUID_PROJECT && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadBASELINE.GUID);
        }

        Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<WORKPACK_ASSIGNMENT>, IQueryable<WORKPACK_ASSIGNMENT>> WORKPACK_ASSIGNMENTProjectionFunc()
        {
            return query => query.Where(x => x.WORKPACK.GUID_PROJECT == loadPROJECT.GUID && x.ISMODIFIEDBASELINE == (mappingType == BaselineMappingSelectionType.Modified));
        }

        Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID);
        }

        Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        Func<IRepositoryQuery<BluePrints.P6Data.PROJECT>, IQueryable<BluePrints.P6Data.PROJECT>> P6PROJECTProjectionFunc()
        {
            string projectName;
            if (isFromPROGRESS)
                projectName = loadPROGRESS.P6PROGRESS_NAME;
            else if (mappingType == BaselineMappingSelectionType.Modified)
                projectName = loadBASELINE.P6MODBASELINE_NAME;
            else
                projectName = loadBASELINE.P6BASELINE_NAME;

            return query => query.Where(x => x.proj_short_name == projectName);
        }

        Func<IRepositoryQuery<TASK>, IQueryable<TASK>> P6TASKProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id);
        }

        Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> PROJWBSProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.WORKPACKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK_Dashboard>> ConstructMainViewModelProjection()
        {
            Func<BASELINE> getBASELINEFunc = loaderCollection.GetObjectFunc<BASELINE>();
            Func<PROGRESS> getPROGRESSFunc = loaderCollection.GetObjectFunc<PROGRESS>();
            Func<IQueryable<BASELINE_ITEM>> getBASELINE_ITEMSFunc = loaderCollection.GetCollectionFunc<BASELINE_ITEM>();
            Func<IQueryable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            Func<IQueryable<RATE>> getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();

            return query => WORKPACK_DashboardQueries.MappingWORKPACKDashboard(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), getPROGRESSFunc, getBASELINEFunc, getBASELINE_ITEMSFunc, getPROGRESS_ITEMSFunc, getRATESFunc, mappingType == BaselineMappingSelectionType.Modified);
        }

        public Action<Func<IQueryable<TASK>>, Func<IQueryable<PROJWBS>>, Func<IQueryable<WORKPACK_Dashboard>>, CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>, bool> windowsFormHostViewInitialization { get; set; }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<WORKPACK_Dashboard> entities)
        {
            CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTCollectionViewModel = (CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK_ASSIGNMENT>();
            
            if (this.isFromPROGRESS)
                mainThreadDispatcher.BeginInvoke(new Action(() => OnPROJECTWORKPACKSMappingViewModelLoaded(entities)));
            else
                mainThreadDispatcher.BeginInvoke(new Action(() => windowsFormHostViewInitialization(loaderCollection.GetCollectionFunc<TASK>(), loaderCollection.GetCollectionFunc<PROJWBS>(), () => entities.AsQueryable(), WORKPACK_ASSIGNMENTCollectionViewModel, mappingType == BaselineMappingSelectionType.Modified)));
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
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
            get
            {
                return "PROJECTWORKPACKSMappingViewModelWrapper";
            }
        }

        public IEnumerable<TASK> P6TASKCollection
        {
            get
            {
                var collection = GetEntities<TASK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.task_name);
                return collection;
            }
        }

        public CollectionViewModel<TASK, int, IP6EntitiesUnitOfWork> P6TASKCollectionViewModel
        {
            get
            {
                return (CollectionViewModel<TASK, int, IP6EntitiesUnitOfWork>)loaderCollection.GetViewModel<TASK>();
            }
        }
        #endregion
    }
}
