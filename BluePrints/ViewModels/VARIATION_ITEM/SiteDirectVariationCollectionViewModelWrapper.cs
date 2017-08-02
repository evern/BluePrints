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
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProgress, ESTIMATION_DIRECT_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork>, ICollectionViewModelsWrapper<ESTIMATION_DIRECT_ITEMVariation>
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
        protected override IQueryable<ESTIMATION_DIRECT_ITEM> BaseEntityQueryCallBack(IRepositoryQuery<ESTIMATION_DIRECT_ITEM> query)
        {
            if (loadVARIATION.APPROVED == null)
                //When variation is not approved, retrieve current live deliverables and variation deliverables
                return query.Where(x => (x.GUID_ESTIMATION_DIRECT == load_context_guid) || (x.GUID_VARIATION == variation_guid && x.GUID_ESTIMATION_DIRECT == null));
            else
                //When variation is approved, retrieve deliverables from variation connected baseline
                return query.Where(x => x.GUID_ESTIMATION_DIRECT == variation_baseline_guid && x.GUID_VARIATION == variation_guid);
        }

        ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper;
        protected override IDeliverableCollectionViewModelWrapper<ESTIMATION_DIRECT_ITEMProgress, ESTIMATION_DIRECT_ITEM> collectionViewModelWrapper
        {
            get
            {
                if (ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null)
                {
                    ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper = ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.Create();
                    ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.SetParentViewModel(this);
                }


                return ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper;
            }
        }

        protected override string ViewName => "CONSTRUCTION_VARIATION_ITEMSViewModelWrapper";

        protected override void StartCreatingMainViewModel()
        {
            collectionViewModelWrapper.DefaultPhaseInternalNumber = BluePrintsResources.Default_Design_Phase;
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMVariation>> ConstructMainViewModelProjection()
        {
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS = new List<VARIATION_ITEM>();
            if (loaderCollection != null)
            {
                VARIATION_ITEMS = loaderCollection.GetCollection<VARIATION_ITEM>();
            }

            return query => Estimation_Direct_ItemVariationQuery.SiteDirectVariationItemTransformation(IReportableEntitiesCollection, loadVARIATION, VARIATION_ITEMS);
        }

        protected override void assign_additional_callbacks(CollectionViewModel<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork> mainViewModel)
        {
            mainViewModel.AdditionalValidateCellCallBack = AdditionalValidateCellCallBack;
        }

        private void AdditionalValidateCellCallBack(GridCellValidationEventArgs e)
        {
        }

        protected override void CellValueNewRowChanging(CellValueChangedEventArgs e)
        {
            base.CellValueNewRowChanging(e);
        }

        protected override void CellValueExistingRowChanging(CellValueChangedEventArgs e)
        {
            base.CellValueExistingRowChanging(e);
        }

        //when internal number is not unique, do not set internal number property
        protected override bool affixOtherFillDownAllowance(ESTIMATION_DIRECT_ITEMVariation fillDownEntity, string fieldName, object fillValue)
        {
            return true;
        }

        protected override bool affixOtherBulkEditAllowance(ESTIMATION_DIRECT_ITEMVariation projection, string fieldName, object editValue)
        {
            if (fieldName == BindableBase.GetPropertyName(() => new ESTIMATION_DIRECT_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS))
            {
                return true;
            }

            return false;
        }


        #region View Properties
        public IEnumerable<WORKPACK> WORKPACKCollection => ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null ? null : ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.WORKPACKCollection;
        public IEnumerable<PHASE> PHASECollection => ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null ? null : ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.PHASECollection;
        public IEnumerable<AREA> AREACollection => ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null ? null : ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.AREACollection;
        public IEnumerable<AREA> SUBAREACollection => ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null ? null : ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.SUBAREACollection;
        public IEnumerable<DEPARTMENT> DEPARTMENTCollection => null;
        public IEnumerable<DISCIPLINE> DISCIPLINECollection => ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null ? null : ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.DISCIPLINECollection;
        public IEnumerable<DOCTYPE> DOCTYPECollection => null;

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection => ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null ? null : ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.ProjectCOMMODITY_CODECollection;
        public IEnumerable<STOCK_CODE> STOCK_CODECollection => ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper == null ? null : ESTIMATION_DIRECT_ITEMCollectionViewModelWrapper.STOCK_CODECollection;

        #endregion
    }
}