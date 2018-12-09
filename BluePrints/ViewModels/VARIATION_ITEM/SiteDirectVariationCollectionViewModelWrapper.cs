using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class SiteDirectVariationCollectionViewModelWrapper :
        BluePrintsEntitiesVariationCollectionWrapper
        <ESTIMATE_ITEM, ESTIMATE_ITEMProgress, ESTIMATE_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork>, ICollectionViewModelsWrapper<ESTIMATE_ITEMVariation>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SiteDirectVariationCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new SiteDirectVariationCollectionViewModelWrapper());
        }

        //have to implement this here because LINQ to entity doesn't support translating interface properties
        protected override IQueryable<ESTIMATE_ITEM> BaseEntityQueryCallBack(IRepositoryQuery<ESTIMATE_ITEM> query)
        {
            if (loadVARIATION.APPROVED == null)
                //When variation is not approved, retrieve current live deliverables and variation deliverables
                return query.Where(x => (x.GUID_ESTIMATE == load_context_guid) || (x.GUID_VARIATION == variation_guid && x.GUID_ESTIMATE == null));
            else
                //When variation is approved, retrieve deliverables from variation connected baseline
                return query.Where(x => x.GUID_ESTIMATE == variation_baseline_guid && x.GUID_VARIATION == variation_guid);
        }

        ESTIMATE_ITEMCollectionViewModelWrapper ESTIMATE_ITEMCollectionViewModelWrapper;
        protected override IDeliverableCollectionViewModelWrapper<ESTIMATE_ITEMProgress, ESTIMATE_ITEM> collectionViewModelWrapper
        {
            get
            {
                if (ESTIMATE_ITEMCollectionViewModelWrapper == null)
                {
                    ESTIMATE_ITEMCollectionViewModelWrapper = ESTIMATE_ITEMCollectionViewModelWrapper.Create();
                    ESTIMATE_ITEMCollectionViewModelWrapper.SetParentViewModel(this);
                }


                return ESTIMATE_ITEMCollectionViewModelWrapper;
            }
        }

        public override void FullRefresh()
        {
            ESTIMATE_ITEMCollectionViewModelWrapper = null;
            base.FullRefresh();
        }

        protected override string ViewName => "CONSTRUCTION_VARIATION_ITEMSViewModelWrapper" + loadPROJECT == null ? Guid.Empty.ToString() : loadPROJECT.GUID.ToString();

        protected override void StartCreatingMainViewModel()
        {
            collectionViewModelWrapper.DefaultPhaseInternalNumber = BluePrintsResources.Default_Design_Phase;
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS);
        }

        protected override Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEMVariation>> specifyMainViewModelProjection()
        {
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS = new List<VARIATION_ITEM>();
            if (loaderCollection != null)
            {
                VARIATION_ITEMS = loaderCollection.GetCollection<VARIATION_ITEM>();
            }

            return query => Estimation_Direct_ItemVariationQuery.SiteDirectVariationItemTransformation(IReportableEntitiesCollection, loadVARIATION, VARIATION_ITEMS);
        }

        protected override void assign_additional_callbacks(CollectionViewModel<ESTIMATE_ITEM, ESTIMATE_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork> mainViewModel, IEnumerable<ESTIMATE_ITEMVariation> entities)
        {
            mainViewModel.ManualRowPasteAction = ManualPasteAction;
        }

        public bool ManualPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, ESTIMATE_ITEMVariation pasteEntity)
        {
            return ((ESTIMATE_ITEMCollectionViewModelWrapper)collectionViewModelWrapper).ManualPasteAction(pasteData, pasteEntity.Entity);
        }

        //when internal number is not unique, do not set internal number property
        protected override bool affixOtherFillDownAllowance(ESTIMATE_ITEMVariation fillDownEntity, string fieldName, object fillValue)
        {
            return true;
        }

        protected override bool affixOtherBulkEditAllowance(ESTIMATE_ITEMVariation projection, string fieldName, object editValue)
        {
            if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATE_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS))
            {
                return true;
            }

            return false;
        }

        #region View Properties
        public IEnumerable<SUBJOB> SUBJOBCollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.SUBJOBCollection;
        public IEnumerable<PHASE> PHASECollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.PHASECollection;
        public IEnumerable<AREA> AREACollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.AREACollection;
        public IEnumerable<AREA> SUBAREACollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.SUBAREACollection;
        public IEnumerable<DEPARTMENT> DEPARTMENTCollection => null;
        public IEnumerable<DISCIPLINE> DISCIPLINECollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.DISCIPLINECollection;
        public IEnumerable<DOCTYPE> DOCTYPECollection => null;

        public IEnumerable<STOCK_GROUP> STOCK_GROUPCollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.STOCK_GROUPCollection;
        public IEnumerable<STOCK_CODE> STOCK_CODECollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.STOCK_CODECollection;
        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection => ESTIMATE_ITEMCollectionViewModelWrapper == null ? null : ESTIMATE_ITEMCollectionViewModelWrapper.COMMODITY_CODECollection;

        public IEnumerable<SUBJOB> ProcurementSUBJOBCollection
        {
            get
            {
                var collection = SUBJOBCollection;
                if (collection != null)
                    collection = SUBJOBCollection.Where(x => x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Indirect).OrderBy(x => x.INTERNAL_NAME1);
                return collection;
            }
        }
        #endregion
    }
}