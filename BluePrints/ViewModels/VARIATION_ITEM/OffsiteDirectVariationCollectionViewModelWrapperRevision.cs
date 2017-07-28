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
    public partial class OffsiteDirectVariationCollectionViewModelWrapperRevision :
        BluePrintsEntitiesVariationCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProgress, BASELINE_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectVariationCollectionViewModelWrapperRevision Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectVariationCollectionViewModelWrapperRevision());
        }

        BASELINE_ITEMCollectionViewModelWrapper baseline_itemCollectionViewModelWrapper;
        protected override IDeliverableCollectionViewModelWrapper<BASELINE_ITEMProgress, BASELINE_ITEM> collectionViewModelWrapper
        {
            get
            {
                if (baseline_itemCollectionViewModelWrapper == null)
                    baseline_itemCollectionViewModelWrapper = BASELINE_ITEMCollectionViewModelWrapper.Create();

                return baseline_itemCollectionViewModelWrapper;
            }
        }

        protected override string ViewName => "VARIATION_ITEMSViewModelWrapper";

        protected override void StartCreatingMainViewModel()
        {
            collectionViewModelWrapper.DefaultPhaseInternalNumber = BluePrintsResources.Default_Design_Phase;
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMVariation>> ConstructMainViewModelProjection()
        {
            var VARIATION = loaderCollection.GetObject<VARIATION>();
            var VARIATION_ITEMS = loaderCollection.GetCollection<VARIATION_ITEM>();

            return query => Baseline_ItemVariationQuery.OffsiteDirectVariationItemTransformation(IReportableEntitiesCollection, collectionViewModelWrapper.loadBASELINE, VARIATION, VARIATION_ITEMS);
        }

        protected override void assign_additional_callbacks(CollectionViewModel<BASELINE_ITEM, BASELINE_ITEMVariation, Guid, IBluePrintsEntitiesUnitOfWork> mainViewModel)
        {
            mainViewModel.AdditionalValidateCellCallBack = AdditionalValidateCellCallBack;
        }

        private void AdditionalValidateCellCallBack(GridCellValidationEventArgs e)
        {
            //estimated hours field is disabled but just in case
            if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                BASELINE_ITEMVariation validateEntity = (BASELINE_ITEMVariation)e.Row;
                if (validateEntity.Entity.Entity.Entity.BY_DURATION && ((decimal)e.Value) > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Cannot set estimated hours when deliverable is by duration";
                }
            }
            //this is not likely to happen, because variation isn't trackable yet but just in case
            else if (e.Column.FieldName == baseEntityString + BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                BASELINE_ITEMVariation validateEntity = (BASELINE_ITEMVariation)e.Row;
                if (validateEntity.Entity.Earned_Units_Total > 0)
                {
                    e.IsValid = false;
                    e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
                    e.ErrorContent = "Cannot change deliverable tracking type when percentage is already earned";
                }
            }
        }

        //when internal number is not unique, do not set internal number property
        protected override bool affixOtherFillDownAllowance(BASELINE_ITEMVariation fillDownEntity, string fieldName, object fillValue)
        {
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().Entity)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity)
                + "."
                + BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                var errorMessage = string.Empty;
                MainViewModel.IsValidEntityCellValue(fillDownEntity, fieldName, fillValue, ref errorMessage);
                if (errorMessage != string.Empty)
                    return false;
            }

            return true;
        }

        protected override bool affixOtherBulkEditAllowance(BASELINE_ITEMVariation projection, string fieldName, object editValue)
        {
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().Entity)
                            + "."
                            + BindableBase.GetPropertyName(() => new BASELINE_ITEMProjection().Entity)
                            + "."
                            + BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                var errorMessage = string.Empty;
                MainViewModel.IsValidEntityCellValue(projection, fieldName, editValue, ref errorMessage);
                if (errorMessage != string.Empty)
                    return false;
                else
                    return true;
            }

            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEMVariation().VARIATION_ITEM)
                + "."
                + BindableBase.GetPropertyName(() => new VARIATION_ITEM().VARIATION_UNITS))
            {
                return true;
            }

            return false;
        }


        #region View Properties
        public IEnumerable<WORKPACK> WORKPACKCollection => baseline_itemCollectionViewModelWrapper == null ? null : baseline_itemCollectionViewModelWrapper.WORKPACKCollection;
        public IEnumerable<PHASE> PHASECollection => baseline_itemCollectionViewModelWrapper == null ? null : baseline_itemCollectionViewModelWrapper.PHASECollection;
        public IEnumerable<AREA> AREACollection => baseline_itemCollectionViewModelWrapper == null ? null : baseline_itemCollectionViewModelWrapper.AREACollection;
        public IEnumerable<AREA> SUBAREACollection => baseline_itemCollectionViewModelWrapper == null ? null : baseline_itemCollectionViewModelWrapper.SUBAREACollection;
        public IEnumerable<DEPARTMENT> DEPARTMENTCollection => baseline_itemCollectionViewModelWrapper == null ? null : baseline_itemCollectionViewModelWrapper.DEPARTMENTCollection;
        public IEnumerable<DISCIPLINE> DISCIPLINECollection => baseline_itemCollectionViewModelWrapper == null ? null : baseline_itemCollectionViewModelWrapper.DISCIPLINECollection;
        public IEnumerable<DOCTYPE> DOCTYPECollection => baseline_itemCollectionViewModelWrapper == null ? null : baseline_itemCollectionViewModelWrapper.DOCTYPECollection;
        #endregion
    }
}