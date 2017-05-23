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

        protected override void InitializeParameters(object parameter)
        {
            if (parameter != null)
            {
                var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
                loadPROJECT = PROJECTParameter.GetEntity();
                isProjectSpecific = true;
            }
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>>
            ConstructMainViewModelProjection()
        {
            if (isProjectSpecific)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.GUID_DOCTYPE).ThenBy(x => x.MAX_PERCENTAGE);
            else
                return query => query.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.GUID_DOCTYPE).ThenBy(x => x.MAX_PERCENTAGE);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<DELIVERABLES_STATUS> entities)
        {
            MainViewModel.IsContinueSaveCallBack = IsContinueSaveCallBack;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private bool IsContinueSaveCallBack(DELIVERABLES_STATUS entity, bool isNewEntity)
        {
            if (isProjectSpecific)
                entity.GUID_PROJECT = loadPROJECT.GUID;

            return true;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "DELIVERABLES_STATUSCollectionViewModelWrapper"; }
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

        public void Duplicate(BarEditItem barEdit)
        {
            Guid doctypeGuid;
            List<DELIVERABLES_STATUS> newEntities = new List<DELIVERABLES_STATUS>();
            if (Guid.TryParse(barEdit.EditValue.ToString(), out doctypeGuid))
            {
                VerifyAndSaveDuplicateItems(MainViewModel.SelectedEntities, doctypeGuid);
            }
        }

        private void VerifyAndSaveDuplicateItems(IEnumerable<DELIVERABLES_STATUS> deliverableStatuses, Guid? docTypeGuid = null)
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

                if(docTypeGuid != null)
                    newDeliverableStatus.GUID_DOCTYPE = docTypeGuid;

                DELIVERABLES_STATUS findEntity = MainViewModel.Entities.FirstOrDefault(x => x.MAX_PERCENTAGE == newDeliverableStatus.MAX_PERCENTAGE && x.GUID_DOCTYPE == newDeliverableStatus.GUID_DOCTYPE);
                if (findEntity == null)
                {
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(newDeliverableStatus, null, null, null, EntityMessageType.Added);
                    duplicateDeliverableStatuses.Add(newDeliverableStatus);
                }
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
            var bulkEditEnumsViewModel =
                            BulkEditEnumsViewModel.Create((IEnumerable<object>)PROJECTCollectionForCopy, "NUMBER");
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
                        VerifyAndSaveDuplicateItems(copyEntities);
                    }
                }
            }
        }
        #endregion  
    }
}