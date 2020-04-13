using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_DesignSubjobCollectionViewModelWrapper :
        EXO_JobPermissionCollectionViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_DesignSubjobCollectionViewModelWrapper CreateDesignSubJobCollection(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_DesignSubjobCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_DesignSubjobCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Code Properties
        private BASELINE liveBASELINE;
        private PROGRESS livePROGRESS;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        #endregion

        #region Loading Operations
        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            tryCombineLocalUsers = true;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, assign_baseline);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, assign_progress);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);
            base.addEntitiesLoader();
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.APPROVED != null);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID && x.SUBJOB.PHASE.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            //legacy subjob restrictions
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == PhaseType.Design && x.STATUS == ProgressStatus.Live);

        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Baseline_Report.ToString());
        }

        private void assign_baseline(BASELINE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));

            liveBASELINE = entity;
        }

        private void assign_progress(PROGRESS progress)
        {
            if (progress == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live progress not found")));

            livePROGRESS = progress;
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobEditableProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetExoDesignSubJobProjection(query.Where(x => x.GUID_BASELINE == liveBASELINE.GUID), WORKPACKCollection, loadPROJECT, livePROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, localPrimeroUnitOfWork, USERCollection, COMMODITY_CODECollection, DOCTYPECollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobEditableProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Events
        public bool CanAlignExoBudget()
        {
            return !IsLoading;
        }

        public void AlignExoBudget()
        {
            if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_ChangeBudget)) == LoginCredentials.PermissionStatus.None)
            {
                MessageBoxService.ShowMessage("You do not have authority to change budget", "Not Authorised", MessageButton.OK);
                return;
            }

            if (MessageBoxService.ShowMessage("This will align budget for selected design jobs in exo to aggregated budget in deliverable's list, do you wish to continue?", "Align Budget", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            foreach (ExoSubJobEditableProjection subJob in SelectedEntities.Where(x => x.IsLineExistsInExo))
            {
                subJob.ExoBudget = subJob.Budget;
                ExoMethods.CommitLineBudgetCost(subJob, localPrimeroUnitOfWork);
                subJob.Update();
            }
        }

        public bool CanAutoAssignPermission()
        {
            return !IsLoading;
        }

        public void AutoAssignPermission()
        {
            if (MessageBoxService.ShowMessage("This will auto grant permission based on document type authorisation by role, but will not delete existing authorisations, do you wish to continue?", "Auto Assign Permission", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            int fullProgress = Entities.Count * USERCollection.Count();
            LoadingScreenManager.ShowLoadingScreen(fullProgress);

            int addedCount = 0;
            foreach (ExoSubJobEditableProjection subJob in Entities.Where(x => x.IsLineExistsInExo))
            {
                subJob.AuthUsers.Clear();
                foreach (USER user in USERCollection)
                {
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    newUser.User = user;
                    newUser.ShouldAssign = newUser.User.ROLE.ROLE_COMMODITY.Where(x => x.DOCTYPE != null).Any(x => SelectedEntities.Any(y => y.CommodityCode == x.DOCTYPE.CODE));
                    if (newUser.ShouldAssign && subJob.SubJobId != null && user.ProjectLocaleExoId != null)
                    {
                        if (ExoMethods.findExistingOrAddResourceAllocation(localPrimeroUnitOfWork, (int)subJob.SubJobId, (int)user.ProjectLocaleExoId))
                            addedCount += 1;

                        newUser.IsAssigned = true;
                        subJob.AuthUsers.Add(newUser);
                    }

                    LoadingScreenManager.Progress();
                }
            }

            LoadingScreenManager.CloseLoadingScreen();
            MessageBoxService.ShowMessage(addedCount + " user permission added");
        }
        #endregion

        #region View Properties
        public override IEnumerable<ExoSubJobAuth> BluePrintsUsers
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                var permissions = new List<ExoSubJobAuth>();
                if (SelectedEntities == null && MainViewModel.Entities.Count > 0)
                    SelectedEntities.Add(MainViewModel.Entities.First());

                if (SelectedEntities == null || SelectedEntities.Count == 0)
                    return null;

                foreach (USER user in USERCollection)
                {
                    IEnumerable<ExoSubJobAuth> findUsers = SelectedEntities.SelectMany(x => x.AuthUsers);
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    newUser.User = user;
                    if (SelectedEntities.All(x => x.AuthUsers.Any(y => y.User.ProjectLocaleExoId == user.ProjectLocaleExoId)))
                        newUser.IsAssigned = true;
                    else if (SelectedEntities.Any(x => x.AuthUsers.Any(y => y.User.ProjectLocaleExoId == user.ProjectLocaleExoId)))
                        newUser.IsAssigned = null;
                    else
                        newUser.IsAssigned = false;

                    if (newUser.User != null && newUser.User.ROLE != null && newUser.User.ROLE.ROLE_COMMODITY.Count > 0 && newUser.User.ROLE.ROLE_COMMODITY.Where(x => x.DOCTYPE != null).Any(x => SelectedEntities.Any(y => y.CommodityCode == x.DOCTYPE.CODE)))
                        newUser.ShouldAssign = true;

                    permissions.Add(newUser);
                }

                isPermissionLoading = false;
                this.RaisePropertyChanged(x => x.IsPermissionLoading);
                return permissions.OrderBy(x => x.User.Full_Name);
            }
        }

        bool showPreferred;
        public bool ShowPreferred
        {
            get
            {
                return showPreferred;
            }
            set
            {
                showPreferred = value;
                if (GridControlService != null)
                {
                    if (value)
                    {
                        CriteriaOperator criteriaOperator = GridControlService.FilterCriteria;
                        CriteriaOperator newCriteriaOperator;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            string filterCriteria = criteriaOperator.ToString() + " And [ShouldAssign] In (True)";
                            newCriteriaOperator = CriteriaOperator.Parse(filterCriteria);
                        }
                        else
                        {
                            newCriteriaOperator = CriteriaOperator.Parse("[ShouldAssign] In (True)");
                        }

                        GridControlService.FilterCriteria = newCriteriaOperator;
                    }
                    else
                    {
                        CriteriaOperator criteriaOperator = GridControlService.FilterCriteria;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            CriteriaOperator newCriteriaOperator;
                            string currentFilterCriteria = criteriaOperator.ToString();
                            string newfilterCriteria = currentFilterCriteria.Replace("And [ShouldAssign] In (True)", "");
                            newfilterCriteria = newfilterCriteria.Replace("[ShouldAssign] In (True)", "");
                            if (newfilterCriteria.Length >= 5)
                            {
                                string firstFiveChar = newfilterCriteria.Substring(0, 5);
                                if (firstFiveChar.ToUpper().Contains("AND"))
                                    newfilterCriteria = newfilterCriteria.Substring(5, newfilterCriteria.Length - 5);
                            }


                            newCriteriaOperator = CriteriaOperator.Parse(newfilterCriteria);
                            GridControlService.FilterCriteria = newCriteriaOperator;
                        }
                    }
                }
            }
        }

        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Exo is connected to " + loadPROJECT.OfficeNameForExo, null, null, null);
            notification1.ShowAsync();
        }

        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "ExoDesignSubJobViewModelWrapper_v2";
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                return GetEntities<DOCTYPE>();
            }
        }

        public IEnumerable<VARIATION> VARIATIONCollection => GetEntities<VARIATION>();
        #endregion
    }
}