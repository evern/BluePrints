using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class USERCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of USERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static USERCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new USERCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the USERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the USERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected USERCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<ROLE, ROLE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.ROLES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.USERS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<USER>, IQueryable<USER>> specifyMainViewModelProjection()
        {
            if (LoginCredentials.CurrentUser.NAME == BluePrintsResources.Default_AdminUsername)
                return query => query.OrderBy(x => x.NAME);
            else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => query.ToArray().Where(x => x.GUID_ROLE == null || x.GUID_ROLE == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE).Contains((Guid)x.GUID_ROLE)).AsQueryable();
        }

        public IEnumerable<Guid> ChildrenRoles(Guid roleGuid)
        {
            foreach (var role in ROLECollection)
                if (role.PARENTGUID == roleGuid)
                {
                    yield return role.GUID;

                    foreach (var entityChild in ChildrenRoles(role.GUID))
                        yield return entityChild;
                }
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<USER> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "USERCollectionViewModelWrapper"; }
        }

        public IEnumerable<ROLE> ROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<ROLE> RestrictedROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (collection != null)
                {
                    if (LoginCredentials.CurrentUser.NAME == BluePrintsResources.Default_AdminUsername)
                        collection = collection.OrderBy(x => x.NAME);
                    else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                        collection = collection.Where(x => x.GUID == Guid.Empty);
                    else
                        collection = collection.Where(x => x.GUID == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE).Contains((Guid)x.GUID)).OrderBy(x => x.NAME);
                }

                return collection;
            }
        }
        #endregion

        #region View Commands

        private IDialogService USERImportDialogService
        {
            get { return this.GetRequiredService<IDialogService>("USERImportDialogService"); }
        }

        public void Import()
        {
            var selectEntitiesViewModel = USERSelectionViewModel.Create(MainViewModel.Entities);
            if (USERImportDialogService.ShowDialog(MessageButton.OKCancel, "Select Users to Import", "USERSelectionView",
                    selectEntitiesViewModel) == MessageResult.OK)
                MainViewModel.BulkSave(selectEntitiesViewModel.SelectedEntities);

            selectEntitiesViewModel = null;
        }

        #endregion
    }
}