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

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTViewModelWrapper : DashboardViewModelWrapper<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportCustomDocumentTypeNameAndParameter
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
        PROJECT loadPROJECT;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            EntitiesParameter<BluePrints.Data.PROJECT> PROJECTParameter = (EntitiesParameter<BluePrints.Data.PROJECT>)parameter;
            this.loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc, null, null, OnAfterEntitiesChanged);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.PROGRESS.STATUS == ProgressStatus.Live && x.PROGRESS.PROJECT.GUID == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT_Dashboard>> ConstructMainViewModelProjection()
        {
            Func<IQueryable<BASELINE>> getBASELINESFunc = loaderCollection.GetCollectionFunc<BASELINE>();
            Func<IQueryable<PROGRESS>> getPROGRESSESFunc = loaderCollection.GetCollectionFunc<PROGRESS>();
            Func<IQueryable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc = loaderCollection.GetCollectionFunc<PROGRESS_ITEM>();
            Func<IQueryable<RATE>> getRATESFunc = loaderCollection.GetCollectionFunc<RATE>();
            Func<IQueryable<VARIATION>> getVARIATIONSFunc = loaderCollection.GetCollectionFunc<VARIATION>();

            return query => PROJECT_DashboardQueries.SummarizePROJECTDashboard(query, getPROGRESSESFunc, getPROGRESS_ITEMSFunc, getBASELINESFunc, getRATESFunc, getVARIATIONSFunc, () => this.RaisePropertyChanged(), true);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            MainViewModel = (CollectionViewModel<PROJECT, PROJECT_Dashboard, Guid, IBluePrintsEntitiesUnitOfWork>)mainEntityLoader.GetViewModel();
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
            MainViewModel.SetParentViewModel(this);

            base.OnMainViewModelLoaded(entities);
            return true;
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if ((sender == MainViewModel && messageType != EntityMessageType.Added) || sender == this || changedType == typeof(PROJECT))
                return;

            if (MainViewModel != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
            else
                mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
        }
        #endregion

        #region View Behavior
        public Action Redraw;

        public void RaisePropertyChanged()
        {
            if (Redraw != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => this.Redraw()));

            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }
        #endregion

        #region View Properties
        BASELINECollectionViewModelWrapper baselineViewModel;
        public BASELINECollectionViewModelWrapper BASELINEViewModel
        {
            get
            {
                if (baselineViewModel == null && this.loadPROJECT != null)
                {
                    baselineViewModel = BASELINECollectionViewModelWrapper.Create();
                    baselineViewModel.SetParentViewModel(this);
                    ISupportParameter baselineSupportParameterObj = baselineViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(this.loadPROJECT);
                }

                return baselineViewModel;
            }
        }

        PROGRESSCollectionViewModelWrapper progressViewModel;
        public PROGRESSCollectionViewModelWrapper PROGRESSViewModel
        {
            get
            {
                if (progressViewModel == null && this.loadPROJECT != null)
                {
                    progressViewModel = PROGRESSCollectionViewModelWrapper.Create();
                    progressViewModel.SetParentViewModel(this);
                    ISupportParameter baselineSupportParameterObj = progressViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(this.loadPROJECT);
                }

                return progressViewModel;
            }
        }

        AREACollectionViewModelWrapper areaViewModel;
        public AREACollectionViewModelWrapper AREAViewModel
        {
            get
            {
                if (areaViewModel == null && this.loadPROJECT != null)
                {
                    areaViewModel = AREACollectionViewModelWrapper.Create();
                    areaViewModel.SetParentViewModel(this);
                    ISupportParameter baselineSupportParameterObj = areaViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(this.loadPROJECT);
                }

                return areaViewModel;
            }
        }

        RATECollectionViewModelWrapper rateViewModel;
        public RATECollectionViewModelWrapper RATEViewModel
        {
            get
            {
                if (rateViewModel == null && this.loadPROJECT != null)
                {
                    rateViewModel = RATECollectionViewModelWrapper.Create();
                    rateViewModel.SetParentViewModel(this);
                    ISupportParameter baselineSupportParameterObj = rateViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(this.loadPROJECT);
                }

                return rateViewModel;
            }
        }

        PHASECollectionViewModelWrapper phaseViewModel;
        public PHASECollectionViewModelWrapper PHASEViewModel
        {
            get
            {
                if (phaseViewModel == null && this.loadPROJECT != null)
                {
                    phaseViewModel = PHASECollectionViewModelWrapper.Create();
                    phaseViewModel.SetParentViewModel(this);
                    ISupportParameter baselineSupportParameterObj = phaseViewModel as ISupportParameter;
                    baselineSupportParameterObj.Parameter = new EntitiesParameter<PROJECT>(this.loadPROJECT);
                }

                return phaseViewModel;
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

        public bool CanEdit(PROJECT_Dashboard entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService { get { return this.GetService<IDocumentManagerService>(); } }
        public void Edit(PROJECT_Dashboard entity)
        {
            if (entity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<WORKPACK_Dashboard, Guid>(this, entity.GUID, string.Empty);
        }


        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "PROJECTViewModelWrapper";
            }
        }
        #endregion

        #region ISupportCustomDocumentTypeNameAndParameter
        public string GetCustomDocumentTypeName()
        {
            return "WORKPACKDashboardView";
        }

        public object GetCustomDocumentParameter()
        {
            return this.SummaryEntity;
        }

        public string GetCustomDocumentTitle()
        {
            return this.SummaryEntity.PROJECT.NUMBER + " - WORKPACKS";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}
