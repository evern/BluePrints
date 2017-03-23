using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTViewModelWrapper :
        DashboardViewModelWrapper<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>,
        ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTViewModelWrapper()
        {
        }

        #region Database Operation

        private PROJECT loadPROJECT;
        public Action<BASELINECollectionViewModelWrapper> AssignBASELINEDelegates;
        public Action<PROGRESSCollectionViewModelWrapper> AssignPROGRESSDelegates;
        public Action<ESTIMATION_DIRECTCollectionViewModelWrapper> AssignESTIMATION_DIRECTDelegates;
        public Action<PHASECollectionViewModelWrapper> AssignPHASEDelegates;
        public Action<AREACollectionViewModelWrapper> AssignAREADelegates;
        public Action<RATECollectionViewModelWrapper> AssignRATEDelegates;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter =
                (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == BaselineStatus.Live && x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return
                query =>
                    query.Where(
                        x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT_Dashboard>>
            ConstructMainViewModelProjection()
        {
            var getBASELINESFunc = loaderCollection.GetCollectionFunc<BASELINE>();
            var getPROGRESSESFunc = loaderCollection.GetCollectionFunc<PROGRESS>();
            var getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            var getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            var getVARIATIONSFunc = loaderCollection.GetCollectionFunc<VARIATION>();
            var getDELIVERABLES_STATUSESFunc = loaderCollection.GetCollectionFunc<DELIVERABLES_STATUS>();

            return
                query =>
                    PROJECT_DashboardQueries.SummarizePROJECTDashboard(query, getPROGRESSESFunc, getPROGRESS_ITEMSFunc,
                        getBASELINESFunc, getRATESFunc, getDELIVERABLES_STATUSESFunc, getVARIATIONSFunc, () => RaisePropertyChanged(),
                        loadPROJECT.GUID);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel =
                (CollectionViewModel<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)
                mainEntityLoaderDescription.GetViewModel();

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);

            DisplaySelectedEntities_CollectionChanged();
            
            base.OnMainViewModelLoaded(entities);
            return true;
        }
        #endregion

        #region View Behavior

        public Action Redraw;

        public void RaisePropertyChanged()
        {
            if (Redraw != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => Redraw()));

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        #endregion

        #region View Properties
        private BASELINECollectionViewModelWrapper baselineViewModel;

        public BASELINECollectionViewModelWrapper BASELINEViewModel
        {
            get
            {
                if (baselineViewModel == null && loadPROJECT != null)
                {
                    baselineViewModel = BASELINECollectionViewModelWrapper.Create();
                    baselineViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = baselineViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignBASELINEDelegates?.Invoke(baselineViewModel);
                }

                return baselineViewModel;
            }
        }

        private PROGRESSCollectionViewModelWrapper progressViewModel;

        public PROGRESSCollectionViewModelWrapper PROGRESSViewModel
        {
            get
            {
                if (progressViewModel == null && loadPROJECT != null)
                {
                    progressViewModel = PROGRESSCollectionViewModelWrapper.Create();
                    progressViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = progressViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignPROGRESSDelegates?.Invoke(progressViewModel);
                }

                return progressViewModel;
            }
        }

        private AREACollectionViewModelWrapper areaViewModel;

        public AREACollectionViewModelWrapper AREAViewModel
        {
            get
            {
                if (areaViewModel == null && loadPROJECT != null)
                {
                    areaViewModel = AREACollectionViewModelWrapper.Create();
                    areaViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = areaViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignAREADelegates?.Invoke(areaViewModel);
                }

                return areaViewModel;
            }
        }

        private RATECollectionViewModelWrapper rateViewModel;

        public RATECollectionViewModelWrapper RATEViewModel
        {
            get
            {
                if (rateViewModel == null && loadPROJECT != null)
                {
                    rateViewModel = RATECollectionViewModelWrapper.Create();
                    rateViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = rateViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignRATEDelegates?.Invoke(rateViewModel);
                }

                return rateViewModel;
            }
        }

        private PHASECollectionViewModelWrapper phaseViewModel;

        public PHASECollectionViewModelWrapper PHASEViewModel
        {
            get
            {
                if (phaseViewModel == null && loadPROJECT != null)
                {
                    phaseViewModel = PHASECollectionViewModelWrapper.Create();
                    phaseViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = phaseViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignPHASEDelegates?.Invoke(phaseViewModel);
                }

                return phaseViewModel;
            }
        }

        private ESTIMATION_DIRECTCollectionViewModelWrapper estimationDirectViewModel;

        public ESTIMATION_DIRECTCollectionViewModelWrapper ESTIMATION_DIRECTViewModel
        {
            get
            {
                if (estimationDirectViewModel == null && loadPROJECT != null)
                {
                    estimationDirectViewModel = ESTIMATION_DIRECTCollectionViewModelWrapper.Create();
                    estimationDirectViewModel.SetParentViewModel(this);
                    var baselineSupportParameterObj = estimationDirectViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(loadPROJECT);
                    AssignESTIMATION_DIRECTDelegates?.Invoke(estimationDirectViewModel);
                }

                return estimationDirectViewModel;
            }
        }

        public bool CanEditReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        public override void FullRefresh()
        {
            InitializeAndLoadEntitiesLoaderDescription();
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<WORKPACK_Dashboard, Guid>(this, DisplaySelectedEntity.GUID, string.Empty);
        }


        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROJECTViewModelWrapper"; }
        }

        #endregion

        #region ISupportCustomDocumentTypeNameAndParameter

        public string GetCustomDocumentTypeName()
        {
            return "WORKPACKDashboardView";
        }

        public object GetCustomDocumentParameter()
        {
            return SummaryEntity;
        }

        public string GetCustomDocumentTitle()
        {
            return SummaryEntity.PROJECT.NUMBER + " - WORKPACKS";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}