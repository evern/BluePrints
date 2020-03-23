using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Dialogs;
using BaseModel.Data.Helpers;
using BaseModel.ViewModel.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using BluePrints.Common.Projections;

namespace BluePrints.ViewModels
{
    public class TENDER_PROFILE_ITEMSelectionViewModelWrapper : TENDER_PROFILE_ITEMCollectionViewModelWrapper
    {
        PROJECT loadPROJECT;
        bool isFullyLoaded;
        public TENDER_PROFILE projectTENDER_PROFILE { get; set; }
        /// <summary>
        /// Creates a new instance of TENDER_PROFILE_ITEMCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static new TENDER_PROFILE_ITEMSelectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new TENDER_PROFILE_ITEMSelectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the TENDER_PROFILE_ITEMSelectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the TENDER_PROFILE_ITEMSelectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected TENDER_PROFILE_ITEMSelectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected override void resolveParameters(object parameter)
        {
            bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
            EntitiesParameter<PROJECT> entityParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = entityParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<TENDER_PROFILE, TENDER_PROFILE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.TENDER_PROFILES);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS);
        }
        
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.TENDER_PROFILE_ITEMS);
        }

        protected override Func<IRepositoryQuery<TENDER_PROFILE_ITEM>, IQueryable<TENDER_PROFILE_ITEM>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.TENDER_PROFILE.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TENDER_PROFILE_ITEM> entities)
        {
            projectTENDER_PROFILE = TENDER_PROFILECollection.FirstOrDefault(x => x.GUID_PROJECT == loadPROJECT.GUID);
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
            if (projectTENDER_PROFILE == null)
                ShowDefaultTenderProfileSelection();

            isFullyLoaded = true;
        }

        #region Collection Call Backs
        protected DevExpress.Mvvm.IDialogService DefaultTenderProfileSelectionDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DefaultTenderProfileSelectionDialog"); }
        }

        /// <summary>
        /// CallBack to apply tender profile association
        /// </summary>
        public override bool OnBeforeEntitySaved(TENDER_PROFILE_ITEM entity)
        {
            if (projectTENDER_PROFILE == null)
                return false;

            entity.GUID_TENDER_PROFILE = projectTENDER_PROFILE.GUID;
            return true;
        }

        public override string UnifiedRowValidation(TENDER_PROFILE_ITEM projection)
        {
            return string.Empty;
        }

        public bool CanShowDefaultTenderProfileSelection()
        {
            return MainViewModel != null;
        }

        public void ShowDefaultTenderProfileSelection()
        {
            var bulkEditEnumsViewModel = BulkEditEnumsViewModel.Create(DefaultTENDER_PROFILECollection, "NAME");
            if (DefaultTenderProfileSelectionDialogService.ShowDialog(MessageButton.OKCancel, "Select Default Tender Profile", "BulkEditEnums", bulkEditEnumsViewModel) == MessageResult.OK)
            {
                if (bulkEditEnumsViewModel.SelectedItem != null)
                {
                    TENDER_PROFILE selectedEntity = bulkEditEnumsViewModel.SelectedItem as TENDER_PROFILE;
                    if (projectTENDER_PROFILE == null || MessageBoxService.ShowMessage("Are you sure you want to replace current profile with default " + selectedEntity.NAME + "?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
                    {
                        projectTENDER_PROFILE = replaceExistingProfile(selectedEntity.NAME);
                        List<TENDER_PROFILE_ITEM> newTENDER_PROFILE_ITEMS = new List<TENDER_PROFILE_ITEM>();
                        foreach(TENDER_PROFILE_ITEM defaultTENDER_PROFILE_ITEM in selectedEntity.TENDER_PROFILE_ITEM)
                        {
                            TENDER_PROFILE_ITEM copyTENDER_PROFILE_ITEM = new TENDER_PROFILE_ITEM();
                            DataUtils.ShallowCopy(copyTENDER_PROFILE_ITEM, defaultTENDER_PROFILE_ITEM);
                            copyTENDER_PROFILE_ITEM.GUID = Guid.Empty;
                            copyTENDER_PROFILE_ITEM.GUID_TENDER_PROFILE = projectTENDER_PROFILE.GUID;
                            bluePrintsUnitOfWork.TENDER_PROFILE_ITEMS.Add(copyTENDER_PROFILE_ITEM);
                        }
                        bluePrintsUnitOfWork.SaveChanges();
                        FullRefresh();
                    }
                }
            }
        }

        public bool CanPopulateTenderDeliverables()
        {
            return true;
        }

        public void PopulateTenderDeliverables()
        {
            initializeDeliverablesViewModel();
        }

        BASELINE_ITEMCollectionViewModelWrapper baseline_itemCollectionViewModel;
        /// <summary>
        /// Create a new baseline and initialize it
        /// </summary>
        private void initializeDeliverablesViewModel()
        {
            if (projectTENDER_PROFILE == null)
            {
                MessageBoxService.ShowMessage("Please select a tender profile before generating deliverables");
                return;
            }

            LoadingScreenManager.ShowLoadingScreen(1, false);
            BASELINE currentLiveBASELINE = bluePrintsUnitOfWork.BASELINES.FirstOrDefault(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
            if (currentLiveBASELINE != null)
                currentLiveBASELINE.STATUS = BaselineStatus.Superseded;

            BASELINE tenderBASELINE = new BASELINE();
            tenderBASELINE.GUID_PROJECT = loadPROJECT.GUID;
            tenderBASELINE.NAME = projectTENDER_PROFILE.NAME;
            tenderBASELINE.Revision = "Tender";
            tenderBASELINE.STATUS = BaselineStatus.Live;
            bluePrintsUnitOfWork.BASELINES.Add(tenderBASELINE);
            bluePrintsUnitOfWork.SaveChanges();
            
            baseline_itemCollectionViewModel = BASELINE_ITEMCollectionViewModelWrapper.Create();
            baseline_itemCollectionViewModel.OnEntitiesLoadedCallBackManualDispose = true;
            baseline_itemCollectionViewModel.OnEntitiesLoadedCallBack = assignDeliverables;
            baseline_itemCollectionViewModel.SetParentViewModel(this);
            var baselineSupportParameterObj = baseline_itemCollectionViewModel as ISupportParameter;

            //since tender baseline already superseeds previous live baseline, we can call using loadPROJECT parameter instead of directly invoking tender baseline in the parameter
            baselineSupportParameterObj.Parameter = new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(loadPROJECT, null, DeliverablesViewType.Both);
        }

        /// <summary>
        /// Called when baseline_itemCollectionViewModel is fully loaded
        /// </summary>
        /// <param name="deliverables">Loaded deliverables</param>
        /// <param name="parameter">Call back parameter</param>
        private void assignDeliverables(IEnumerable<BASELINE_ITEMProgress> deliverables, object parameter)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => assignDeliverablesWrapper()));
        }

        private void assignDeliverablesWrapper()
        {
            if (baseline_itemCollectionViewModel.DOCTYPECollection.Count() == 0)
            {
                MessageBoxService.ShowMessage("Please create a doc type before proceeding");
                return;
            }

            if (baseline_itemCollectionViewModel.AREACollection.Count() == 0)
            {
                MessageBoxService.ShowMessage("Please create an area before proceeding");
                return;
            }

            if (baseline_itemCollectionViewModel.loadBASELINE == null)
            {
                MessageBoxService.ShowMessage("Live baseline doesn't exists yet, please create live baseline before proceeding");
                return;
            }

            if (baseline_itemCollectionViewModel.DisplayEntities.Count() > 0)
            {
                if (MessageBoxService.ShowMessage("Deliverables list is not empty, this action will clear the list, are you sure you wish to proceed?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                    return;
            }

            int createdDeliverablesCount = 0;
            foreach (TENDER_PROFILE_ITEM tenderItem in DisplayEntities)
            {
                decimal assignHours = projectTENDER_PROFILE.TENDER_HOURS * tenderItem.HOURS_PERCENTAGE;
                Guid assignDepartment = tenderItem.GUID_DEPARTMENT;
                Guid assignDiscipline = tenderItem.GUID_DISCIPLINE;

                //User couldn't proceed to this stage without having the following property validated as not null from PROJECTCollectionView
                DateTime startDate = (DateTime)loadPROJECT.TENDER_PROJECT_START;
                decimal tenderDuration = (decimal)loadPROJECT.TENDER_PROJECT_DURATION;
                int totalDurationInDays = Convert.ToInt32(tenderDuration * 7);
                DateTime endDate = startDate.AddDays(totalDurationInDays);

                BASELINE_ITEMProgress baseline_item = new BASELINE_ITEMProgress();
                baseline_item.Entity.Entity.GUID_DEPARTMENT = assignDepartment;
                baseline_item.Entity.Entity.GUID_DISCIPLINE = assignDiscipline;

                //Default area has been validated before
                baseline_item.Entity.Entity.GUID_AREA = baseline_itemCollectionViewModel.AREACollection.First().GUID;
                baseline_item.Entity.Entity.GUID_BASELINE = baseline_itemCollectionViewModel.loadBASELINE.GUID;
                baseline_item.Entity.Entity.BUDGET_HOURS = assignHours;
                //Doc type has been validated before and it doesn't matter which is used
                baseline_item.Entity.Entity.GUID_DOCTYPE = baseline_itemCollectionViewModel.DOCTYPECollection.First().GUID;

                //pro-rate the dates of the deliverable based on tender item
                int startProrateDurationInDays = Convert.ToInt32(totalDurationInDays * tenderItem.SCHEDULE_START_PERCENTAGE);
                DateTime proRatedStartDate = startDate.AddDays(startProrateDurationInDays);
                int endProrateDurationInDays = Convert.ToInt32(totalDurationInDays * (1 - tenderItem.SCHEDULE_FINISH_PERCENTAGE));
                DateTime proRatedEndDate = endDate.AddDays(-1 * endProrateDurationInDays);

                baseline_item.Entity.Entity.START_DATE = proRatedStartDate;
                baseline_item.Entity.Entity.END_DATE = proRatedEndDate;
                baseline_itemCollectionViewModel.Save(baseline_item);

                //subjobs will be generated in baseline_itemCollectionViewModel and notify SUBJOBCollection
                SUBJOB baseline_itemSubJob = SUBJOBCollection.First(x => x.GUID == (Guid)baseline_item.Subjob_Guid);
                baseline_itemSubJob.STARTDATE = startDate;
                baseline_itemSubJob.ENDDATE = endDate;
                baseline_itemSubJob.BELLCURVESHAPE = tenderItem.BELLCURVESHAPE;
                //nullify effects of review dates
                baseline_itemSubJob.REVIEWSTARTDATE = startDate;
                baseline_itemSubJob.REVIEWENDDATE = baseline_itemSubJob.REVIEWSTARTDATE;
                SUBJOBCollectionViewModel.Save(baseline_itemSubJob);

                createdDeliverablesCount += 1;
            }

            Refresh_From_P6();
            LoadingScreenManager.CloseLoadingScreen();
            MessageBoxService.ShowMessage(createdDeliverablesCount.ToString() + " deliverables created");
        }

        public async void Refresh_From_P6()
        {
            await BluePrintsContextHelper.RefreshDeliverablesDataPointsByProject(loadPROJECT.NUMBER);
        }

        /// <summary>
        /// Try to find project specific TENDER_PROFILE from data context and replace it
        /// </summary>
        /// <param name="profileName">Profile name to use</param>
        /// <returns>Return existing or new TENDER_PROFILE</returns>
        private TENDER_PROFILE replaceExistingProfile(string profileName)
        {
            TENDER_PROFILE findProject_TENDER_PROFILE = TENDER_PROFILECollection.FirstOrDefault(x => x.GUID_PROJECT == loadPROJECT.GUID);
            if (findProject_TENDER_PROFILE != null)
            {
                findProject_TENDER_PROFILE.NAME = profileName;
                TENDER_PROFILECollectionViewModel.Delete(findProject_TENDER_PROFILE);
            }

            TENDER_PROFILE newProject_TENDER_PROFILE = new TENDER_PROFILE();
            newProject_TENDER_PROFILE.NAME = profileName;
            newProject_TENDER_PROFILE.GUID_PROJECT = loadPROJECT.GUID;
            TENDER_PROFILECollectionViewModel.Save(newProject_TENDER_PROFILE);
            return newProject_TENDER_PROFILE;
        }
        #endregion

        #endregion

        #region View Properties
        public decimal? TenderHours
        {
            get
            {
                if (projectTENDER_PROFILE == null)
                    return null;

                return projectTENDER_PROFILE.TENDER_HOURS;
            }
            set
            {
                if(value != null)
                    projectTENDER_PROFILE.TENDER_HOURS = (decimal)value;

                if(isFullyLoaded)
                    TENDER_PROFILECollectionViewModel.Save(projectTENDER_PROFILE);
            }
        }

        public CollectionViewModel<TENDER_PROFILE, TENDER_PROFILE, Guid, IBluePrintsEntitiesUnitOfWork> TENDER_PROFILECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<TENDER_PROFILE, TENDER_PROFILE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<TENDER_PROFILE>();
            }
        }

        public CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork> SUBJOBCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<SUBJOB>();
            }
        }

        public IEnumerable<TENDER_PROFILE> DefaultTENDER_PROFILECollection
        {
            get
            {
                var collection = GetEntities<TENDER_PROFILE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
                return collection;
            }
        }

        public IEnumerable<TENDER_PROFILE> TENDER_PROFILECollection
        {
            get
            {
                var collection = GetEntities<TENDER_PROFILE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        protected override void OnClose(CancelEventArgs e)
        {
            if (baseline_itemCollectionViewModel != null)
                baseline_itemCollectionViewModel.CleanUpEntitiesLoader();

            base.OnClose(e);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "[" + getTenderProfileProjectName() + "] Tender Profile"; }
        }

        private string getTenderProfileProjectName()
        {
            if (loadPROJECT == null)
                return string.Empty;

            return loadPROJECT.NUMBER;
        }

        #endregion
    }
}