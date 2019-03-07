using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
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
    public partial class OffsiteDirectVariationCollectionViewModelWrapper : BASELINE_ITEMCollectionViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static OffsiteDirectVariationCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new OffsiteDirectVariationCollectionViewModelWrapper());
        }

        protected VARIATION loadVARIATION;
        protected override void resolveParameters(object parameter)
        {            
            var receiveParameter =
                (DualEntitiesParameter<PROJECT, VARIATION>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadVARIATION = receiveParameter.GetSecondEntity();
            base.resolveParameters(parameter);
        }

        protected override IQueryable<BASELINE_ITEM> base_entity_query(IRepositoryQuery<BASELINE_ITEM> query)
        {
            if (loadVARIATION.APPROVED == null)
                //When variation is not approved, retrieve current live deliverables and variation deliverables
                return query.Where(x => (x.GUID_BASELINE == load_context_guid) || (x.GUID_VARIATION == loadVARIATION.GUID && x.GUID_BASELINE == null));
            else
                //When variation is approved, retrieve deliverables from variation connected baseline
                return query.Where(x => x.GUID_BASELINE == loadVARIATION.GUID_BASELINE && x.GUID_VARIATION == loadVARIATION.GUID);
        }

        protected override string ViewName => "DESIGN_VARIATION_ITEMSViewModelWrapper_v3" + loadPROJECT == null ? Guid.Empty.ToString() : loadPROJECT.GUID.ToString();

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>> specifyMainViewModelProjection()
        {
            IEnumerable<VARIATION_ITEM> VARIATION_ITEMS = new List<VARIATION_ITEM>();
            if (loaderCollection != null)
            {
                VARIATION_ITEMS = loaderCollection.GetCollection<VARIATION_ITEM>();
            }

            return query => Baseline_ItemVariationQuery.OffsiteDirectVariationItemTransformation(base_entity_query(query), loadPROJECT, livePROGRESS, PROGRESS_ITEMCollection, loadBASELINE, loadVARIATION, VARIATION_ITEMS, RATECollection);
        }

        public override string UnifiedRowValidation(BASELINE_ITEMProgress projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProgress projection, string field_name, object newValue)
        {
            //budgeted hours field is disabled but just in case
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Variation_Units)))
            {
                if (projection.Entity.Entity.BY_DURATION && ((decimal)newValue) > 0)
                    return "Cannot set variation hours when deliverable is by duration";
            }
            //this is not likely to happen, because variation isn't trackable yet but just in case
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION)))
            {
                if (projection.Earned_Units_Total > 0)
                    return "Cannot change deliverable tracking type when percentage is already earned";
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE)))
            {
                if(projection.Entity.Entity.GUID_PHASE != null && newValue != null)
                {
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == (Guid)newValue);
                    PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_PHASE);
                    if ((findPHASE != null && findDOCTYPE != null) && findDOCTYPE.IS_INDIRECT_ONLY && findPHASE.CHARGE_TYPE == ChargeType.Direct)
                        return "Selected document type is valid for indirect only, please change phase to indirect";
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_PHASE)))
            {
                if (projection.Entity.Entity.GUID_DOCTYPE != null && newValue != null)
                {
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DOCTYPE);
                    PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.GUID == (Guid)newValue);
                    if ((findPHASE != null && findDOCTYPE != null) && findDOCTYPE.IS_INDIRECT_ONLY && findPHASE.CHARGE_TYPE == ChargeType.Direct)
                        return "Selected document type is valid for indirect only, please change phase to indirect";
                }
            }

            return base.UnifiedValueValidation(projection, field_name, newValue);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION)))
            {
                decimal oldValue = projection.Variation_Units;
                decimal newValue = 0;
                projection.UnapprovedVariation_Units = newValue;
                if(!isNew)
                {
                    MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Variation_Units), oldValue, newValue, EntityMessageType.Changed);
                }
                else
                    projection.Update();
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }
    }
}