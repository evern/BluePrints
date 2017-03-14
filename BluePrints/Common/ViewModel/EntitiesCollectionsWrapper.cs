using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Data.Helpers;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BluePrints.ViewModels;

namespace BluePrints.Common.ViewModel
{
    public abstract class CollectionViewModelsWrapper1<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork, TMainViewModel> : ICollectionViewModelsWrapper, IDocumentContent, ISupportParameter, ISupportViewRestoration
        where TMainEntity : class, IHaveGUID
        where TMainProjectionEntity : class, IHaveGUID
        where TMainEntityUnitOfWork : IUnitOfWork
        where TMainViewModel :
        CollectionViewModel<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork>
    {
        protected bool isSubEntitiesAdded;
        protected EntitiesLoaderDescriptionCollection loaderCollection = null;

        protected
            EntitiesLoaderDescription<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork>
            mainEntityLoader;

        public TMainViewModel MainViewModel { get; set; }
        protected Dispatcher mainThreadDispatcher = Application.Current.Dispatcher;

        public virtual void InvokeEntitiesLoaderDescriptionLoading()
        {
            if (MainViewModel != null)
                return;
            else if (isAllEntitiesLoaded())
                mainThreadDispatcher.BeginInvoke(new Action(() => OnAllEntitiesCollectionLoaded()));
            else
                mainThreadDispatcher.BeginInvoke(new Action(() => loadEntitiesCollectionOnMainThread()));
        }

        /// <summary>
        /// Begins loading the collection of entities loader
        /// </summary>
        private void loadEntitiesCollectionOnMainThread()
        {
            var entitiesLoader = loaderCollection.Where(x => !x.isLoaded);
            if (entitiesLoader == null || entitiesLoader.Count() == 0)
                return;

            var currentLoadOrder = entitiesLoader.Min(x => x.loadOrder);
            var entitiesLoaderDescription =
                loaderCollection.First(x => x.loadOrder == currentLoadOrder);

            if (entitiesLoaderDescription.dependencyType != null)
                if (loaderCollection.IsEntitiesLoaderExists(entitiesLoaderDescription.dependencyType))
                {
                    var dependentEntitiesLoaderDescription =
                        loaderCollection.GetLoader(entitiesLoaderDescription.dependencyType);
                    if (!dependentEntitiesLoaderDescription.isLoaded)
                        throw new InvalidOperationException(
                            "Dependent entities loader is sequenced after the current entities loader.");
                    else
                        entitiesLoaderDescription.CreateCollectionViewModel();
                }
                else
                {
                    throw new InvalidOperationException("Dependent entities loader not added.");
                }
            else
                entitiesLoaderDescription.CreateCollectionViewModel();
        }

        private bool isAllEntitiesLoaded()
        {
            if (loaderCollection == null)
                return false;

            return loaderCollection.Where(x => !x.isLoaded).Count() == 0 ? true : false;
        }

        protected IEnumerable<TProjection> GetEntities<TProjection>()
            where TProjection : class
        {
            if (loaderCollection == null)
                return null;

            Func<IEnumerable<TProjection>> getCollectionFunc = loaderCollection.GetCollectionFunc<TProjection>();
            return getCollectionFunc();
        }

        protected virtual void OnParameterChanged(object parameter)
        {
            InitializePresentationProperties();
            InitializeParameters(parameter);

            InitializeAndLoadEntitiesLoaderDescription();
        }

        protected virtual void InitializeParameters(object parameter)
        {
            throw new NotImplementedException("Override this method to initialize primary parameter attributes in inherited member.");
        }

        public virtual void InitializeAndLoadEntitiesLoaderDescription()
        {
            throw new NotImplementedException("Override this method to initialize EntitiesLoaderDescriptionCollection.");
        }

        protected virtual void OnAllEntitiesCollectionLoaded()
        {
            throw new NotImplementedException("Override this method to initialize main entity loader.");
        }

        protected void CreateMainViewModel(
            IUnitOfWorkFactory<TMainEntityUnitOfWork> unitOfWorkFactory,
            Func<TMainEntityUnitOfWork, IRepository<TMainEntity, TMainEntityPrimaryKey>> getRepositoryFunc)
        {
            mainEntityLoader =
                new EntitiesLoaderDescription
                    <TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork>(this, 0,
                        unitOfWorkFactory, getRepositoryFunc, OnMainViewModelLoaded, OnBeforeEntitiesChanged, 
                        null, OnEntitiesRefreshed, 
                        ConstructMainViewModelProjection);
        }

        protected virtual Func<IRepositoryQuery<TMainEntity>, IQueryable<TMainProjectionEntity>>
            ConstructMainViewModelProjection()
        {
            throw new NotImplementedException(
                "Override this method to define how main view model should be constructed.");
        }

        protected virtual bool OnMainViewModelLoaded(IEnumerable<TMainProjectionEntity> entities)
        {
            MainViewModel = (TMainViewModel) mainEntityLoader.GetViewModel();
            AssignCallBacksAndRaisePropertyChange(entities);
            return true;
        }

        protected virtual void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainProjectionEntity> entities)
        {
            MainViewModel.SelectedEntities = this.DisplaySelectedEntities;
            RefreshView();
            //throw new NotImplementedException("Override this method to assign call backs and also notify the view.");
        }

        protected virtual bool OnBeforeEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            storeViewState();
            return true;
        }

        IEnumerable<IEntitiesLoaderDescription> compulsoryLoaders { get; set; }
        IEnumerable<IEntitiesLoaderDescription> CompulsoryLoaders
        {
            get
            {
                if(compulsoryLoaders == null)
                    compulsoryLoaders = loaderCollection.Where(x => x.isCompulsory);

                return compulsoryLoaders;
            }
        }
        
        protected virtual void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType,
            object sender)
        {
            IEntitiesLoaderDescription currentCompulsoryEntitiesLoader = CompulsoryLoaders.FirstOrDefault(x => x.GetEntitiesProjectionType() == changedType);

            if (currentCompulsoryEntitiesLoader != null)
            {
                if (messageType == EntityMessageType.Deleted && currentCompulsoryEntitiesLoader.GetEntitiesCount() == 0)
                {
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed,
                        StringFormatUtils.GetEntityNameByType(changedType)));

                    FullRefresh();
                }
                else if (messageType == EntityMessageType.Added && compulsoryLoaders.All(x => x.GetEntitiesCount() > 0))
                {
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored,
                        StringFormatUtils.GetEntityNameByType(changedType)));

                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
                }
            }
        }

        protected virtual void OnEntitiesRefreshed(IEnumerable<TMainProjectionEntity> refreshedEntities)
        {
            RefreshView();
        }

        #region ISupportParameter

        object ISupportParameter.Parameter
        {
            get { return null; }
            set { OnParameterChanged(value); }
        }

        #endregion

        #region SpellChecker
        public SpellCheckerModule SpellCheckerModule { get; set; }

        public void Loaded()
        {
            SpellCheckerModule = new SpellCheckerModule();
            SpellCheckerModule.ApplySpellCheckMode(true);
        }
        #endregion

        #region Presentation
        public Action StoreActiveCell { get; set; }
        public Action RestoreActiveCell { get; set; }

        private Guid RestoreSelectedEntityGuid;
        private List<Guid> RestoreSelectedEntitiesGuids = new List<Guid>();
        public TMainProjectionEntity DisplaySelectedEntity { get; set; }
        public ObservableCollection<TMainProjectionEntity> DisplaySelectedEntities { get; set; }
        private BackgroundWorker refreshBackgroundWorker;

        private void InitializePresentationProperties()
        {
            refreshBackgroundWorker = new BackgroundWorker();
            refreshBackgroundWorker.DoWork += refreshBackgroundWorker_DoWork;
            refreshBackgroundWorker.WorkerSupportsCancellation = true;
            DisplaySelectedEntities = new ObservableCollection<TMainProjectionEntity>();
        }

        public virtual void RefreshSelectedEntity()
        {
            this.RaisePropertyChanged(x => x.DisplaySelectedEntity);
        }

        public virtual void FullRefresh()
        {
            MainViewModel.Refresh();
            RefreshView();
        }

        protected void RefreshView()
        {
            if (!refreshBackgroundWorker.IsBusy)
                refreshBackgroundWorker.RunWorkerAsync();
        }

        private void refreshBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(500);
            if (refreshBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            mainThreadDispatcher.BeginInvoke(new Action(() => this.refreshViewWithStateRestoration()));
        }

        public virtual ObservableCollection<TMainProjectionEntity> DisplayEntities
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return MainViewModel.Entities;
            }
        }

        protected virtual void storeViewState()
        {
            if (DisplayEntities == null)
                return;

            StoreActiveCell?.Invoke();

            RestoreSelectedEntityGuid = Guid.Empty;
            RestoreSelectedEntitiesGuids.Clear();

            foreach (var selectedEntity in DisplaySelectedEntities)
                RestoreSelectedEntitiesGuids.Add(new Guid(selectedEntity.GUID.ToString()));

            if (DisplaySelectedEntity != null)
                RestoreSelectedEntityGuid = DisplaySelectedEntity.GUID;
        }

        protected virtual void restoreViewState()
        {
            if (DisplayEntities == null)
                return;

            var restoreSelectedEntities =
                DisplayEntities.Where(x => RestoreSelectedEntitiesGuids.Any(y => y == x.GUID));
            DisplaySelectedEntities.Clear();
            if (restoreSelectedEntities.Count() > 0)
                foreach (var restoreSelectedEntity in restoreSelectedEntities)
                    DisplaySelectedEntities.Add(restoreSelectedEntity);

            if (RestoreSelectedEntityGuid != Guid.Empty)
            {
                var restoreSelectedEntity =
                    DisplayEntities.FirstOrDefault(x => x.GUID == RestoreSelectedEntityGuid);
                if (restoreSelectedEntity != null)
                    DisplaySelectedEntity = restoreSelectedEntity;
            }

            RestoreActiveCell?.Invoke();
        }

        private void refreshViewWithStateRestoration()
        {
            this.RaisePropertiesChanged();
            restoreViewState();
        }
        #endregion

        #region IDocumentContent

        protected IDocumentOwner DocumentOwner { get; private set; }

        object IDocumentContent.Title
        {
            get { return null; }
        }

        protected virtual string ViewName
        {
            get { throw new NotImplementedException("Override this method to specify the view name."); }
        }

        public virtual void OnLoaded()
        {
            PersistentLayoutHelper.TryDeserializeLayout(LayoutSerializationService, ViewName);
        }

        public bool IsLoading
        {
            get
            {
                if (this.IsInDesignMode())
                    return true;
                if (MainViewModel == null)
                    return true;

                //assuming RaisePropertyChanged will be always be called upon on MainViewModel entities loaded
                return false;
            }
        }

        protected virtual void OnClose(CancelEventArgs e)
        {
            refreshBackgroundWorker.CancelAsync();
        }

        void IDocumentContent.OnClose(CancelEventArgs e)
        {
            OnClose(e);
        }

        /// <summary>
        /// Unregister any messaging listener
        /// </summary>
        protected void CleanUpEntitiesLoader()
        {
            compulsoryLoaders = null;

            if (mainEntityLoader != null)
                mainEntityLoader.OnDestroy();

            if (loaderCollection == null)
                return;

            foreach (var entityLoaderDescription in loaderCollection)
                entityLoaderDescription.OnDestroy();
        }

        void IDocumentContent.OnDestroy()
        {
            CleanUpEntitiesLoader();
        }

        IDocumentOwner IDocumentContent.DocumentOwner
        {
            get { return DocumentOwner; }
            set { DocumentOwner = value; }
        }

        #endregion

        #region View Interactions
        public void SetMainNestedValueWithUndoAndRefresh(TMainProjectionEntity entity, string propertyName, object newValue)
        {
            MainViewModel.SetNestedValueWithUndo(entity, propertyName, newValue);
            this.RaisePropertyChanged(x => x.DisplaySelectedEntity);
        }
        #endregion

        #region Services
        protected IMessageBoxService MessageBoxService
        {
            get { return this.GetRequiredService<IMessageBoxService>(); }
        }

        protected ILayoutSerializationService LayoutSerializationService
        {
            get { return this.GetService<ILayoutSerializationService>(); }
        }
        #endregion

        #region Layout
        public void SaveLayout()
        {
            PersistentLayoutHelper.TrySerializeLayout(LayoutSerializationService, ViewName);
            PersistentLayoutHelper.SaveLayout();
        }

        public void ResetLayout()
        {
            if (
                MessageBoxService.ShowMessage(CommonResources.Confirmation_ResetLayout,
                    CommonResources.Confirmation_Caption, MessageButton.YesNo) != MessageResult.Yes)
                return;

            PersistentLayoutHelper.ResetLayout(ViewName);
        }
        #endregion
    }

    public interface ICollectionViewModelsWrapper
    {
        void InvokeEntitiesLoaderDescriptionLoading();

        void InitializeAndLoadEntitiesLoaderDescription();
    }
}