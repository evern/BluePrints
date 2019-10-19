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
    /// Represents the RA_STUDY_TYPE collection view model.
    /// </summary>
    public partial class RA_STUDY_TYPECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <RA_STUDY_TYPE, RA_STUDY_TYPEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of RA_STUDY_TYPECollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RA_STUDY_TYPECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RA_STUDY_TYPECollectionViewModelWrapper(unitOfWorkFactory));
        }
        
        /// <summary>
        /// Initializes a new instance of the RA_STUDY_TYPECollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the RA_STUDY_TYPECollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RA_STUDY_TYPECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> BluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<RA_GUIDE_PROMPT, RA_GUIDE_PROMPT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_PROMPTS);
            loaderCollection.AddLoaderDescription<RA_GUIDE_SUBPROMPT, RA_GUIDE_SUBPROMPT, Guid, IBluePrintsEntitiesUnitOfWork>(BluePrintsUnitOfWorkFactory, x => x.RA_GUIDE_SUBPROMPTS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(BluePrintsUnitOfWorkFactory, x => x.RA_STUDY_TYPES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<RA_STUDY_TYPE>, IQueryable<RA_STUDY_TYPEProjection>> specifyMainViewModelProjection()
        {
            return query => RA_STUDY_TYPEProjectionQueries.RA_STUDY_TYPEProjection(query, RA_GUIDE_PROMPTCollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RA_STUDY_TYPEProjection> entities)
        {
            RA_GUIDE_PROMPTViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeGUIDE_PROMPTSavedIsContinue;
            RA_GUIDE_PROMPTViewModel.OnSelectedEntityChangedCallBack = onSelectedGuide_PromptChanged;
            RA_GUIDE_SUBPROMPTViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeGUIDE_SUBPROMPTSavedIsContinue;
            MainViewModel.SetParentViewModel(this);
            RA_GUIDE_PROMPTViewModel.SetParentViewModel(this);
            RA_GUIDE_SUBPROMPTViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(RA_GUIDE_PROMPT))
                refreshGuidePrompts();

            if (changedType == typeof(RA_GUIDE_SUBPROMPT))
                refreshGuideSubPrompts();

            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }
        #endregion

        #region View Behavior
        public bool PROMPTEnabled => DisplaySelectedEntity != null;
        public bool SUBPROMPTEnabled => PROMPTEnabled && RA_GUIDE_PROMPTViewModel != null && RA_GUIDE_PROMPTViewModel.SelectedEntity != null;

        public override void OnDisplaySelectedEntityChanged(RA_STUDY_TYPEProjection entity)
        {
            refreshGuidePrompts();
            base.OnDisplaySelectedEntityChanged(entity);
        }

        private void refreshGuidePrompts()
        {
            guide_prompts = null;
            guide_subprompts = null;
            this.RaisePropertyChanged(x => x.NewPromptInstruction);
            this.RaisePropertyChanged(x => x.PROMPTEnabled);
            this.RaisePropertyChanged(x => x.SUBPROMPTEnabled);
            this.RaisePropertyChanged(x => x.GUIDE_PROMPTS);
        }

        #region Guide Prompt
        private bool onBeforeGUIDE_PROMPTSavedIsContinue(RA_GUIDE_PROMPT guide_prompt)
        {
            if (DisplaySelectedEntity == null)
                return false;

            guide_prompt.GUID_STUDY_TYPE = DisplaySelectedEntity.GUID;
            return true;
        }

        ObservableCollection<RA_GUIDE_PROMPT> guide_prompts;
        public ObservableCollection<RA_GUIDE_PROMPT> GUIDE_PROMPTS
        {
            get
            {
                if (RA_GUIDE_PROMPTCollection == null || DisplaySelectedEntity == null)
                    return null;

                if(guide_prompts == null)
                {
                    guide_prompts = new ObservableCollection<RA_GUIDE_PROMPT>();
                    foreach (RA_GUIDE_PROMPT guide_prompt in RA_GUIDE_PROMPTCollection.Where(x => x.GUID_STUDY_TYPE == DisplaySelectedEntity.GUID))
                        guide_prompts.Add(guide_prompt);
                }

                return guide_prompts;
            }
        }

        public string NewPromptInstruction
        {
            get
            {
                if (DisplaySelectedEntity == null)
                    return "Please select a study type before adding new prompt";

                return "Type here to add new agenda, push enter when complete";
            }
        }

        private void onSelectedGuide_PromptChanged(RA_GUIDE_PROMPT entity)
        {
            refreshGuideSubPrompts();
        }

        private void refreshGuideSubPrompts()
        {
            guide_subprompts = null;
            this.RaisePropertyChanged(x => x.NewPromptInstruction);
            this.RaisePropertyChanged(x => x.PROMPTEnabled);
            this.RaisePropertyChanged(x => x.SUBPROMPTEnabled);
            this.RaisePropertyChanged(x => x.GUIDE_SUBPROMPTS);
        }
        #endregion

        #region Guide Sub Prompt
        private bool onBeforeGUIDE_SUBPROMPTSavedIsContinue(RA_GUIDE_SUBPROMPT guide_subprompt)
        {
            if (RA_GUIDE_PROMPTViewModel == null || RA_GUIDE_PROMPTViewModel.SelectedEntity == null)
                return false;

            guide_subprompt.GUID_GUIDE_PROMPT = RA_GUIDE_PROMPTViewModel.SelectedEntity.GUID;
            return true;
        }

        ObservableCollection<RA_GUIDE_SUBPROMPT> guide_subprompts;
        public ObservableCollection<RA_GUIDE_SUBPROMPT> GUIDE_SUBPROMPTS
        {
            get
            {
                if (RA_GUIDE_SUBPROMPTCollection == null || RA_GUIDE_PROMPTViewModel == null || RA_GUIDE_PROMPTViewModel.SelectedEntity == null)
                    return null;

                if (guide_subprompts == null)
                {
                    guide_subprompts = new ObservableCollection<RA_GUIDE_SUBPROMPT>();
                    foreach (RA_GUIDE_SUBPROMPT guide_subprompt in RA_GUIDE_SUBPROMPTCollection.Where(x => x.GUID_GUIDE_PROMPT == RA_GUIDE_PROMPTViewModel.SelectedEntity.GUID))
                        guide_subprompts.Add(guide_subprompt);
                }

                return guide_subprompts;
            }
        }
        #endregion
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "RA_STUDY_TYPEViewModelWrapper_V1"; }
        }

        #endregion

        #region Navigation
        public override string UnifiedValueValidation(RA_STUDY_TYPEProjection projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(RA_STUDY_TYPEProjection projection)
        {
            return string.Empty;
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

        public CollectionViewModel<RA_GUIDE_SUBPROMPT, RA_GUIDE_SUBPROMPT, Guid, IBluePrintsEntitiesUnitOfWork> RA_GUIDE_SUBPROMPTViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return (CollectionViewModel<RA_GUIDE_SUBPROMPT, RA_GUIDE_SUBPROMPT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<RA_GUIDE_SUBPROMPT>();
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

        public IEnumerable<RA_GUIDE_SUBPROMPT> RA_GUIDE_SUBPROMPTCollection
        {
            get
            {
                var collection = GetEntities<RA_GUIDE_SUBPROMPT>();
                return collection;
            }

        }
        #endregion
    }
}
