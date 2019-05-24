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
    public class PO_CUSTOMDATECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PO_CUSTOMDATE, PO_CUSTOMDATE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PO_CUSTOMDATECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PO_CUSTOMDATECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PO_CUSTOMDATECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the PO_CUSTOMDATECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PO_CUSTOMDATECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PO_CUSTOMDATECollectionViewModelWrapper(
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
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PO_CUSTOMDATES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PO_CUSTOMDATE>, IQueryable<PO_CUSTOMDATE>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.FORECAST_PO_GUID == loadFORECAST_PO.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PO_CUSTOMDATE> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public bool onBeforeEntitySaved(PO_CUSTOMDATE entity)
        {
            entity.FORECAST_PO_GUID = loadFORECAST_PO.GUID;
            return true;
        }

        public override string UnifiedValueValidation(PO_CUSTOMDATE projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(PO_CUSTOMDATE projection)
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
            get { return "PO_CUSTOMDATECollectionViewModelWrapper"; }
        }

        #endregion
    }
}