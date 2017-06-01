using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class PHASECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PHASECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PHASECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PHASECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the PHASECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PHASECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PHASECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            InvokeEntitiesLoaderDescriptionLoading();
        }
        
        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PHASES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> ConstructMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PHASE> entities)
        {
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(PHASE entity)
        {
        }

        #endregion

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "PHASECollectionViewModelWrapper"; }
        }

        #endregion
    }
}