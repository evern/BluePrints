using BaseModel.DataModel;
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
    public class DAYWORKCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <DAYWORK, DAYWORK, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of DAYWORKCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DAYWORKCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DAYWORKCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the DAYWORKCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the DAYWORKCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DAYWORKCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public PROJECT loadPROJECT { get; set; }
        protected override void resolveParameters(object parameter)
        {
            loadPROJECT = (PROJECT)parameter;
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_LABOURS, DAYWORK_LABOURProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_EQUIPMENTS, DAYWORK_EQUIPMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DAYWORK_STAFF_ROLES, DAYWORK_STAFF_ROLEProjectionFunc);
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
        }

        private Func<IRepositoryQuery<DAYWORK_LABOUR>, IQueryable<DAYWORK_LABOUR>> DAYWORK_LABOURProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DAYWORK_EQUIPMENT>, IQueryable<DAYWORK_EQUIPMENT>> DAYWORK_EQUIPMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DAYWORK_STAFF_ROLE>, IQueryable<DAYWORK_STAFF_ROLE>> DAYWORK_STAFF_ROLEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.DAYWORKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<DAYWORK>, IQueryable<DAYWORK>> specifyMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DAYWORK> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(DAYWORK projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(DAYWORK projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "DAYWORKCollectionViewModelWrapper"; }
        }

        #endregion
    }
}