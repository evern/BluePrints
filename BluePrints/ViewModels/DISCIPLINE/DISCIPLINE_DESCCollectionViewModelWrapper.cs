using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class DISCIPLINE_DESCCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <DISCIPLINE_DESC, DISCIPLINE_DESC, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of DISCIPLINE_DESCCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DISCIPLINE_DESCCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DISCIPLINE_DESCCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the DISCIPLINE_DESCCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the DISCIPLINE_DESCCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DISCIPLINE_DESCCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        public string DisciplineRegex { get; set; }
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity(); 
            DisciplineRegex = BluePrintsResources.Regex_DISCIPLINE;
        }

        protected override void addEntitiesLoader()
        {
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINE_DESCS);
        }

        protected override Func<IRepositoryQuery<DISCIPLINE_DESC>, IQueryable<DISCIPLINE_DESC>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DISCIPLINE_DESC> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(DISCIPLINE_DESC projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(DISCIPLINE_DESC projection)
        {
            return string.Empty;
        }
        #endregion

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(DISCIPLINE_DESC projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "DISCIPLINE_DESCCollectionViewModelWrapper_v1"; }
        }

        #endregion
    }
}