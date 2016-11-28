using BluePrints.Common.DataModel;
using BluePrints.Common.Utils;
using BluePrints.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BluePrints.Data.Helpers
{
    public class EntitiesLoaderDescriptionCollection : List<IEntitiesLoaderDescription>
    {
        readonly ICollectionViewModelsWrapper owner;
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
            Action<object, Type, EntityMessageType, object> collectionViewModelChangedCallBack = null)
            where TEntity : class
            where TProjection : class
            where TUnitOfWork : IUnitOfWork
        {
            this.Add(new EntitiesLoaderDescription<TEntity, TProjection, TPrimaryKey, TUnitOfWork>(this.owner, loadOrder, unitOfWorkFactory, getRepositoryFunc, isContinueLoadingCallBack, collectionViewModelChangedCallBack, constructProjectionCallBackFunc, dependencyType));
        }

        public IEntitiesLoaderDescription GetLoader(Type dependencyType)
        {
            return this.FirstOrDefault(x => x.GetProjectionEntityType() == dependencyType);
        }

        public IEntitiesViewModel<TProjection> GetViewModel<TProjection>()
            where TProjection : class
        {
            IEntitiesLoaderDescription<TProjection> entitiesLoader = (IEntitiesLoaderDescription<TProjection>)GetLoader(typeof(TProjection));
            if (entitiesLoader == null)
                throw new InvalidOperationException("Entities loader not added");

            return entitiesLoader.GetViewModel();
        }

        public Func<TProjection> GetObjectFunc<TProjection>()
            where TProjection : class
        {
            IEntitiesLoaderDescription<TProjection> entitiesLoader = (IEntitiesLoaderDescription<TProjection>)GetLoader(typeof(TProjection));
            if (entitiesLoader == null)
                throw new InvalidOperationException("Entities loader not added");

            return entitiesLoader.GetSingleObject;
        }

        public Func<IQueryable<TProjection>> GetCollectionFunc<TProjection>()
            where TProjection : class
        {
            IEntitiesLoaderDescription<TProjection> entitiesLoader = (IEntitiesLoaderDescription<TProjection>)GetLoader(typeof(TProjection));
            if (entitiesLoader == null)
                throw new InvalidOperationException("Entities loader not added");

            return entitiesLoader.GetCollection;
        }

        public IQueryable<TProjection> GetCollection<TProjection>()
            where TProjection : class
        {
            Func<IQueryable<TProjection>> GetCollectionFunc = GetCollectionFunc<TProjection>();
            return GetCollectionFunc();
        }

        public TProjection GetObject<TProjection>()
            where TProjection : class
        {
            Func<TProjection> GetSingleObjectFunc = GetObjectFunc<TProjection>();
            return GetSingleObjectFunc();
        }

        public bool IsEntitiesLoaderExists(Type type)
        {
            return this.Any(x => x.GetProjectionEntityType() == type);
        }

        public void OnDestroy()
        {
            foreach(IEntitiesLoaderDescription entitiesLoaderDescription in this)
            {
                entitiesLoaderDescription.OnDestroy();
            }
        }
    }

    public class EntitiesLoaderDescription<TEntity, TProjection, TPrimaryKey, TUnitOfWork> : IEntitiesLoaderDescription<TProjection>
        where TEntity : class
        where TProjection : class
        where TUnitOfWork : IUnitOfWork
    {
        readonly ICollectionViewModelsWrapper owner;
        public int loadOrder { get; set; }
        public bool isLoaded { get; set; }
        public Type dependencyType { get; set; }

        IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory;
        Func<TUnitOfWork, IRepository<TEntity, TPrimaryKey>> getRepositoryFunc;

        Func<Func<IRepositoryQuery<TEntity>, IQueryable<TProjection>>> constructProjectionCallBackFunc;

        IEntitiesViewModel<TProjection> collectionViewModel;
        Func<IEnumerable<TProjection>, bool> isContinueLoadingCallBack;
        Action<object, Type, EntityMessageType, object> collectionViewModelChangedCallBack;

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
            Action<object, Type, EntityMessageType, object> collectionViewModelChangedCallBack = null,
            Func<Func<IRepositoryQuery<TEntity>, IQueryable<TProjection>>> constructProjectionCallBackFunc = null,
            Type dependencyType = null)
        {
            this.owner = owner;
            this.loadOrder = loadOrder;
            this.dependencyType = dependencyType;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.getRepositoryFunc = getRepositoryFunc;
            this.constructProjectionCallBackFunc = constructProjectionCallBackFunc;
            this.isContinueLoadingCallBack = isContinueLoadingCallBack;
            this.collectionViewModelChangedCallBack = collectionViewModelChangedCallBack;
        }

        public void CreateCollectionViewModel()
        {
            Func<IRepositoryQuery<TEntity>, IQueryable<TProjection>> projection = null;
            if (constructProjectionCallBackFunc != null)
                projection = constructProjectionCallBackFunc();

            this.collectionViewModel = CollectionViewModel<TEntity, TProjection, TPrimaryKey, TUnitOfWork>.CreateCollectionViewModel(this.unitOfWorkFactory, this.getRepositoryFunc, projection);
            this.collectionViewModel.OnEntitiesLoadedCallBack = OnEntitiesFirstLoaded;
            this.collectionViewModel.OnEntitiesChangedCallBack = collectionViewModelChangedCallBack;

            this.collectionViewModel.Entities.ToList();
        }

        public void LoadCollectionViewModel()
        {
            this.collectionViewModel.Entities.ToList();
        }

        void OnEntitiesFirstLoaded(IEnumerable<TProjection> loadedEntities)
        {
            isLoaded = true;

            if (this.isContinueLoadingCallBack != null)
                if (!isContinueLoadingCallBack(loadedEntities))
                {
                    collectionViewModel.OnEntitiesLoadedCallBack = null;
                    return;
                }

            collectionViewModel.OnEntitiesLoadedCallBack = null;
            owner.InvokeEntitiesLoaderDescriptionLoading();
        }

        public Type GetProjectionEntityType()
        {
            return typeof(TProjection);
        }

        public IEntitiesViewModel<TProjection> GetViewModel()
        {
            return this.collectionViewModel;
        }

        /// <summary>
        /// Call this only after entities has been loaded as notified by OnEntitiesLoadedCallBackFunc
        /// </summary>
        public IQueryable<TProjection> GetCollection()
        {
            //this.collectionViewModel.OnEntitiesLoadedCallBack = null;
            if (this.collectionViewModel == null || this.collectionViewModel.Entities == null)
                return new List<TProjection>().AsQueryable();
            else
                return this.collectionViewModel.Entities.AsQueryable();
        }

        /// <summary>
        /// Call this only after entities has been loaded as notified by OnEntitiesLoadedCallBackFunc
        /// </summary>
        public TProjection GetSingleObject()
        {
            //this.collectionViewModel.OnEntitiesLoadedCallBack = null;
            if (this.collectionViewModel.Entities == null)
                return null;
            else
            {
                if (this.collectionViewModel.Entities.Count == 0)
                    return null;

                return this.collectionViewModel.Entities.First();
            }
        }

        public void OnDestroy()
        {
            if (this.collectionViewModel != null)
            {
                collectionViewModel.OnDestroy();

                collectionViewModel.OnEntitiesLoadedCallBack = null;
                this.collectionViewModel.OnEntitiesLoadedCallBack = null;
                this.collectionViewModel.OnEntitiesChangedCallBack = null;
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
        void LoadCollectionViewModel();
        bool isLoaded { get; set; }
        int loadOrder { get; set; }
        Type dependencyType { get; set; }
    }
}
