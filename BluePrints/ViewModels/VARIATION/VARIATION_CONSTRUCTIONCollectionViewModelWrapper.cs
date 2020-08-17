using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class VARIATION_CONSTRUCTIONCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <VARIATION_CONSTRUCTION, VARIATION_CONSTRUCTION, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATION_CONSTRUCTIONCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new VARIATION_CONSTRUCTIONCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the VARIATION_REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the VARIATION_REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected VARIATION_CONSTRUCTIONCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        BackgroundWorker exoLoadingBackgroundWorker;
        private Data.PROJECT loadPROJECT;
        protected JOBCOST_HDR masterJob;
        protected JOBCOST_LINES copyLine;
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> localPrimeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            localPrimeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            localPrimeroUnitOfWork = localPrimeroUnitOfWorkFactory.CreateUnitOfWork();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTION_IMPACTS, VARIATION_CONSTRUCTION_IMPACTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        protected virtual Func<IRepositoryQuery<VARIATION_CONSTRUCTION_IMPACT>, IQueryable<VARIATION_CONSTRUCTION_IMPACT>> VARIATION_CONSTRUCTION_IMPACTProjectionFunc()
        {
            return query => query.Where(x => x.VARIATION_CONSTRUCTION.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTIONS);
        }

        protected override Func<IRepositoryQuery<VARIATION_CONSTRUCTION>, IQueryable<VARIATION_CONSTRUCTION>> specifyMainViewModelProjection()
        {
            return query => setAssignedImpacts(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), VARIATION_CONSTRUCTION_IMPACTCollection);
        }

        private IQueryable<VARIATION_CONSTRUCTION> setAssignedImpacts(IQueryable<VARIATION_CONSTRUCTION> query, IEnumerable<VARIATION_CONSTRUCTION_IMPACT> VARIATION_CONSTRUCTION_IMPACTCollection)
        {
            List<VARIATION_CONSTRUCTION> VARIATION_CONSTRUCTIONCollection = query.ToList();
            foreach (var VARIATION_CONSTRUCTION in VARIATION_CONSTRUCTIONCollection)
            {
                VARIATION_CONSTRUCTION.SetAssignedImpacts(AllVARIATION_CONSTRUCTION_IMPACTCollection, VARIATION_CONSTRUCTION_IMPACTCollection.Where(x => x.GUID_CONSTRUCTION_VARIATION == VARIATION_CONSTRUCTION.GUID));
            }

            return VARIATION_CONSTRUCTIONCollection.AsQueryable();
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<VARIATION_CONSTRUCTION> entities)
        {
            loadExoMethodsData();
            return base.OnMainViewModelLoaded(entities);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION_CONSTRUCTION> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(VARIATION_CONSTRUCTION projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(VARIATION_CONSTRUCTION projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(VARIATION_CONSTRUCTION projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, VARIATION_CONSTRUCTION projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new VARIATION_CONSTRUCTION().STATUS))
            {
                VariationConstructionStatus variationConstructionStatusOldValue = (VariationConstructionStatus)old_value;
                VariationConstructionStatus variationConstructionStatus = (VariationConstructionStatus)new_value;

                //when changing from approved to submitted, budget will be set to zero
                //when changing from submitted to approved, budget will be populated
                //when changing from cancelled, pending, rejected to submitted/approved, budget will be set depending on whether it's approved
                if ((variationConstructionStatusOldValue == VariationConstructionStatus.Approved && variationConstructionStatus == VariationConstructionStatus.Submitted) ||
                    (variationConstructionStatusOldValue == VariationConstructionStatus.Submitted && variationConstructionStatus == VariationConstructionStatus.Approved) || 
                    ((variationConstructionStatusOldValue == VariationConstructionStatus.Cancelled || variationConstructionStatusOldValue == VariationConstructionStatus.Pending || variationConstructionStatusOldValue == VariationConstructionStatus.Rejected) && (variationConstructionStatus == VariationConstructionStatus.Submitted || variationConstructionStatus == VariationConstructionStatus.Approved)))
                {
                    List<ExoSubJobProjection> exoJobs = projection.GetConstructionItemsForExoCommit(variationConstructionStatus == VariationConstructionStatus.Approved);
                    if (exoJobs.Count > 0)
                    {
                        List<ErrorMessage> errorMessages;
                        IEnumerable<ExoSubJobProjection> addedProjections = ExoMethods.CommitToExo(exoJobs, MessageBoxService, masterJob, copyLine, loadPROJECT, USERCollection, localPrimeroUnitOfWork, BulkColumnEditDialogService, out errorMessages, null, true);
                        if (errorMessages.Count > 0)
                        {
                            DialogCollectionViewModel<ErrorMessage> errorMessagesViewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "These jobs cannot commit to EXO because of the following error");
                            ErrorMessagesDialogService.ShowDialog(MessageButton.OK, string.Empty, "ListErrorMessages", errorMessagesViewModel);
                        }

                        if (addedProjections.Count() > 0)
                        {
                            DialogCollectionViewModel<ExoSubJobProjection> viewModel = DialogCollectionViewModel<ExoSubJobProjection>.Create(addedProjections, addedProjections.Count() + " variation job(s) pushed to exo");
                            ConfirmationDialogService.ShowDialog(MessageButton.OK, string.Empty, "ExoVariationConfirmation", viewModel);
                        }
                    }
                }
                else if ((variationConstructionStatus == VariationConstructionStatus.Cancelled || variationConstructionStatus == VariationConstructionStatus.Pending
                   || variationConstructionStatus == VariationConstructionStatus.Rejected) && (variationConstructionStatusOldValue == VariationConstructionStatus.Submitted || variationConstructionStatusOldValue == VariationConstructionStatus.Approved))
                {
                    List<ExoSubJobProjection> exoJobs = projection.GetConstructionItemsForExoCommit(false);
                    if (exoJobs.Count > 0)
                    {
                        List<ExoSubJobProjection> removedJobs = new List<ExoSubJobProjection>();
                        foreach(ExoSubJobProjection exoJob in exoJobs)
                        {
                            JOBCOST_LINES line = ExoQueries.GetProjectLine(localPrimeroUnitOfWork, loadPROJECT.NUMBER, exoJob);
                            if (line != null)
                            {
                                localPrimeroUnitOfWork.JOBCOST_LINES.Remove(line);
                                removedJobs.Add(exoJob);
                            }
                        }

                        if (removedJobs.Count > 0)
                        {
                            localPrimeroUnitOfWork.SaveChanges();
                            DialogCollectionViewModel<ExoSubJobProjection> viewModel = DialogCollectionViewModel<ExoSubJobProjection>.Create(removedJobs, removedJobs.Count() + " variation job(s) removed from exo");
                            ConfirmationDialogService.ShowDialog(MessageButton.OK, string.Empty, "ExoVariationConfirmation", viewModel);
                        }
                    }
                }
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }
        #endregion

        #region View Behaviours
        protected override void OnAfterProjectionSave(VARIATION_CONSTRUCTION projection, VARIATION_CONSTRUCTION entity, bool isNew)
        {
            DeleteProjectionImpacts(projection);
            SaveProjectionImpacts(projection);
            base.OnAfterProjectionSave(projection, entity, isNew);
        }

        private void DeleteProjectionImpacts(VARIATION_CONSTRUCTION projectionEntity)
        {
            List<VARIATION_CONSTRUCTION_IMPACT> assignedProjectionImpacts = projectionEntity.GetAssignedImpacts();
            List<VARIATION_CONSTRUCTION_IMPACT> deleteProjectionImpacts = new List<VARIATION_CONSTRUCTION_IMPACT>();

            foreach (VARIATION_CONSTRUCTION_IMPACT impact in VARIATION_CONSTRUCTION_IMPACTCollection.Where(x => x.GUID_CONSTRUCTION_VARIATION == projectionEntity.GUID))
            {
                if (assignedProjectionImpacts == null)
                    deleteProjectionImpacts.Add(impact);
                else
                {
                    var assignedImpact = assignedProjectionImpacts.FirstOrDefault(x => x.IMPACT == impact.IMPACT);
                    if (assignedImpact == null)
                        deleteProjectionImpacts.Add(impact);
                }
            }

            foreach (VARIATION_CONSTRUCTION_IMPACT deleteProjectionImpact in deleteProjectionImpacts)
            {
                VARIATION_CONSTRUCTION_IMPACTCollectionViewModel.Delete(deleteProjectionImpact);
            }
        }

        private void SaveProjectionImpacts(VARIATION_CONSTRUCTION projectionEntity)
        {
            List<VARIATION_CONSTRUCTION_IMPACT> projectionImpacts = projectionEntity.GetAssignedImpacts();
            if (projectionImpacts == null)
                return;

            foreach (VARIATION_CONSTRUCTION_IMPACT projectionImpact in projectionImpacts)
            {
                VARIATION_CONSTRUCTION_IMPACT repositoryAssignedImpact = VARIATION_CONSTRUCTION_IMPACTCollection.FirstOrDefault(x => x.GUID_CONSTRUCTION_VARIATION == projectionEntity.GUID && x.IMPACT == projectionImpact.IMPACT);
                if (repositoryAssignedImpact == null)
                {
                    VARIATION_CONSTRUCTION_IMPACT newImpact = new VARIATION_CONSTRUCTION_IMPACT();
                    newImpact.GUID = Guid.Empty;
                    newImpact.GUID_CONSTRUCTION_VARIATION = projectionEntity.GUID;
                    newImpact.IMPACT = projectionImpact.IMPACT;
                    VARIATION_CONSTRUCTION_IMPACTCollectionViewModel.Save(newImpact);
                }
            }

        }
        #endregion

        #region View Properties
        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public bool CanEdit()
        {
            if (SelectedEntity == null)
                return false;

            return true;
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            string view_name = "VARIATION_CONSTRUCTION_ITEMCollectionView";
            string tab_title = "Construction Variation";

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, VARIATION_CONSTRUCTION>(loadPROJECT, SelectedEntity), view_name, "[" + loadPROJECT.NUMBER + "] " + "[" + SelectedEntity.NUMBER + "] " + tab_title);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        private void loadExoMethodsData()
        {
            IPrimeroEntitiesUnitOfWork threadSafePrimeroEntitiesUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal).CreateUnitOfWork();
            masterJob = ExoQueries.GetProjectSubJob(threadSafePrimeroEntitiesUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            copyLine = ExoQueries.GetAnyProjectLineByJobNumber(threadSafePrimeroEntitiesUnitOfWork, loadPROJECT.NUMBER);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "VARIATION_CONSTRUCTIONCollectionViewModelWrapper_v2"; }
        }

        private DevExpress.Mvvm.IDialogService ConfirmationDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ConfirmationDialogService"); }
        }

        public IEnumerable<VARIATION_CONSTRUCTION_IMPACT> VARIATION_CONSTRUCTION_IMPACTCollection
        {
            get
            {
                return GetEntities<VARIATION_CONSTRUCTION_IMPACT>();
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

        List<VARIATION_CONSTRUCTION_IMPACT> VARIATION_CONSTRUCTION_IMPACTS;
        public IEnumerable<VARIATION_CONSTRUCTION_IMPACT> AllVARIATION_CONSTRUCTION_IMPACTCollection
        {
            get
            {
                if(VARIATION_CONSTRUCTION_IMPACTS == null)
                {
                    VARIATION_CONSTRUCTION_IMPACTS = new List<VARIATION_CONSTRUCTION_IMPACT>();
                    IEnumerable<VariationConstructionImpact> Impacts = Enum.GetValues(typeof(VariationConstructionImpact)).Cast<VariationConstructionImpact>();
                    foreach (VariationConstructionImpact impact in Impacts)
                    {
                        VARIATION_CONSTRUCTION_IMPACT newVARIATION_CONSTRUCTION_IMPACT = new VARIATION_CONSTRUCTION_IMPACT();
                        newVARIATION_CONSTRUCTION_IMPACT.IMPACT = impact;
                        VARIATION_CONSTRUCTION_IMPACTS.Add(newVARIATION_CONSTRUCTION_IMPACT);
                    }
                }

                return VARIATION_CONSTRUCTION_IMPACTS;
            }
        }

        public CollectionViewModel<VARIATION_CONSTRUCTION_IMPACT, VARIATION_CONSTRUCTION_IMPACT, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_CONSTRUCTION_IMPACTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<VARIATION_CONSTRUCTION_IMPACT, VARIATION_CONSTRUCTION_IMPACT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<VARIATION_CONSTRUCTION_IMPACT>();
            }
        }
        #endregion
    }
}