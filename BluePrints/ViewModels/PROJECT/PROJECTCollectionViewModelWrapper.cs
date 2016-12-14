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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class PROJECTCollectionViewModelWrapper : CollectionViewModelsWrapper<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>>, ISupportCustomDocumentTypeNameAndParameter
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTCollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.BASELINES, null, null, null, OnAfterEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, null, null, null, OnAfterEntitiesChanged);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> ConstructMainViewModelProjection()
        {
            return query => query.OrderBy(x => x.NUMBER);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROJECT> entities)
        {
            MainViewModel.PostSave = this.PostSave;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected override void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender.ToString() == MainViewModel.ToString() || sender.ToString() == this.ToString())
                return;

            if (MainViewModel != null)
                mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
            else
                mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
        }

        #region Collection Call Backs
        private void PostSave(PROJECT entity, bool isNewEntity)
        {
            if(isNewEntity)
            {
                BASELINE newBASELINE = new BASELINE();
                newBASELINE.GUID_PROJECT = entity.GUID;
                newBASELINE.NAME = entity.NUMBER + "_001";
                newBASELINE.REVISION = "A";
                newBASELINE.STATUS = BaselineStatus.Live;
                BASELINEViewModel.Save(newBASELINE);

                PROGRESS newPROGRESS = new PROGRESS();
                newPROGRESS.GUID_PROJECT = entity.GUID;
                newPROGRESS.NAME = entity.NUMBER + "WEEKLY_001";
                newPROGRESS.PROGRESS_START = DateTime.Now;
                newPROGRESS.DATA_DATE = DateTime.Now;
                newPROGRESS.INTERVAL_COUNT = 1;
                newPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Weekly;
                newPROGRESS.STATUS = ProgressStatus.Live;
                PROGRESSViewModel.Save(newPROGRESS);
            }
        }
        #endregion
        #endregion

        #region View Properties
        public CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            }
        }

        public CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESSViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS>();
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "PROJECTCollectionViewModelWrapper";
            }
        }

        #endregion

        #region ISupportCustomDocumentTypeNameAndParameter
        public bool CanEdit(PROJECT entity)
        {
            if (MainViewModel == null || MainViewModel.SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService { get { return this.GetService<IDocumentManagerService>(); } }
        public void Edit(PROJECT entity)
        {
            if (entity == null)
                return;

            DocumentManagerService.ShowExistingEntityDocument<PROJECT, Guid>(this, entity.GUID, string.Empty);
        }

        public string GetCustomDocumentTypeName()
        {
            return "PROJECTView";
        }

        public object GetCustomDocumentParameter()
        {
            return new EntitiesParameter<PROJECT>(MainViewModel.SelectedEntity);
        }

        public string GetCustomDocumentTitle()
        {
            return "[" + this.MainViewModel.SelectedEntity.NUMBER + "]";
        }

        public bool IsCustomModeEnabled()
        {
            return true;
        }
        #endregion
    }
}
