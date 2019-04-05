using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class ConstructVariationViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <VARIATION_CONS, VARIATION_CONS, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BluePrintsEntitiesCollectionWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ConstructVariationViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new ConstructVariationViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected ConstructVariationViewModelWrapper()
        {
            DoNotAutoRefresh = true;
        }
        #region Database Operation

        private PROJECT loadPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var projectParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = projectParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<VARIATION_CONS>, IQueryable<VARIATION_CONS>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION_CONS> entities)
        {
            VariationSummary = ViewModelSource.Create(() => new ConstructionVariationSummary(loadPROJECT, entities));
            MainViewModel.OnAfterEntitySavedCallBack = entityChangedNotifyChanges;
            MainViewModel.OnAfterNewRowAdded = newRowAddedNotifyChanges;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private void newRowAddedNotifyChanges(VARIATION_CONS variation)
        {
            this.RaisePropertyChanged(x => x.DisplayEntities);
        }

        private void entityChangedNotifyChanges(VARIATION_CONS projection, VARIATION_CONS entity, bool isNewRow)
        {
            projection.Update();
        }

        #region CallBacks
        protected override void OnBeforeApplyProjectionPropertiesToEntity(VARIATION_CONS projectionEntity, VARIATION_CONS entity)
        {
            if (entity.CREATED.Date.Year == 1)
            {
                //Although EF convention will generate this but we require it immediately in the view
                projectionEntity.CREATEDBY = LoginCredentials.CurrentUserGuid;
            }

            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        public bool OnBeforeEntitySaved(VARIATION_CONS entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }
        #endregion

        #endregion

        #region View Properties
        Guid? selectedEntityKey = null;
        public override void FullRefresh()
        {
            if (DisplaySelectedEntity != null)
                selectedEntityKey = DisplaySelectedEntity.GUID;

            ReloadEntitiesCollection();
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
            restoreSelectedEntity();
        }

        private void restoreSelectedEntity()
        {
            if (selectedEntityKey != null && DisplayEntities != null)
            {
                DisplaySelectedEntity = DisplayEntities.FirstOrDefault(x => x.GUID == selectedEntityKey);
                if (DisplaySelectedEntity != null)
                {
                    DisplaySelectedEntities.Clear();
                    DisplaySelectedEntities.Add(DisplaySelectedEntity);
                    selectedEntityKey = null;
                    this.RaisePropertyChanged(x => x.DisplaySelectedEntity);
                    this.RaisePropertyChanged(x => x.DisplaySelectedEntities);
                }
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "ConstructionVariationViewModelWrapper_V1"; }
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

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);

                return collection;
            }
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            string view_name = "ConstructionVariationCollectionView";
            string tab_title = "Construction Variation";

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, VARIATION_CONS>(loadPROJECT, DisplaySelectedEntity), view_name, "[" + loadPROJECT.NUMBER + "] " + "[" + DisplaySelectedEntity.NAME + "] " + tab_title);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public override string UnifiedRowValidation(VARIATION_CONS projection)
        {
            return string.Empty;
        }


        public override string UnifiedValueValidation(VARIATION_CONS projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        private DevExpress.Mvvm.IDialogService ConfirmationDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ConfirmationDialogService"); }
        }

        public ConstructionVariationSummary VariationSummary { get; set; }
        protected override void OnPersistentAfterAuxiliaryEntitiesChanges(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(VARIATION_CONS))
                VariationSummary.Update();

            base.OnPersistentAfterAuxiliaryEntitiesChanges(key, changedType, messageType, sender, isBulkRefresh);
        }

        public decimal OriginalContractSum
        {
            get
            {
                return loadPROJECT == null ? 0 : loadPROJECT.CONSTRUCT_ORI_SUM == null ? 0 : (decimal)loadPROJECT.CONSTRUCT_ORI_SUM;
            }
            set
            {
                loadPROJECT.CONSTRUCT_ORI_SUM = value;
                PROJECTViewModel.Save(loadPROJECT);
                this.RaisePropertyChanged(x => x.OriginalContractSum);
                VariationSummary.Update();
            }
        }

        List<ConstructionVariationTypeDesc> constructionVariationTypes;
        public IEnumerable<ConstructionVariationTypeDesc> ConstructionVariationTypes
        {
            get
            {
                if(constructionVariationTypes == null)
                {
                    constructionVariationTypes = new List<ConstructionVariationTypeDesc>();
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.UOM_QTY_Increase, "An increase to in scope qty's from tender"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.Site_Instruction, "Any Client issued Site instruction or Request for Quotation that requires an estimate"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.Client, "Any Client issued Variation notice or letter (might be rates based or fully estimated)"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.DayWorks, "Any Dayworks captured - These should be formalised as a Primero VAR and then cancelled once they become a VAR"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.Primero, "Any 'change' types submitted as formal letters with supporting docs etc. such as dayworks"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.Rework, "Any costs associated with remedying Primero mistakes, whatever the cause"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.EOT, "Any direct delays or Project extension of time claims"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.NCR, "Any Non conformances raised on subcontractors or the Client that may have financial consequences"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.TQ, "Any Technical Query that may have financial consequences to solve"));
                    constructionVariationTypes.Add(new ConstructionVariationTypeDesc(ConstructionVariationType.Budget, "Tracking of budget transfers made by the P.M. or delegate for record purposes. This may be to offset overruns and alike"));
                }

                return constructionVariationTypes;
            }
        }

        public void ClearFilter()
        {
            constructionTypeFilter = null;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GridControlService.ClearGrouping();
        }

        public void FilterByUOM_QTY_Increase()
        {
            constructionTypeFilter = ConstructionVariationType.UOM_QTY_Increase;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterBySite_Instruction()
        {
            constructionTypeFilter = ConstructionVariationType.Site_Instruction;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByClient()
        {
            constructionTypeFilter = ConstructionVariationType.Client;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByDayWorks()
        {
            constructionTypeFilter = ConstructionVariationType.DayWorks;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByPrimero()
        {
            constructionTypeFilter = ConstructionVariationType.Primero;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByRework()
        {
            constructionTypeFilter = ConstructionVariationType.Rework;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByEOT()
        {
            constructionTypeFilter = ConstructionVariationType.EOT;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByNCR()
        {
            constructionTypeFilter = ConstructionVariationType.NCR;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByTQ()
        {
            constructionTypeFilter = ConstructionVariationType.TQ;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        public void FilterByBudget()
        {
            constructionTypeFilter = ConstructionVariationType.Budget;
            this.RaisePropertyChanged(x => x.FilterCriteria);
            GroupGridByType();
        }

        ConstructionVariationType? constructionTypeFilter = null;
        CriteriaOperator filterCriteria = null;
        public CriteriaOperator FilterCriteria
        {
            get
            {
                if (!ReferenceEquals(filterCriteria, null))
                    return filterCriteria;

                if (constructionTypeFilter != null)
                    return CriteriaOperator.Parse("[TYPE] In ('" + EnumHelper<ConstructionVariationType>.GetDisplayValue((ConstructionVariationType)constructionTypeFilter) + "')");
                else
                    return null;
            }
            set
            {
                filterCriteria = value;
            }
        }

        public void GroupGridByType()
        {
            GridControlService.ClearGrouping();
            GridControlService.GroupBy(BindableBase.GetPropertyName(() => new VARIATION_CONS().STATUS));
        }

        public CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTViewModel
        {
            get
            {
                return (CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT>();
            }
        }

        /// <summary>
        /// Construction variation type with description
        /// </summary>
        public class ConstructionVariationTypeDesc
        {
            public ConstructionVariationTypeDesc(ConstructionVariationType type, string description)
            {
                Type = type;
                Description = description;
            }

            public ConstructionVariationType Type { get; set; }
            public string Name => EnumHelper<ConstructionVariationType>.GetDisplayValue(Type);
            public string Description { get; set; }
        }

        public class ConstructionVariationSummary
        {
            private PROJECT project;
            private IEnumerable<VARIATION_CONS> variation_cons;
            public ConstructionVariationSummary(PROJECT project, IEnumerable<VARIATION_CONS> variation_cons)
            {
                this.project = project;
                this.variation_cons = variation_cons;
            }

            public decimal OriginalContractSum => project.CONSTRUCT_ORI_SUM == null ? 0 : (decimal)project.CONSTRUCT_ORI_SUM;

            public decimal VariationsApproved => variation_cons.Where(x => x.APPROVED_VALUE != null).Sum(x => (decimal)x.APPROVED_VALUE);

            public decimal RevisedContractSum => OriginalContractSum + VariationsApproved;

            public decimal ChangesPendingApproval => variation_cons.Where(x => x.OUTSTANDING_VALUE != null && (x.STATUS == ConstructionVariationStatus.Pending || x.STATUS == ConstructionVariationStatus.Submitted)).Sum(x => (decimal)x.OUTSTANDING_VALUE);

            public decimal ChangesCancelled => variation_cons.Where(x => x.OUTSTANDING_VALUE != null && (x.STATUS == ConstructionVariationStatus.Rejected || x.STATUS == ConstructionVariationStatus.Cancelled)).Sum(x => (decimal)x.OUTSTANDING_VALUE);

            public decimal PotentialContractSum => RevisedContractSum + ChangesPendingApproval;

            public void Update()
            {
                this.RaisePropertiesChanged();
            }
        }
        #endregion
    }
}