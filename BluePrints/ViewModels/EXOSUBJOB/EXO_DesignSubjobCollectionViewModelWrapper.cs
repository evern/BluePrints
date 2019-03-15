using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXO_DesignSubjobCollectionViewModelWrapper :
        EXO_SubjobCollectionViewModelWrapper
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
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        #endregion

        #region Loading Operations
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            initializeCompulsoryViewProperties();
            initializeOptionalViewCollectionsOnRefresh = false;
            SubJobRegex = loadPROJECT.NUMBER + BluePrintsResources.Regex_SUBJOB;
            DisciplineRegex = BluePrintsResources.Regex_DISCIPLINE;
            //Not linking to base because it contains background planned subjob check
            //base.resolveParameters(parameter);
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
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live);

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
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobEditableProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetExoSubJobProjection(query.Where(x => x.GUID_BASELINE == liveBASELINE.GUID), WORKPACKCollection, loadPROJECT, livePROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, primeroUnitOfWork, USERCollection, COMMODITY_CODECollection);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobEditableProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Events
        public void UploadToExo()
        {
            base.CommitToExo(DisplaySelectedEntities);
        }

        public void AutoAssignPermission()
        {
            if (MessageBoxService.ShowMessage("This will auto grant permission based on document type authorisation by role, but will not delete existing authorisations, do you wish to continue?", "Auto Assign Permission", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            int fullProgress = DisplayEntities.Count * USERCollection.Count();
            LoadingScreenManager.ShowLoadingScreen(fullProgress);

            int addedCount = 0;
            foreach (ExoSubJobEditableProjection subJob in DisplayEntities.Where(x => x.IsLineExistsInExo))
            {
                subJob.AuthUsers.Clear();
                foreach (USER user in USERCollection)
                {
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    newUser.User = user;
                    newUser.ShouldAssign = newUser.User.ROLE.ROLE_COMMODITY.Any(x => DisplaySelectedEntities.Any(y => y.CommodityCode == x.DOCTYPE.CODE));
                    if (newUser.ShouldAssign && subJob.SubJobId != null)
                    {
                        if (ExoMethods.findExistingOrAddResourceAllocation(newUser, (int)subJob.SubJobId))
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
        public override IEnumerable<ExoSubJobAuth> Users
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                var permissions = new List<ExoSubJobAuth>();
                if (DisplaySelectedEntities == null && MainViewModel.Entities.Count > 0)
                    DisplaySelectedEntities.Add(MainViewModel.Entities.First());

                if (DisplaySelectedEntities == null || DisplaySelectedEntities.Count == 0)
                    return null;

                foreach (USER user in USERCollection)
                {
                    IEnumerable<ExoSubJobAuth> findUsers = DisplaySelectedEntities.SelectMany(x => x.AuthUsers);
                    ExoSubJobAuth newUser = new ExoSubJobAuth();
                    newUser.User = user;
                    if (DisplaySelectedEntities.All(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == user.EXO_STAFF_ID)))
                        newUser.IsAssigned = true;
                    else if (DisplaySelectedEntities.Any(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == user.EXO_STAFF_ID)))
                        newUser.IsAssigned = null;
                    else
                        newUser.IsAssigned = false;

                    if (newUser.User != null && newUser.User.ROLE != null && newUser.User.ROLE.ROLE_COMMODITY.Count > 0 && newUser.User.ROLE.ROLE_COMMODITY.Any(x => DisplaySelectedEntities.Any(y => y.CommodityCode == x.DOCTYPE.CODE)))
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
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
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

                        GridControlService.SetFilterCriteria(newCriteriaOperator);
                    }
                    else
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
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
                            GridControlService.SetFilterCriteria(newCriteriaOperator);
                        }
                    }
                }
            }
        }

        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "ExoDesignSubJobViewModelWrapper";
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

        public IEnumerable<VARIATION> VARIATIONCollection => GetEntities<VARIATION>();
        #endregion
    }
}