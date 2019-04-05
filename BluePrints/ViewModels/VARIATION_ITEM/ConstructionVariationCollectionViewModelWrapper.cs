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
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class ConstructionVariationCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper
        <VARIATION_CONS_ITEM, VARIATION_CONS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ConstructionVariationCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new ConstructionVariationCollectionViewModelWrapper());
        }

        protected VARIATION_CONS loadVARIATION;
        protected PROJECT loadPROJECT { get; set; }
        protected readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        public bool IsReadOnly => loadVARIATION == null ? true : loadVARIATION.SUBMITTED != null;
        public List<STOCK_ITEMS> STOCK_ITEMCollection { get; set; }
        public List<JOB_COSTGROUPS> COST_GROUPCollection { get; set; }
        public List<JOB_COSTTYPES> COST_TYPECollection { get; set; }
        public List<JOBCOST_HDR> JOBCOST_HDRCollection { get; set; }
        protected override void resolveParameters(object parameter)
        {            
            var receiveParameter = (DualEntitiesParameter<PROJECT, VARIATION_CONS>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            loadVARIATION = receiveParameter.GetSecondEntity();
            
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_ITEMS, VARIATION_ITEMProjectionFunc);
        }

        private Func<IRepositoryQuery<VARIATION_ITEM>, IQueryable<VARIATION_ITEM>> VARIATION_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_VARIATION == loadVARIATION.GUID);
        }

        public override string ViewName => "ConstructionVariationItemsViewModelWrapper_v3";

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONS_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<VARIATION_CONS_ITEM>, IQueryable<VARIATION_CONS_ITEM>> specifyMainViewModelProjection()
        {
            return query => variationConsItemsQuery(query, loadVARIATION);
        }

        public IQueryable<VARIATION_CONS_ITEM> variationConsItemsQuery(IQueryable<VARIATION_CONS_ITEM> query, VARIATION_CONS variation)
        {
            List<VARIATION_CONS_ITEM> variationItems = query.Where(x => x.GUID_VARIATION == variation.GUID).ToList();
            return variationItems.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION_CONS_ITEM> entities)
        {
            STOCK_ITEMCollection = ExoQueries.GetSTOCK_ITEMS(primeroUnitOfWork);
            COST_GROUPCollection = ExoQueries.GetJOB_COSTGROUPS(primeroUnitOfWork);
            COST_TYPECollection = ExoQueries.GetJOB_COSTTYPES(primeroUnitOfWork);
            JOBCOST_HDRCollection = ExoQueries.GetJOBCOST_HDR(primeroUnitOfWork, loadPROJECT.NUMBER);
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySavedIsContinue;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected bool OnBeforeEntitySavedIsContinue(VARIATION_CONS_ITEM projectionEntity)
        {
            projectionEntity.GUID_VARIATION = loadVARIATION.GUID;
            return true;
        }

        public override string UnifiedValueValidation(VARIATION_CONS_ITEM projection, string field_name, object newValue)
        {
            return string.Empty;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, VARIATION_CONS_ITEM projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new VARIATION_CONS_ITEM().TYPE))
            {
                if (new_value != null)
                {
                    ConstructionVariationItemType oldType = projection.TYPE;
                    ConstructionVariationItemType newType = (ConstructionVariationItemType)new_value;

                    string phaseSearchCode = string.Empty;
                    int? oldJobNo = projection.SUBJOB_JOBNO;
                    if (newType == ConstructionVariationItemType.Indirect || newType == ConstructionVariationItemType.Equipment)
                        phaseSearchCode = BluePrintsResources.Default_Indirect_Phase;
                    else if (newType == ConstructionVariationItemType.Engineering)
                        phaseSearchCode = BluePrintsResources.Default_Design_Phase;
                    else if (newType == ConstructionVariationItemType.Material)
                        phaseSearchCode = BluePrintsResources.Default_Procurement_Phase;
                    else if(newType == ConstructionVariationItemType.Trade)
                        phaseSearchCode = BluePrintsResources.Default_Construction_Phase;

                    if (phaseSearchCode != string.Empty)
                    {
                        JOBCOST_HDR findSubJob = JOBCOST_HDRCollection.FirstOrDefault(x => x.JOBCODE.Contains(phaseSearchCode));
                        if (findSubJob != null)
                            projection.SUBJOB_JOBNO = findSubJob.JOBNO;
                        else
                            projection.SUBJOB_JOBNO = null;

                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, field_name, oldJobNo, projection.SUBJOB_JOBNO, EntityMessageType.Changed);
                    }

                    string oldCode = projection.ITEM_ID;
                    if(oldCode == null || oldCode == string.Empty)
                    {
                        IEnumerable<VARIATION_CONS_ITEM> orderedItemIdByType = DisplayEntities.Where(x => x.TYPE == newType && (x.ITEM_ID != null && x.ITEM_ID != string.Empty)).OrderByDescending(x => x.TYPE);
                        string newCode = string.Empty;
                        if (orderedItemIdByType.Count() == 0)
                        {
                            if (newType == ConstructionVariationItemType.Indirect)
                                newCode = "1.1";
                            else if (newType == ConstructionVariationItemType.Engineering)
                                newCode = "2.1";
                            else if (newType == ConstructionVariationItemType.Trade)
                                newCode = "3.1";
                            else if (newType == ConstructionVariationItemType.Equipment)
                                newCode = "4.1";
                            else if (newType == ConstructionVariationItemType.Material)
                                newCode = "5.1";
                        }
                        else
                        {
                            VARIATION_CONS_ITEM largestItemIdByType = orderedItemIdByType.First();
                            List<string> deconstructedItemId = largestItemIdByType.ITEM_ID.Split('.').ToList();
                            string lastString = deconstructedItemId.Last();
                            int lastInt = 0;
                            if(Int32.TryParse(lastString, out lastInt))
                            {
                                for(int i=0;i<deconstructedItemId.Count;i++)
                                {
                                    if (i == deconstructedItemId.Count - 1)
                                        newCode += (lastInt + 1).ToString();
                                    else
                                        newCode += deconstructedItemId[i] + ".";
                                }
                            }
                        }

                        if(newCode != string.Empty)
                        {
                            projection.ITEM_ID = newCode;
                            MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, field_name, oldCode, projection.ITEM_ID, EntityMessageType.Changed);
                        }
                    }
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new VARIATION_CONS_ITEM().STOCKCODE))
            {
                int? oldCostGroupSeqNo = projection.COSTGROUP_SEQNO;
                int? oldCostTypeSeqNo = projection.COSTTYPE_SEQNO;
                if (new_value != null && new_value.ToString() != string.Empty)
                {
                    string newStockCode = new_value.ToString();
                    STOCK_ITEMS findStockItem = STOCK_ITEMCollection.FirstOrDefault(x => x.STOCKCODE == new_value.ToString());
                    if(findStockItem != null)
                    {
                        MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                        JOB_COSTGROUPS findCostGroup = COST_GROUPCollection.FirstOrDefault(x => x.SEQNO == findStockItem.COSTGROUP);
                        if (findCostGroup != null)
                            projection.COSTGROUP_SEQNO = findCostGroup.SEQNO;
                        else
                            projection.COSTGROUP_SEQNO = null;

                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, field_name, oldCostGroupSeqNo, projection.COSTGROUP_SEQNO, EntityMessageType.Changed);

                        JOB_COSTTYPES findCostType = COST_TYPECollection.FirstOrDefault(x => x.SEQNO == findStockItem.COSTTYPE);
                        if (findCostType != null)
                            projection.COSTTYPE_SEQNO = findCostType.SEQNO;
                        else
                            projection.COSTTYPE_SEQNO = null;

                        MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, field_name, oldCostTypeSeqNo, projection.COSTTYPE_SEQNO, EntityMessageType.Changed);
                        MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
                        projection.Update();
                    }
                }
                else
                {
                    projection.COSTGROUP_SEQNO = null;
                    projection.COSTTYPE_SEQNO = null;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, field_name, oldCostGroupSeqNo, projection.COSTGROUP_SEQNO, EntityMessageType.Changed);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, field_name, oldCostTypeSeqNo, projection.COSTTYPE_SEQNO, EntityMessageType.Changed);
                }
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, VARIATION_CONS_ITEM projection, bool isNew)
        {
            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedRowValidation(VARIATION_CONS_ITEM projection)
        {
            return string.Empty;
        }

        #region View Property
        public NewItemRowPosition NewItemRowPosition
        {
            get
            {
                if (loadVARIATION != null && loadVARIATION.SUBMITTED == null)
                    return NewItemRowPosition.Top;

                return NewItemRowPosition.None;
            }
        }
        #endregion
    }
}