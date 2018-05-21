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
using DevExpress.Xpf.Editors;

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
            return query => guideSubPromptProjection(query);
        }

        private IQueryable<RA_GUIDE_SUBPROMPT> guideSubPromptProjection(IRepositoryQuery<RA_GUIDE_SUBPROMPT> guideSubPrompt)
        {
            List<RA_GUIDE_SUBPROMPT> guideSubPromptCollection = guideSubPrompt.ToList();

            foreach (RA_GUIDE_SUBPROMPT subPrompt in guideSubPromptCollection)
            {
                RA_GUIDE_PROMPT guidePrompt = RA_GUIDE_PROMPTCollection.FirstOrDefault(x => x.GUID == subPrompt.GUID_GUIDE_PROMPT);
                if(guidePrompt != null)
                {
                    subPrompt.GUID_STUDY_TYPE = guidePrompt.GUID_STUDY_TYPE;
                }
            }

            return guideSubPromptCollection.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RA_GUIDE_SUBPROMPT> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEntitySaved;
            MainViewModel.ManualPasteAction = ManualPasteAction;
            MainViewModel.SetParentViewModel(this);
            RA_GUIDE_PROMPTViewModel.SetParentViewModel(this);
            RA_STUDY_TYPEViewModel.SetParentViewModel(this);
            
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            //if (changedType == typeof(RA_GUIDE_PROMPT))
            //    RA_GUIDE_PROMPTViewModel.Refresh();

            //if (changedType == typeof(RA_STUDY_TYPE))
            //    RA_STUDY_TYPEViewModel.Refresh();

            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }

        public void ProcessStudyNewValue(ProcessNewValueEventArgs e)
        {
            if (!e.Handled)
            {
                RA_STUDY_TYPE study_type = RA_STUDY_TYPECollection.FirstOrDefault(x => x.STUDY_TYPE == e.DisplayText);
                if(study_type == null)
                {
                    RA_STUDY_TYPE new_study_type = new RA_STUDY_TYPE();
                    new_study_type.STUDY_TYPE = e.DisplayText;
                    RA_STUDY_TYPEViewModel.Save(new_study_type);
                }
            }
        }

        public void ProcessGuidePromptNewValue(ProcessNewValueEventArgs e)
        {
            if (!e.Handled)
            {
                RA_GUIDE_PROMPT guide_prompt = RA_GUIDE_PROMPTCollection.FirstOrDefault(x => x.GUIDE_PROMPT == e.DisplayText);

                if (guide_prompt == null && DisplaySelectedEntity != null && DisplaySelectedEntity.GUID_STUDY_TYPE != null)
                {
                    RA_GUIDE_PROMPT new_guide_prompt = new RA_GUIDE_PROMPT();
                    new_guide_prompt.GUID_STUDY_TYPE = DisplaySelectedEntity.GUID_STUDY_TYPE;
                    new_guide_prompt.GUIDE_PROMPT = e.DisplayText;
                    RA_GUIDE_PROMPTViewModel.Save(new_guide_prompt);
                }
            }
        }

        public bool onBeforeEntitySaved(RA_GUIDE_SUBPROMPT subPrompt)
        {
            if (subPrompt.GUID_STUDY_TYPE != null)
            {
                RA_GUIDE_PROMPT guide_prompt_by_guid = RA_GUIDE_PROMPTCollection.FirstOrDefault(x => x.GUID == subPrompt.GUID_GUIDE_PROMPT);
                if(guide_prompt_by_guid != null)
                {
                    RA_GUIDE_PROMPT guide_prompt_by_verification = RA_GUIDE_PROMPTCollection.FirstOrDefault(x => x.GUIDE_PROMPT == guide_prompt_by_guid.GUIDE_PROMPT && x.GUID_STUDY_TYPE == subPrompt.GUID_STUDY_TYPE);
                    if (guide_prompt_by_verification == null)
                    {
                        RA_GUIDE_PROMPT new_guide_prompt = new RA_GUIDE_PROMPT();
                        new_guide_prompt.GUID_STUDY_TYPE = subPrompt.GUID_STUDY_TYPE;
                        new_guide_prompt.GUIDE_PROMPT = guide_prompt_by_guid.GUIDE_PROMPT;
                        RA_GUIDE_PROMPTViewModel.Save(new_guide_prompt);
                        subPrompt.GUID_GUIDE_PROMPT = new_guide_prompt.GUID;
                    }
                    else if (guide_prompt_by_verification != null)
                        subPrompt.GUID_GUIDE_PROMPT = guide_prompt_by_verification.GUID;
                }

            }

            return true;
        }

        public override string UnifiedRowValidation(RA_GUIDE_SUBPROMPT projection)
        {
            if (projection.GUID_GUIDE_PROMPT == Guid.Empty || projection.GUID_STUDY_TYPE == Guid.Empty)
            {
                return projection.GUID_STUDY_TYPE == Guid.Empty ? "Study type is empty" : "Guide prompt is empty";
            }

            return string.Empty;
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
            KeyValuePair<ColumnBase, string> study_type_paste_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new RA_GUIDE_SUBPROMPT().GUID_STUDY_TYPE)));
            if(study_type_paste_data.Key != null)
            {
                RA_STUDY_TYPE study_type = RA_STUDY_TYPECollection.FirstOrDefault(x => x.STUDY_TYPE == study_type_paste_data.Value);
                if (study_type == null)
                {
                    RA_STUDY_TYPE new_study_type = new RA_STUDY_TYPE();
                    new_study_type.STUDY_TYPE = study_type_paste_data.Value;
                    RA_STUDY_TYPEViewModel.Save(new_study_type);
                    pasteEntity.GUID_STUDY_TYPE = new_study_type.GUID;
                }
                else
                    pasteEntity.GUID_STUDY_TYPE = study_type.GUID;
            }

            if (pasteEntity.GUID_STUDY_TYPE != null)
            {
                KeyValuePair<ColumnBase, string> guide_prompt_paste_data = pasteData.FirstOrDefault(x => x.Key.FieldName.Contains(BindableBase.GetPropertyName(() => new RA_GUIDE_SUBPROMPT().GUID_GUIDE_PROMPT)));
                if (guide_prompt_paste_data.Key != null)
                {
                    RA_GUIDE_PROMPT guide_prompt = RA_GUIDE_PROMPTCollection.FirstOrDefault(x => x.GUIDE_PROMPT == guide_prompt_paste_data.Value && x.GUID_STUDY_TYPE == pasteEntity.GUID_STUDY_TYPE);
                    if (guide_prompt == null)
                    {
                        RA_GUIDE_PROMPT new_guide_prompt = new RA_GUIDE_PROMPT();
                        new_guide_prompt.GUID_STUDY_TYPE = pasteEntity.GUID_STUDY_TYPE;
                        new_guide_prompt.GUIDE_PROMPT = guide_prompt_paste_data.Value;
                        RA_GUIDE_PROMPTViewModel.Save(new_guide_prompt);
                        pasteEntity.GUID_GUIDE_PROMPT = new_guide_prompt.GUID;
                    }
                    else
                        pasteEntity.GUID_GUIDE_PROMPT = guide_prompt.GUID;
                }
            }
            else
                return false;

            if (pasteEntity.GUIDE_SUBPROMPT != null)
                return true;
            else
                return false;
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
