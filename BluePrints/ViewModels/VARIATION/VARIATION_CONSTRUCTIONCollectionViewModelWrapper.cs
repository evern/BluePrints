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
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Common.Misc;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTION_ITEMS, VARIATION_CONSTRUCTION_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTION_IMPACTS, VARIATION_CONSTRUCTION_IMPACTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Construction_Variation_Report.ToString());
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        protected virtual Func<IRepositoryQuery<VARIATION_CONSTRUCTION_IMPACT>, IQueryable<VARIATION_CONSTRUCTION_IMPACT>> VARIATION_CONSTRUCTION_IMPACTProjectionFunc()
        {
            return query => query.Where(x => x.VARIATION_CONSTRUCTION.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<VARIATION_CONSTRUCTION_ITEM>, IQueryable<VARIATION_CONSTRUCTION_ITEM>> VARIATION_CONSTRUCTION_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.VARIATION_CONSTRUCTION.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTIONS);
        }

        protected override Func<IRepositoryQuery<VARIATION_CONSTRUCTION>, IQueryable<VARIATION_CONSTRUCTION>> specifyMainViewModelProjection()
        {
            return query => setAuxiliaryCollections(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), VARIATION_CONSTRUCTION_IMPACTCollection, VARIATION_CONSTRUCTION_ITEMCollection);
        }

        private IQueryable<VARIATION_CONSTRUCTION> setAuxiliaryCollections(IQueryable<VARIATION_CONSTRUCTION> query, IEnumerable<VARIATION_CONSTRUCTION_IMPACT> VARIATION_CONSTRUCTION_IMPACTCollection, IEnumerable<VARIATION_CONSTRUCTION_ITEM> VARIATION_CONSTRUCTION_ITEMCollection)
        {
            List<VARIATION_CONSTRUCTION> VARIATION_CONSTRUCTIONCollection = query.ToList();
            foreach (var VARIATION_CONSTRUCTION in VARIATION_CONSTRUCTIONCollection)
            {
                VARIATION_CONSTRUCTION.UpdateVariationConstructionItems(VARIATION_CONSTRUCTION_ITEMCollection.Where(x => x.GUID_VARIATION_CONSTRUCTION == VARIATION_CONSTRUCTION.GUID));
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
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(VARIATION_CONSTRUCTION_ITEM))
            {
                Guid variationConstructionItemKey = new Guid(key.ToString());
                VARIATION_CONSTRUCTION_ITEM findVARIATION_CONSTRUCTION_ITEM = VARIATION_CONSTRUCTION_ITEMCollection.FirstOrDefault(x => x.GUID == variationConstructionItemKey);
                if (findVARIATION_CONSTRUCTION_ITEM != null)
                {
                    VARIATION_CONSTRUCTION findVARIATION_CONSTRUCTION = MainViewModel.Entities.FirstOrDefault(x => x.GUID == findVARIATION_CONSTRUCTION_ITEM.GUID_VARIATION_CONSTRUCTION);
                    if (findVARIATION_CONSTRUCTION != null)
                    {
                        IEnumerable<VARIATION_CONSTRUCTION_ITEM> variationConstructionItems = VARIATION_CONSTRUCTION_ITEMCollection.Where(x => x.GUID_VARIATION_CONSTRUCTION == findVARIATION_CONSTRUCTION.GUID);
                        findVARIATION_CONSTRUCTION.UpdateVariationConstructionItems(variationConstructionItems);
                        findVARIATION_CONSTRUCTION.Update();
                    }
                }
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

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
                            DialogCollectionViewModel<ErrorMessage> errorMessagesViewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "These variation job(s) cannot be commit to EXO because of the following error");
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


        public bool CanViewReport()
        {
            if (IsLoading || MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Construction_Variation_Report);
            
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public void ViewReport()
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            var constructionVariationReport = new XtraReportConstructionVariation();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    constructionVariationReport.LoadLayout(sw.BaseStream);
                }
            }

            constructionVariationReport.AssignProperties(loadPROJECT, SelectedEntity, SelectedEntity.UpdatableVariationConstructionItems);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = constructionVariationReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            constructionVariationReport.RequestParameters = false;
            constructionVariationReport.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
        }

        public bool CanExportSelected()
        {
            return !IsLoading;
        }

        public void ExportSelected()
        {
            if(SelectedEntities.Count == 0)
            {
                MessageBoxService.ShowMessage("Please select variation(s) to export");
                return;
            }

            if (FolderBrowserDialogService.ShowDialog())
            {
                string exportPath = FolderBrowserDialogService.ResultPath + "\\";
                var constructionVariationReport = new XtraReportConstructionVariation();
                var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
                if (dbProjectReport != null)
                {
                    var reportString = dbProjectReport.REPORT.ToString();
                    using (var sw = new StreamWriter(new MemoryStream()))
                    {
                        sw.Write(reportString);
                        sw.Flush();
                        constructionVariationReport.LoadLayout(sw.BaseStream);
                    }
                }

                LoadingScreenManager.ShowLoadingScreen(SelectedEntities.Count);
                foreach (VARIATION_CONSTRUCTION variationConstruction in SelectedEntities)
                {
                    LoadingScreenManager.SetMessage("Exporting " + variationConstruction.DocumentNumber + "...");
                    constructionVariationReport.AssignProperties(loadPROJECT, variationConstruction, variationConstruction.UpdatableVariationConstructionItems);
                    constructionVariationReport.RequestParameters = false;
                    constructionVariationReport.CreateDocument(true);
                    constructionVariationReport.ExportToPdf(exportPath + variationConstruction.DocumentNumber + ".pdf");
                    LoadingScreenManager.Progress();
                }
                LoadingScreenManager.CloseLoadingScreen();
            }
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

        public IEnumerable<VARIATION_CONSTRUCTION_ITEM> VARIATION_CONSTRUCTION_ITEMCollection
        {
            get
            {
                return GetEntities<VARIATION_CONSTRUCTION_ITEM>();
            }
        }

        public CollectionViewModel<VARIATION_CONSTRUCTION_ITEM, VARIATION_CONSTRUCTION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_CONSTRUCTION_ITEMCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<VARIATION_CONSTRUCTION_ITEM, VARIATION_CONSTRUCTION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<VARIATION_CONSTRUCTION_ITEM>();
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