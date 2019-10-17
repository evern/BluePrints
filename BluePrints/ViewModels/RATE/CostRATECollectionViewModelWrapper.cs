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
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
        }

        protected override Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query;
        }

        List<TransactionRate> transactionRates = new List<TransactionRate>();
        List<ExoDataPoint> actualDataPoints;
        protected override IQueryable<RATE> rateCommodityProjection(IRepositoryQuery<RATE> rates)
        {
            List<RATE> returnRATES = new List<RATE>();
            List<RATE> committedRATES = rates.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.COST_TYPE == CostType.Cost).ToList();

            if(actualDataPoints == null)
            {
                actualDataPoints = new List<ExoDataPoint>();
                List<ExoDataPoint> burnedDataPoints = BluePrintsDataUtils.GetBurned(primeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, null, 1, true);
                List<ExoDataPoint> materialDataPoints = BluePrintsDataUtils.GetMaterials(primeroUnitOfWork, loadPROJECT.NUMBER, DateTime.Now, null, 1, true);

                actualDataPoints.AddRange(burnedDataPoints);
                actualDataPoints.AddRange(materialDataPoints);
                transactionRates = actualDataPoints.GroupBy(x => new { DisciplineCode = x.CostGroup, CommodityCode = x.CostType }).Select(group => new TransactionRate() { RawDisciplineCode = group.Key.DisciplineCode, RawCommodityCode = group.Key.CommodityCode, Transactions = group.ToList() }).ToList();
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

                if(findDISCIPLINE != null && findCOMMODITY_CODE != null)
                {
                    IEnumerable<RATE> findCommittedRATES = committedRATES.Where(x => x.GUID_DISCIPLINE == findDISCIPLINE.GUID && x.COMMODITY_CODE == findCOMMODITY_CODE.CODE);
                    if(findCommittedRATES.Count() == 0)
                    {
                        PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == findCOMMODITY_CODE.PHASE_TYPE);
                        RATE uncommittedRATE = new RATE() { GUID = Guid.Empty, GUID_PHASE = findPHASE == null ? (Guid?)null : findPHASE.GUID, GUID_DISCIPLINE = findDISCIPLINE.GUID, COMMODITY_CODE = findCOMMODITY_CODE.CODE, RATE1 = transactionRate.AverageRate, Transactions = transactionRate.Transactions.ToList() };
                        uncommittedRATE.PHASE_TYPE = findCOMMODITY_CODE.PHASE_TYPE;

                        initializeRATE(uncommittedRATE);
                        returnRATES.Add(uncommittedRATE);
                    }
                }
            }

            //List<ExoDataPoint> burnedDataPoints = BluePrintsDataUtils.GetBurned(primero)
            return returnRATES.AsQueryable();
        }

        private void setRecommendedRate(RATE rate)
        {
            if (rate.DISCIPLINE != null && rate.COMMODITY_CODE != null)
            {
                List<TransactionRate> burned = transactionRates.Where(x => x.DisciplineCode == rate.DISCIPLINE.CODE && x.CommodityCode == rate.COMMODITY_CODE).ToList();
                if (burned.Count > 0)
                    rate.Transactions = burned.SelectMany(x => x.Transactions).ToList();
            }
            else if(rate.COMMODITY_CODE != null && rate.DISCIPLINE == null)
            {
                List<TransactionRate> burned = transactionRates.Where(x => x.CommodityCode == rate.COMMODITY_CODE).ToList();
                if (burned.Count > 0)
                    rate.Transactions = burned.SelectMany(x => x.Transactions).ToList();
            }
            else if (rate.COMMODITY_CODE == null && rate.DISCIPLINE != null)
            {
                List<TransactionRate> burned = transactionRates.Where(x => x.DisciplineCode == rate.DISCIPLINE.CODE).ToList();
                if (burned.Count > 0)
                    rate.Transactions = burned.SelectMany(x => x.Transactions).ToList();
            }
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

            setRecommendedRate(projection);
        }

        public override string UnifiedValueValidation(RATE projection, string field_name, object new_value)
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
                setRecommendedRate(projection);
            }

            return base.UnifiedValueValidation(projection, field_name, new_value);
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

    public class TransactionRate
    {
        public string RawDisciplineCode { get; set; }
        public string RawCommodityCode { get; set; }
        public decimal AverageRate => Transactions == null ? 0 : Transactions.Average(x => x.CostPerQty);
        public decimal TransactionCount => Transactions == null ? 0 : Transactions.Count;
        public string DisciplineCode => RawDisciplineCode.Length >= 2 ? RawDisciplineCode.Substring(0, 2) : string.Empty;
        public string CommodityCode => RawCommodityCode.Length >= 3 ? RawCommodityCode.Substring(0, 3) : string.Empty;

        public List<ExoDataPoint> Transactions { get; set; }
    }
}