using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
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
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>>
            specifyMainViewModelProjection()
        {
            if (isProjectSpecific)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.GUID_DOCTYPE).ThenBy(x => x.MAX_PERCENTAGE);
            else
                return query => query.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.GUID_DOCTYPE).ThenBy(x => x.MAX_PERCENTAGE);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DELIVERABLES_STATUS> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = applyProjectionProperties;
            MainViewModel.SetParentViewModel(this);
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

        private bool applyProjectionProperties(DELIVERABLES_STATUS projection)
        {
            if (isProjectSpecific)
            {
                projection.GUID_PROJECT = loadPROJECT.GUID;
            }

            return true;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "DELIVERABLES_STATUSCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "DELIVERABLES_STATUSCollectionViewModelWrapper_v2" + view_project_specific_affix; }
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
            VerifyAndSaveDuplicateItems(MainViewModel.SelectedEntities);
            TableViewService.SetImmediateUpdateRowPosition(false);
        }

        public void Duplicate(BarEditItem barEdit)
        {
            TableViewService.SetImmediateUpdateRowPosition(true);
            Guid doctypeGuid;
            List<DELIVERABLES_STATUS> newEntities = new List<DELIVERABLES_STATUS>();
            if (Guid.TryParse(barEdit.EditValue.ToString(), out doctypeGuid))
            {
                VerifyAndSaveDuplicateItems(MainViewModel.SelectedEntities, doctypeGuid);
            }
            TableViewService.SetImmediateUpdateRowPosition(false);
        }

        private void VerifyAndSaveDuplicateItems(IEnumerable<DELIVERABLES_STATUS> deliverableStatuses, Guid? docTypeGuid = null, bool isCopyFrom = false)
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
            List<DELIVERABLES_STATUS> duplicateDeliverableStatuses = new List<DELIVERABLES_STATUS>();
            foreach (var entities in deliverableStatuses)
            {
                var newDeliverableStatus = new DELIVERABLES_STATUS();
                DataUtils.ShallowCopy(newDeliverableStatus, entities);
                newDeliverableStatus.GUID = Guid.Empty;
                if (isProjectSpecific)
                    newDeliverableStatus.GUID_PROJECT = loadPROJECT.GUID;

                if(!isCopyFrom)
                {
                    if (docTypeGuid != null)
                        newDeliverableStatus.GUID_DOCTYPE = docTypeGuid;
                    //insert mode by default
                    else
                    {
                        if (newDeliverableStatus.MAX_PERCENTAGE > 1m)
                        {
                            MessageBoxService.ShowMessage("Cannot insert record after 100% max percentage");
                            continue;
                        }

                        newDeliverableStatus.AUTO_PERCENTAGE = newDeliverableStatus.MAX_PERCENTAGE + 0.01m;
                        newDeliverableStatus.MAX_PERCENTAGE = 1m;
                    }
                }

                DELIVERABLES_STATUS findEntity = MainViewModel.Entities.FirstOrDefault(x => x.AUTO_PERCENTAGE == newDeliverableStatus.AUTO_PERCENTAGE && x.MAX_PERCENTAGE == newDeliverableStatus.MAX_PERCENTAGE && x.GUID_DOCTYPE == newDeliverableStatus.GUID_DOCTYPE);
                if (findEntity == null)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(newDeliverableStatus, null, null, null, EntityMessageType.Added);
                    duplicateDeliverableStatuses.Add(newDeliverableStatus);
                }
                else
                    MessageBoxService.ShowMessage("Doctype with autopercentage: " + findEntity.AUTO_PERCENTAGE * 100 + "% already exists");
            }

            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
            MainViewModel.BulkSave(duplicateDeliverableStatuses);
        }

        private DevExpress.Mvvm.IDialogService BulkColumnEditDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BulkColumnEditService"); }
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
                        Guid? queryGuid = entityWithGuid.EntityKey == Guid.Empty ? (Guid?)null : entityWithGuid.EntityKey;
                        var copyEntities = bluePrintsUnitOfWorkFactory.CreateUnitOfWork().DELIVERABLES_STATUSES.Where(x => x.GUID_PROJECT == queryGuid);
                        VerifyAndSaveDuplicateItems(copyEntities, null, true);
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

        public override string UnifiedValueValidation(DELIVERABLES_STATUS projection, string field_name, object new_value)
        {
            return string.Empty;
        }
        #endregion
    }
}