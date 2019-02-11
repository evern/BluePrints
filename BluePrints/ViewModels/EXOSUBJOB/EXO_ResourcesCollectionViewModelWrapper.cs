using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_ResourcesCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoResourceProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_ResourcesCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_ResourcesCollectionViewModelWrapper(unitOfWorkFactory));
        }
        
        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_ResourcesCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {

        }

        #region Database Operations
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<STAFF, STAFF, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STAFF);
            loaderCollection.AddLoaderDescription<PROFILE, PROFILE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.PROFILE);
        }
        #endregion

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoResourceProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetResources(primeroUnitOfWork);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoResourceProjection> entities)
        {
            MainViewModel.ManualRowPasteAction = ManualRowPasteAction;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        protected override void OnSelectedEntitiesChanged()
        {
        }

        protected bool onBeforeEntitySaved(ExoResourceProjection projection)
        {
            commitToExo(projection);
            return false;
        }

        /// <summary>
        /// Since CollectionViewModelBase is a POCO view model, an the instance of this class will also expose the AddUndoCommand property that can be used as a binding source in views.
        /// </summary>
        public virtual void CommitNewRow(RowEventArgs e)
        {
            if (e.RowHandle == DataControlBase.NewItemRowHandle)
            {
                ExoResourceProjection exoResource = (ExoResourceProjection)e.Row;
                commitToExo(exoResource);
                OnAfterNewRowAdded(exoResource);
            }
        }

        private void commitToExo(ExoResourceProjection projection)
        {
            List<ExoResourceProjection> newLines = new List<ExoResourceProjection>();
            ExoResourceProjection newLine = projection;
            newLines.Add(newLine);
            commitToExo(newLines);
        }

        private void commitToExo(IEnumerable<ExoResourceProjection> projections)
        {
            foreach(ExoResourceProjection resource in projections)
            {
                STAFF addedStaff = ExoMethods.FindExistingOrAddStaff(primeroUnitOfWork, resource.STAFFNO, resource.RESOURCENAME, resource.TITLE, resource.SECURITYPROFILEID, resource.USERPROFILEID, resource.REPORTS_TO_STAFFNO);
                if(addedStaff != null)
                {
                    resource.STAFFNO = addedStaff.STAFFNO;
                    JOBCOST_RESOURCE addedResource = ExoMethods.FindExistingOrAddResource(primeroUnitOfWork, resource.STAFFNO, resource.RESOURCE_SEQNO, resource.RESOURCENAME, resource.TITLE, resource.DEFAULT_STOCKCODE, resource.SHORTCODE);
                    resource.DEFAULT_STOCKCODE = addedResource.DEFAULT_STOCKCODE;
                }

                resource.Update();
            }

            primeroUnitOfWork.SaveChanges();
        }

        public void DeleteSelected()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to remove selected resource(s)\n\nThis will mark resource as inactive in EXO", "Warning", MessageButton.OKCancel, MessageIcon.Warning) == MessageResult.Cancel)
                return;

            delete(displaySelectedEntities);

            List<ExoResourceProjection> removeEntities = displaySelectedEntities.ToList();
            foreach (var selectedEntity in removeEntities)
            {
                DisplayEntities.Remove(selectedEntity);
            }
        }

        private void delete(IEnumerable<ExoResourceProjection> projections)
        {
            ExoMethods.RemoveStaff(primeroUnitOfWork, projections);
            ExoMethods.RemoveResources(primeroUnitOfWork, projections);
            primeroUnitOfWork.SaveChanges();
        }

        public bool ManualRowPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, ExoResourceProjection pasteEntity)
        {
            commitToExo(pasteEntity);
            DisplayEntities.Insert(0, pasteEntity);
            OnAfterNewRowAdded(pasteEntity);
            return false;
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXO_ResourcesCollectionViewModelWrapper";
            }
        }

        public override string UnifiedRowValidation(ExoResourceProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(ExoResourceProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public IEnumerable<PROFILE> SecurityPROFILECollection
        {
            get
            {
                var collection = GetEntities<PROFILE>();
                if (collection != null)
                    collection = collection.Where(x => x.PROFILETYPE == 4).OrderBy(x => x.PROFILENAME);
                return collection;
            }
        }

        public IEnumerable<PROFILE> UserPROFILECollection
        {
            get
            {
                var collection = GetEntities<PROFILE>();
                if (collection != null)
                    collection = collection.Where(x => x.PROFILETYPE == 2).OrderBy(x => x.PROFILENAME);
                return collection;
            }
        }

        public IEnumerable<STAFF> STAFFCollection
        {
            get
            {
                var collection = GetEntities<STAFF>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }
    }
}