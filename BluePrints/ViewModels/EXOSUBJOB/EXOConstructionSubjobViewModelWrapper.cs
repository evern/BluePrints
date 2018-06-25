using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
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

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class EXOConstructionSubjobViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, ExoSubJobProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXOConstructionSubjobViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXOConstructionSubjobViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXOConstructionSubjobViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private Data.PROJECT loadPROJECT;
        private BASELINE liveBASELINE;
        private PROGRESS livePROGRESS;
        private List<STAFF> exoSTAFFS;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private readonly IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            exoSTAFFS = primeroUnitOfWork.STAFF.Where(x => x.ISACTIVE == "Y").ToList();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID && x.SUBJOB.PHASE.PHASE_TYPE == PhaseType.Design);
        }

        private Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
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
        #endregion

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<ExoSubJobProjection>> specifyMainViewModelProjection()
        {
            return query => ExoQueries.GetNativeExoSubJobProjection(primeroUnitOfWork, loadPROJECT, exoSTAFFS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ExoSubJobProjection> entities)
        {
            MainViewModel.RawPasteOverride = rawPasteOverride;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnSelectedEntitiesChanged()
        {
            refreshPermissions();
        }

        public ExoSubJobAuth SelectedUser { get; set; }
        public IEnumerable<ExoSubJobAuth> Users
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

                List<ExoSubJobAuth> orderedAuthUsers = new List<ExoSubJobAuth>();
                foreach (STAFF staff in exoSTAFFS)
                {
                    ExoSubJobAuth displayUserAuth = new ExoSubJobAuth();
                    USER newUser = USERCollection.FirstOrDefault(x => x.EXO_STAFF_ID == staff.STAFFNO);
                    if (newUser == null)
                        newUser = new USER();

                    newUser.NAME = staff.NAME;
                    newUser.EXO_STAFF_ID = staff.STAFFNO;
                    displayUserAuth.User = newUser;

                    if (DisplaySelectedEntities.All(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == newUser.EXO_STAFF_ID)))
                        displayUserAuth.IsAssigned = true;
                    else if (DisplaySelectedEntities.Any(x => x.AuthUsers.Any(y => y.User.EXO_STAFF_ID == newUser.EXO_STAFF_ID)))
                        displayUserAuth.IsAssigned = null;
                    else
                        displayUserAuth.IsAssigned = false;

                    displayUserAuth.ShouldAssign = false;
                    orderedAuthUsers.Add(displayUserAuth);
                }

                permissions.AddRange(orderedAuthUsers.OrderBy(x => x.User.Full_Name));
                return permissions;
            }
        }

        private void rawPasteOverride(IEnumerable<string> rowData)
        {
            foreach (string row in rowData)
            {
                List<string> ColumnStrings = row.Split('\t').ToList();
                if (ColumnStrings.Count < 3)
                    continue;

                ExoSubJobProjection tempSubJobProjection = ViewModelSource.Create(() => new ExoSubJobProjection());
                tempSubJobProjection.SubJob = new PrimeroSubJob();
                tempSubJobProjection.SubJob.Code = ColumnStrings[0];
                tempSubJobProjection.Discipline = new PrimeroDiscipline();
                tempSubJobProjection.Discipline.Code = ColumnStrings[1];
                tempSubJobProjection.Commodity = new PrimeroCommodity();
                tempSubJobProjection.Commodity.Code = ColumnStrings[2];

                tempSubJobProjection.AuthUsers = new System.Collections.ObjectModel.ObservableCollection<ExoSubJobAuth>();
                ExoSubJobProjection existingSameSubJobLine = DisplayEntities.FirstOrDefault(x => x.SubJob.Code == tempSubJobProjection.SubJob.Code);
                if (existingSameSubJobLine != null)
                {
                    foreach (ExoSubJobAuth authUser in existingSameSubJobLine.AuthUsers)
                    {
                        ExoSubJobAuth newUser = new ExoSubJobAuth();
                        DataUtils.ShallowCopy(newUser, authUser);
                        tempSubJobProjection.AuthUsers.Add(newUser);
                    }
                }

                MainViewModel.Entities.Add(tempSubJobProjection);
            }

            this.RaisePropertyChanged(x => x.DisplayEntities);
        }

        public bool CanCommitUnbookableToExo()
        {
            if (DisplayEntities == null)
                return false;

            return DisplayEntities.Any(x => x.SubJob.Id == null);
        }

        public void CommitUnbookableToExo()
        {
            JOBCOST_HDR masterJob = ExoQueries.GetProjectSubJob(primeroUnitOfWork, loadPROJECT.NUMBER, loadPROJECT.NUMBER);
            if (masterJob.CATEGORY == null || ((int)masterJob.CATEGORY) >= 5)
            {
                MessageBoxService.ShowMessage("This job is in tender phase and hence pushing to exo is disabled, please contact Michelle Wilson or Ryan McFarlane to enable this feature");
                return;
            }

            JOBCOST_LINES masterLine = ExoQueries.GetProjectLineByCode(primeroUnitOfWork, loadPROJECT.NUMBER);
            foreach (ExoSubJobProjection projection in DisplayEntities.Where(x => !x.IsLineExistsInExo))
            {
                int? subJobId = ExoMethods.findExistingOrAddSubJob(projection.SubJob.Code, masterJob, loadPROJECT.NUMBER);
                if (subJobId != null)
                {
                    projection.SubJob.Id = subJobId;
                }

                int? disciplineId = ExoMethods.findExistingOrAddDiscipline(projection.Discipline.Code);
                if (disciplineId != null)
                {
                    projection.Discipline.Id = disciplineId;
                    int? commodityId = ExoMethods.findExistingOrAddCommodity(projection.Commodity.Code, string.Empty, (int)disciplineId);
                    if (commodityId != null)
                    {
                        projection.Commodity.Id = commodityId;
                        projection.LineId = ExoMethods.findExistingOrAddLine(projection, masterLine, loadPROJECT.NUMBER);
                        projection.Update();
                    }
                }
            }
        }

        public void RemoveSelected()
        {
            List<ExoSubJobProjection> removeProjections = DisplaySelectedEntities.Where(x => x.IsLineExistsInExo).ToList();
            foreach(ExoSubJobProjection removeProjection in removeProjections)
                MainViewModel.Entities.Remove(removeProjection);

            this.RaisePropertyChanged(x => x.DisplayEntities);
        }

        private void refreshPermissions()
        {
            this.RaisePropertyChanged(x => x.Users);
        }

        public void PermissionCellValueChanging(CellValueChangedEventArgs e)
        {
            ExoSubJobAuth editingSubJobAuth = (ExoSubJobAuth)e.Row;
            //don't need to validate fieldname since only this field is changeable in role permission grid control

            bool newValue = (bool)e.Value;
            if (newValue)
            {
                foreach(ExoSubJobProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJob != null && x.SubJob.Id != null))
                {
                    ExoMethods.findExistingOrAddResourceAllocation(editingSubJobAuth, (int)selectedEntity.SubJob.Id);
                    editingSubJobAuth.IsAssigned = true;
                    selectedEntity.AuthUsers.Add(editingSubJobAuth);

                    foreach (ExoSubJobProjection sameSubJobEntity in DisplayEntities.Where(x => x.SubJob != null && x.SubJob.Id == selectedEntity.SubJob.Id))
                    {
                        ExoSubJobAuth findAuth = sameSubJobEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                        if (findAuth == null)
                        {
                            sameSubJobEntity.AuthUsers.Add(editingSubJobAuth);
                        }
                        else if (findAuth.IsAssigned == null || !(bool)findAuth.IsAssigned)
                            findAuth.IsAssigned = true;
                    }
                }

                e.Handled = true;
            }
            else
            {
                foreach (ExoSubJobProjection selectedEntity in DisplaySelectedEntities.Where(x => x.IsLineExistsInExo && x.SubJob != null && x.SubJob.Id != null))
                {
                    ExoSubJobAuth existingPermission = selectedEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                    if (existingPermission != null)
                    {
                        ExoMethods.deleteResourceAllocation(editingSubJobAuth, (int)selectedEntity.SubJob.Id);
                        selectedEntity.AuthUsers.Remove(existingPermission);
                        e.Handled = true;
                    }

                    foreach (ExoSubJobProjection sameSubJobEntity in DisplayEntities.Where(x => x.SubJob != null && x.SubJob.Id == selectedEntity.SubJob.Id))
                    {
                        ExoSubJobAuth findAuth = sameSubJobEntity.AuthUsers.FirstOrDefault(x => x.User.EXO_STAFF_ID == editingSubJobAuth.User.EXO_STAFF_ID);
                        if (findAuth != null)
                            sameSubJobEntity.AuthUsers.Remove(findAuth);
                    }
                }
            }

            //refreshPermissions();
            base.CellValueChanging(e);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "EXOConstructionSubjobViewModelWrapper";
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public override string UnifiedRowValidation(ExoSubJobProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(ExoSubJobProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }
    }
}