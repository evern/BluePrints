using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
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
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        PhaseType loadPhaseType = PhaseType.Construct;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (DualEntitiesParameter<PROJECT, object>) parameter;
            loadPROJECT = PROJECTParameter.GetFirstEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            loadPhaseType = (PhaseType)PROJECTParameter.GetSecondEntity();
        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
        }

        protected override Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == loadPhaseType);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RATE> entities)
        {
            MainViewModel.OnBeforeEntityDeletedIsContinueCallBack = onBeforeEntityDeleted;
            MainViewModel.OnBeforeEntitiesDeleteIsContinueCallBack = onBeforeEntitiesDeleted;
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public bool IsDepartmentVisible => loadPhaseType != PhaseType.Construct;
        List<TransactionRate> transactionRates = new List<TransactionRate>();
        List<ExoDataPoint> actualDataPoints;
        protected override IQueryable<RATE> rateCommodityProjection(IRepositoryQuery<RATE> rates)
        {
            List<RATE> returnRATES = new List<RATE>();
            List<RATE> committedRATES = rates.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COST_TYPE == CostType.Cost && x.PHASE_TYPE == loadPhaseType).ToList();

            if(actualDataPoints == null)
            {
                actualDataPoints = new List<ExoDataPoint>();
                List<ExoDataPoint> burnedDataPoints = BluePrintsDataUtils.GetBurned(primeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, null, 1, true);
                List<ExoDataPoint> materialDataPoints = BluePrintsDataUtils.GetMaterials(primeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, 1, true);

                actualDataPoints.AddRange(burnedDataPoints);
                actualDataPoints.AddRange(materialDataPoints);
                transactionRates = actualDataPoints.GroupBy(x => new { SubjobCode = x.Subjob_Name, DisciplineCode = x.CostGroup, CommodityCode = x.CostType }).Select(group => new TransactionRate() { RawSubjobCode = group.Key.SubjobCode, RawDisciplineCode = group.Key.DisciplineCode, RawCommodityCode = group.Key.CommodityCode, Transactions = group.ToList() }).ToList();
            }

            foreach (RATE committedRATE in committedRATES)
            {
                initializeRATE(committedRATE);
                setRecommendedRate(committedRATE);
                returnRATES.Add(committedRATE);
            }

            foreach (var transactionRate in transactionRates)
            {
                DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.CODE == transactionRate.DisciplineCode);
                COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODECollection.FirstOrDefault(x => x.CODE == transactionRate.CommodityCode);
                PHASE findPHASE = PHASECollection.FirstOrDefault(x => transactionRate.RawSubjobCode.Contains(x.INTERNAL_NUM));

                if(findPHASE != null && findPHASE.PHASE_TYPE != null && findPHASE.PHASE_TYPE == loadPhaseType && findPHASE.CHARGE_TYPE == ChargeType.Chargeable && findDISCIPLINE != null && findCOMMODITY_CODE != null)
                {
                    IEnumerable<RATE> findCommittedRATES = returnRATES.Where(x => x.GUID_PHASE == findPHASE.GUID && x.GUID_DISCIPLINE == findDISCIPLINE.GUID && x.COMMODITY_CODE == findCOMMODITY_CODE.CODE);
                    if(findCommittedRATES.Count() == 0)
                    {
                        RATE uncommittedRATE = new RATE() { GUID = Guid.Empty, GUID_PHASE = findPHASE.GUID, GUID_DISCIPLINE = findDISCIPLINE.GUID, COMMODITY_CODE = findCOMMODITY_CODE.CODE };
                        uncommittedRATE.PHASE_TYPE = (PhaseType)findPHASE.PHASE_TYPE;

                        initializeRATE(uncommittedRATE);
                        setRecommendedRate(uncommittedRATE);
                        returnRATES.Add(uncommittedRATE);
                    }
                }
            }

            //List<ExoDataPoint> burnedDataPoints = BluePrintsDataUtils.GetBurned(primero)
            return returnRATES.AsQueryable();
        }

        public void UpdateFloatingRates()
        {
            if (MessageBoxService.ShowMessage("This will update all rates that have floating ticked to recommended rate, do you wish to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            List<RATE> saveEntities = new List<RATE>();
            foreach(RATE entity in DisplayEntities.Where(x => x.IsRateExists && x.IS_FLOATING))
            {
                entity.RATE1 = entity.RecommendedRate;
                saveEntities.Add(entity);
            }

            MainViewModel.BulkSave(saveEntities);
        }

        private void setRecommendedRate(RATE rate, bool isEditRateField = false)
        {
            //try to retrieve rate's discipline if GUID_DISCIPLINE is not null
            DISCIPLINE findDISCIPLINE = rate.DISCIPLINE;
            if(findDISCIPLINE == null && rate.GUID_DISCIPLINE != null)
                findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == rate.GUID_DISCIPLINE);

            PHASE findPHASE = rate.PHASE;
            if (findPHASE == null && rate.GUID_PHASE != null)
                findPHASE = PHASECollection.FirstOrDefault(x => x.GUID == rate.GUID_PHASE);

            IEnumerable<TransactionRate> transactionRatesByPhase = transactionRates.Where(x => (findPHASE == null || x.RawSubjobCode.Contains(findPHASE.INTERNAL_NUM)));
            IEnumerable<TransactionRate> transactionRatesByDiscipline = transactionRatesByPhase.Where(x => (findDISCIPLINE == null || x.DisciplineCode == findDISCIPLINE.CODE));
            IEnumerable<TransactionRate> transactionRatesByCommodity = transactionRatesByDiscipline.Where(x => (rate.COMMODITY_CODE == string.Empty || rate.COMMODITY_CODE == null || x.CommodityCode == rate.COMMODITY_CODE));
            List <TransactionRate> burned = transactionRatesByCommodity.ToList();
            if (burned.Count > 0)
                rate.Transactions = burned.SelectMany(x => x.Transactions).ToList();

            if (!rate.IsRateExists && !isEditRateField)
                rate.RATE1 = rate.RecommendedRate;
        }

        #region Collection Call Backs
        //skip inactive entity
        protected DeleteInterceptMode onBeforeEntityDeleted(RATE entity)
        {
            if (!entity.IsRateExists)
                return DeleteInterceptMode.Skip;
            else
                return DeleteInterceptMode.Continue;
        }

        //disallow deletion of projection when it's not active
        protected virtual bool onBeforeEntitiesDeleted(IEnumerable<RATE> entities)
        {
            if (entities.All(x => x.IsRateExists))
                return true;
            else if (entities.All(x => !x.IsRateExists))
            {
                MessageBoxService.ShowMessage("Cannot delete selected rate(s) because they aren't active", "Error", MessageButton.OK);
                return false;
            }
            else if (MessageBoxService.ShowMessage("Only active rate(s) will be deleted, do you wish to continue?", "Warning", MessageButton.OKCancel) == MessageResult.Cancel)
            {
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public override bool OnBeforeEntitySaved(RATE entity)
        {
            compulsoryOnBeforeEntitySaved(entity);
            entity.COST_TYPE = CostType.Cost;
            populatePHASE(entity);

            if (entity.COMMODITY_CODE == null)
                entity.COMMODITY_CODE = string.Empty;

            return true;
        }

        public override void OnAfterEntitySaved(RATE projection, RATE entity, bool isNewEntity)
        {
            RATE uncommittedRATE = DisplayEntities.FirstOrDefault(x => !x.IsRateExists && x.GUID != entity.GUID && x.GUID_PHASE == entity.GUID_PHASE && x.GUID_DISCIPLINE == entity.GUID_DISCIPLINE && x.GUID_DEPARTMENT == entity.GUID_DEPARTMENT && x.COMMODITY_CODE == entity.COMMODITY_CODE);
            if (uncommittedRATE != null)
                DisplayEntities.Remove(uncommittedRATE);
            base.OnAfterEntitySaved(projection, entity, isNewEntity);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, RATE projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_PHASE) || field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                //rate is not instantiated with commodity codes to be selected, hence initialization begins here
                if (isNew && new_value != null)
                {
                    projection.SetLookupProperties(CombinedCommodityCodeCollection, DISCIPLINECollection);
                }
                //Guid? oldValue = projection.GUID_COMMODITY_CODE;
                //Guid? newValue = null;
                //projection.GUID_COMMODITY_CODE = newValue;
                //projection.ManualCOMMODITY_CODE = null;
                //MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                //MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().GUID_COMMODITY_CODE), oldValue, newValue, EntityMessageType.Changed);
                projection.Update();
            }

            projection.Update();
            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, RATE projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_PHASE) || field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                populatePHASE(projection);
                projection.SetLookupProperties(CombinedCommodityCodeCollection, DISCIPLINECollection);
            }

            setRecommendedRate(projection, field_name == BindableBase.GetPropertyName(() => new RATE().RATE1));
        }

        public override string UnifiedValueValidation(RATE projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new RATE().GUID_DEPARTMENT))
            {
                if(projection.GUID_PHASE != null)
                {
                    PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.GUID == projection.GUID_PHASE);
                    if(findPHASE != null)
                    {
                        if (findPHASE.PHASE_TYPE != PhaseType.Design)
                            return "Department is only applicable for design phase";
                    }
                }

                populatePHASE(projection);
                projection.SetLookupProperties(CombinedCommodityCodeCollection, DISCIPLINECollection);

                //set recommended rate here so that paste data will pick it up
                setRecommendedRate(projection);
            }
            else if(field_name == BindableBase.GetPropertyName(() => new RATE().IsRateExists))
            {
                if(!isPaste && new_value != null && !(bool)new_value)
                {
                    return "Cannot set inactive, please delete this rate instead";
                }
            }

            return base.UnifiedValueValidation(projection, field_name, new_value, isPaste);
        }

        protected override void populatePHASE(RATE entity)
        {
            PHASE selectedPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == loadPhaseType && x.CHARGE_TYPE == ChargeType.Chargeable);
            if (selectedPHASE != null)
            {
                entity.GUID_PHASE = selectedPHASE.GUID;
                entity.CHARGE_TYPE = (ChargeType)selectedPHASE.CHARGE_TYPE;
                entity.PHASE_TYPE = (PhaseType)selectedPHASE.PHASE_TYPE;
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

    public class TransactionRate
    {
        public string RawSubjobCode { get; set; }
        public string RawDisciplineCode { get; set; }
        public string RawCommodityCode { get; set; }
        public decimal AverageRate => Transactions == null ? 0 : Transactions.Average(x => x.CostPerQty);
        public decimal TransactionCount => Transactions == null ? 0 : Transactions.Count;
        public string DisciplineCode => RawDisciplineCode.Length >= 2 ? RawDisciplineCode.Substring(0, 2) : string.Empty;
        public string CommodityCode => RawCommodityCode.Length >= 3 ? RawCommodityCode.Substring(0, 3) : string.Empty;

        public List<ExoDataPoint> Transactions { get; set; }
    }
}