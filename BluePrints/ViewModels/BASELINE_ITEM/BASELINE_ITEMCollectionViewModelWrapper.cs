using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Reports;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Linq.Expressions;
using DevExpress.Data.Filtering;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.Common.Misc;
using System.Net.Mail;
using Microsoft.Exchange.WebServices.Data;
using BluePrints.Common.ViewModel.Misc;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE object view model.
    /// </summary>
    public partial class BASELINE_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, ISupportFiltering<BASELINE_ITEMProgress>
    {
        public string DefaultPhaseInternalNumber { get; set; }
        public virtual IEnumerable<BASELINE_ITEMProgress> EditableAllEntities => GetEditableAllEntitiesCallBack != null ? GetEditableAllEntitiesCallBack() : MainViewModel.Entities;
        public Func<IEnumerable<BASELINE_ITEMProgress>> GetEditableAllEntitiesCallBack { get; set; }
        protected DeliverablesViewType viewType { get; set; }

        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINE_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINE_ITEMCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected PROJECT loadPROJECT { get; set; }
        public BASELINE loadBASELINE { get; set; }
        protected Guid load_context_guid => loadBASELINE == null ? Guid.Empty : loadBASELINE.GUID;
        protected PROGRESS livePROGRESS { get; set; }
        protected bool isQueryForLiveStatus;
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        protected IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        //public bool Is_Autofill_Internal_Number { get; set; }
        private bool allow_drag_drop { get; set; }
        public bool Allow_Drag_Drop
        {
            get => allow_drag_drop;
            set
            {
                allow_drag_drop = value;
                this.RaisePropertyChanged(x => x.Allow_Drag_Drop);
            }
        }

        public bool CanFinaliseClientNumbers => BASELINEViewModel != null;
        public void FinaliseClientNumbers()
        {
            if (!LoginCredentials.hasPermission(PermissionResources.FinaliseNumbers))
            {
                MessageBoxService.ShowMessage("You do not have the authority to finalise client numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will finalize selected deliverable's client numbers and send to doc control to lock it.\nThis action excludes any newly added deliverable.\nare you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach(BASELINE_ITEMProgress entity in DisplaySelectedEntities)
                {
                    entity.Entity.Entity.CLIENTNUM_STATUS = DocumentNumberStatus.Awaiting;
                    entity.Update();
                }

                MainViewModel.SimpleSaveAll();
                GridControlService.RefreshData();

                //if(loadBASELINE.FIN_CLIENTNUM_BY == null)
                ActiveDirectory.SendEmail(LoginCredentials.CurrentUser.NAME, "Deliverable(s) client number in project " + loadPROJECT.NUMBER + " has been finalised, please review project deliverable(s) in document control module", "Deliverable Client Numbers Locked in " + loadPROJECT.NUMBER, true);

                loadBASELINE.FIN_CLIENTNUM_BY = LoginCredentials.CurrentUserGuid;
                BASELINECollectionViewModel.Save(loadBASELINE);
            };
        }

        public bool CanFinaliseInternalNumbers => BASELINEViewModel != null;
        public void FinaliseInternalNumbers()
        {
            if (!LoginCredentials.hasPermission(PermissionResources.FinaliseNumbers))
            {
                MessageBoxService.ShowMessage("You do not have the authority to finalise internal numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will finalize selected deliverable's internal numbers and send to doc control to lock it.\nare you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach (BASELINE_ITEMProgress entity in DisplaySelectedEntities)
                {
                    entity.Entity.Entity.INTERNALNUM_STATUS = DocumentNumberStatus.Awaiting;
                    entity.Update();
                }

                MainViewModel.SimpleSaveAll();
                GridControlService.RefreshData();

                //if (loadBASELINE.FIN_CLIENTNUM_BY == null)
                ActiveDirectory.SendEmail(LoginCredentials.CurrentUser.NAME, "Deliverable(s) internal number in project " + loadPROJECT.NUMBER + " has been finalised, please review project deliverable(s) in document control module", "Deliverable Internal Numbers Locked In " + loadPROJECT.NUMBER, true);

                loadBASELINE.FIN_INTERNALNUM_BY = LoginCredentials.CurrentUserGuid;
                BASELINECollectionViewModel.Save(loadBASELINE);
            };
        }

        public bool CanUnapproveSelectedInternalNumbers => DisplaySelectedEntities.Count() > 0;
        public void UnapproveSelectedInternalNumbers()
        {
            if (!LoginCredentials.hasPermission(PermissionResources.UnapproveInternalNumbers))
            {
                MessageBoxService.ShowMessage("You do not have the authority to unapprove internal numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will unlock selected internal numbers, if you still couldn't edit the internal number, please make sure internal number mode is set to always editable, are you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach (BASELINE_ITEMProgress entity in DisplaySelectedEntities)
                {
                    entity.Entity.Entity.INTERNALNUM_STATUS = DocumentNumberStatus.Preliminary;
                    entity.Update();
                }

                List<string> internalNumbers = DisplaySelectedEntities.Select(x => x.Entity.Entity.INTERNAL_NUM).ToList();
                MainViewModel.SimpleSaveAll();
                GridControlService.RefreshData();

                //string emailMessage = @"<html> 
                //      <body> 
                //      <p>The following internal number for deliverable(s) in project " + loadPROJECT.NUMBER + " has been unapproved</p>";

                //foreach (string internalNumber in internalNumbers)
                //{
                //    emailMessage += "<p>" + internalNumber + "</p>";
                //}
                //emailMessage += "</body></html>";
                //ActiveDirectory.SendEmail(LoginCredentials.CurrentUser.NAME, emailMessage, "Deliverable's Internal Numbers Unapproved in " + loadPROJECT.NUMBER, true);
            };
        }

        public bool CanUnapproveSelectedClientNumbers => DisplaySelectedEntities.Count() > 0;
        public void UnapproveSelectedClientNumbers()
        {
            if (!LoginCredentials.hasPermission(PermissionResources.UnapproveInternalNumbers))
            {
                MessageBoxService.ShowMessage("You do not have the authority to unapprove client numbers", "Unauthorised");
                return;
            }

            if (MessageBoxService.ShowMessage("This will unlock selected client numbers, are you sure you want to continue?", "Confirmation", MessageButton.OKCancel) == MessageResult.OK)
            {
                foreach (BASELINE_ITEMProgress entity in DisplaySelectedEntities)
                {
                    entity.Entity.Entity.CLIENTNUM_STATUS = DocumentNumberStatus.Preliminary;
                    entity.Update();
                }

                List<string> clientNumbers = DisplaySelectedEntities.Select(x => x.Entity.Entity.CLIENT_NUM).ToList();
                MainViewModel.SimpleSaveAll();
                GridControlService.RefreshData();

                //string emailMessage = @"<html> 
                //      <body> 
                //      <p>The following client number for deliverable(s) in project " + loadPROJECT.NUMBER + " has been unapproved</p>";

                //foreach (string clientNumber in clientNumbers)
                //{
                //    emailMessage += "<p>" + clientNumber + "</p>";
                //}
                //emailMessage += "</body></html>";
                //ActiveDirectory.SendEmail(LoginCredentials.CurrentUser.NAME, emailMessage, "Deliverable's Client Numbers Unapproved in " + loadPROJECT.NUMBER, true);
            };
        }

        public bool InternalNumAlwaysEditable
        {
            get => InternalNumberMode == DeliverableInternalNumberMode.AlwaysEditable;
            set
            {
                if (value)
                {
                    changeInternalNumberMode(DeliverableInternalNumberMode.AlwaysEditable);
                }
            }
        }

        public bool InternalNumDefault
        {
            get => InternalNumberMode == DeliverableInternalNumberMode.Default;
            set
            {
                if (value)
                {
                    changeInternalNumberMode(DeliverableInternalNumberMode.Default);
                }
            }
        }

        public bool InternalNumManual
        {
            get => InternalNumberMode == DeliverableInternalNumberMode.Manual;
            set
            {
                if (value)
                {
                    changeInternalNumberMode(DeliverableInternalNumberMode.Manual);
                }
            }
        }

        private void changeInternalNumberMode(DeliverableInternalNumberMode newMode)
        {
            InternalNumberMode = newMode;
            FullRefresh();

            this.RaisePropertyChanged(x => x.InternalNumAlwaysEditable);
            this.RaisePropertyChanged(x => x.InternalNumDefault);
            this.RaisePropertyChanged(x => x.InternalNumManual);
        }


        protected DeliverableInternalNumberMode InternalNumberMode { get; set; }

        protected readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var receiveParameter = (TripleEntitiesParameter<PROJECT, IAmBaseline, object>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            loadBASELINE = (BASELINE)receiveParameter.GetSecondEntity();
            viewType = (DeliverablesViewType)receiveParameter.GetThirdEntity();

            if (loadBASELINE == null)
                isQueryForLiveStatus = true;

            Allow_Drag_Drop = false;
        }

        //#region Interface Delegates
        public void Save(BASELINE_ITEMProgress progress_entity)
        {
            MainViewModel.Save(progress_entity);
        }

        public void Delete(BASELINE_ITEMProgress progress_entity)
        {
            MainViewModel.Delete(progress_entity);
        }

        public override void FullRefresh()
        {
            base.FullRefresh();
        }
        #endregion

        public virtual void Interface_InitializeParameters(object parameter)
        {

            //Is_Autofill_Internal_Number = true;
        }

        public override void OnLoaded()
        {
            if (AppNotificationService == null || GlobalVariables.IsBaselineItemNotificationShown)
            {
                base.OnLoaded();
                return;
            }

            //INotification notification = AppNotificationService.CreatePredefinedNotification("Resource has been changed to allow multiple resources, please re-assign them if you use them, sorry for any inconvenience!", null, null, null);
            //GlobalVariables.IsBaselineItemNotificationShown = true;
            //notification.ShowAsync();

            base.OnLoaded();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINES, BASELINEProjectionFunc, assign_baseline);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, assign_progress);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES, DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES, DELIVERABLES_STATUSProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DSTATUS_DOCTYPES, DSTATUS_DOCTYPEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEM_WORKS, BASELINE_ITEM_WORKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.P6_ASSIGNMENTS, P6_ASSIGNMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATIONS, VARIATIONProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription<REGISTER_HOLD_REF, REGISTER_HOLD_REF, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.REGISTER_HOLD_REF);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
        }

        private Func<IRepositoryQuery<VARIATION>, IQueryable<VARIATION>> VARIATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null && (x.PHASE_TYPE == PhaseType.Design || x.PHASE_TYPE == PhaseType.Indirect));
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>> P6_ASSIGNMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private void assign_baseline(BASELINE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live baseline not found")));
                return;
            }
            else if (entity == null)
                return;

            loadBASELINE = entity;
        }

        private void assign_progress(PROGRESS progress)
        {
            if (progress == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live progress not found")));

            livePROGRESS = progress;
        }

        protected virtual Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return query => query.Where(x => x.GUID == loadPROJECT.GUID);
            else
                return query => query.Where(x => x.GUID == loadBASELINE.GUID_PROJECT);
        }

        protected virtual Func<IRepositoryQuery<BASELINE_ITEM_WORK>, IQueryable<BASELINE_ITEM_WORK>> BASELINE_ITEM_WORKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<DOCTYPE>, IQueryable<DOCTYPE>> DOCTYPEProjectionFunc()
        {
            if (viewType == DeliverablesViewType.Both)
                return query => query;
            else if (viewType == DeliverablesViewType.Direct)
                return query => query.Where(x => !x.IS_INDIRECT_ONLY);
            else
                return query => query;
        }

        protected virtual Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> BASELINEProjectionFunc()
        {
            if (isQueryForLiveStatus)
                return
                    query =>
                        query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == BaselineStatus.Live);
            else
                return query => query.Where(x => x.GUID == loadBASELINE.GUID);
        }

        protected virtual Func<IRepositoryQuery<REGISTER_HOLD>, IQueryable<REGISTER_HOLD>> REGISTER_HOLDProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            //not ready for this yet because some active projects are still using legacy subjob name
            //if (viewType == DeliverablesViewType.Direct)
            //    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design && x.PHASE.CHARGE_TYPE == ChargeType.Direct));
            //else if (viewType == DeliverablesViewType.Indirect)
            //    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design && x.PHASE.CHARGE_TYPE == ChargeType.Indirect));
            //else
            //    return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.PHASE != null && x.PHASE.PHASE_TYPE == PhaseType.Design));
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => (x.PHASE_TYPE == PhaseType.Design || x.PHASE_TYPE == PhaseType.Indirect) && x.CHARGE_TYPE == ChargeType.Chargeable);
        }

        protected virtual Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<DELIVERABLES_STATUS>, IQueryable<DELIVERABLES_STATUS>> DELIVERABLES_STATUSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<DSTATUS_DOCTYPE>, IQueryable<DSTATUS_DOCTYPE>> DSTATUS_DOCTYPEProjectionFunc()
        {
            return query => query.Where(x => x.DELIVERABLES_STATUS.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        protected virtual Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == PhaseType.Design && x.STATUS == ProgressStatus.Live);
        }

        protected virtual Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            if (livePROGRESS == null)
                return query => query.Where(x => x.GUID_PROGRESS == Guid.Empty);
            else
                return query => query.Where(x => x.GUID_PROGRESS == livePROGRESS.GUID);
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Baseline_Report.ToString());
        }

        protected List<ExoTimeAuthorisation> exoAuthorisations = null;
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            System.Threading.Tasks.Task.Run(() => loadExoData());
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        private void loadExoData()
        {
            //when loading from document control module this will be null
            if (loadPROJECT == null)
                return;

            List<ProjectUnitOfWorkContext> projectContexts = new List<ProjectUnitOfWorkContext>();
            projectContexts.Add(new ProjectUnitOfWorkContext(loadPROJECT.NUMBER, primeroUnitOfWork));

            BluePrintsUtils.LoadExoAuthorisation<BASELINE_ITEMProgress>(DisplayEntities, ref exoAuthorisations, projectContexts);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            return query => ProgressQueries.OffsiteDirectProgressItemTransformation(baseQueryFilter(query), loadPROJECT, livePROGRESS, RATECollection, PROGRESS_ITEMCollection, VARIATIONCollection, false, P6_ASSIGNMENTCollection, InternalNumberMode, false, null, USERCollection, BASELINE_ITEM_WORKCollection, false, REGISTER_HOLD_REFCollection, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection, null, DOCTYPECollection, COMMODITY_CODECollection);
        }

        public Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BaseEntityQueryCallBack { get; set; }
        protected virtual IQueryable<BASELINE_ITEM> baseQueryFilter(IRepositoryQuery<BASELINE_ITEM> query)
        {
            if (BaseEntityQueryCallBack != null)
                return BaseEntityQueryCallBack(query);

            //if(viewType == DeliverablesViewType.Direct)
            //    return query.Where(x => x.GUID_BASELINE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Direct);
            //else if(viewType == DeliverablesViewType.Indirect)
            //    return query.Where(x => x.GUID_BASELINE == load_context_guid && x.PHASE != null && x.PHASE.CHARGE_TYPE == ChargeType.Indirect);
            //else
            return query.Where(x => x.GUID_BASELINE == load_context_guid);
        }

        public Action<IEnumerable<BASELINE_ITEMProgress>> OnReportablesLoadedCallBack { get; set; }
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            FilterTreeViewModel = FiltersSettings.GetBASELINE_ITEMProgressFilterTree(this, entities);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertyChanged(x => x.FilterTreeViewModel)));
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnAfterEntitySavedCallBack = OnEntitiesSavedCallBack;
            MainViewModel.OnBeforeEntitiesDeleteIsContinueCallBack = onBeforeEntitiesDeleted;
            MainViewModel.FuncManualCellPastingIsContinue = BluePrintsDataUtils.FuncManualCellPastingIsContinue;
            MainViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);

            //used for interface when this is loaded from variation
            if (OnReportablesLoadedCallBack != null)
            {
                OnReportablesLoadedCallBack(entities);
                return;
            }

            SetViewSpecificProperties();
        }

        //allows raise property change to propagate to parent
        public Action<object> RaisePropertyChangeCallBack { get; set; }
        protected override bool IsSingleMainEntityRefreshIdentified(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            return base.IsSingleMainEntityRefreshIdentified(key, changedType, messageType, sender, isBulkRefresh);
        }

        protected virtual bool onBeforeEntitiesDeleted(IEnumerable<BASELINE_ITEMProgress> entities)
        {
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            List<BASELINE_ITEMProgress> deleteEntities = new List<BASELINE_ITEMProgress>();
            bool showErrorMessage = false;
            foreach(BASELINE_ITEMProgress entity in entities)
            {
                IEnumerable<VARIATION> attachedVARIATIONS = VARIATIONCollection.Where(x => x.VARIATION_ITEM.Any(y => canDeleteDeliverable(y, entity)));

                //when there are variations that relates to this deliverable
                if (attachedVARIATIONS.Count() > 0)
                {
                    string variations = string.Empty;
                    foreach(VARIATION attachedVARIATION in attachedVARIATIONS)
                    {
                        variations += attachedVARIATION.NAME + ", ";
                    }

                    if(variations.Length > 2)
                        variations = variations.Substring(0, variations.Length - 2);

                    errorMessages.Add(new ErrorMessage(entity.Deliverable_Name, "Variations exists: " + variations));
                    showErrorMessage = true;
                }
                else if (entity.PROGRESS_ITEMS.Count > 0 && entity.PROGRESS_ITEMS.Sum(x => x.EARNED_UNITS) > 0)
                {
                    errorMessages.Add(new ErrorMessage(entity.Deliverable_Name, "Has been progressed"));
                    showErrorMessage = true;
                }
                else
                {
                    errorMessages.Add(new ErrorMessage(entity.Deliverable_Name, "Deleted"));
                    deleteEntities.Add(entity);
                }
            }

            if(showErrorMessage)
            {
                MainViewModel.BaseBulkDelete(deleteEntities);
                DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(errorMessages, "Cannot delete deliverable(s) due to the following error");
                ErrorMessagesDialogService.ShowDialog(MessageButton.OK, string.Empty, "ListErrorMessages", viewModel);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Because this class can be inherited by variation deliverable, allow deletion of deliverable if variation item exists only in the current variation deliverable's list
        /// </summary>
        protected virtual bool canDeleteDeliverable(VARIATION_ITEM variation_item, BASELINE_ITEMProgress deliverable)
        {
            return variation_item.GUID_ORIBASEITEM == deliverable.GUID_ORIGINAL && variation_item.ACTION != VariationAction.NoAction;
        }

        private void save_deliverable_users(BASELINE_ITEMProgress entity)
        {
            List<BASELINE_ITEM_WORK> remove_baseline_item_work = new List<BASELINE_ITEM_WORK>();

            if (entity.Entity.AssignUsers != null)
            {
                foreach (BASELINE_ITEM_WORK assignment in BASELINE_ITEM_WORKCollection.Where(x => x.GUID_BASELINE_ITEM_ORIGINAL == entity.OriginalEntityKey))
                {
                    if (!entity.Entity.AssignUsers.Any(x => x.GUID == assignment.GUID_USER))
                        remove_baseline_item_work.Add(assignment);
                }

                BASELINE_ITEM_WORKCollectionViewModel.BaseBulkDelete(remove_baseline_item_work);
                List<BASELINE_ITEM_WORK> add_project_disciplines = new List<BASELINE_ITEM_WORK>();
                foreach (USER user in entity.Entity.AssignUsers)
                {
                    if (!BASELINE_ITEM_WORKCollection.Any(x => x.GUID_USER == user.GUID && x.GUID_BASELINE_ITEM_ORIGINAL == entity.OriginalEntityKey))
                    {
                        add_project_disciplines.Add(new BASELINE_ITEM_WORK() { GUID_USER = user.GUID, GUID_BASELINE_ITEM_ORIGINAL = entity.OriginalEntityKey, GUID_PROJECT = loadBASELINE.GUID_PROJECT, WEIGHTING = 1 });
                    }

                }

                BASELINE_ITEM_WORKCollectionViewModel.BulkSave(add_project_disciplines);
            }
            else
            {
                foreach (BASELINE_ITEM_WORK assignment in BASELINE_ITEM_WORKCollection.Where(x => x.GUID_BASELINE_ITEM_ORIGINAL == entity.OriginalEntityKey))
                {
                    remove_baseline_item_work.Add(assignment);
                }

                BASELINE_ITEM_WORKCollectionViewModel.BaseBulkDelete(remove_baseline_item_work);
            }
        }

        public decimal? FreeUnits
        {
            get
            {
                if (MainViewModel == null || DisplayEntities == null)
                    return 0;

                if (loadBASELINE.BUDGETED_UNITS == null || loadBASELINE.BUDGETED_UNITS == 0)
                    return null;

                //use Entity.Budget_Units to retrieve unadjusted budget units
                return (decimal)loadBASELINE.BUDGETED_UNITS - DisplayEntities.Sum(x => x.Entity.Budget_Units);
            }
        }

        /// <summary>
        /// Show document type even when it is not valid
        /// </summary>
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE)) && e.Row != null)
            {
                BASELINE_ITEMProgress projection = (BASELINE_ITEMProgress)e.Row;
                if(projection.Entity.Entity.DOCTYPE != null)
                    e.DisplayText = projection.Entity.Entity.DOCTYPE.NAME;
            }
        }
        /// <summary>
        /// this view model can be used in variation or default collection view, only default collection view specific properties are set here
        /// </summary>
        private void SetViewSpecificProperties()
        {
            DisplaySelectedEntities = DisplaySelectedEntities;
            DefaultPhaseInternalNumber = BluePrintsResources.Default_Design_Phase;
        }

        #region Collection Call Backs
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(PROGRESS_ITEM))
            {
                FullRefreshWithoutClearingUndoRedo();
                return;
            }
            else if(changedType == typeof(BASELINE_ITEM))
            {
                this.RaisePropertyChanged(x => x.FreeUnits);
                //Need to raise property change to stimulate converter to calculate maxValue for each deliverable
                this.RaisePropertyChanged(x => x.DisplayEntities);
            }
            else if(changedType == typeof(SUBJOB))
            {
                //so that when new subjobs are automatically generated it's displaymember can  be shown in comboboxes
                this.RaisePropertyChanged(x => x.SUBJOBCollection);
            }
            else if (changedType == typeof(WORKPACK))
            {
                //so that when new workpacks are automatically generated it's displaymember can  be shown in comboboxes
                this.RaisePropertyChanged(x => x.WORKPACKCollection);
            }
            else if(changedType == typeof(DOCTYPE) && (messageType != EntityMessageType.Changed))
            {
                foreach(var entity in DisplayEntities)
                {
                    entity.Entity.Entity.ResetValidDocTypes();
                }
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public virtual bool OnBeforeEntitySaved(BASELINE_ITEMProgress entity)
        {
            //if (MainViewModel.isBackgroundEdit)
            //    return true;

            PhaseType? phaseType = null;
            ChargeType? chargeType = null;

            PHASE defaultPHASE = PHASECollection.FirstOrDefault(x => (x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design) && (x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Chargeable));
            if (viewType == DeliverablesViewType.Direct)
            {
                phaseType = PhaseType.Design;
                chargeType = ChargeType.Chargeable;
                if (defaultPHASE != null)
                    entity.Phase_Guid = defaultPHASE.GUID;
            }
            else if(viewType == DeliverablesViewType.Indirect)
            {
                phaseType = PhaseType.Design;
                chargeType = ChargeType.NotChargeable;
                PHASE indirectPHASE = PHASECollection.FirstOrDefault(x => (x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design) && (x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.NotChargeable));
                if (indirectPHASE != null)
                    entity.Phase_Guid = indirectPHASE.GUID;
            }
            else if (entity.Phase_Guid == null && defaultPHASE != null)
            {
                entity.Phase_Guid = defaultPHASE.GUID;
            }

            string errorMessage = string.Empty;
            if (entity.GUID == Guid.Empty && entity.Entity.Entity.INTERNAL_NUM == string.Empty && entity.IsInternalNumberEditable)
                entity.Entity.Entity.INTERNAL_NUM = generateInternalNumber(entity, out errorMessage);

            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, entity, bluePrintsUnitOfWork, phaseType, chargeType, false, allowSubJobDeletion);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(entity, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection, allowWorkpackDeletion);
            entity.Update();
            //entity.Entity.Entity.GUID_ESTIMATE = loadESTIMATE.GUID;
            return true;
        }

        public Action<BASELINE_ITEMProgress> ApplyViewSpecificPropertiesToEntityCallBack { get; set; }
        protected override void OnBeforeApplyProjectionPropertiesToEntity(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity)
        {
            //if (projectionEntity.Entity.Entity.GUID_PHASE == null)
            //{
            //    IEnumerable<PHASE> phase_collection = loaderCollection.GetCollection<PHASE>();
            //    if (phase_collection != null)
            //    {
            //        PHASE default_design_phase = phase_collection.FirstOrDefault(x => x.INTERNAL_NUM == DefaultPhaseInternalNumber);
            //        if (default_design_phase != null)
            //            projectionEntity.Entity.Entity.GUID_PHASE = default_design_phase.GUID;
            //    }
            //}

            if (ApplyViewSpecificPropertiesToEntityCallBack == null)
                projectionEntity.Entity.Entity.GUID_BASELINE = loadBASELINE.GUID;
            else
                ApplyViewSpecificPropertiesToEntityCallBack.Invoke(projectionEntity);

            //because TProjection is not IProjection<TMainEntity>, do it manually here
            DataUtils.ShallowCopy(entity, projectionEntity.Entity.Entity);
            base.OnBeforeApplyProjectionPropertiesToEntity(projectionEntity, entity);
        }

        public Action<BASELINE_ITEMProgress> OnAfterDuplicateCallBack { get; set; }
        public virtual void OnEntitiesSavedCallBack(BASELINE_ITEMProgress projectionEntity, BASELINE_ITEM entity, bool isNewEntity)
        {
            #region Send Email
            if (!InVariationMode)
            {
                //if (isNewEntity && DisplayEntities.Any(x => x.Entity.Entity.INTERNALNUM_STATUS == DocumentNumberStatus.Approved))
                //{
                //    ActiveDirectory.SendEmail(LoginCredentials.CurrentUser.NAME, "Deliverable with internal number " + entity.INTERNAL_NUM + " has been added to project " + loadPROJECT.NUMBER + ", please review", "Deliverable Added in " + loadPROJECT.NUMBER, true);
                //}
            }

            #endregion
            projectionEntity.Entity.Entity.GUID_ORIGINAL = entity.GUID_ORIGINAL;
            if (isNewEntity)
                OnAfterDuplicateCallBack?.Invoke(projectionEntity);
            //save_deliverable_users(projectionEntity);
        }
        #endregion

        #region View Localization Helpers
        private void setNestedValueWithUndo(BASELINE_ITEMProgress entity, string propertyName, object newValue, Dictionary<Guid, string> internalNumberUndoInfos = null)
        {
            object oldValue = null;
            if (internalNumberUndoInfos != null)
            {
                var keyValuePair = internalNumberUndoInfos.FirstOrDefault(x => x.Key == entity.GUID);
                oldValue = keyValuePair.Value;
            }
            else
            {
                oldValue = DataUtils.GetNestedValue(propertyName, entity);
            }

            DataUtils.SetNestedValue(propertyName, entity, newValue);
            AddUndo(entity, propertyName, oldValue, newValue, EntityMessageType.Changed);
            entity.Update();
            RaisePropertyChangeCallBack?.Invoke(entity.GUID);
        }
        #endregion

        #region View Behavior
        protected DevExpress.Mvvm.IDialogService MapDeliverableDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("MapDeliverableDialog"); }
        }

        public bool CanMapClientNumber()
        {
            return MainViewModel != null && MainViewModel.Entities.Count() > 0;
        }

        public void MapClientNumber()
        {
            List<ClientNumberAssignment> internal_number_mapping = new List<ClientNumberAssignment>();

            foreach (BASELINE_ITEMProgress entity in MainViewModel.Entities)
            {
                internal_number_mapping.Add(ViewModelSource.Create(() => new ClientNumberAssignment() { INTERNAL_NUM = entity.Entity.Entity.INTERNAL_NUM, CLIENT_NUM = entity.Entity.Entity.CLIENT_NUM }));
            }

            MapDeliverablesClientNumberDialogViewModel<ClientNumberAssignment> internal_number_remap_view_model = MapDeliverablesClientNumberDialogViewModel<ClientNumberAssignment>.CreateViewModel(internal_number_mapping);
            if (MapDeliverableDialogService.ShowDialog(MessageButton.OKCancel, "Re-Assign Client Number", "MapDeliverablesClientNumber", internal_number_remap_view_model) == MessageResult.OK)
            {
                IEnumerable<ClientNumberAssignment> reassignments = internal_number_mapping.Where(x => x.CLIENT_NUM != null && x.CLIENT_NUM != string.Empty);

                List<BASELINE_ITEMProgress> reassigned_deliverables = new List<BASELINE_ITEMProgress>();
                foreach (ClientNumberAssignment reassignment in reassignments)
                {
                    BASELINE_ITEMProgress user_remapped_deliverable = MainViewModel.Entities.FirstOrDefault(x => x.Entity.Entity.INTERNAL_NUM == reassignment.INTERNAL_NUM);
                    if (user_remapped_deliverable != null)
                    {
                        user_remapped_deliverable.Entity.Entity.CLIENT_NUM = reassignment.CLIENT_NUM;
                        reassigned_deliverables.Add(user_remapped_deliverable);
                    }
                }

                if (reassigned_deliverables.Count > 0)
                {
                    MainViewModel.BulkSave(reassigned_deliverables);
                    MessageBoxService.ShowMessage(reassigned_deliverables.Count + " internal number re-assigned");
                }
            }
        }

        public void ValidateCellValue(GridCellValidationEventArgs e)
        {
            string fieldName = DataUtils.FormatColumnFieldname(e.Column.FieldName);
            string error_message = UnifiedValueValidation((BASELINE_ITEMProgress)e.Row, fieldName, e.Value, false);
            if (error_message != string.Empty)
            {
                e.IsValid = false;
                e.ErrorContent = error_message;
                e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
            }
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProgress entity, string column_name, object newValue, bool isPaste)
        {
            string fieldName = DataUtils.FormatColumnFieldname(column_name);
            //budget hours field is disabled but just in case
            if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BUDGET_HOURS))
            {
                if (entity.Entity.Entity.BY_DURATION && ((decimal)newValue) > 0)
                    return "Cannot set budgeted hours when deliverables is by duration";
                else if ((decimal)newValue < entity.MinEstimateUnits)
                    return "Budgeted hours cannot be less than " + entity.MinEstimateUnits.ToString();
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION))
            {
                if (entity.Earned_Units_Total > 0)
                    return "Cannot set budgeted hours when deliverables is by duration";
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM))
            {
                if (!entity.IsInternalNumberEditable)
                    return "Cannot change internal number because deliverables has already been progressed";
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_PHASE))
            {
                if(newValue != null)
                {
                    if (!isDocTypePhaseValid(entity.Entity.Entity.GUID_DOCTYPE, (Guid)newValue))
                    {
                        return "Selected document type is reserved for indirect only";
                    }
                }
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE))
            {
                if (newValue != null)
                {
                    if (!isDocTypePhaseValid((Guid)newValue, entity.Entity.Entity.GUID_PHASE))
                    {
                        return "Selected document type is reserved for indirect only";
                    }
                }
            }

            return string.Empty;
        }

        public override string UnifiedRowValidation(BASELINE_ITEMProgress projection)
        {
            if(!isDocTypePhaseValid(projection.Entity.Entity.GUID_DOCTYPE, projection.Entity.Entity.GUID_PHASE))
                return "Selected document type is reserved for indirect only, please change phase to indirect or change to a direct document type";

            return string.Empty;
        }

        private bool isDocTypePhaseValid(Guid? doctypeGuid, Guid? phaseGuid)
        {
            if (doctypeGuid != null && phaseGuid != null)
            {
                PHASE phase = PHASECollection.FirstOrDefault(x => x.GUID == phaseGuid);
                if (phase != null)
                {
                    DOCTYPE doctype = DOCTYPECollection.FirstOrDefault(x => x.GUID == doctypeGuid);
                    if (doctype != null)
                    {
                        if (doctype.IS_INDIRECT_ONLY && phase.PHASE_TYPE != PhaseType.Indirect)
                            return false;
                    }
                }
            }

            return true;
        }

        public void AddUndo(BASELINE_ITEMProgress changedEntity, string propertyName, object oldValue, object newValue, EntityMessageType messageType)
        {
            MainViewModel.EntitiesUndoRedoManager.AddUndo(changedEntity, propertyName, oldValue, newValue, messageType);
        }

        public void PauseUndoRedo()
        {
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();
        }

        public void UnpauseUndoRedo()
        {
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();
        }

        //public void OnCustomColumnSort(CustomColumnSortEventArgs e)
        //{
        //    if (e.Column.FieldName == Base_Entity_String + BindableBase.GetPropertyName(() => new BASELINE_ITEM().BUDGET_HOURS))
        //    {
        //        decimal decimal_value1 = (decimal)e.Value1;
        //        decimal decimal_value2 = (decimal)e.Value2;

        //        e.Result = decimal_value1.CompareTo(decimal_value2);
        //        e.Handled = true;
        //    }
        //}

        //anything with AddUndo needs to be added to unified value changed to prevent it from getting added twice
        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            field_name = DataUtils.FormatColumnFieldname(field_name);
            if (isNew)
            {
                projection.Entity.Entity.OFFICE = loadPROJECT.OFFICE;
                projection.Entity.Entity.PopulateDocumentTypes(DOCTYPECollection, COMMODITY_CODECollection);
            }

            //only new row will change department according to doc type selection
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE)))
            {
                var chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)new_value);
                if (isNew)
                {
                    if (chosenDOCTYPE != null)
                    {
                        if (chosenDOCTYPE.GUID_DDEPARTMENT != null)
                            projection.Entity.Entity.GUID_DEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;

                        //Baseline and Department is required immediately for deliverables status selection
                        projection.Entity.Entity.BASELINE = loadBASELINE;
                        projection.Entity.Entity.DOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                        projection.Update();
                    }
                }

                if (chosenDOCTYPE.IS_INDIRECT_ONLY)
                {
                    PHASE indirectPhase = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE == ChargeType.NotChargeable);
                    if (indirectPhase != null)
                        projection.Entity.Entity.GUID_PHASE = indirectPhase.GUID;
                }
            }

            //only new row will change area and subarea according to subjob selection
            if (isNew && field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBJOB)))
            {
                var chosenSUBJOB = SUBJOBCollection.FirstOrDefault(entity => entity.GUID == (Guid)new_value);
                if (chosenSUBJOB != null)
                {
                    projection.Entity.Entity.GUID_AREA = chosenSUBJOB.GUID_DAREA;
                    //Area is required immediately for subarea selection
                    projection.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == chosenSUBJOB.GUID_DAREA);
                    projection.Entity.Entity.GUID_SUBAREA = chosenSUBJOB.GUID_DSUBAREA;
                    projection.Entity.Entity.GUID_PHASE = chosenSUBJOB.PHASE != null ? chosenSUBJOB.GUID_DPHASE : null;
                    projection.Update();
                }
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_SUBJOB)) && new_value == null)
            {
                allowSubJobDeletion = true;
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_WORKPACK)) && new_value == null)
            {
                allowWorkpackDeletion = true;
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedNewRowInitialization(BASELINE_ITEMProgress projection)
        {
            projection.Entity.Entity.NewItemRowSubAREACollection = SUBAREACollection;
            base.UnifiedNewRowInitialization(projection);
        }

        //anything with AddUndo needs to be added to unified value changed to prevent it from getting added twice
        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, BASELINE_ITEMProgress projection, bool isNew)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().BY_DURATION)))
            {
                if ((bool)new_value)
                {
                    decimal oldValue = projection.Entity.Entity.BUDGET_HOURS;
                    if (oldValue > 0)
                    {
                        decimal newValue = 0;
                        string budgetHoursFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().BUDGET_HOURS));
                        projection.Entity.Entity.BUDGET_HOURS = newValue;
                        PauseUndoRedo();
                        AddUndo(projection, budgetHoursFieldName, oldValue, newValue, EntityMessageType.Changed);
                    }
                }
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DISCIPLINE)))
            {
                if (isNew)
                {
                    //Area is required immediately for subarea selection
                    projection.Entity.Entity.DISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    projection.Update();
                }
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA)))
            {
                Guid? oldValue = projection.Entity.Entity.GUID_SUBAREA;
                Guid? newValue = (Guid?)null;

                projection.Entity.Entity.GUID_SUBAREA = newValue;
                if (!isNew)
                {
                    string subAreaFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().SubAreaGuid));
                    PauseUndoRedo();
                    AddUndo(projection, subAreaFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
                else
                {
                    //Area is required immediately for subarea selection
                    projection.Entity.Entity.AREA = AREACollection.FirstOrDefault(x => x.GUID == (Guid)new_value);
                    projection.Update();
                }
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DISCIPLINE)) || field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().DELIVERABLE_TYPE)))
            {
                projection.Entity.Entity.ResetValidDocTypes();
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_DOCTYPE)) || field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEM().DELIVERABLE_TYPE)))
            {
                Guid? oldValue = projection.Entity.Entity.GUID_STATUS;
                Guid? newValue = (Guid?)null;
                projection.Entity.Entity.GUID_STATUS = newValue;

                if (!isNew)
                {
                    string statusFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_STATUS));
                    PauseUndoRedo();
                    AddUndo(projection, statusFieldName, oldValue, newValue, EntityMessageType.Changed);
                }
                else
                    projection.Update();
            }

            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_AREA)) ||
                field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DOCTYPE)) ||
                field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DISCIPLINE)))
            {
                bool skipIrrelevantInternalNumberGeneration = false;
                if (projection.IsInternalNumberEditable && !projection.IsInternalNumberManualOnly)
                {
                    if (projection.Entity.Entity.GUID_DOCTYPE != null)
                    {
                        DOCTYPE findDocType = DOCTYPECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DOCTYPE);
                        if (findDocType != null)
                        {
                            if (!findDocType.IS_AREA_SIGNIFICANT)
                                skipIrrelevantInternalNumberGeneration = true;
                        }
                    }

                    //commit the latest value for internal number generation
                    DataUtils.SetNestedValue(field_name, projection, new_value);
                    string oldValue = projection.Entity.Entity.INTERNAL_NUM;
                    string errorMessage = string.Empty;
                    string newValue = skipIrrelevantInternalNumberGeneration ? oldValue : generateInternalNumber(projection, out errorMessage);
                    projection.Entity.Entity.INTERNAL_NUM = newValue;
                    string internalNumberFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.INTERNAL_NUM));
                    //when it's new this entity will be added as EntityMessageType.Added later
                    if(!isNew)
                    {
                        PauseUndoRedo();
                        AddUndo(projection, internalNumberFieldName, oldValue, newValue, EntityMessageType.Changed);
                    }

                    projection.Update();
                }
            }

            //when deliverable type or doc type is changed remove deliverable status
            if (field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.DELIVERABLE_TYPE)) ||
                field_name.Contains(BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.GUID_DOCTYPE)))
            {
                if (projection.IsInternalNumberEditable && !projection.IsInternalNumberManualOnly)
                {
                    Guid? oldValue = projection.Entity.Entity.GUID_STATUS;
                    Guid? newValue = null;
                    projection.Entity.Entity.GUID_STATUS = newValue;

                    PauseUndoRedo();
                    AddUndo(projection, field_name, oldValue, newValue, EntityMessageType.Changed);
                    projection.Update();
                }
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        bool allowSubJobDeletion = false;
        bool allowWorkpackDeletion = false;
        #endregion

        #region View Commands
        public bool IsBASELINELocked
        {
            get
            {
                if (loadBASELINE == null)
                    return true;
                else
                    return loadBASELINE.BUDGETED_UNITS != null && loadBASELINE.BUDGETED_UNITS > 0;
            }
            set
            {
                LockUnlockBASELINE(value);
                this.RaisePropertyChanged(x => x.FreeUnits);
            }
        }

        private void LockUnlockBASELINE(bool isLock)
        {
            var BASELINECollectionViewModel = (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            if (!isLock)
                loadBASELINE.BUDGETED_UNITS = 0;
            else
            {
                decimal totalEstimatedHours = MainViewModel.Entities.Sum(x => x.Entity.Entity.BUDGET_HOURS);
                loadBASELINE.BUDGETED_UNITS = totalEstimatedHours;
            }

            BASELINECollectionViewModel.Save(loadBASELINE);
            this.RaisePropertiesChanged();
        }

        public bool CanDuplicate()
        {
            if (MainViewModel == null || DisplaySelectedEntities == null || DisplaySelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public bool CanInsert()
        {
            return CanDuplicate();
        }
        
        public void Insert()
        {
            TableViewService.SetImmediateUpdateRowPosition(true);
            if (!_isProcessingMultiple)
                PauseUndoRedo();

            List<BASELINE_ITEMProgress> newEntities = getNewProgressEntities(1, true, DisplayEntities, DisplaySelectedEntities);
            newEntities = concatenateNewEntitiesWithExistingRenameEntities(newEntities, EditableAllEntities);

            foreach(BASELINE_ITEMProgress newEntity in newEntities)
                MainViewModel.Save(newEntity);

            //Add undo must happen after save so that variation can pick it up
            foreach (BASELINE_ITEMProgress newEntity in newEntities)
                AddUndo(newEntity, null, null, null, EntityMessageType.Added);

            if (!_isProcessingMultiple)
                UnpauseUndoRedo();
            TableViewService.SetImmediateUpdateRowPosition(false);
        }

        /// <summary>
        /// Concatenate entities to be saved and entities to be renamed.
        /// </summary>
        /// <param name="newEntities">Entities to be saved.</param>
        /// <returns></returns>
        private List<BASELINE_ITEMProgress> concatenateNewEntitiesWithExistingRenameEntities(List<BASELINE_ITEMProgress> newEntities, IEnumerable<BASELINE_ITEMProgress> existingEntities)
        {
            List<BASELINE_ITEMProgress> concatenatedEntities = new List<BASELINE_ITEMProgress>();
            concatenatedEntities.AddRange(newEntities);

            List<string> processedValueToFillStringOnly = new List<string>();
            foreach(BASELINE_ITEMProgress entity in newEntities.OrderBy(x => x.Entity.Entity.INTERNAL_NUM))
            {
                long lowestUnsavedNumericValue = 0;
                long highestUnsavedNumericValue = 0;

                int numericFieldLength = 0;
                long arbitraryNumericValue = 0;
                string valueToFill = entity.Entity.Entity.INTERNAL_NUM;
                if (valueToFill == string.Empty)
                    return concatenatedEntities;

                string valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(valueToFill, out numericFieldLength, out arbitraryNumericValue);

                List<BASELINE_ITEMProgress> relatedNewEntities = newEntities.Where(x => x.Entity.Entity.INTERNAL_NUM.Contains(valueToFillStringOnly)).ToList();
                BASELINE_ITEMProgress smallestNumberEntity = relatedNewEntities.First();
                BASELINE_ITEMProgress largestNumberEntity = relatedNewEntities.Last();

                string smallestInternalNum = smallestNumberEntity.Entity.Entity.INTERNAL_NUM;
                string largestInternalNum = largestNumberEntity.Entity.Entity.INTERNAL_NUM;

                valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(smallestInternalNum, out numericFieldLength, out lowestUnsavedNumericValue);
                valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(largestInternalNum, out numericFieldLength, out highestUnsavedNumericValue);
                if(!processedValueToFillStringOnly.Contains(valueToFillStringOnly))
                {
                    processedValueToFillStringOnly.Add(valueToFillStringOnly);
                    List<BASELINE_ITEMProgress> renameEntities = getRenameExistingEntities(valueToFillStringOnly, lowestUnsavedNumericValue, highestUnsavedNumericValue, existingEntities, formatFieldNameForProjectionProperty);
                    concatenatedEntities.AddRange(renameEntities);
                }
            }

            return concatenatedEntities;
        }

        public void Duplicate()
        {
            TableViewService.SetImmediateUpdateRowPosition(true);
            if (!_isProcessingMultiple)
                PauseUndoRedo();

            List<BASELINE_ITEMProgress> newEntities = getNewProgressEntities(1, false, DisplayEntities, DisplaySelectedEntities);

            //because bulk save will invoke refresh on this collectionviewmodel. Variation will not know about the refresh
            foreach(BASELINE_ITEMProgress newEntity in newEntities)
                MainViewModel.Save(newEntity);

            //Add undo must happen after save so that variation can pick it up
            foreach (BASELINE_ITEMProgress newEntity in newEntities)
                AddUndo(newEntity, null, null, null, EntityMessageType.Added);

            if (!_isProcessingMultiple)
                UnpauseUndoRedo();

            TableViewService.SetImmediateUpdateRowPosition(false);
        }

        /// <summary>
        /// Identify entities which internal number require to be named.
        /// </summary>
        /// <param name="renameStringOnly">Rename internal number string component only.</param>
        /// <param name="startNumber">Start of internal number to be named</param>
        /// <param name="endNumber">End if internal number to be named</param>
        /// <returns></returns>
        private List<BASELINE_ITEMProgress> getRenameExistingEntities(string renameStringOnly, long startNumber, long endNumber, IEnumerable<BASELINE_ITEMProgress> existingEditableEntities, Func<string, string>formatFieldNameFunc)
        {
            long valueToAdd = (endNumber - startNumber) + 1;
            List<BASELINE_ITEMProgress> renameEntities = new List<BASELINE_ITEMProgress>();
            foreach (BASELINE_ITEMProgress entity in existingEditableEntities)
            {
                string stringValueToFill = entity.Entity.Entity.INTERNAL_NUM;
                if (stringValueToFill == null)
                    continue;

                if (!stringValueToFill.Contains(renameStringOnly))
                    continue;

                int numericFieldLength = 0;
                long valueToFillNumberOnly = 0;
                string valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(stringValueToFill, out numericFieldLength, out valueToFillNumberOnly);

                if (valueToFillNumberOnly >= startNumber)
                {
                    long increasedNumber = valueToFillNumberOnly + valueToAdd;
                    string oldInternalNum = entity.Entity.Entity.INTERNAL_NUM;
                    string internal_number_fieldname = formatFieldNameFunc(BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM));
                    entity.Entity.Entity.INTERNAL_NUM = StringFormatUtils.AppendStringWithEnumerator(valueToFillStringOnly, increasedNumber, numericFieldLength);

                    //only add undo if it's an existing entities
                    if(entity.Entity.Entity.INTERNAL_NUM != oldInternalNum)
                        AddUndo(entity, internal_number_fieldname, oldInternalNum, entity.Entity.Entity.INTERNAL_NUM, EntityMessageType.Changed);

                    renameEntities.Add(entity);
                }
            }

            return renameEntities;
        }

        private List<BASELINE_ITEMProgress> getNewProgressEntities(int timesToDuplicate, bool isInsert, IEnumerable<BASELINE_ITEMProgress> all_entities, IEnumerable<BASELINE_ITEMProgress> selected_entities)
        {
            List<BASELINE_ITEMProgress> unsavedEntities = new List<BASELINE_ITEMProgress>();
            for(int i = 0; i < timesToDuplicate; i++)
            {
                foreach (var selectedEntity in selected_entities)
                {
                    var newProjection = new BASELINE_ITEMProgress();
                    DataUtils.ShallowCopy(newProjection.Entity.Entity, selectedEntity.Entity.Entity);
                    newProjection.Entity.GUID = Guid.Empty;
                    newProjection.Entity.Entity.GUID_ORIGINAL = Guid.Empty;
                    newProjection.DuplicateFromGuid = selectedEntity.GUID;

                    //because this function is used in variation, let ApplyProjection handle this
                    newProjection.Entity.Entity.GUID_BASELINE = null;
                    newProjection.Entity.Entity.GUID_VARIATION = null;

                    //when duplicated by variation this should be 0
                    if(BaseEntityQueryCallBack != null)
                        newProjection.Entity.Entity.BUDGET_HOURS = 0;
                    else
                        newProjection.Entity.Entity.BUDGET_HOURS = IsBASELINELocked ? 0 : selectedEntity.Entity.Entity.BUDGET_HOURS;
                    
                    newProjection.Entity.Entity.DC_HOURS = 0;
                    var selectedAREA = AREACollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_AREA);
                    var selectedDISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DISCIPLINE);
                    var selectedDOCTYPE =
                        DOCTYPECollection.FirstOrDefault(x => x.GUID == newProjection.Entity.Entity.GUID_DOCTYPE);

                    newProjection.Entity.Entity.INTERNAL_NUM = 
                        BluePrintsDataUtils.GetNewInternalNumber(all_entities.Select(x => x.Entity), unsavedEntities.Select(x => x.Entity), selectedEntity.Entity.Entity.INTERNAL_NUM, selected_entities.Select(x => x.Entity), isInsert);

                    newProjection.Entity.Entity.GUID_STATUS = null;
                    newProjection.Entity.Entity.INTERNALNUM_STATUS = DocumentNumberStatus.Preliminary;
                    newProjection.Entity.Entity.CLIENTNUM_STATUS = DocumentNumberStatus.Preliminary;
                    newProjection.Entity.Entity.GUID_USER = null;
                    onBeforeEntitiesDuplicated(selectedEntity, newProjection);
                    //newProjection.Entity.Entity.INTERNAL_NUM = string.Empty;
                    AddUndo(newProjection, null, null, null, EntityMessageType.Added);
                    unsavedEntities.Add(newProjection);
                }
            }

            return unsavedEntities;
        }

        protected virtual void onBeforeEntitiesDuplicated(BASELINE_ITEMProgress copyEntity, BASELINE_ITEMProgress newEntity)
        {

        }

        public bool CanDuplicateMultiple(BarEditItem barEdit)
        {
            if (DisplaySelectedEntities == null || DisplaySelectedEntities.Count() == 0)
                return false;

            return true;
        }

        public bool CanInsertMultiple(BarEditItem barEdit)
        {
            return CanDuplicateMultiple(barEdit);
        }

        private bool _isProcessingMultiple;

        public void DuplicateMultiple(BarEditItem barEdit)
        {
            TableViewService.SetImmediateUpdateRowPosition(true);
            PauseUndoRedo();
            _isProcessingMultiple = true;
            var timesToDuplicate = 0;
            
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToDuplicate))
            {

                TableViewService.SetImmediateUpdateRowPosition(true);
                if (!_isProcessingMultiple)
                    PauseUndoRedo();

                List<BASELINE_ITEMProgress> newEntities = getNewProgressEntities(timesToDuplicate, false, DisplayEntities, DisplaySelectedEntities);

                //because bulk save will invoke refresh on this collectionviewmodel. Variation will not know about the refresh
                foreach (BASELINE_ITEMProgress newEntity in newEntities)
                    MainViewModel.Save(newEntity);

                //Add undo must happen after save so that variation can pick it up
                foreach (BASELINE_ITEMProgress newEntity in newEntities)
                    AddUndo(newEntity, null, null, null, EntityMessageType.Added);

                if (!_isProcessingMultiple)
                    UnpauseUndoRedo();

                //List<BASELINE_ITEMProgress> currentEnumerationSaveEntities = getNewProgressEntities(timesToDuplicate, false, MainViewModel.Entities, DisplaySelectedEntities);
                //newEntities.AddRange(currentEnumerationSaveEntities);
            }

            //MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            UnpauseUndoRedo();
            TableViewService.SetImmediateUpdateRowPosition(false);
        }

        public void InsertMultiple(BarEditItem barEdit)
        {
            TableViewService.SetImmediateUpdateRowPosition(true);
            PauseUndoRedo();
            _isProcessingMultiple = true;
            var timesToInsert = 0;
            List<BASELINE_ITEMProgress> newEntities = new List<BASELINE_ITEMProgress>();
            if (int.TryParse(barEdit.EditValue.ToString(), out timesToInsert))
            {
                for (int i = 0; i < timesToInsert; i++)
                {
                    Insert();
                }
                //List<BASELINE_ITEMProgress> currentEnumerationSaveEntities = getNewProgressEntities(timesToInsert, true, MainViewModel.Entities, DisplaySelectedEntities);
                //newEntities.AddRange(currentEnumerationSaveEntities);
            }

            //newEntities = concatenateNewEntitiesWithExistingRenameEntities(newEntities, MainViewModel.Entities);
       
            //MainViewModel.BulkSave(newEntities);
            _isProcessingMultiple = false;
            UnpauseUndoRedo();
            TableViewService.SetImmediateUpdateRowPosition(false);
        }
        
        public bool CanAutoPopulate(object button)
        {
            if (DisplaySelectedEntities == null || DisplaySelectedEntities.Count() == 0)
                    return false;

            return true;
        }

        public void AutoPopulate(object button)
        {
            MainViewModel.isBackgroundEdit = true;
            PauseUndoRedo();
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject) button) as GridMenuInfo;
            if (info.Column == null)
                return;

            var areaFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_AREA));
            var subAreaFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().SubAreaGuid));
            var subjobFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_SUBJOB));
            var workpackFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().GUID_WORKPACK));
            var internalNumberFieldName = formatFieldNameForProjectionProperty(BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM));

            var entitiesToSave = new List<BASELINE_ITEMProgress>();
            string fieldName = formatFieldNameForProjectionProperty(info.Column.FieldName);

            Dictionary<Guid, string> internalNumberUndoInfos = new Dictionary<Guid, string>();
            if (fieldName == internalNumberFieldName)
                foreach (var entity in DisplaySelectedEntities)
                {
                    if(entity.IsInternalNumberEditable)
                    {
                        internalNumberUndoInfos.Add(entity.GUID, entity.Entity.Entity.INTERNAL_NUM);
                        entity.Entity.Entity.INTERNAL_NUM = string.Empty;
                    }
                }

            foreach (var entity in DisplaySelectedEntities)
            {
                var entitySUBJOB = SUBJOBCollection.FirstOrDefault(x => x.GUID == entity.Entity.Entity.GUID_SUBJOB);
                if (fieldName == internalNumberFieldName && entity.IsInternalNumberEditable)
                {
                    string errorMessage = string.Empty;
                    string internalNumber = generateInternalNumber(entity, out errorMessage);
                    if(errorMessage != string.Empty)
                    {
                        MessageBoxService.ShowMessage(errorMessage);
                    }
                    else
                    {
                        setNestedValueWithUndo(entity, fieldName, internalNumber, internalNumberUndoInfos);
                        entitiesToSave.Add(entity);
                    }
                }
                else if (fieldName == areaFieldName || fieldName == subAreaFieldName)
                {
                    if (entitySUBJOB == null)
                        continue;

                    if (fieldName == areaFieldName)
                        setNestedValueWithUndo(entity, fieldName, entitySUBJOB.GUID_DAREA);
                    else if(fieldName == subAreaFieldName)
                        setNestedValueWithUndo(entity, fieldName, entitySUBJOB.GUID_DSUBAREA);

                    entitiesToSave.Add(entity);
                }
                else if (fieldName == subjobFieldName)
                {
                    if (entity.Entity.Entity.GUID_AREA == null || entity.Entity.Entity.GUID_DISCIPLINE == null)
                        continue;

                    Guid? phase_guid;
                    string internalName = BluePrintsDataUtils.SUBJOB_Generate_InternalNumber(
                        entity.Entity.Entity.GUID_AREA, entity.Entity.Entity.GUID_SUBAREA, 
                        loadPROJECT, AREACollection, SUBAREACollection, out phase_guid, entity.Entity.Entity.GUID_PHASE, PHASECollection);

                    if (internalName == string.Empty)
                        return;

                    //check database instead of SUBJOBCollection to avoid concurrency issues
                    var findSUBJOB = bluePrintsUnitOfWork.SUBJOBS.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).FirstOrDefault(x => x.INTERNAL_NAME1 == internalName);
                    if (findSUBJOB == null)
                    {
                        var newSUBJOB = new SUBJOB();

                        List<AREA> sub_area_collection = SUBAREACollection.ToList();
                        AREA defaultSubArea = sub_area_collection.Count() == 0 ? null : sub_area_collection.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.Default_Sub_Area);

                        newSUBJOB.GUID_PROJECT = loadPROJECT.GUID;
                        newSUBJOB.GUID_DAREA = entity.Entity.Entity.GUID_AREA;
                        newSUBJOB.GUID_DSUBAREA = entity.Entity.Entity.GUID_SUBAREA == null ? defaultSubArea != null ? defaultSubArea.GUID : (Guid?)null : entity.Entity.Entity.GUID_SUBAREA;
                        newSUBJOB.GUID_DPHASE = entity.Entity.Entity.GUID_PHASE;

                        newSUBJOB.INTERNAL_NAME1 = internalName; 
                        newSUBJOB.STARTDATE = DateTime.Now;
                        newSUBJOB.ENDDATE =
                            BluePrintsDataUtils.SUBJOB_Calculate_EndDate((DateTime) newSUBJOB.STARTDATE, loadPROJECT);
                        var reviewStartDate = (DateTime) newSUBJOB.STARTDATE;
                        var reviewEndDate = (DateTime) newSUBJOB.ENDDATE;
                        BluePrintsDataUtils.SUBJOB_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate,
                            loadPROJECT, false);
                        newSUBJOB.REVIEWSTARTDATE = reviewStartDate;
                        newSUBJOB.REVIEWENDDATE = reviewEndDate;
                        newSUBJOB.AUTOGENERATED = true;

                        bluePrintsUnitOfWork.SUBJOBS.Add(newSUBJOB);
                        bluePrintsUnitOfWork.SaveChanges();
                        Messenger.Default.Send(new EntityMessage<SUBJOB, Guid>(newSUBJOB.GUID, Guid.NewGuid(), EntityMessageType.Added));

                        setNestedValueWithUndo(entity, fieldName, newSUBJOB.GUID);
                    }
                    else
                    {
                        setNestedValueWithUndo(entity, fieldName, findSUBJOB.GUID);
                    }

                    entitiesToSave.Add(entity);
                }
                else if(fieldName == workpackFieldName)
                {
                    if (entity.Entity.Entity.GUID_SUBJOB == null|| entity.Entity.Entity.GUID_DISCIPLINE == null)
                        continue;

                    WORKPACK queryWORKPACK = WORKPACKCollection.FirstOrDefault(x => x.GUID_SUBJOB == entity.Entity.Entity.GUID_SUBJOB && x.GUID_DISCIPLINE == entity.Entity.Entity.GUID_DISCIPLINE && x.DISCIPLINE_NUM == entity.Entity.Entity.DISCIPLINE_NUM);
                    if(queryWORKPACK != null)
                    {
                        setNestedValueWithUndo(entity, fieldName, queryWORKPACK.GUID);
                        entitiesToSave.Add(entity);
                    }
                    else
                    {
                        WORKPACK newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_SUBJOB = (Guid)entity.Entity.Entity.GUID_SUBJOB;
                        newWORKPACK.GUID_DISCIPLINE = (Guid)entity.Entity.Entity.GUID_DISCIPLINE;
                        newWORKPACK.DISCIPLINE_NUM = entity.Entity.Entity.DISCIPLINE_NUM;
                        BluePrintsDataUtils.WORKPACK_Populate_Name(newWORKPACK, SUBJOBCollection, DISCIPLINECollection);
                        ((CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)
                        loaderCollection.GetViewModel<WORKPACK>()).Save(newWORKPACK);

                        setNestedValueWithUndo(entity, fieldName, newWORKPACK.GUID);
                        entitiesToSave.Add(entity);
                    }
                }
            }

            MainViewModel.BulkSave(entitiesToSave);
            MainViewModel.isBackgroundEdit = false;
            UnpauseUndoRedo();
            BackgroundRefresh();
        }

        private string formatFieldNameForProjectionProperty(string fieldName)
        {
            string cleanFieldName = DataUtils.FormatColumnFieldname(fieldName);
            return "Entity.Entity." + cleanFieldName;
        }

        protected override void onAfterRefresh()
        {
            allowSubJobDeletion = false;
            allowWorkpackDeletion = false;
            base.onAfterRefresh();
        }

        private string generateInternalNumber(BASELINE_ITEMProgress projectionEntity, out string errorMessage)
        {
            AREA currentItemAREA = AREACollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_AREA));
            DISCIPLINE currentItemDISCIPLINE = DISCIPLINECollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_DISCIPLINE));
            DOCTYPE currentItemDOCTYPE = DOCTYPECollection.FirstOrDefault((x => x.GUID == projectionEntity.Entity.Entity.GUID_DOCTYPE));

            errorMessage = string.Empty;
            if (currentItemAREA == null)
                errorMessage += "Area, ";

            if (currentItemDISCIPLINE == null)
                errorMessage += "Discipline, ";

            if (currentItemDOCTYPE == null)
                errorMessage += "Document Type, ";

            if(errorMessage.Length > 2)
                errorMessage = errorMessage.Substring(0, errorMessage.Length - 2) + " is missing";

            var internalNum = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, MainViewModel.Entities.Select(x => x.Entity.Entity), currentItemAREA, currentItemDISCIPLINE, currentItemDOCTYPE, projectionEntity.GUID);

            return internalNum;
        }
        #endregion

        #region Find and Replace
        private DevExpress.Mvvm.IDialogService BookTimeDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("BookTimeDialog"); }
        }

        public bool CanFindReplace(object button)
        {
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            if (info == null)
                return false;

            if (info.Column == null)
                return false;

            if (info.Column.ReadOnly)
                return false;

            if (DisplaySelectedEntity == null || DisplaySelectedEntities.Count() < 2 || info.Column.ReadOnly == true)
                return false;

            var columnPropertyInfo = DataUtils.GetNestedPropertyInfo(info.Column.FieldName, DisplaySelectedEntity);
            if (columnPropertyInfo.PropertyType == typeof(string))
            {
                var constraintString = DataUtils.GetConstraintPropertyStrings(DisplaySelectedEntity.GetType());
                if (constraintString == null)
                    constraintString = DataUtils.GetConstraintPropertyStrings(DisplaySelectedEntity.GetType().BaseType);

                var bulkEditDisabledString =
                    DataUtils.GetBulkEditDisabledPropertyStrings(DisplaySelectedEntity.GetType());
                if (bulkEditDisabledString == null)
                    bulkEditDisabledString =
                        DataUtils.GetBulkEditDisabledPropertyStrings(DisplaySelectedEntity.GetType().BaseType);

                if (constraintString != null && constraintString.Any(x => x == columnPropertyInfo.Name) ||
                    bulkEditDisabledString != null && bulkEditDisabledString.Any(x => x == columnPropertyInfo.Name))
                    return false;
                else
                    return true;
            }

            return false;
        }

        public void FindReplace(object button)
        {
            var info = GridPopupMenuBase.GetGridMenuInfo((DependencyObject)button) as GridMenuInfo;
            BASELINE_ITEMProgress first_selected_entity = DisplaySelectedEntities.First();
            string fieldName = info.Column.FieldName;
            object find_nested_value = DataUtils.GetNestedValue(fieldName, first_selected_entity);

            string find_value;
            if (find_nested_value == null)
                find_value = string.Empty;
            else
                find_value = find_nested_value.ToString();

            if (find_value == string.Empty)
            {
                MessageBoxService.ShowMessage("Cannot find anything to replace");
                return;
            }

            var bulkFindAndReplaceViewModel = BulkFindAndReplaceViewModel.Create(find_value);

            List<BASELINE_ITEMProgress> save_entities = new List<BASELINE_ITEMProgress>();
            if (BulkColumnEditDialogService.ShowDialog(MessageButton.OKCancel, "Type in text to replace", "BulkFindAndReplace", bulkFindAndReplaceViewModel) == MessageResult.OK)
            {
                PauseUndoRedo();
                string new_find_value = bulkFindAndReplaceViewModel.FindValue;
                string replace_value = bulkFindAndReplaceViewModel.ReplaceValue;
                foreach (BASELINE_ITEMProgress selected_entity in DisplaySelectedEntities)
                {
                    object nested_value = DataUtils.GetNestedValue(fieldName, selected_entity);
                    string old_column_value;
                    if (nested_value == null)
                        old_column_value = string.Empty;
                    else
                        old_column_value = nested_value.ToString();

                    string new_column_value = old_column_value.Replace(new_find_value, replace_value);
                    setNestedValueWithUndo(selected_entity, fieldName, new_column_value);
                    save_entities.Add(selected_entity);
                }

                UnpauseUndoRedo();
                MainViewModel.BulkSave(save_entities);
            }

            //FullRefresh();
        }
        #endregion

        #region DragDrop
        public void TableView_Drop(GridDropEventArgs e)
        {
            e.Handled = true;
        }

        public void TableView_Dropped(GridDroppedEventArgs e)
        {
            IEnumerable<BASELINE_ITEMProgress> dropped_deliverables = ((IEnumerable<object>)e.DraggedRows).Select(x => (BASELINE_ITEMProgress)x).AsEnumerable();
            BASELINE_ITEMProgress target_deliverable = (BASELINE_ITEMProgress)e.TargetRow;

            if(dropped_deliverables.Count() > 0 && target_deliverable != null)
            {
                BASELINE_ITEMProgress first_dropped_deliverable = dropped_deliverables.First();
                string old_value = first_dropped_deliverable.Entity.Entity.INTERNAL_NUM;
                string new_value = target_deliverable.Deliverable_Name;
                string internal_number_fieldname = BindableBase.GetPropertyName(() => new BASELINE_ITEM().INTERNAL_NUM);

                PauseUndoRedo();
                first_dropped_deliverable.Entity.Entity.INTERNAL_NUM = new_value;
                AddUndo(first_dropped_deliverable, internal_number_fieldname, old_value, new_value, EntityMessageType.Changed);

                target_deliverable.Entity.Entity.INTERNAL_NUM = old_value;
                AddUndo(target_deliverable, internal_number_fieldname, new_value, old_value, EntityMessageType.Changed);

                MainViewModel.Save(first_dropped_deliverable);
                MainViewModel.Save(target_deliverable);

                UnpauseUndoRedo();
            }
        }
        #endregion

        #region View Properties
        public override void ShowNotification()
        {
            if (AppNotificationService == null)
                return;

            if (loadPROJECT == null)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Exo is connected to " + loadPROJECT.OfficeNameForExo + " for time booking", null, null, null);
            notification1.ShowAsync();
        }

        public bool CanShowBookable()
        {
            if (MainViewModel == null || DisplaySelectedEntities == null || DisplaySelectedEntities.Count() == 0 || exoAuthorisations == null)
                return false;

            return true;
        }

        bool showBookable;
        public bool ShowBookable
        {
            get
            {
                return showBookable;
            }
            set
            {
                showBookable = value;
                BluePrintsUtils.ApplyShowBookableFilter(GridControlService, value);
            }
        }

        public decimal TotalAllowedUnits
        {
            get
            {
                return (loadBASELINE == null || loadBASELINE.BUDGETED_UNITS == null) ? 1000000000 : (decimal)loadBASELINE.BUDGETED_UNITS;
            }
        }

        public bool IsTender => loadPROJECT == null ? false : loadPROJECT.IsTender;

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get
            {
                //return "BASELINE_ITEMSViewModelWrapper" + view_project_specific_affix;
                return "BASELINE_ITEMSViewModelWrapper_v4";
            }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
        }

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
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

        public IEnumerable<VARIATION> VARIATIONCollection => GetEntities<VARIATION>();

        public IEnumerable<REGISTER_HOLD_REF> REGISTER_HOLD_REFCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_HOLD_REF>();
                return collection;
            }
        }

        public IEnumerable<REGISTER_HOLD> REGISTER_HOLDCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_HOLD>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
                return collection;
            }
        }

        public IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM_WORK>();
                return collection;
            }
        }

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                return GetEntities<P6_ASSIGNMENT>();
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

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                return GetSUBAREACollection();
            }
        }

        public IEnumerable<AREA> GetSUBAREACollection()
        {
            var collection = GetEntities<AREA>();
            if (collection != null)
                collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
            return collection;
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection
        {
            get
            {
                var collection = GetEntities<DELIVERABLES_STATUS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.MAX_PERCENTAGE);
                return collection;
            }
        }

        public IEnumerable<DSTATUS_DOCTYPE> DSTATUS_DOCTYPECollection
        {
            get
            {
                return GetEntities<DSTATUS_DOCTYPE>();
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

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<OFFICE> OFFICECollection
        {
            get
            {
                var collection = GetEntities<OFFICE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.NAME);
                }

                return collection;
            }
        }

        public CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE>();
            }
        }


        public CollectionViewModel<BASELINE_ITEM_WORK, BASELINE_ITEM_WORK, Guid, IBluePrintsEntitiesUnitOfWork> BASELINE_ITEM_WORKCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<BASELINE_ITEM_WORK, BASELINE_ITEM_WORK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<BASELINE_ITEM_WORK>();
            }
        }

        public CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork> SUBJOBSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<SUBJOB>();
            }
        }

        public CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<WORKPACK>();
            }
        }
        #endregion

        #region Reporting

        public bool CanEditReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Baseline_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public Func<IEnumerable<BASELINE_ITEMProgress>> GetGridVisibleRows;

        public void ViewReport()
        {
            var baselineReport = new XtraReportBASELINE_ITEMS();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    baselineReport.LoadLayout(sw.BaseStream);
                }
            }

            //make sure disciplines are all populated
            PopulateNavigationalProperties();
            IEnumerable<object> gridVisibleRows = GridControlService.GetVisibleRowObjects();
            baselineReport.AssignProperties(loadPROJECT, loadBASELINE, gridVisibleRows.Select(x => ((BASELINE_ITEMProgress)x)));
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = baselineReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            baselineReport.RequestParameters = false;
            baselineReport.CreateDocument(true);
            previewWindow.Show();
        }

        public void ViewRoleCostReport()
        {
            var groupByDepartmentDisciplineDeliverables = (from entity in DisplayEntities
                                                           where entity.Entity.Entity.GUID_DEPARTMENT != null && entity.Entity.Entity.GUID_DISCIPLINE != null
                                                           group entity by new { entity.Entity.Entity.GUID_PHASE, entity.Entity.Entity.GUID_DEPARTMENT, entity.Entity.Entity.GUID_DISCIPLINE, entity.Entity.Entity.GUID_DOCTYPE }
                                                           into entitiesGroup
                                                           select new
                                                           {
                                                               entitiesGroup.Key.GUID_PHASE,
                                                               entitiesGroup.Key.GUID_DEPARTMENT,
                                                               entitiesGroup.Key.GUID_DISCIPLINE,
                                                               entitiesGroup.Key.GUID_DOCTYPE,
                                                               Hours = entitiesGroup.Sum(x => x.Total_Units)
                                                           }).ToList();

            //because final report demands by department and discipline when in reality it's splitted by phase and doc type also
            List<DeliverableRoleCost> preliminaryRoleCosts = new List<DeliverableRoleCost>();
            List<ErrorMessage> invalidDeliverables = new List<ErrorMessage>();
            foreach (var deliverable in groupByDepartmentDisciplineDeliverables)
            {
                DEPARTMENT findDEPARTMENT = DEPARTMENTCollection.FirstOrDefault(x => x.GUID == deliverable.GUID_DEPARTMENT);
                DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == deliverable.GUID_DISCIPLINE);
                if(findDEPARTMENT != null && findDISCIPLINE != null)
                {
                    string errorName = "Department: " + findDEPARTMENT.NAME + ", Discipline: " + findDISCIPLINE.NAME;
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == deliverable.GUID_DOCTYPE);
                    RATE findRATE = BluePrintsDataUtils.CascadeRateSearch(deliverable.GUID_PHASE, deliverable.GUID_DISCIPLINE, deliverable.GUID_DEPARTMENT, findDOCTYPE == null ? string.Empty : findDOCTYPE.CODE, RATECollection, CostType.Charge);
                    if(findRATE != null && findRATE.RATE1 != null)
                    {
                        if(findRATE.IsUsingGangRate)
                        {
                            if (findRATE.ManagerPercent > 0)
                            {
                                DeliverableRoleCost managerRoleCost = new DeliverableRoleCost();
                                managerRoleCost.Department = findDEPARTMENT.NAME;
                                managerRoleCost.Discipline = findDISCIPLINE.NAME;
                                managerRoleCost.Role = RateRole.Manager.ToString();
                                managerRoleCost.Hours = deliverable.Hours * findRATE.ManagerPercent;
                                managerRoleCost.Rate = findRATE.ManagerRate;
                                managerRoleCost.TotalCosts = managerRoleCost.Hours * managerRoleCost.Rate;
                                preliminaryRoleCosts.Add(managerRoleCost);
                            }
                            if (findRATE.PrincipalPercent > 0)
                            {
                                DeliverableRoleCost principalRoleCost = new DeliverableRoleCost();
                                principalRoleCost.Department = findDEPARTMENT.NAME;
                                principalRoleCost.Discipline = findDISCIPLINE.NAME;
                                principalRoleCost.Role = RateRole.Principal.ToString();
                                principalRoleCost.Hours = deliverable.Hours * findRATE.PrincipalPercent;
                                principalRoleCost.Rate = findRATE.PrincipalRate;
                                principalRoleCost.TotalCosts = principalRoleCost.Hours * principalRoleCost.Rate;
                                preliminaryRoleCosts.Add(principalRoleCost);
                            }
                            if (findRATE.LeadPercent > 0)
                            {
                                DeliverableRoleCost leadRoleCost = new DeliverableRoleCost();
                                leadRoleCost.Department = findDEPARTMENT.NAME;
                                leadRoleCost.Discipline = findDISCIPLINE.NAME;
                                leadRoleCost.Role = RateRole.Lead.ToString();
                                leadRoleCost.Hours = deliverable.Hours * findRATE.LeadPercent;
                                leadRoleCost.Rate = findRATE.LeadRate;
                                leadRoleCost.TotalCosts = leadRoleCost.Hours * leadRoleCost.Rate;
                                preliminaryRoleCosts.Add(leadRoleCost);
                            }
                            if (findRATE.SeniorPercent > 0)
                            {
                                DeliverableRoleCost seniorRoleCost = new DeliverableRoleCost();
                                seniorRoleCost.Department = findDEPARTMENT.NAME;
                                seniorRoleCost.Discipline = findDISCIPLINE.NAME;
                                seniorRoleCost.Role = RateRole.Senior.ToString();
                                seniorRoleCost.Hours = deliverable.Hours * findRATE.SeniorPercent;
                                seniorRoleCost.Rate = findRATE.SeniorRate;
                                seniorRoleCost.TotalCosts = seniorRoleCost.Hours * seniorRoleCost.Rate;
                                preliminaryRoleCosts.Add(seniorRoleCost);
                            }
                            if (findRATE.EngineerPercent > 0)
                            {
                                DeliverableRoleCost engineerRoleCost = new DeliverableRoleCost();
                                engineerRoleCost.Department = findDEPARTMENT.NAME;
                                engineerRoleCost.Discipline = findDISCIPLINE.NAME;
                                engineerRoleCost.Role = RateRole.Engineer.ToString();
                                engineerRoleCost.Hours = deliverable.Hours * findRATE.EngineerPercent;
                                engineerRoleCost.Rate = findRATE.EngineerRate;
                                engineerRoleCost.TotalCosts = engineerRoleCost.Hours * engineerRoleCost.Rate;
                                preliminaryRoleCosts.Add(engineerRoleCost);
                            }
                            if (findRATE.GraduatePercent > 0)
                            {
                                DeliverableRoleCost graduateRoleCost = new DeliverableRoleCost();
                                graduateRoleCost.Department = findDEPARTMENT.NAME;
                                graduateRoleCost.Discipline = findDISCIPLINE.NAME;
                                graduateRoleCost.Role = RateRole.Graduate.ToString();
                                graduateRoleCost.Hours = deliverable.Hours * findRATE.GraduatePercent;
                                graduateRoleCost.Rate = findRATE.GraduateRate;
                                graduateRoleCost.TotalCosts = graduateRoleCost.Hours * graduateRoleCost.Rate;
                                preliminaryRoleCosts.Add(graduateRoleCost);
                            }
                            if (findRATE.UndergraduatePercent > 0)
                            {
                                DeliverableRoleCost undergraduateRoleCost = new DeliverableRoleCost();
                                undergraduateRoleCost.Department = findDEPARTMENT.NAME;
                                undergraduateRoleCost.Discipline = findDISCIPLINE.NAME;
                                undergraduateRoleCost.Role = RateRole.Undergraduate.ToString();
                                undergraduateRoleCost.Hours = deliverable.Hours * findRATE.UndergraduatePercent;
                                undergraduateRoleCost.Rate = findRATE.UndergraduateRate;
                                undergraduateRoleCost.TotalCosts = undergraduateRoleCost.Hours * undergraduateRoleCost.Rate;
                                preliminaryRoleCosts.Add(undergraduateRoleCost);
                            }
                        }
                        else
                        {
                            if (findRATE.COMMODITY_CODE != string.Empty && findDOCTYPE != null)
                                errorName += ", Commodity: " + findDOCTYPE.NAME;

                            invalidDeliverables.Add(new ErrorMessage(errorName, "Not using gang rate"));
                        }
                    }
                    else
                    {
                        invalidDeliverables.Add(new ErrorMessage(errorName, "Rate not found"));
                    }
                }
            }

            if (invalidDeliverables.Count > 0)
            {
                DialogCollectionViewModel<ErrorMessage> viewModel = DialogCollectionViewModel<ErrorMessage>.Create(invalidDeliverables, "Report is not accurate due to the following error, do you wish to continue?");
                if (ErrorMessagesDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListErrorMessages", viewModel) == MessageResult.Cancel)
                    return;
            }

            List<DeliverableRoleCost> finalRoleCosts = (from roleCost in preliminaryRoleCosts
                                                        group roleCost by new { roleCost.Department, roleCost.Discipline, roleCost.Role }
                                                        into roleCostGroup
                                                        select new DeliverableRoleCost() { Department = roleCostGroup.Key.Department, Discipline = roleCostGroup.Key.Discipline, Role = roleCostGroup.Key.Role, Hours = roleCostGroup.Sum(x => x.Hours), Rate = roleCostGroup.Average(x => x.Rate), TotalCosts = roleCostGroup.Sum(x => x.TotalCosts)}).ToList();

            var roleCostReport = new XtraReportDeliverableRoleCost();

            //make sure disciplines are all populated
            PopulateNavigationalProperties();
            IEnumerable<object> gridVisibleRows = GridControlService.GetVisibleRowObjects();
            roleCostReport.AssignProperties(finalRoleCosts, loadPROJECT.NUMBER + " Cost Report");
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = roleCostReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            roleCostReport.RequestParameters = false;
            roleCostReport.CreateDocument(true);
            previewWindow.Show();
        }

        private void PopulateNavigationalProperties()
        {
            foreach (var projection in MainViewModel.Entities)
            {
                if (projection.Entity.Entity.GUID_DISCIPLINE != null && projection.Entity.Entity.DISCIPLINE == null)
                    projection.Entity.Entity.DISCIPLINE =
                        DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DISCIPLINE);

                if (projection.Entity.Entity.GUID_AREA != null && projection.Entity.Entity.AREA == null)
                    projection.Entity.Entity.AREA =
                        AREACollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_AREA);
            }
        }

        public void BookTime()
        {
            if (exoAuthorisations == null)
                MessageBoxService.ShowMessage("Exo data is still loading, please wait awhile before using this function");
            else
            {
                BluePrintsUtils.BookTime(DisplaySelectedEntity, primeroUnitOfWork, exoAuthorisations, DisplaySelectedEntity.Deliverable_Name, MessageBoxService, BookTimeDialogService);
            }
        }

        protected override string ExportFilename()
        {
            return loadPROJECT.NUMBER + "_Baseline_Rev_" + loadBASELINE.REVISION;
        }

        #endregion

        #region For Variation Usage
        public CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> BASELINEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<BASELINE>();
            }
        }

        public bool InVariationMode { get; set; }
        #endregion
    }

    public class DeliverableRoleCost
    {
        public string Department { get; set; }
        public string Discipline { get; set; }
        public string Role { get; set; }
        public decimal Hours { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalCosts { get; set; }
    }
}