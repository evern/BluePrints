using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using BaseModel.ViewModel.Base;
using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;
using BaseModel.Misc;
using DevExpress.Xpf.Grid.TreeList;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System.Threading;
using BaseModel.ViewModel.Document;
using BluePrints.Common.Resources;
using System.Globalization;
using BluePrints.Common.Projections;
using BaseModel.Data.Helpers;
using BluePrints.Common;
using BluePrints.Common.Base;
using System.Collections.ObjectModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the RA_STUDY collection view model.
    /// </summary>
    public partial class RA_STUDYCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <RA_STUDY, RA_STUDY, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of RA_STUDYCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RA_STUDYCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RA_STUDYCollectionViewModelWrapper(unitOfWorkFactory));
        }

        private Data.PROJECT loadPROJECT;
        /// <summary>
        /// Initializes a new instance of the RA_STUDYCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the RA_STUDYCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RA_STUDYCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> BluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<RA_STUDY_TYPE, RA_STUDY_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_TYPES);
            loaderCollection.AddLoaderDescription<RA_GUIDE_PROMPT, RA_GUIDE_PROMPT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_PROMPTS);
            loaderCollection.AddLoaderDescription<RA_GUIDE_SUBPROMPT, RA_GUIDE_SUBPROMPT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_SUBPROMPTS);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.RA_STUDIES);
        }

        protected override Func<IRepositoryQuery<RA_STUDY>, IQueryable<RA_STUDY>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RA_STUDY> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Behavior

        public bool CanEdit()
        {
            if (SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new DualEntitiesParameter<RA_STUDY, PROJECT>(SelectedEntity, loadPROJECT), "RA_STUDYSingleObjectView", "[" + SelectedEntity.NAME + "] Risk Assessment");
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        #endregion

        #region View Properties
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(RA_STUDY projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "RA_STUDYCollectionViewModelWrapper_V1"; }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection == null)
                    return new List<USER>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection.OrderBy(x => x.NAME).ToList();
            }
        }

        public IEnumerable<RA_STUDY_TYPE> RA_STUDY_TYPECollection
        {
            get
            {
                var collection = GetEntities<RA_STUDY_TYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.STUDY_TYPE);
                return collection;
            }
        }

        #endregion

        #region Navigation
        public override string UnifiedRowValidation(RA_STUDY projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(RA_STUDY projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion
    }
}
