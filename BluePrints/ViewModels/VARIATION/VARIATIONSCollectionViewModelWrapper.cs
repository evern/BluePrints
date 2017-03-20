using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.DataModel;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class VARIATIONSCollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork>>,
        ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATIONSCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new VARIATIONSCollectionViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected VARIATIONSCollectionViewModelWrapper()
        {
        }

        #region Database Operation

        private PROJECT loadPROJECT;
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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        //private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        //{
        //    return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        //}

        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            if (LiveBASELINE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_BASELINE == LiveBASELINE.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NAME);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION> entities)
        {
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            MainViewModel.IsContinueSaveCallBack = BeforeSaveValidation;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region CallBacks
        public bool BeforeSaveValidation(VARIATION entity, bool isNewEntity)
        {
            if (LiveBASELINE == null)
                return false;

            return true;
        }

        public void OnBeforeEntitySaved(VARIATION entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;

            if (entity.APPROVED != null)
                entity.GUID_ORIBASELINE = entity.GUID_ORIBASELINE ?? LiveBASELINE.GUID;
            else
                entity.GUID_ORIBASELINE = null;
        }
        #endregion

        #endregion

        #region Variation_Item revision
        private VARIATION_ITEMSCollectionViewModelWrapper variation_itemsViewModelWrapper;

        public VARIATION_ITEMSCollectionViewModelWrapper VARIATION_ITEMSViewModelWrapper(VARIATION loadVARIATION)
        {
            if (variation_itemsViewModelWrapper == null && loadPROJECT != null)
            {
                variation_itemsViewModelWrapper = VARIATION_ITEMSCollectionViewModelWrapper.Create();
                variation_itemsViewModelWrapper.SuppressNotification = true;
                variation_itemsViewModelWrapper.SetParentViewModel(this);
                variation_itemsViewModelWrapper.OnEntitiesLoadedCallBack = OnVARIATION_ITEMSLoaded;
                var baselineSupportParameterObj = variation_itemsViewModelWrapper as ISupportParameter;
                baselineSupportParameterObj.Parameter = new OptionalEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, loadVARIATION);
            }

            return variation_itemsViewModelWrapper;
        }

        public void CleanUpVARIATION_ITEMS()
        {
            if (variation_itemsViewModelWrapper != null)
                variation_itemsViewModelWrapper.CleanUpEntitiesLoader();

            variation_itemsViewModelWrapper = null;
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "VARIATIONSViewModelWrapper"; }
        }

        public CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINEViewModel
        {
            get
            {
                return (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            }
        }

        public CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEMSViewModel
        {
            get
            {
                return (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE_ITEM>();
            }
        }

        public BASELINE LiveBASELINE
        {
            get
            {
                if (BASELINECollection == null || BASELINECollection.Count() == 0)
                    return null;
                else
                    return BASELINECollection.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
            }
        }

        public PROGRESS LivePROGRESS
        {
            get
            {
                if (PROGRESSCollection == null || PROGRESSCollection.Count() == 0)
                    return null;
                else
                    return PROGRESSCollection.FirstOrDefault(x => x.STATUS == ProgressStatus.Live);
            }
        }

        public IEnumerable<BASELINE> BASELINECollection
        {
            get
            {
                var collection = GetEntities<BASELINE>();
                return collection;
            }
        }

        public IEnumerable<PROGRESS> PROGRESSCollection
        {
            get
            {
                var collection = GetEntities<PROGRESS>();
                return collection;
            }
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<VARIATION_ITEM, Guid>(this, DisplaySelectedEntity.GUID, string.Empty);
        }

        /// <summary>
        /// Determines whether an entities can be Submitd
        /// Since CollectionViewModelBase is a POCO view model, this method will be used as a CanExecute callback for SubmitCommand.
        /// </summary>
        /// <param name="projectionEntity">Entities to Submit.</param>
        public bool CanSubmit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            if (LiveBASELINE == null)
                return false;

            if (DisplaySelectedEntity.SUBMITTED != null)
                return false;

            if (DisplaySelectedEntity != null && DisplaySelectedEntity.APPROVED != null)
                return false;

            return true;
        }

        /// <summary>
        /// Submits an entity.
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the SubmitCommand property that can be used as a binding source in views.
        /// </summary>
        /// <param name="projectionEntity">An entity to Submit.</param>
        public void Submit()
        {
            DisplaySelectedEntity.SUBMITTED = DateTime.Now;
            DisplaySelectedEntity.SUBMITTEDBY = LoginCredentials.CurrentUserGuid();
            MainViewModel.Save(DisplaySelectedEntity);
        }

        /// <summary>
        /// Determines whether an entities can be approved
        /// Since CollectionViewModelBase is a POCO view model, this method will be used as a CanExecute callback for ApproveCommand.
        /// </summary>
        /// <param name="projectionEntity">Entities to approve.</param>
        public bool CanApprove()
        {
            if (MainViewModel == null || DisplaySelectedEntity == null)
                return false;

            if (DisplaySelectedEntity.SUBMITTED == null)
                return false;

            if (DisplaySelectedEntity != null && DisplaySelectedEntity.APPROVED != null)
                return false;

            return true;
        }

        /// <summary>
        /// Approves an entity.
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the ApproveCommand property that can be used as a binding source in views.
        /// </summary>
        /// <param name="projectionEntity">An entity to approve.</param>
        public void Approve()
        {
            var errorMessage = string.Empty;
            if (DisplaySelectedEntity == null)
                errorMessage = "Nothing within variation to approve";
            else if (loadPROJECT == null)
                errorMessage = "Project doesn't exists";
            else if (LiveBASELINE == null)
                errorMessage = "Live baseline doesn't exists";
            else if (LivePROGRESS == null)
                errorMessage = "Live progress doesn't exists";

            if (errorMessage != string.Empty)
            {
                MessageBoxService.ShowMessage(errorMessage);
                return;
            }

            invokedRevision = false;
            VARIATION_ITEMSViewModelWrapper(DisplaySelectedEntity);
        }

        bool invokedRevision = false;
        private void OnVARIATION_ITEMSLoaded(IEnumerable<VARIATION_ITEMProjection> projections)
        {
            if (invokedRevision)
                return;

            mainThreadDispatcher.BeginInvoke(new Action(() => ReviseBASELINE(projections)));

            invokedRevision = true;
        }

        public void ReviseBASELINE(IEnumerable<VARIATION_ITEMProjection> projections)
        {
            var newBASELINE = new BASELINE();
            BASELINE liveBASELINE = LiveBASELINE;
            liveBASELINE.STATUS = BaselineStatus.Superseded;
            BASELINEViewModel.Save(liveBASELINE);

            DataUtils.ShallowCopy(newBASELINE, liveBASELINE);
            newBASELINE.GUID = Guid.Empty;
            newBASELINE.REVISION = ((char)(liveBASELINE.REVISION.Last() + 1)).ToString();
            //not saving new baseline as live yet because editBASELINE_ITEMS still depends on the current live baseline for copying BASELINE_ITEMS
            newBASELINE.STATUS = BaselineStatus.Live;
            BASELINEViewModel.Save(newBASELINE);

            DisplaySelectedEntity.APPROVED = DateTime.Now;
            DisplaySelectedEntity.GUID_ORIBASELINE = LiveBASELINE.GUID;
            DisplaySelectedEntity.GUID_BASELINE = newBASELINE.GUID;

            //var newBASELINE_ITEMS = new ObservableCollection<BASELINE_ITEM>();
            List<BASELINE_ITEM> baseline_itemForInternalNumberGeneration = new List<BASELINE_ITEM>();
            List<VARIATION_ITEMProjection> variation_item = projections.ToList();

            foreach (var currentVARIATION_ITEM in variation_item)
            {
                var newBASELINE_ITEM = new BASELINE_ITEM();
                DataUtils.ShallowCopy(newBASELINE_ITEM, currentVARIATION_ITEM.BASELINE_ITEMJoinRATE.BASELINE_ITEM);

                if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION == VariationAction.Cancel)
                {
                    if (currentVARIATION_ITEM.TOTAL_EARNED_UNITS == 0)
                        newBASELINE_ITEM.DC_HOURS = -1 * newBASELINE_ITEM.TOTAL_HOURS;
                    else
                        newBASELINE_ITEM.DC_HOURS = -1 * (newBASELINE_ITEM.TOTAL_HOURS - currentVARIATION_ITEM.TOTAL_EARNED_UNITS);
                }
                else if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION == VariationAction.Append)
                {
                    if (currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS < 0)
                    {
                        decimal maximumReducibleUnits = -1 * currentVARIATION_ITEM.TOTAL_EARNED_UNITS;
                        if (currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS < maximumReducibleUnits)
                            newBASELINE_ITEM.DC_HOURS += maximumReducibleUnits;
                        else
                            newBASELINE_ITEM.DC_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                    }
                    else
                        newBASELINE_ITEM.DC_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                }
                else if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION == VariationAction.Add)
                {
                    newBASELINE_ITEM.GUID = Guid.Empty;
                    newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                    newBASELINE_ITEM.INTERNAL_NUM = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(
                        loadPROJECT, baseline_itemForInternalNumberGeneration, newBASELINE_ITEM.AREA, newBASELINE_ITEM.DISCIPLINE,
                        newBASELINE_ITEM.DOCTYPE);

                    if (DisplaySelectedEntity.TYPE == VariationType.Internal)
                        newBASELINE_ITEM.ESTIMATED_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;
                    else
                        newBASELINE_ITEM.DC_HOURS += currentVARIATION_ITEM.VARIATION_ITEM.VARIATION_UNITS;

                    newBASELINE_ITEM.GUID_VARIATION = DisplaySelectedEntity.GUID;
                    baseline_itemForInternalNumberGeneration.Add(newBASELINE_ITEM);
                }

                if (currentVARIATION_ITEM.VARIATION_ITEM.ACTION != VariationAction.NoAction)
                    newBASELINE_ITEM.GUID_VARIATION = DisplaySelectedEntity.GUID;

                newBASELINE_ITEM.GUID = Guid.Empty;
                newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                BASELINE_ITEMSViewModel.Save(newBASELINE_ITEM);
            }

            CleanUpVARIATION_ITEMS();
            InitializeAndLoadEntitiesLoaderDescription();
        }
        #endregion

        #region ISupportCustomDocumentTypeNameAndParameter

        public string GetCustomDocumentTypeName()
        {
            return "VARIATION_ITEMCollectionView";
        }

        public object GetCustomDocumentParameter()
        {
            return new OptionalEntitiesParameter<PROJECT, VARIATION>(loadPROJECT, DisplaySelectedEntity);
        }

        public string GetCustomDocumentTitle()
        {
            return "[" + loadPROJECT.NUMBER + "] VARIATION";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}