using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class REGISTER_CHANGECollectionViewModelWrapper :
        EntitiesAutoNumberCollectionWrapper
        <REGISTER_CHANGE, REGISTER_CHANGE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_CHANGECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_CHANGECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_CHANGECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        int defaultNumericFieldLengthForRegisters;
        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            defaultNumericFieldLengthForRegisters = Int32.Parse(BluePrintsResources.REGISTER_DefaultNumberFieldLength);
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_CHANGE);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<REGISTER_CHANGE>, IQueryable<REGISTER_CHANGE>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_CHANGE> entities)
        {
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(REGISTER_CHANGE entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.DATE_RAISED = DateTime.Now.Date;
        }
        #endregion

        #endregion

        #region IEntityNumber
        protected override string GetEntityNumberFieldName()
        {
            return BindableBase.GetPropertyName(() => new REGISTER_CHANGE().NUMBER);
        }

        protected override int DefaultNumericFieldLength()
        {
            return Int32.Parse(BluePrintsResources.REGISTER_DefaultNumberFieldLength);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "REGISTER_CHANGECollectionViewModelWrapper"; }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }
        #endregion
    }
}