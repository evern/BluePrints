using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Data.Helpers
{
    public class EntitiesLoaderDescriptionCollection : List<IEntitiesLoaderDescription>
    {
        private readonly ICollectionViewModelsWrapper owner;

        public EntitiesLoaderDescriptionCollection(ICollectionViewModelsWrapper owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Add collection view model into parent entity
        /// </summary>
        /// <typeparam name="TEntity">Corresponding type of entity of CollectionViewModel</typeparam>
        /// <typeparam name="TPrimaryKey">Corresponding type of primary key of CollectionViewModel</typeparam>
        /// <typeparam name="TUnitOfWork">Corresponding type of unit of work for CollectionViewModel</typeparam>
        /// <param name="loadOrder">Load order of the parent entity loader</param>
        /// <param name="entitiesLoader"></param>
        /// <param name="dependencyType"></param>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        /// <param name="getRepositoryFunc">A function that returns a repository representing entities of the given type.</param>
        /// <param name="additionalProjection">An optional parameter that provides a LINQ function used to customize a query for entities. The parameter, for example, can be used for sorting data.</param>
        public void AddEntitiesLoader<TEntity, TProjection, TPrimaryKey, TUnitOfWork>(
            int loadOrder,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            Func<TUnitOfWork, IRepository<TEntity, TPrimaryKey>> getRepositoryFunc,
            Func<Func<IRepositoryQuery<TEntity>, IQueryable<TProjection>>> constructProjectionCallBackFunc = null,
            Type dependencyType = null,
            Func<IEnumerable<TProjection>, bool> isContinueLoadingCallBack = null,
            Func<object, Type, EntityMessageType, object, bool> collectionViewModelBeforeChangedCallBack = null,
            Action<object, Type, EntityMessageType, object> collectionViewModelChangedCallBack = null,
            Action<IEnumerable<TProjection>> collectionViewModelRefreshedCallBack = null, 
            bool isCompulsory = false)
            where TEntity : class
            where TProjection : class
            where TUnitOfWork : IUnitOfWork
        {
            Add(new EntitiesLoaderDescription<TEntity, TProjection, TPrimaryKey, TUnitOfWork>(owner, loadOrder,
                unitOfWorkFactory, getRepositoryFunc, isContinueLoadingCallBack, collectionViewModelBeforeChangedCallBack, collectionViewModelChangedCallBack, collectionViewModelRefreshedCallBack, 
                constructProjectionCallBackFunc, dependencyType, isCompulsory));
        }

        public IEntitiesLoaderDescription GetLoader(Type dependencyType)
        {
            return this.FirstOrDefault(x => x.GetProjectionEntityType() == dependencyType);
        }

        public IEntitiesViewModel<TProjection> GetViewModel<TProjection>()
            where TProjection : class
        {
            var entitiesLoader =
                (IEntitiesLoaderDescription<TProjection>)GetLoader(typeof(TProjection));
            if (entitiesLoader == null)
                throw new InvalidOperationException("Entities loader not added");

            return entitiesLoader.GetViewModel();
        }

        public Func<TProjection> GetObjectFunc<TProjection>()
            where TProjection : class
        {
            var entitiesLoader =
                (IEntitiesLoaderDescription<TProjection>)GetLoader(typeof(TProjection));
            if (entitiesLoader == null)
                throw new InvalidOperationException("Entities loader not added");

            return entitiesLoader.GetSingleObject;
        }

        public Func<IQueryable<TProjection>> GetCollectionFunc<TProjection>()
            where TProjection : class
        {
            var entitiesLoader =
                (IEntitiesLoaderDescription<TProjection>)GetLoader(typeof(TProjection));
            if (entitiesLoader == null)
                throw new InvalidOperationException("Entities loader not added");

            return entitiesLoader.GetCollection;
        }

        public IQueryable<TProjection> GetCollection<TProjection>()
            where TProjection : class
        {
            var GetCollectionFunc = GetCollectionFunc<TProjection>();
            return GetCollectionFunc();
        }

        public TProjection GetObject<TProjection>()
            where TProjection : class
        {
            var GetSingleObjectFunc = GetObjectFunc<TProjection>();
            return GetSingleObjectFunc();
        }

        public bool IsEntitiesLoaderExists(Type type)
        {
            return this.Any(x => x.GetProjectionEntityType() == type);
        }

        public void OnDestroy()
        {
            foreach (var entitiesLoaderDescription in this)
                entitiesLoaderDescription.OnDestroy();
        }
    }

    public class EntitiesLoaderDescription<TEntity, TProjection, TPrimaryKey, TUnitOfWork> :
        IEntitiesLoaderDescription<TProjection>
        where TEntity : class
        where TProjection : class
        where TUnitOfWork : IUnitOfWork
    {
        private readonly ICollectionViewModelsWrapper owner;
        public int loadOrder { get; set; }
        public bool isLoaded { get; set; }
        public bool isCompulsory { get; set; }
        public Type dependencyType { get; set; }

        readonly IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory;
        readonly Func<TUnitOfWork, IRepository<TEntity, TPrimaryKey>> getRepositoryFunc;

        readonly Func<Func<IRepositoryQuery<TEntity>, IQueryable<TProjection>>> constructProjectionCallBackFunc;

        private IEntitiesViewModel<TProjection> collectionViewModel;
        readonly Func<IEnumerable<TProjection>, bool> isContinueLoadingCallBack;
        readonly Action<IEnumerable<TProjection>> onEntitiesRefreshedCallBack;
        readonly Action<object, Type, EntityMessageType, object> collectionViewModelChangedCallBack;
        readonly Func<object, Type, EntityMessageType, object, bool> collectionViewModelBeforeChangedCallBack;


        /// <summary>
        /// Describe how should entities be handled within EntitiesCollectionWrapper
        /// </summary>
        /// <param name="loadOrder"></param>
        /// <param name="isRequired"></param>
        /// <param name="entitiesLoader"></param>
        /// <param name="dependencyType"></param>
        public EntitiesLoaderDescription(
            ICollectionViewModelsWrapper owner,
            int loadOrder,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            Func<TUnitOfWork, IRepository<TEntity, TPrimaryKey>> getRepositoryFunc,
            Func<IEnumerable<TProjection>, bool> isContinueLoadingCallBack = null,
            Func<object, Type, EntityMessageType, object, bool> collectionViewModelBeforeChangedCallBack = null,
            Action<object, Type, EntityMessageType, object> collectionViewModelChangedCallBack = null,
            Action<IEnumerable<TProjection>> collectionViewModelRefreshedCallBack = null,
            Func<Func<IRepositoryQuery<TEntity>, IQueryable<TProjection>>> constructProjectionCallBackFunc = null,
            Type dependencyType = null, bool isCompulsory = false)
        {
            this.owner = owner;
            this.loadOrder = loadOrder;
            this.dependencyType = dependencyType;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.getRepositoryFunc = getRepositoryFunc;
            this.constructProjectionCallBackFunc = constructProjectionCallBackFunc;
            this.isContinueLoadingCallBack = isContinueLoadingCallBack;
            this.onEntitiesRefreshedCallBack = collectionViewModelRefreshedCallBack;
            this.collectionViewModelChangedCallBack = collectionViewModelChangedCallBack;
            this.collectionViewModelBeforeChangedCallBack = collectionViewModelBeforeChangedCallBack;
            this.isCompulsory = isCompulsory;
        }

        public void CreateCollectionViewModel()
        {
            Func<IRepositoryQuery<TEntity>, IQueryable<TProjection>> projection = null;
            if (constructProjectionCallBackFunc != null)
                projection = constructProjectionCallBackFunc();

            collectionViewModel =
                CollectionViewModel<TEntity, TProjection, TPrimaryKey, TUnitOfWork>.CreateCollectionViewModel(
                    unitOfWorkFactory, getRepositoryFunc, projection);
            collectionViewModel.OnEntitiesLoadedCallBack = OnEntitiesFirstLoaded;
            collectionViewModel.OnAfterEntitiesChangedCallBack = collectionViewModelChangedCallBack;
            collectionViewModel.OnBeforeEntitiesChangedCallBack = collectionViewModelBeforeChangedCallBack;
            collectionViewModel.Entities.ToList();
        }

        public int GetEntitiesCount()
        {
            if (collectionViewModel == null || collectionViewModel.IsLoading)
                return 0;

            return collectionViewModel.Entities.Count();
        }

        public Type GetEntitiesProjectionType()
        {
            return typeof(TProjection);
        }

        private void OnEntitiesFirstLoaded(IEnumerable<TProjection> loadedEntities)
        {
            isLoaded = true;

            if (isContinueLoadingCallBack != null)
                if (!isContinueLoadingCallBack(loadedEntities))
                {
                    collectionViewModel.OnEntitiesLoadedCallBack = null;
                    return;
                }

            collectionViewModel.OnEntitiesLoadedCallBack = null;
            collectionViewModel.OnEntitiesLoadedCallBack = OnEntitiesSubsequentLoading;
            owner.InvokeEntitiesLoaderDescriptionLoading();
        }

        private void OnEntitiesSubsequentLoading(IEnumerable<TProjection> loadedEntities)
        {
            onEntitiesRefreshedCallBack?.Invoke(loadedEntities);
        }

        public Type GetProjectionEntityType()
        {
            return typeof(TProjection);
        }

        public IEntitiesViewModel<TProjection> GetViewModel()
        {
            return collectionViewModel;
        }

        /// <summary>
        /// Call this only after entities has been loaded as notified by OnEntitiesLoadedCallBackFunc
        /// </summary>
        public IQueryable<TProjection> GetCollection()
        {
            //this.collectionViewModel.OnEntitiesLoadedCallBack = null;
            if (collectionViewModel == null || collectionViewModel.Entities == null)
                return new List<TProjection>().AsQueryable();
            else
                return collectionViewModel.Entities.AsQueryable();
        }

        /// <summary>
        /// Call this only after entities has been loaded as notified by OnEntitiesLoadedCallBackFunc
        /// </summary>
        public TProjection GetSingleObject()
        {
            //this.collectionViewModel.OnEntitiesLoadedCallBack = null;
            if (collectionViewModel.Entities == null)
                return null;
            else
            {
                if (collectionViewModel.Entities.Count == 0)
                    return null;

                return collectionViewModel.Entities.First();
            }
        }

        public void OnDestroy()
        {
            if (collectionViewModel != null)
            {
                collectionViewModel.OnDestroy();

                collectionViewModel.OnEntitiesLoadedCallBack = null;
                collectionViewModel.OnEntitiesLoadedCallBack = null;
                collectionViewModel.OnBeforeEntitiesChangedCallBack = null;
                collectionViewModel = null;
            }
        }
    }

    public interface IEntitiesLoaderDescription<TProjection> : IEntitiesLoaderDescription
        where TProjection : class
    {
        TProjection GetSingleObject();
        IQueryable<TProjection> GetCollection();
        IEntitiesViewModel<TProjection> GetViewModel();
    }

    public interface IEntitiesLoaderDescription
    {
        void OnDestroy();
        Type GetProjectionEntityType();
        void CreateCollectionViewModel();
        bool isLoaded { get; set; }
        bool isCompulsory { get; set; }
        int GetEntitiesCount();
        Type GetEntitiesProjectionType();
        int loadOrder { get; set; }
        Type dependencyType { get; set; }
    }
}