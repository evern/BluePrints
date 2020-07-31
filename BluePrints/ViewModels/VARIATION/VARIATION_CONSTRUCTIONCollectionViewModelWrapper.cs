using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class VARIATION_CONSTRUCTIONCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <VARIATION_CONSTRUCTION, VARIATION_CONSTRUCTION, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATION_CONSTRUCTIONCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new VARIATION_CONSTRUCTIONCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the VARIATION_REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the VARIATION_REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected VARIATION_CONSTRUCTIONCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private Data.PROJECT loadPROJECT;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
        }
        
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTIONS);
        }

        protected override Func<IRepositoryQuery<VARIATION_CONSTRUCTION>, IQueryable<VARIATION_CONSTRUCTION>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION_CONSTRUCTION> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(VARIATION_CONSTRUCTION projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(VARIATION_CONSTRUCTION projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(VARIATION_CONSTRUCTION projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #endregion

        #region View Properties
        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public bool CanEdit()
        {
            if (SelectedEntity == null)
                return false;

            return true;
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            string view_name = "VARIATION_CONSTRUCTION_ITEMCollectionView";
            string tab_title = "Construction Variation";

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, VARIATION_CONSTRUCTION>(loadPROJECT, SelectedEntity), view_name, "[" + loadPROJECT.NUMBER + "] " + "[" + SelectedEntity.NUMBER + "] " + tab_title);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "VARIATION_CONSTRUCTIONCollectionViewModelWrapper_v2"; }
        }

        #endregion
    }
}