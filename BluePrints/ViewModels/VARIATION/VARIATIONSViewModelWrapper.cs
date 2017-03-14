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
    public class VARIATIONSViewModelWrapper :
        CollectionViewModelsWrapper
        <VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork>>,
        ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATIONSViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new VARIATIONSViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected VARIATIONSViewModelWrapper()
        {
        }

        #region Database Operation

        private PROJECT loadPROJECT;
        private BASELINE loadBASELINE;
        private PROGRESS loadPROGRESS;

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
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0,
                bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, null,
                OnAfterEntitiesChanged, null, true);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(1,
                bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, typeof(PROJECT),
                isContinueLoadingAfterBASELINE, null, OnAfterEntitiesChanged, null, true);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(2,
                bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, typeof(PROJECT),
                isContinueLoadingAfterPROGRESS, null, OnAfterEntitiesChanged, null, true);
            loaderCollection.AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(3,
                bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, typeof(PROGRESS));
            loaderCollection.AddEntitiesLoader<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(4,
                bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc, typeof(BASELINE));
            loaderCollection.AddEntitiesLoader<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(5,
                bluePrintsUnitOfWorkFactory, x => x.USERS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            loadPROJECT = entities.First();
            return true;
        }

        private bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "BASELINE"))));
                return false;
            }

            loadBASELINE = entities.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
            return true;
        }

        private bool isContinueLoadingAfterPROGRESS(IEnumerable<PROGRESS> entities)
        {
            if (!entities.Any())
            {
                mainThreadDispatcher.BeginInvoke(
                    new Action(
                        () =>
                            MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROGRESS"))));
                return false;
            }

            loadPROGRESS = entities.First();
            return true;
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

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            if (loadBASELINE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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
            if (loadBASELINE == null)
                return false;

            return true;
        }

        public void OnBeforeEntitySaved(VARIATION entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;

            if (entity.APPROVED != null)
                entity.GUID_ORIBASELINE = entity.GUID_ORIBASELINE ?? loadBASELINE.GUID;
            else
                entity.GUID_ORIBASELINE = null;
        }

        #endregion

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PROGRESS_ITEMSViewModelWrapper"; }
        }

        public IEnumerable<BASELINE> BASELINECollection
        {
            get
            {
                var collection = GetEntities<BASELINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
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
        public bool CanSubmit(VARIATION entity)
        {
            if (DisplaySelectedEntity == null)
                return false;

            if (loadBASELINE == null)
                return false;

            else if (entity != null && entity.APPROVED != null)
                return false;

            return true;
        }

        /// <summary>
        /// Submits an entity.
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the SubmitCommand property that can be used as a binding source in views.
        /// </summary>
        /// <param name="projectionEntity">An entity to Submit.</param>
        public void Submit(VARIATION entity)
        {
            entity.SUBMITTED = DateTime.Now;
            entity.SUBMITTEDBY = LoginCredentials.CurrentUserGuid();
            MainViewModel.Save(entity);
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

            if (loadBASELINE == null || loadPROGRESS == null)
                return false;
            else if (DisplaySelectedEntity.SUBMITTED == null)
                return false;
            else if (DisplaySelectedEntity != null && DisplaySelectedEntity.APPROVED != null)
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
            else if (loadBASELINE == null)
                errorMessage = "Live baseline doesn't exists";
            else if (loadPROGRESS == null)
                errorMessage = "Live progress doesn't exists";

            if (errorMessage != string.Empty)
            {
                MessageBoxService.ShowMessage(errorMessage);
                return;
            }

            var unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            var LiveBASELINE = loadBASELINE;
            var editVARIATION_ITEMS =
                unitOfWork.VARIATION_ITEMS.Where(x => x.GUID_VARIATION == DisplaySelectedEntity.GUID).ToArray().AsEnumerable();
            var addBASELINE_ITEMS =
                unitOfWork.BASELINE_ITEMS.Where(x => x.GUID_VARIATION == DisplaySelectedEntity.GUID && x.GUID_BASELINE == null)
                    .ToArray()
                    .AsEnumerable();
            var editBASELINE_ITEMS = loaderCollection.GetCollection<BASELINE_ITEM>();
            IEnumerable<PROGRESS_ITEM> livePROGRESS_ITEMS = loaderCollection.GetCollection<PROGRESS_ITEM>();

            var newBASELINE = new BASELINE();
            DataUtils.ShallowCopy(newBASELINE, LiveBASELINE);
            newBASELINE.GUID = Guid.Empty;
            newBASELINE.REVISION = ((char) (LiveBASELINE.REVISION.Last() + 1)).ToString();
            //not saving new baseline as live yet because editBASELINE_ITEMS still depends on the current live baseline for copying BASELINE_ITEMS
            newBASELINE.STATUS = BaselineStatus.Superseded;

            var BASELINECollectionViewModel =
                (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<BASELINE>();
            var
                BASELINE_ITEMCollectionViewModel =
                    (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<BASELINE_ITEM>();
            BASELINECollectionViewModel.Save(newBASELINE);

            DisplaySelectedEntity.APPROVED = DateTime.Now;
            DisplaySelectedEntity.GUID_ORIBASELINE = LiveBASELINE.GUID;
            DisplaySelectedEntity.GUID_BASELINE = newBASELINE.GUID;
            MainViewModel.Save(DisplaySelectedEntity);

            var newBASELINE_ITEMS = new ObservableCollection<BASELINE_ITEM>();

            foreach (var editBASELINE_ITEM in editBASELINE_ITEMS)
            {
                var copyBASELINE_ITEM = new BASELINE_ITEM();
                DataUtils.ShallowCopy(copyBASELINE_ITEM, editBASELINE_ITEM);

                var editVARIATION_ITEM =
                    editVARIATION_ITEMS.FirstOrDefault(x => x.GUID_ORIBASEITEM == editBASELINE_ITEM.GUID_ORIGINAL);
                if (editVARIATION_ITEM != null)
                {
                    if (editVARIATION_ITEM.ACTION == VariationAction.Cancel)
                    {
                        var progressItemEARNED_UNITS =
                            livePROGRESS_ITEMS.Where(x => x.GUID_ORIBASEITEM == editBASELINE_ITEM.GUID_ORIGINAL)
                                .Sum(y => y.EARNED_UNITS);
                        if (progressItemEARNED_UNITS == 0)
                        {
                            if (DisplaySelectedEntity.TYPE == VariationType.Internal)
                                copyBASELINE_ITEM.ESTIMATED_HOURS = 0;
                            else
                                copyBASELINE_ITEM.DC_HOURS = -1 * copyBASELINE_ITEM.ESTIMATED_HOURS;
                        }
                        else
                        {
                            if (DisplaySelectedEntity.TYPE == VariationType.Internal)
                                copyBASELINE_ITEM.ESTIMATED_HOURS = progressItemEARNED_UNITS;
                            else
                                copyBASELINE_ITEM.DC_HOURS = -1 *
                                                             (copyBASELINE_ITEM.TOTAL_HOURS - progressItemEARNED_UNITS);
                        }
                    }
                    else if (editVARIATION_ITEM.ACTION == VariationAction.Append)
                    {
                        copyBASELINE_ITEM.DC_HOURS += editVARIATION_ITEM.VARIATION_UNITS;
                    }

                    if (editVARIATION_ITEM.ACTION != VariationAction.NoAction)
                        copyBASELINE_ITEM.GUID_VARIATION = DisplaySelectedEntity.GUID;
                }

                copyBASELINE_ITEM.GUID = Guid.Empty;
                copyBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                newBASELINE_ITEMS.Add(copyBASELINE_ITEM);
            }

            foreach (var addBASELINE_ITEM in addBASELINE_ITEMS)
            {
                var newBASELINE_ITEM = new BASELINE_ITEM();
                DataUtils.ShallowCopy(newBASELINE_ITEM, addBASELINE_ITEM);
                newBASELINE_ITEM.GUID = Guid.Empty;
                newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                newBASELINE_ITEM.INTERNAL_NUM = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(
                    loadPROJECT, newBASELINE_ITEMS, addBASELINE_ITEM.AREA, addBASELINE_ITEM.DISCIPLINE,
                    addBASELINE_ITEM.DOCTYPE);
                var editVARIATION_ITEM =
                    editVARIATION_ITEMS.First(x => x.GUID_ORIBASEITEM == newBASELINE_ITEM.GUID_ORIGINAL);

                if (DisplaySelectedEntity.TYPE == VariationType.Internal)
                    newBASELINE_ITEM.ESTIMATED_HOURS += editVARIATION_ITEM.VARIATION_UNITS;
                else
                    newBASELINE_ITEM.DC_HOURS = editVARIATION_ITEM.VARIATION_UNITS;

                newBASELINE_ITEM.GUID_VARIATION = DisplaySelectedEntity.GUID;
                newBASELINE_ITEMS.Add(newBASELINE_ITEM);
            }

            foreach (var newBASELINE_ITEM in newBASELINE_ITEMS)
                BASELINE_ITEMCollectionViewModel.Save(newBASELINE_ITEM);

            var repoBASELINE = BASELINECollectionViewModel.Entities.FirstOrDefault(x => x.GUID == loadBASELINE.GUID);
            if (repoBASELINE != null)
            {
                repoBASELINE.STATUS = BaselineStatus.Superseded;
                BASELINECollectionViewModel.Save(repoBASELINE);
            }

            newBASELINE.STATUS = BaselineStatus.Live;
            BASELINECollectionViewModel.Save(newBASELINE);
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