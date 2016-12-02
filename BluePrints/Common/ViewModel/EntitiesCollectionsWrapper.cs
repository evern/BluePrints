using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Data.Helpers;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.Common.ViewModel
{
    public abstract class CollectionViewModelsWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork, TMainViewModel> : ICollectionViewModelsWrapper, IDocumentContent, ISupportParameter
        where TMainEntity : class
        where TMainProjectionEntity : class
        where TMainEntityUnitOfWork : IUnitOfWork
        where TMainViewModel : CollectionViewModel<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork>
    {
        protected bool isSubEntitiesAdded;
        protected EntitiesLoaderDescriptionCollection loaderCollection = null;
        protected EntitiesLoaderDescription<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork> mainEntityLoader;
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
        void loadEntitiesCollectionOnMainThread()
        {
            IEnumerable<IEntitiesLoaderDescription> entitiesLoader = loaderCollection.Where(x => !x.isLoaded);
            if (entitiesLoader == null || entitiesLoader.Count() == 0)
                return;

            int currentLoadOrder = entitiesLoader.Min(x => x.loadOrder);
            IEntitiesLoaderDescription entitiesLoaderDescription = loaderCollection.First(x => x.loadOrder == currentLoadOrder);

            if (entitiesLoaderDescription.dependencyType != null)
            {
                if (loaderCollection.IsEntitiesLoaderExists(entitiesLoaderDescription.dependencyType))
                {
                    IEntitiesLoaderDescription dependentEntitiesLoaderDescription = loaderCollection.GetLoader(entitiesLoaderDescription.dependencyType);
                    if (!dependentEntitiesLoaderDescription.isLoaded)
                        throw new InvalidOperationException("Dependent entities loader is sequenced after the current entities loader.");
                    else
                        entitiesLoaderDescription.CreateCollectionViewModel();
                }
                else
                    throw new InvalidOperationException("Dependent entities loader not added.");
            }
            else
                entitiesLoaderDescription.CreateCollectionViewModel();
        }

        bool isAllEntitiesLoaded()
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
            mainEntityLoader = new EntitiesLoaderDescription<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork>(this, 0, unitOfWorkFactory, getRepositoryFunc, OnMainViewModelLoaded, OnEntitiesChanged, ConstructMainViewModelProjection);
        }

        protected virtual Func<IRepositoryQuery<TMainEntity>, IQueryable<TMainProjectionEntity>> ConstructMainViewModelProjection()
        {
            throw new NotImplementedException("Override this method to define how main view model should be constructed.");
        }

        protected virtual bool OnMainViewModelLoaded(IEnumerable<TMainProjectionEntity> entities)
        {
            MainViewModel = (TMainViewModel)mainEntityLoader.GetViewModel();
            AssignCallBacksAndRaisePropertyChange(entities);
            return true;
        }

        protected virtual void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainProjectionEntity> entities)
        {
            throw new NotImplementedException("Override this method to assign call backs and also notify the view.");
        }

        protected virtual void OnEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            throw new NotImplementedException("Override this method to reload or refresh the main view model.");
        }

        #region ISupportParameter
        object ISupportParameter.Parameter
        {
            get { return null; }
            set { OnParameterChanged(value); }
        }
        #endregion

        #region IDocumentContent
        protected IDocumentOwner DocumentOwner { get; private set; }
        object IDocumentContent.Title { get { return null; } }
        protected virtual string ViewName
        {
            get
            {
                throw new NotImplementedException("Override this method to specify the view name.");
            }
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

        protected virtual void OnClose(CancelEventArgs e) { }
        void IDocumentContent.OnClose(CancelEventArgs e)
        {
            OnClose(e);
        }

        void IDocumentContent.OnDestroy()
        {
            this.SaveLayout();

            if (mainEntityLoader != null)
                mainEntityLoader.OnDestroy();

            if (loaderCollection == null)
                return;

            foreach (IEntitiesLoaderDescription entityLoaderDescription in loaderCollection)
            {
                entityLoaderDescription.OnDestroy();
            }
        }

        IDocumentOwner IDocumentContent.DocumentOwner
        {
            get { return DocumentOwner; }
            set { DocumentOwner = value; }
        }
        #endregion

        protected IMessageBoxService MessageBoxService { get { return this.GetRequiredService<IMessageBoxService>(); } }
        protected ILayoutSerializationService LayoutSerializationService { get { return this.GetService<ILayoutSerializationService>(); } }
        void SaveLayout()
        {
            PersistentLayoutHelper.TrySerializeLayout(LayoutSerializationService, ViewName);
            PersistentLayoutHelper.SaveLayout();
        }
    }

    public interface ICollectionViewModelsWrapper
    {
        void InvokeEntitiesLoaderDescriptionLoading();

        void InitializeAndLoadEntitiesLoaderDescription();
    }
}
