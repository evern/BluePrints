using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class DELIVERABLES_STATUSCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of DELIVERABLES_STATUSCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DELIVERABLES_STATUSCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DELIVERABLES_STATUSCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the DELIVERABLES_STATUSCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the DELIVERABLES_STATUSCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DELIVERABLES_STATUSCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        private bool isProjectSpecific;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            if (parameter != null)
            {
                var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
                loadPROJECT = PROJECTParameter.GetEntity();
                isProjectSpecific = true;
            }
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DSTATUS_DOCTYPES, DSTATUS_DOCTYPEProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<DSTATUS_DOCTYPE>, IQueryable<DSTATUS_DOCTYPE>> DSTATUS_DOCTYPEProjectionFunc()
        {
            if (isProjectSpecific)
                return query => query.Where(x => x.DELIVERABLES_STATUS.GUID_PROJECT == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.DELIVERABLES_STATUS.GUID_PROJECT == null);
        }

        protected virtual Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE != PhaseType.Construct && x.PHASE_TYPE != PhaseType.Procurement);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
        }

        protected override Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>>
            specifyMainViewModelProjection()
        {
            if (isProjectSpecific)
                return query => setAssignedDocumentTypes(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.MAX_PERCENTAGE), DSTATUS_DOCTYPECollection);
            else
                return query => setAssignedDocumentTypes(query.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.MAX_PERCENTAGE), DSTATUS_DOCTYPECollection);
        }

        private IQueryable<DELIVERABLES_STATUS> setAssignedDocumentTypes(IQueryable<DELIVERABLES_STATUS> query, IEnumerable<DSTATUS_DOCTYPE> DSTATUS_DOCTYPES)
        {
            List<DELIVERABLES_STATUS> deliverable_statuses = query.ToList();
            foreach(var deliverable_status in deliverable_statuses)
            {
                deliverable_status.SetAssignedDocTypes(DOCTYPECollection, DSTATUS_DOCTYPES);
            }

            //deliverable_statuses.ForEach(x => x.SetAssignedDocTypes(DOCTYPECollection, DSTATUS_DOCTYPES));
            return deliverable_statuses.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DELIVERABLES_STATUS> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedRowValidation(DELIVERABLES_STATUS projection)
        {
            if(projection.AUTO_PERCENTAGE > projection.MAX_PERCENTAGE)
            {
                return "Auto assign percentage cannot be more than max percentage";
            }

            return string.Empty;
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(DELIVERABLES_STATUS projection, out bool isNew)
        {
            if (isProjectSpecific)
                projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        //Save assigned document types on projection
        protected override void OnAfterProjectionSave(DELIVERABLES_STATUS projection, DELIVERABLES_STATUS entity, bool isNew)
        {
            DeleteProjectionDocTypes(projection);
            SaveProjectionDocTypes(projection);

            base.OnAfterProjectionSave(projection, entity, isNew);
        }

        private void DeleteProjectionDocTypes(DELIVERABLES_STATUS projectionEntity)
        {
            List<DOCTYPE> projectionDocTypes = projectionEntity.GetAssignedDocTypes();
            List<DOCTYPE> assignedDocTypes = projectionDocTypes == null ? new List<DOCTYPE>() : projectionDocTypes.Select(x => x).ToList();
            List<DSTATUS_DOCTYPE> deleteStatusDocTypes = new List<DSTATUS_DOCTYPE>();

            foreach (DSTATUS_DOCTYPE statusDocType in DSTATUS_DOCTYPECollection.Where(x => x.GUID_STATUS == projectionEntity.GUID))
            {
                if (assignedDocTypes.Count == 0)
                    deleteStatusDocTypes.Add(statusDocType);
                else
                {
                    var assignedDocType = assignedDocTypes.FirstOrDefault(x => x.GUID == statusDocType.GUID_DOCTYPE);
                    if (assignedDocType == null)
                        deleteStatusDocTypes.Add(statusDocType);
                }
            }

            foreach(DSTATUS_DOCTYPE deleteStatusDocType in deleteStatusDocTypes)
            {
                DSTATUS_DOCTYPECollectionViewModel.Delete(deleteStatusDocType);
            }
        }

        private void SaveProjectionDocTypes(DELIVERABLES_STATUS projectionEntity)
        {
            List<DOCTYPE> projectionDocTypes = projectionEntity.GetAssignedDocTypes();
            List<DOCTYPE> assignedDocTypes = projectionDocTypes == null ? new List<DOCTYPE>() : projectionDocTypes.Select(x => x).ToList();

            List<DSTATUS_DOCTYPE> currentProjectionDocTypeAssignments = DSTATUS_DOCTYPECollection.Where(x => x.GUID_STATUS == projectionEntity.GUID).ToList();
            foreach (DOCTYPE assignedDocType in assignedDocTypes)
            {
                DSTATUS_DOCTYPE repositoryAssignedDocType = currentProjectionDocTypeAssignments.FirstOrDefault(x => x.GUID_DOCTYPE == assignedDocType.GUID);
                if (repositoryAssignedDocType == null)
                {
                    DSTATUS_DOCTYPE newDocType = new DSTATUS_DOCTYPE();
                    newDocType.GUID = Guid.Empty;
                    newDocType.GUID_DOCTYPE = assignedDocType.GUID;
                    newDocType.GUID_STATUS = projectionEntity.GUID;

                    DSTATUS_DOCTYPECollectionViewModel.Save(newDocType);
                }
            }

        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "DELIVERABLES_STATUSCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "DELIVERABLES_STATUSCollectionViewModelWrapper_v3" + view_project_specific_affix; }
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

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public List<PROJECT> PROJECTCollectionForCopy
        {
            get
            {
                List<PROJECT> returnPROJECTS = null;
                var collection = GetEntities<PROJECT>();
                if (collection != null)
                {
                    returnPROJECTS = new List<PROJECT>(collection.OrderBy(x => x.NUMBER));
                    returnPROJECTS.Insert(0, new PROJECT() { GUID = Guid.Empty, NUMBER = "Default" });
                }

                return returnPROJECTS;
            }
        }
        #endregion

        #region View Command
        public bool CanDuplicate(BarEditItem barEdit)
        {
            return barEdit.EditValue != null;
        }

        public void Insert()
        {
            TableViewService.SetImmediateUpdateRowPosition(true);
            List<ErrorMessage> errorMessages;
            VerifyAndSaveDuplicateItems(MainViewModel.SelectedEntities, out errorMessages);
            ShowErrorMessage("Cannot insert status", errorMessages);
            TableViewService.SetImmediateUpdateRowPosition(false);
        }

        //public void Duplicate(BarEditItem barEdit)
        //{
        //    TableViewService.SetImmediateUpdateRowPosition(true);
        //    Guid doctypeGuid;
        //    List<DELIVERABLES_STATUS> newEntities = new List<DELIVERABLES_STATUS>();
        //    if (Guid.TryParse(barEdit.EditValue.ToString(), out doctypeGuid))
        //    {
        //        VerifyAndSaveDuplicateItems(MainViewModel.SelectedEntities, doctypeGuid);
        //    }
        //    TableViewService.SetImmediateUpdateRowPosition(false);
        //}

        private void VerifyAndSaveDuplicateItems(IEnumerable<DELIVERABLES_STATUS> deliverableStatuses, out List<ErrorMessage> errorMessages, bool isCopyFrom = false)
        {
            errorMessages = new List<ErrorMessage>();
            List<DELIVERABLES_STATUS> newDeliverableStatuses = new List<DELIVERABLES_STATUS>();
            foreach (var entity in deliverableStatuses)
            {
                var newDeliverableStatus = new DELIVERABLES_STATUS();
                DataUtils.ShallowCopy(newDeliverableStatus, entity);
                newDeliverableStatus.GUID = Guid.Empty;
                if (isProjectSpecific)
                    newDeliverableStatus.GUID_PROJECT = loadPROJECT.GUID;

                if (!isCopyFrom)
                {
                    if (newDeliverableStatus.MAX_PERCENTAGE >= 1m)
                    {
                        errorMessages.Add(new ErrorMessage(newDeliverableStatus.NAME, "Cannot insert record after 100% max percentage"));
                        continue;
                    }

                    newDeliverableStatus.AUTO_PERCENTAGE = newDeliverableStatus.MAX_PERCENTAGE + 0.0001m;
                    newDeliverableStatus.MAX_PERCENTAGE = 1m;
                }

                List<DOCTYPE> newAssignedDocTypes = newDeliverableStatus.GetAssignedDocTypes();
                DELIVERABLES_STATUS findEntity = MainViewModel.Entities.FirstOrDefault(x => x.AUTO_PERCENTAGE == newDeliverableStatus.AUTO_PERCENTAGE && x.MAX_PERCENTAGE == newDeliverableStatus.MAX_PERCENTAGE && newAssignedDocTypes.Any(y => x.GetAssignedDocTypes().Any(z => z.GUID == y.GUID)));

                if (findEntity == null)
                    newDeliverableStatuses.Add(newDeliverableStatus);
                else
                {
                    List<DOCTYPE> assignedDOCTYPE = entity.GetAssignedDocTypes();
                    foreach (DOCTYPE doctype in assignedDOCTYPE)
                    {
                        errorMessages.Add(new ErrorMessage(newDeliverableStatus.NAME, "Document type: " + doctype.CODE + " with auto percentage: " + findEntity.AUTO_PERCENTAGE * 100 + "% already exists"));
                    }
                }
            }

            MainViewModel.BaseBulkSave(newDeliverableStatuses);
        }

        public bool CanCopyFrom()
        {
            return isProjectSpecific;
        }

        public void CopyFrom()
        {
            var bulkEditEnumsViewModel = BulkEditEnumsViewModel.Create((IEnumerable<object>)PROJECTCollectionForCopy, "NUMBER");
            if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Select project to copy",
                    "BulkEditEnums", bulkEditEnumsViewModel) == MessageResult.OK)
            {
                if (bulkEditEnumsViewModel.SelectedItem != null)
                {
                    IGuidEntityKey entityWithGuid = bulkEditEnumsViewModel.SelectedItem as IGuidEntityKey;
                    if (entityWithGuid != null)
                    {
                        Guid? queryGuid = entityWithGuid.GUID == Guid.Empty ? (Guid?)null : entityWithGuid.GUID;
                        IBluePrintsEntitiesUnitOfWork unitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
                        var copyEntities = unitOfWork.DELIVERABLES_STATUSES.Where(x => x.GUID_PROJECT == queryGuid);
                        setAssignedDocumentTypes(copyEntities, unitOfWork.DSTATUS_DOCTYPES.Where(x => x.DELIVERABLES_STATUS.GUID_PROJECT == queryGuid));

                        List<ErrorMessage> errorMessages;
                        VerifyAndSaveDuplicateItems(copyEntities, out errorMessages, true);
                        ShowErrorMessage("Failed to copy these statuses", errorMessages);
                    }
                }
            }
        }

        protected override string ExportFilename()
        {
            if (isProjectSpecific)
                return loadPROJECT.NUMBER + "_DeliverablesStatus";
            else
                return "Global_DeliverablesStatus";
        }

        public override string UnifiedValueValidation(DELIVERABLES_STATUS projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public IEnumerable<DSTATUS_DOCTYPE> DSTATUS_DOCTYPECollection
        {
            get
            {
                return GetEntities<DSTATUS_DOCTYPE>();
            }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public CollectionViewModel<DSTATUS_DOCTYPE, DSTATUS_DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork> DSTATUS_DOCTYPECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<DSTATUS_DOCTYPE, DSTATUS_DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<DSTATUS_DOCTYPE>();
            }
        }
        #endregion
    }
}