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
    public class VARIATIONSViewModelWrapper : CollectionViewModelsWrapper<VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<VARIATION, VARIATION, Guid, IBluePrintsEntitiesUnitOfWork>>, ISupportCustomDocumentTypeNameAndParameter
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
        PROJECT loadPROJECT;
        BASELINE loadBASELINE;
        PROGRESS loadPROGRESS;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            this.loadPROJECT = bluePrintsUnitOfWorkFactory.CreateUnitOfWork().PROJECTS.First(x => x.GUID == (Guid)parameter);
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, typeof(PROJECT), isContinueLoadingAfterBASELINE, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, typeof(PROJECT), isContinueLoadingAfterPROGRESS, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc, typeof(PROGRESS), null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc, typeof(BASELINE), null, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.USERS);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        bool isContinueLoadingAfterBASELINE(IEnumerable<BASELINE> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "BASELINE"))));
                return false;
            }

            this.loadBASELINE = entities.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
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

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == this.loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);
        }

        Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            if (this.loadBASELINE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_BASELINE == loadBASELINE.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.VARIATIONS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION> entities)
        {
            MainViewModel.OnBeforeEntitySavedCallBack = this.OnBeforeEntitySaved;
            MainViewModel.PreSave = this.BeforeSaveValidation;
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected override void OnEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender == MainViewModel || sender == this)
                return;

            if (loadPROJECT != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
        }
        #region CallBacks
        public bool BeforeSaveValidation(VARIATION entity)
        {
            if (loadBASELINE == null)
                return false;

            return true;
        }

        public void OnBeforeEntitySaved(VARIATION entity)
        {
            entity.GUID_PROJECT = this.loadPROJECT.GUID;

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
            get
            {
                return "PROGRESS_ITEMSViewModelWrapper";
            }
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

        protected IDocumentManagerService DocumentManagerService { get { return this.GetService<IDocumentManagerService>(); } }
        public bool CanEdit(VARIATION entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            return true;
        }

        public void Edit(VARIATION entity)
        {
            if (entity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<VARIATION_ITEM, Guid>(this, entity.GUID, string.Empty);
        }

        /// <summary>
        /// Determines whether an entities can be Submitd
        /// Since CollectionViewModelBase is a POCO view model, this method will be used as a CanExecute callback for SubmitCommand.
        /// </summary>
        /// <param name="projectionEntity">Entities to Submit.</param>
        public bool CanSubmit(VARIATION entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
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
        public bool CanApprove(VARIATION entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            if (loadBASELINE == null || loadPROGRESS == null)
                return false;
            else if (entity.SUBMITTED == null)
                return false;
            else if (entity != null && entity.APPROVED != null)
                return false;

            return true;
        }

        /// <summary>
        /// Approves an entity.
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the ApproveCommand property that can be used as a binding source in views.
        /// </summary>
        /// <param name="projectionEntity">An entity to approve.</param>
        public void Approve(VARIATION entity)
        {
            string errorMessage = string.Empty;
            if (entity == null)
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

            IBluePrintsEntitiesUnitOfWork unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            BASELINE LiveBASELINE = loadBASELINE;
            IEnumerable<VARIATION_ITEM> editVARIATION_ITEMS = unitOfWork.VARIATION_ITEMS.Where(x => x.GUID_VARIATION == entity.GUID).ToArray().AsEnumerable();
            IEnumerable<BASELINE_ITEM> addBASELINE_ITEMS = unitOfWork.BASELINE_ITEMS.Where(x => x.GUID_VARIATION == entity.GUID && x.GUID_BASELINE == null).ToArray().AsEnumerable();
            IQueryable<BASELINE_ITEM> editBASELINE_ITEMS = loaderCollection.GetCollection<BASELINE_ITEM>();
            IEnumerable<PROGRESS_ITEM> livePROGRESS_ITEMS = loaderCollection.GetCollection<PROGRESS_ITEM>();

            BASELINE newBASELINE = new BASELINE();
            DataUtils.ShallowCopy(newBASELINE, LiveBASELINE);
            newBASELINE.GUID = Guid.Empty;
            newBASELINE.REVISION = ((char)(LiveBASELINE.REVISION.Last() + 1)).ToString();
            //not saving new baseline as live yet because editBASELINE_ITEMS still depends on the current live baseline for copying BASELINE_ITEMS
            newBASELINE.STATUS = BaselineStatus.Superseded;

            CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINECollectionViewModel = (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEMCollectionViewModel = (CollectionViewModel<BASELINE_ITEM, BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE_ITEM>();
            BASELINECollectionViewModel.Save(newBASELINE);

            entity.APPROVED = DateTime.Now;
            entity.GUID_ORIBASELINE = LiveBASELINE.GUID;
            entity.GUID_BASELINE = newBASELINE.GUID;
            MainViewModel.Save(entity);

            ObservableCollection<BASELINE_ITEM> newBASELINE_ITEMS = new ObservableCollection<BASELINE_ITEM>();

            foreach (BASELINE_ITEM editBASELINE_ITEM in editBASELINE_ITEMS)
            {
                BASELINE_ITEM copyBASELINE_ITEM = new BASELINE_ITEM();
                DataUtils.ShallowCopy(copyBASELINE_ITEM, editBASELINE_ITEM);

                VARIATION_ITEM editVARIATION_ITEM = editVARIATION_ITEMS.FirstOrDefault(x => x.GUID_ORIBASEITEM == editBASELINE_ITEM.GUID_ORIGINAL);
                if (editVARIATION_ITEM != null)
                {
                    if (editVARIATION_ITEM.ACTION == VariationAction.Cancel)
                    {
                        decimal progressItemEARNED_UNITS = livePROGRESS_ITEMS.Where(x => x.GUID_ORIBASEITEM == editBASELINE_ITEM.GUID_ORIGINAL).Sum(y => y.EARNED_UNITS);
                        if (progressItemEARNED_UNITS == 0)
                        {
                            if (entity.TYPE == VariationType.Internal)
                                copyBASELINE_ITEM.ESTIMATED_HOURS = 0;
                            else
                                copyBASELINE_ITEM.DC_HOURS = -1 * copyBASELINE_ITEM.ESTIMATED_HOURS;
                        }
                        else
                        {
                            if(entity.TYPE == VariationType.Internal)
                                copyBASELINE_ITEM.ESTIMATED_HOURS = progressItemEARNED_UNITS;
                            else
                                copyBASELINE_ITEM.DC_HOURS = -1 * (copyBASELINE_ITEM.TOTAL_HOURS - progressItemEARNED_UNITS);
                        }
                    }
                    else if (editVARIATION_ITEM.ACTION == VariationAction.Append)
                        copyBASELINE_ITEM.DC_HOURS += editVARIATION_ITEM.VARIATION_UNITS;

                    if (editVARIATION_ITEM.ACTION != VariationAction.NoAction)
                    {
                        copyBASELINE_ITEM.GUID_VARIATION = entity.GUID;
                    }
                }

                copyBASELINE_ITEM.GUID = Guid.Empty;
                copyBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                newBASELINE_ITEMS.Add(copyBASELINE_ITEM);
            }

            foreach (BASELINE_ITEM addBASELINE_ITEM in addBASELINE_ITEMS)
            {
                BASELINE_ITEM newBASELINE_ITEM = new BASELINE_ITEM();
                DataUtils.ShallowCopy(newBASELINE_ITEM, addBASELINE_ITEM);
                newBASELINE_ITEM.GUID = Guid.Empty;
                newBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                newBASELINE_ITEM.INTERNAL_NUM = BluePrintDataUtils.BASELINEITEM_Generate_InternalNumber(this.loadPROJECT, newBASELINE_ITEMS, addBASELINE_ITEM.AREA, addBASELINE_ITEM.DISCIPLINE, addBASELINE_ITEM.DOCTYPE);
                VARIATION_ITEM editVARIATION_ITEM = editVARIATION_ITEMS.First(x => x.GUID_ORIBASEITEM == newBASELINE_ITEM.GUID_ORIGINAL);

                if (entity.TYPE == VariationType.Internal)
                    newBASELINE_ITEM.ESTIMATED_HOURS += editVARIATION_ITEM.VARIATION_UNITS;
                else
                    newBASELINE_ITEM.DC_HOURS = editVARIATION_ITEM.VARIATION_UNITS;

                newBASELINE_ITEM.GUID_VARIATION = entity.GUID;
                newBASELINE_ITEMS.Add(newBASELINE_ITEM);
            }

            foreach (BASELINE_ITEM newBASELINE_ITEM in newBASELINE_ITEMS)
            {
                BASELINE_ITEMCollectionViewModel.Save(newBASELINE_ITEM);
            }

            BASELINE repoBASELINE = BASELINECollectionViewModel.Entities.FirstOrDefault(x => x.GUID == loadBASELINE.GUID);
            if(repoBASELINE != null)
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
            return new OptionalEntitiesParameter<PROJECT, VARIATION>(this.loadPROJECT, MainViewModel.SelectedEntity);
        }

        public string GetCustomDocumentTitle()
        {
            return "[" + this.loadPROJECT.NUMBER + "] VARIATION";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}
