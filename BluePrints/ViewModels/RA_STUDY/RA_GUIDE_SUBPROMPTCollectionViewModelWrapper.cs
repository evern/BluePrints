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
    /// Represents the RA_GUIDE_SUBPROMPT collection view model.
    /// </summary>
    public partial class RA_GUIDE_SUBPROMPTCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <RA_GUIDE_SUBPROMPT, RA_GUIDE_SUBPROMPT, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of RA_STUDY_TYPECollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RA_GUIDE_SUBPROMPTCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RA_GUIDE_SUBPROMPTCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the RA_GUIDE_SUBPROMPTCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the RA_STUDY_TYPECollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RA_GUIDE_SUBPROMPTCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> BluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<RA_STUDY_TYPE, RA_STUDY_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_TYPES);
            loaderCollection.AddLoaderDescription<RA_GUIDE_PROMPT, RA_GUIDE_PROMPT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_PROMPTS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_SUBPROMPTS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<RA_GUIDE_SUBPROMPT>, IQueryable<RA_GUIDE_SUBPROMPT>> specifyMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RA_GUIDE_SUBPROMPT> entities)
        {
            MainViewModel.ManualPasteAction = ManualPasteAction;
            MainViewModel.SetParentViewModel(this);
            RA_GUIDE_PROMPTViewModel.SetParentViewModel(this);
            RA_STUDY_TYPEViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }

        public override string UnifiedValueValidation(RA_GUIDE_SUBPROMPT projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, RA_GUIDE_SUBPROMPT projection, bool isNew)
        {
            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }


        public bool ManualPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, RA_GUIDE_SUBPROMPT pasteEntity)
        {
            KeyValuePair<ColumnBase, string> study_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new RA_GUIDE_SUBPROMPT().GUID_STUDY_TYPE)));
            //KeyValuePair<ColumnBase, string> study_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new RA_GUIDE_SUBPROMPT().GUID_STUDY_TYPE)));

            string s = study_data.ToString();

            return true;
        }
        #endregion

        #region View Behavior

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "RA_GUIDE_SUBPROMPTViewModelWrapper_V1"; }
        }

        #endregion

        #region Navigation

        public CollectionViewModel<RA_STUDY_TYPE, RA_STUDY_TYPE, Guid, IBluePrintsEntitiesUnitOfWork> RA_STUDY_TYPEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<RA_STUDY_TYPE, RA_STUDY_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<RA_STUDY_TYPE>();
            }
        }

        public CollectionViewModel<RA_GUIDE_PROMPT, RA_GUIDE_PROMPT, Guid, IBluePrintsEntitiesUnitOfWork> RA_GUIDE_PROMPTViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<RA_GUIDE_PROMPT, RA_GUIDE_PROMPT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<RA_GUIDE_PROMPT>();
            }
        }

        public IEnumerable<RA_STUDY_TYPE> RA_STUDY_TYPECollection
        {
            get
            {
                var collection = GetEntities<RA_STUDY_TYPE>();
                return collection;
            }

        }

        public IEnumerable<RA_GUIDE_PROMPT> RA_GUIDE_PROMPTCollection
        {
            get
            {
                var collection = GetEntities<RA_GUIDE_PROMPT>();
                return collection;
            }
        }

        #endregion
    }
}
