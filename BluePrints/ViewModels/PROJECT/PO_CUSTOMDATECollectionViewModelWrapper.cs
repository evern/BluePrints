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
    public class FORECAST_PO_DATECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <FORECAST_PO, FORECAST_PO, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of FORECAST_PO_DATECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static FORECAST_PO_DATECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new FORECAST_PO_DATECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the FORECAST_PO_DATECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the FORECAST_PO_DATECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected FORECAST_PO_DATECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private FORECAST_PO loadFORECAST_PO;
        protected override void resolveParameters(object parameter)
        {
            loadFORECAST_PO = (FORECAST_PO)parameter;
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.FORECAST_POS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<FORECAST_PO>, IQueryable<FORECAST_PO>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadFORECAST_PO.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<FORECAST_PO> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public bool onBeforeEntitySaved(FORECAST_PO entity)
        {
            entity.GUID_PROJECT = loadFORECAST_PO.GUID;
            return true;
        }

        public override string UnifiedValueValidation(FORECAST_PO projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(FORECAST_PO projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "FORECAST_PO_DATECollectionViewModelWrapper"; }
        }

        #endregion
    }
}