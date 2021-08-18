using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class CONSTRUCTION_STAGECollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <CONSTRUCTION_STAGE, CONSTRUCTION_STAGE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of CONSTRUCTION_STAGECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static CONSTRUCTION_STAGECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new CONSTRUCTION_STAGECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the CONSTRUCTION_STAGECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the CONSTRUCTION_STAGECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected CONSTRUCTION_STAGECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT contextPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            contextPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.CONSTRUCTION_STAGES);
        }

        protected override Func<IRepositoryQuery<CONSTRUCTION_STAGE>, IQueryable<CONSTRUCTION_STAGE>> specifyMainViewModelProjection()
        {
            return query => mainViewModelFilter(query);
        }

        private IQueryable<CONSTRUCTION_STAGE> mainViewModelFilter(IQueryable<CONSTRUCTION_STAGE> query)
        {
            return query.Where(x => x.GUID_PROJECT == contextPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<CONSTRUCTION_STAGE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            updateDependencies();
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if (changedType == typeof(CONSTRUCTION_STAGE))
            {
                updateDependencies();
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        private void updateDependencies()
        {
            List<CONSTRUCTION_STAGE> stages = MainViewModel.Entities.ToList();
            foreach(CONSTRUCTION_STAGE stage in stages)
            {
                //Need to raise property change to stimulate IDXDataErrorInfo
                stage.OtherConstructionStage = MainViewModel.Entities;
                stage.Update();
            }
        }
        #endregion

        #region Saving Behavior
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(CONSTRUCTION_STAGE projection, out bool isNew)
        {
            if(projection.SORT_ORDER == 0)
            {
                List<CONSTRUCTION_STAGE> unsavedEntities = new List<CONSTRUCTION_STAGE>();
                //projection.EntityNumber = StringFormatUtils.GetNewRegisterNumber(MainViewModel.Entities, unsavedEntities, MainViewModel.SelectedEntities, projection.EntityGroup);
            }

            projection.GUID_PROJECT = contextPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }
        public override string UnifiedRowValidation(CONSTRUCTION_STAGE projection)
        {
            if (MainViewModel.Entities.Where(x => x.GUID != projection.GUID).Any(x => x.SCORE_CARD_DISCIPLINE == projection.SCORE_CARD_DISCIPLINE && x.SORT_ORDER == projection.SORT_ORDER))
                return "ID of " + projection.SORT_ORDER.ToString() + " already exist for " + projection.SCORE_CARD_DISCIPLINE.ToString();

            return string.Empty;
        }

        public override string UnifiedValueValidation(CONSTRUCTION_STAGE projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override void UnifiedNewRowInitializationFromView(CONSTRUCTION_STAGE projection)
        {
            projection.OtherConstructionStage = MainViewModel.Entities;
            base.UnifiedNewRowInitializationFromView(projection);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, CONSTRUCTION_STAGE projection, bool isNew)
        {
            if (field_name == GetPropertyName(() => new CONSTRUCTION_STAGE().SCORE_CARD_DISCIPLINE))
            {
                List<CONSTRUCTION_STAGE> unsavedEntities = new List<CONSTRUCTION_STAGE>();
                projection.SCORE_CARD_DISCIPLINE = (ScoreCardDiscipline)new_value;
                updateProjectionEntityNumber(projection);
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, CONSTRUCTION_STAGE projection, bool isNew)
        {
            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        #region IEntityNumber
        protected override int DefaultNumericFieldLength()
        {
            return 1;
        }

        protected override string GetEntityNumberFieldName()
        {
            return GetPropertyName(() => new CONSTRUCTION_STAGE().SCORE_CARD_DISCIPLINE);
        }
        #endregion
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "CONSTRUCTION_STAGECollectionViewModelWrapper_v1"; }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                return GetEntities<COMMODITY_CODE>();
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                return GetEntities<DISCIPLINE>();
            }
        }
        #endregion
    }
}