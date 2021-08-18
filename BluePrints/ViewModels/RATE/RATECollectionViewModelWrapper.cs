using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
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
        protected PhaseType loadPhaseType;
        protected virtual CostType loadCostType => CostType.Charge;
        public bool IsConstructionPhase => loadPhaseType == PhaseType.Construct;
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (TripleEntitiesParameter<PROJECT, object, object>) parameter;
            loadPROJECT = PROJECTParameter.GetFirstEntity();
            loadPhaseType = (PhaseType)PROJECTParameter.GetSecondEntity();
            loadChargeType = (ChargeType)PROJECTParameter.GetThirdEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS, ESTIMATE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEM>> ESTIMATE_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.ESTIMATE.STATUS == BaselineStatus.Live && x.ESTIMATE.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
             return query => query.Where(x => (x.PHASE_TYPE == loadPhaseType && x.CHARGE_TYPE == loadChargeType));
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.RATES);
        }

        protected override Func<IRepositoryQuery<RATE>, IQueryable<RATE>> specifyMainViewModelProjection()
        {
            return query => rateCommodityProjection(query);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null) && x.PHASE_TYPE != PhaseType.Procurement);
        }

        protected virtual IQueryable<RATE> rateCommodityProjection(IRepositoryQuery<RATE> rates)
        {
            List<RATE> rateCollection = rates.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.CHARGE_TYPE == loadChargeType && x.PHASE_TYPE == loadPhaseType && x.COST_TYPE == loadCostType).ToList();
            rateCollection.ForEach(x => initializeRATE(x));

            return rateCollection.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RATE> entities)
        {
            MainViewModel.OnBeforePasteWithValidation = initializeRATE;
            MainViewModel.OnAfterProjectionSavedCallBack = OnAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        public override void UnifiedNewRowInitializationFromView(RATE projection)
        {
            compulsoryOnBeforeEntitySaved(projection);
            base.UnifiedNewRowInitializationFromView(projection);
        }

        protected virtual void populatePHASE(RATE entity)
        {
            if (entity.GUID_PHASE == null)
            {
                PHASE selectedPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == loadPhaseType && x.CHARGE_TYPE == ChargeType.Chargeable);
                if (selectedPHASE != null)
                {
                    entity.GUID_PHASE = selectedPHASE.GUID;
                    entity.CHARGE_TYPE = (ChargeType)selectedPHASE.CHARGE_TYPE;
                    entity.PHASE_TYPE = (PhaseType)selectedPHASE.PHASE_TYPE;
                    entity.COST_TYPE = CostType.Charge;
                }
            }
        }
        
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public virtual void OnAfterEntitySaved(RATE projection, RATE entity, bool isNewEntity)
        {
            this.RaisePropertyChanged(x => x.Entities);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(RATE projection, out bool isNew)
        {
            compulsoryOnBeforeEntitySaved(projection);
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        string errorMessage = "Duplicate entries exists";
        public override string UnifiedRowValidation(RATE projection)
        {
            //because projection commodity code is formatted to empty string when null before saving and this method is called before entity is going to be saved
            string commodityCode = projection.COMMODITY_CODE == null ? string.Empty : projection.COMMODITY_CODE;
            populatePHASE(projection);
            if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == commodityCode && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                return errorMessage;

            return string.Empty;
        }

        public override string UnifiedValueValidation(RATE projection, string field_name, object new_value, bool isPaste)
        {
            //do not validate phase type on new row because when enum is instantiated with default values user might get stucked in the cell due to duplication error
            if (projection.GUID != Guid.Empty && field_name == BindableBase.GetPropertyName(() => new RATE().Phase_Type))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == (PhaseType)new_value && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == projection.COMMODITY_CODE && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            if (field_name == BindableBase.GetPropertyName(() => new RATE().CHARGE_TYPE))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.Phase_Type && x.CHARGE_TYPE == (ChargeType)new_value && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == projection.COMMODITY_CODE && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_AREA))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == (Guid?)new_value && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == projection.COMMODITY_CODE && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_SUBAREA))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == (Guid?)new_value && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == projection.COMMODITY_CODE && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DEPARTMENT))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == (Guid?)new_value && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == projection.COMMODITY_CODE && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == (Guid?)new_value && x.COMMODITY_CODE == projection.COMMODITY_CODE && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().COMMODITY_CODE))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == new_value.ToString() && x.VARIATION_CODE == projection.VARIATION_CODE) && x.GUID != projection.GUID))
                    return errorMessage;
            }
            else if (field_name == BindableBase.GetPropertyName(() => new RATE().VARIATION_CODE))
            {
                if (Entities.Where(x => x.IsRateExists).Any(x => (x.PHASE_TYPE == projection.PHASE_TYPE && x.CHARGE_TYPE == projection.CHARGE_TYPE && x.GUID_AREA == projection.GUID_AREA && x.GUID_SUBAREA == projection.GUID_SUBAREA && x.GUID_DEPARTMENT == projection.GUID_DEPARTMENT && x.GUID_DISCIPLINE == projection.GUID_DISCIPLINE && x.COMMODITY_CODE == projection.COMMODITY_CODE && x.VARIATION_CODE == new_value.ToString()) && x.GUID != projection.GUID))
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

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, RATE projection, bool isNew)
        {
            compulsoryOnBeforeEntitySaved(projection);
            if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_AREA) || field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DEPARTMENT) || field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                populatePHASE(projection);
                projection.SetLookupProperties(CombinedCommodityCodeCollection, DISCIPLINECollection, SUBAREACollection);
            }

            //commodity code must be empty string to avoid ambiguity when querying
            if (projection.COMMODITY_CODE == null)
                projection.COMMODITY_CODE = string.Empty;

            populatePHASE(projection);
            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        protected void compulsoryOnBeforeEntitySaved(RATE entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;

            if (entity.COMMODITY_CODE == null)
                entity.COMMODITY_CODE = string.Empty;

            if (entity.VARIATION_CODE == null)
                entity.VARIATION_CODE = string.Empty;

            if (entity.IsGangRateCalculatable)
            {
                entity.RATE1 = entity.GangRate;
            }
        }
        #endregion

        #endregion

        #region View Properties
        public virtual void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_AREA) || 
                e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_SUBAREA) || e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_DEPARTMENT) || 
                e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                if (e.Row != null && e.Value == null)
                {
                    e.DisplayText = "Any";
                }
            }
            else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new RATE().COMMODITY_CODE)) && e.Row != null)
            {
                RATE projection = (RATE)e.Row;
                if(projection.COMMODITY_CODE == string.Empty || projection.COMMODITY_CODE == null)
                    e.DisplayText = "Any";
                else
                    e.DisplayText = projection.COMMODITY_CODE;
            }
            else if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new RATE().VARIATION_CODE)) && e.Row != null)
            {
                RATE projection = (RATE)e.Row;
                if (projection.VARIATION_CODE == string.Empty || projection.VARIATION_CODE == null)
                    e.DisplayText = "Any";
                else
                    e.DisplayText = projection.VARIATION_CODE;
            }
        }

        public bool CanRemoveDuplicates()
        {
            return !IsLoading;
        }

        public void RemoveDuplicates()
        {
            List<RATE> Rates = new List<RATE>(Entities);
            List<RATE> DeleteRates = new List<RATE>();
            for(int i = 0;i < Rates.Count;i++)
            {
                RATE entity = Rates[i];
                if(entity.IsRateExists)
                {
                    if (Rates.Any(x => x.IsRateExists && x.Phase_Type == entity.Phase_Type && x.CHARGE_TYPE == entity.CHARGE_TYPE && x.GUID_AREA == entity.GUID_AREA && x.GUID_DEPARTMENT == entity.GUID_DEPARTMENT && x.GUID_DISCIPLINE == entity.GUID_DISCIPLINE && x.COMMODITY_CODE == entity.COMMODITY_CODE && x.GUID != entity.GUID))
                    {
                        Rates.Remove(entity);
                        DeleteRates.Add(entity);
                    }
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

        public override void InitNewRow(InitNewRowEventArgs e)
        {
            var gridView = (TableView)e.OriginalSource;
            var grid = gridView.Grid;
            RATE projection = (RATE)grid.GetRow(e.RowHandle);
            initializeRATE(projection);
        }

        protected bool initializeRATE(RATE rate)
        {
            rate.SetLookupProperties(CombinedCommodityCodeCollection, DISCIPLINECollection, SUBAREACollection);

            rate.PHASE_TYPE = loadPhaseType;
            rate.COST_TYPE = loadCostType;
            rate.CHARGE_TYPE = loadChargeType;
            return true;
        }

        public IEnumerable<CombinedCommodityCode> CombinedCommodityCodeCollection
        {
            get
            {
                List<CombinedCommodityCode> combinedCommodityCodes = new List<CombinedCommodityCode>();
                if(loadCostType == CostType.Charge)
                {
                    PhaseType commodityPhaseType = loadChargeType == ChargeType.Chargeable ? PhaseType.Design : PhaseType.Indirect;
                    foreach(DOCTYPE doctype in DOCTYPECollection)
                    {
                        if (loadChargeType == ChargeType.NotChargeable || !doctype.IS_INDIRECT_ONLY)
                        {
                            IEnumerable<COMMODITY_CODE> findCOMMODITY_CODES = COMMODITY_CODECollection.Where(x => x.CODE == doctype.CODE);
                            foreach(COMMODITY_CODE findCOMMODITY_CODE in findCOMMODITY_CODES)
                            {
                                Guid? disciplineGuid = findCOMMODITY_CODE == null ? (Guid?)null : findCOMMODITY_CODE.GUID_DISCIPLINE;

                                CombinedCommodityCode newCommodityCode = new CombinedCommodityCode()
                                { PhaseType = commodityPhaseType, GuidDepartment = doctype.GUID_DDEPARTMENT, GuidDiscipline = disciplineGuid, Code = doctype.CODE, Key = doctype.GUID, Description = doctype.NAME };
                                combinedCommodityCodes.Add(newCommodityCode);
                            }
                        }
                    }
                }
                else if(loadCostType == CostType.Cost)
                    combinedCommodityCodes.AddRange(COMMODITY_CODECollection.Select(x => new CombinedCommodityCode() { PhaseType = x.PHASE_TYPE, GuidDepartment = null, GuidDiscipline = x.GUID_DISCIPLINE, Code = x.CODE, Key = x.GUID, Name = x.NAME, Description = x.DESCRIPTION }));

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

        public IEnumerable<string> VariationCodeCollection
        {
            get
            {
                var collection = GetEntities<ESTIMATE_ITEM>();
                return collection.Select(x => x.Variation_Code).Distinct().OrderBy(x => x);
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

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
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