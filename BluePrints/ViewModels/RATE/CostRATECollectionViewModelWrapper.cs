using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class CostRATECollectionViewModelWrapper : RATECollectionViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of CostRATECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static CostRATECollectionViewModelWrapper CreateCostRate(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new CostRATECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the CostRATECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the CostRATECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected CostRATECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        protected override CostType loadCostType => CostType.Cost;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
        }

        protected override Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query;
        }

        protected override IQueryable<RATE> rateCommodityProjection(IRepositoryQuery<RATE> rates)
        {
            //List<ExoDataPoint> burnedDataPoints = BluePrintsDataUtils.GetBurned(primero)
            List<RATE> rateCollection = rates.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COST_TYPE == CostType.Cost).ToList();
            rateCollection.ForEach(x => x.SetCommodityCodes(CombinedCommodityCodeCollection));

            return rateCollection.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RATE> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public override bool OnBeforeEntitySaved(RATE entity)
        {
            compulsoryOnBeforeEntitySaved(entity);
            entity.COST_TYPE = CostType.Cost;
            populatePHASE(entity);

            return true;
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, RATE projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_PHASE) || field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                //rate is not instantiated with commodity codes to be selected, hence initialization begins here
                if (isNew && new_value != null)
                {
                    projection.SetCommodityCodes(CombinedCommodityCodeCollection);
                }

                //Guid? oldValue = projection.GUID_COMMODITY_CODE;
                //Guid? newValue = null;
                //projection.GUID_COMMODITY_CODE = newValue;
                //projection.ManualCOMMODITY_CODE = null;
                //MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                //MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().GUID_COMMODITY_CODE), oldValue, newValue, EntityMessageType.Changed);
                projection.Update();
            }
            else if(field_name == BindableBase.GetPropertyName(() => new RATE().GUID_COMMODITY))
            {
                if (new_value != null)
                {
                    //need to set it immediately so CustomColumnDisplayText will show the updated value without user having to exit the cell
                    COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    if (findCOMMODITY_CODE != null)
                        projection.ManualCOMMODITY_CODE = findCOMMODITY_CODE;
                }
                else
                {
                    projection.ManualCOMMODITY_CODE = null;
                    projection.COMMODITY_CODE = null;
                }

                projection.Update();
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, RATE projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_PHASE) || field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                populatePHASE(projection);
                projection.SetCommodityCodes(CombinedCommodityCodeCollection);
            }
        }

        protected override void populatePHASE(RATE entity)
        {
            if (entity.GUID_PHASE != null)
            {
                PHASE selectedPHASE = PHASECollection.FirstOrDefault(x => x.GUID == (Guid)entity.GUID_PHASE);
                if (selectedPHASE != null)
                {
                    entity.CHARGE_TYPE = (ChargeType)selectedPHASE.CHARGE_TYPE;
                    entity.PHASE_TYPE = (PhaseType)selectedPHASE.PHASE_TYPE;
                }
            }
        }

        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "CostRATECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "CostRATECollectionViewModelWrapper_v2"; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }
        #endregion
    }
}