using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseModel.ViewModel.Document;

namespace BluePrints.ViewModels
{
    public class TENDER_PROFILECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <TENDER_PROFILE, TENDER_PROFILE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of TENDER_PROFILECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TENDER_PROFILECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new TENDER_PROFILECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the TENDER_PROFILECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the TENDER_PROFILECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected TENDER_PROFILECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {

        }

        protected override void addEntitiesLoader()
        {
        }
        
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.TENDER_PROFILES);
        }

        protected override Func<IRepositoryQuery<TENDER_PROFILE>, IQueryable<TENDER_PROFILE>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TENDER_PROFILE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(TENDER_PROFILE entity)
        {
            return true;
        }

        public override string UnifiedRowValidation(TENDER_PROFILE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(TENDER_PROFILE projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        #endregion

        #endregion

        #region View Properties
        public bool CanEdit()
        {
            if (SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new EntitiesParameter<TENDER_PROFILE>(SelectedEntity) , "TENDER_PROFILE_ITEMCollectionView", "Tender Profile Items");
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "TENDER_PROFILECollectionViewModelWrapper_v2"; }
        }

        #endregion
    }
}