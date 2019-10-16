using BaseModel.DataModel;
using BaseModel.Misc;
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
    public class RATECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of RATECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RATECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RATECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the RATECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the RATECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RATECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        protected PROJECT loadPROJECT;
        protected ChargeType loadChargeType;
        protected virtual CostType loadCostType => CostType.Charge;
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (DualEntitiesParameter<PROJECT, object>) parameter;
            loadPROJECT = PROJECTParameter.GetFirstEntity();
            loadChargeType = (ChargeType)PROJECTParameter.GetSecondEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            if(loadChargeType == ChargeType.Chargeable)
                return query => query.Where(x => (x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE == ChargeType.Chargeable));
            else
                return query => query.Where(x => (x.PHASE_TYPE == PhaseType.Indirect && x.CHARGE_TYPE == ChargeType.Chargeable));
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.RATES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<RATE>, IQueryable<RATE>> specifyMainViewModelProjection()
        {
            return query => rateCommodityProjection(query);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null) && x.PHASE_TYPE != PhaseType.Design);
        }

        protected virtual IQueryable<RATE> rateCommodityProjection(IRepositoryQuery<RATE> rates)
        {
            List<RATE> rateCollection = rates.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.CHARGE_TYPE == loadChargeType && x.COST_TYPE == loadCostType).ToList();
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
        public virtual bool OnBeforeEntitySaved(RATE entity)
        {
            compulsoryOnBeforeEntitySaved(entity);
            entity.COST_TYPE = CostType.Charge;
            entity.CHARGE_TYPE = loadChargeType;

            if(loadChargeType == ChargeType.Chargeable)
            {
                PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == "D1");
                if (findPHASE != null)
                    entity.GUID_PHASE = findPHASE.GUID;
            }
            else if(loadChargeType == ChargeType.NotChargeable)
            {
                PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == "I1");
                if (findPHASE != null)
                    entity.GUID_PHASE = findPHASE.GUID;
            }

            return true;
        }

        protected void compulsoryOnBeforeEntitySaved(RATE entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            if (entity.IsGangRateCalculatable)
            {
                entity.RATE1 = entity.GangRate;
            }
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnAfterEntitySaved(RATE projection, RATE entity, bool isNewEntity)
        {
            this.RaisePropertyChanged(x => x.DisplayEntities);
        }

        public override string UnifiedRowValidation(RATE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(RATE projection, string field_name, object new_value)
        {
            string errorMessage = "Duplicate entries by phase type, charge type, department, discipline and commodity";

            //do not validate phase type on new row because when enum is instantiated with default values user might get stucked in the cell due to duplication error
            if (projection.GUID != Guid.Empty && field_name == BindableBase.GetPropertyName(() => new RATE().Phase_Type))
            {
                if (DisplayEntities.Any(x => (x.PHASE_TYPE == (PhaseType)new_value && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.GUID_COMMODITY == projection.GUID_COMMODITY) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            if (field_name == BindableBase.GetPropertyName(() => new RATE().CHARGE_TYPE))
            {
                if (DisplayEntities.Any(x => (x.PHASE_TYPE == projection.Phase_Type && x.CHARGE_TYPE == (ChargeType)new_value && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.GUID_COMMODITY == projection.GUID_COMMODITY) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DEPARTMENT))
            {
                if (DisplayEntities.Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_DEPARTMENT == (Guid?)new_value && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.GUID_COMMODITY == projection.GUID_COMMODITY) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                if (DisplayEntities.Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == (Guid?)new_value && x.GUID_COMMODITY == projection.GUID_COMMODITY) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().CommodityCodeId))
            {
                if (DisplayEntities.Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.GUID_COMMODITY == (Guid?)new_value) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().RATE1))
            {
                if (projection.IsUsingGangRate)
                {
                    if (MessageBoxService.ShowMessage("Gang rate is being used, do you wish to clear the gang rate parameters and continue to manually set the rate?", "Confirmation", MessageButton.OKCancel, MessageIcon.Exclamation) == MessageResult.OK)
                    {
                        clearGangRate(projection);
                        return string.Empty;
                    }
                    else
                    {
                        return "Rate change cancelled";
                    }
                }
            }

            return string.Empty;
        }

        private void clearGangRate(RATE projection)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().MANAGER_PERCENT), projection.MANAGER_PERCENT, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().PRINCIPAL_PERCENT), projection.PRINCIPAL_PERCENT, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().LEAD_PERCENT), projection.LEAD_PERCENT, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().SENIOR_PERCENT), projection.SENIOR_PERCENT, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().ENGINEER_PERCENT), projection.ENGINEER_PERCENT, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().GRADUATE_PERCENT), projection.GRADUATE_PERCENT, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().UNDERGRADUATE_PERCENT), projection.UNDERGRADUATE_PERCENT, null, EntityMessageType.Changed);

            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().MANAGER_RATE), projection.MANAGER_RATE, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().PRINCIPAL_RATE), projection.PRINCIPAL_RATE, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().LEAD_RATE), projection.LEAD_RATE, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().SENIOR_RATE), projection.SENIOR_RATE, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().ENGINEER_RATE), projection.ENGINEER_RATE, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().GRADUATE_RATE), projection.GRADUATE_RATE, null, EntityMessageType.Changed);
            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().UNDERGRADUATE_RATE), projection.UNDERGRADUATE_RATE, null, EntityMessageType.Changed);

            projection.MANAGER_PERCENT = null;
            projection.PRINCIPAL_PERCENT = null;
            projection.LEAD_PERCENT = null;
            projection.SENIOR_PERCENT = null;
            projection.ENGINEER_PERCENT = null;
            projection.GRADUATE_PERCENT = null;
            projection.UNDERGRADUATE_PERCENT = null;

            projection.MANAGER_RATE = null;
            projection.PRINCIPAL_RATE = null;
            projection.LEAD_RATE = null;
            projection.SENIOR_RATE = null;
            projection.ENGINEER_RATE = null;
            projection.GRADUATE_RATE = null;
            projection.UNDERGRADUATE_RATE = null;

            projection.Update();
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, RATE projection, bool isNew)
        {      
            if (field_name == BindableBase.GetPropertyName(() => new RATE().Phase_Type))
            {
                //rate is not instantiated with commodity codes to be selected, hence initialization begins here
                if(isNew && new_value != null)
                {
                    projection.SetCommodityCodes(CombinedCommodityCodeCollection);
                }

                Guid? oldValue = projection.CommodityCodeId;
                Guid? newValue = null;
                projection.CommodityCodeId = newValue;
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().CommodityCodeId), oldValue, newValue, EntityMessageType.Changed);
                projection.Update();
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }
        #endregion

        #endregion

        #region View Properties
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().CommodityCodeId) ||
                e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_DEPARTMENT) ||
                e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                if (e.Row != null && e.Value == null)
                {
                    e.DisplayText = "Any";
                }
            }
        }

        public void RemoveDuplicates()
        {
            List<RATE> Rates = new List<RATE>(DisplayEntities);
            List<RATE> DeleteRates = new List<RATE>();
            for(int i = 0;i < Rates.Count;i++)
            {
                RATE entity = Rates[i];
                if (Rates.Any(x => x.Phase_Type == entity.Phase_Type && x.CHARGE_TYPE == entity.CHARGE_TYPE && x.GUID_DEPARTMENT == entity.GUID_DEPARTMENT && x.GUID_DISCIPLINE == entity.GUID_DISCIPLINE && x.GUID_COMMODITY == entity.GUID_COMMODITY && x.GUID != entity.GUID))
                {
                    Rates.Remove(entity);
                    DeleteRates.Add(entity);
                }
            }

            int removeCount = DeleteRates.Count;
            MainViewModel.BaseBulkDelete(DeleteRates);
            MessageBoxService.ShowMessage(removeCount + " duplicates entries removed");
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "RATECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "RATECollectionViewModelWrapper_v3"; }
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

        public IEnumerable<CombinedCommodityCode> CombinedCommodityCodeCollection
        {
            get
            {
                List<CombinedCommodityCode> combinedCommodityCodes = new List<CombinedCommodityCode>();
                if (DOCTYPECollection != null)
                    combinedCommodityCodes.AddRange(DOCTYPECollection.Select(x => new CombinedCommodityCode() { Code = x.CODE, Key = x.GUID, Description = x.NAME }));

                //if (COMMODITY_CODECollection != null)
                //    combinedCommodityCodes.AddRange(COMMODITY_CODECollection.Select(x => new CombinedCommodityCode() { PhaseType = PhaseType.Construct, Code = x.CODE, Key = x.GUID, Description = x.DESCRIPTION }));

                //if (COMMODITY_CODECollection != null)
                //    combinedCommodityCodes.AddRange(COMMODITY_CODECollection.Select(x => new CombinedCommodityCode() { PhaseType = PhaseType.Indirect, Code = x.CODE, Key = x.GUID, Description = x.DESCRIPTION }));

                return combinedCommodityCodes.OrderBy(x => x.Code);
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();

                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);

                return collection;
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();

                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);

                return collection;
            }
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);

                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);

                return collection;
            }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();

                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);

                return collection;
            }
        }
        #endregion
    }
}