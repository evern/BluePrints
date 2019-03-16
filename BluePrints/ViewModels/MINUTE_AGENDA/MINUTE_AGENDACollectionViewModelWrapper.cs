using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BaseModel.ViewModel.Base;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Common.Resources;
using DevExpress.Mvvm;
using DevExpress.Xpf.Editors;
using BluePrints.Common;
using BluePrints.Common.Reports;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Reports;
using System.IO;
using DevExpress.Xpf.Printing;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class MINUTE_AGENDACollectionViewModelWrapper :
        BluePrintsEntitiesMasterDetailCollectionsWrapper
        <MINUTE_AGENDA, MINUTE_AGENDAMasterDetailProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of MINUTE_AGENDACollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static MINUTE_AGENDACollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new MINUTE_AGENDACollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the MINUTE_AGENDACollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the MINUTE_AGENDACollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected MINUTE_AGENDACollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        private MEETING loadMEETING;
        public List<MeetingUser> MeetingUserCollection { get; set; }
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        MINUTE_TITLECollectionViewModelWrapper minute_title_collection_viewmodel_wrapper;
        public MINUTE_TITLECollectionViewModelWrapper MINUTE_TITLECollectionViewModelWrapper
        {
            get
            {
                if (minute_title_collection_viewmodel_wrapper == null)
                {
                    minute_title_collection_viewmodel_wrapper = MINUTE_TITLECollectionViewModelWrapper.Create();
                    minute_title_collection_viewmodel_wrapper.SetParentViewModel(this);
                }

                return minute_title_collection_viewmodel_wrapper;
            }
        }

        protected override void resolveParameters(object parameter)
        {
            var meetingParameter = (DualEntitiesParameter<PROJECT, MEETING>)parameter;
            loadPROJECT = meetingParameter.GetFirstEntity();
            loadMEETING = meetingParameter.GetSecondEntity();

            List<MeetingUser> AllMeetingUsers = new List<MeetingUser>();

            if (loadMEETING.Meeting_Attendees != null)
                AllMeetingUsers.AddRange(loadMEETING.Meeting_Attendees);
            if (loadMEETING.Meeting_Apologies != null)
                AllMeetingUsers.AddRange(loadMEETING.Meeting_Apologies);
            if (loadMEETING.Meeting_Distribution != null)
                AllMeetingUsers.AddRange(loadMEETING.Meeting_Distribution);
            if (loadMEETING.Meeting_Signoff != null)
                AllMeetingUsers.AddRange(loadMEETING.Meeting_Signoff);

            if(loadMEETING.Meeting_ChairUser != null)
                AllMeetingUsers.Add(loadMEETING.Meeting_ChairUser);

            MeetingUserCollection = new List<MeetingUser>();
            foreach (MeetingUser meetingUser in AllMeetingUsers.OrderBy(x => x.Full_Name))
            {
                if (!MeetingUserCollection.Any(x => x.Guid == meetingUser.Guid))
                    MeetingUserCollection.Add(meetingUser);
            }

            disable_immediate_post = true;
            MINUTE_TITLECollectionViewModelWrapper.OnParameterChange(new EntitiesParameter<MEETING_TYPE>(loadMEETING.MEETING_TYPE));
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<MEETING_ACTION, MEETING_ACTION, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.MEETING_ACTIONS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<MINUTE_TITLE>, IQueryable<MINUTE_TITLE>> MINUTE_TITLEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_MEETING_TYPE == loadMEETING.GUID_MEETING_TYPE);
        }

        private Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Meeting_Minute.ToString());
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.MINUTE_AGENDAS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<MINUTE_AGENDA>, IQueryable<MINUTE_AGENDAMasterDetailProjection>> specifyMainViewModelProjection()
        {
            return query => MINUTE_AGENDAMasterDetailProjectionQueries.MINUTE_AGENDA_Master_Detail_Transformation(query, loadPROJECT.GUID, minute_title_guid, loadMEETING.MEETING_TYPE);
        }

        private Guid minute_title_guid
        {
            get
            {
                MINUTE_TITLE selected_minute_title = MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity;
                if (selected_minute_title == null)
                    return Guid.Empty;

                return selected_minute_title.GUID;
            }
        }

        protected override bool OnBeforeParentAssigned(MINUTE_AGENDAMasterDetailProjection masterEntity, MINUTE_AGENDAMasterDetailProjection childEntity)
        {
            //initializeDetailProperties(masterEntity, childEntity);
            return base.OnBeforeParentAssigned(masterEntity, childEntity);
        }

        private void initializeDetailProperties(MINUTE_AGENDAMasterDetailProjection masterEntity, MINUTE_AGENDAMasterDetailProjection childEntity)
        {
            childEntity.Entity.NUMBER = string.Empty;
            childEntity.Entity.GUID_MINUTE_TITLE = masterEntity.Entity.GUID_MINUTE_TITLE;
            childEntity.Entity.RAISE_DATE = loadMEETING.MEETING_DATE;

            if (masterEntity.DetailEntities != null && masterEntity.DetailEntities.Count > 0)
            {
                DateTime lastEntity = masterEntity.DetailEntities.Max(x => x.Entity.EntityCreatedDate);
                MINUTE_AGENDAMasterDetailProjection latestChildEntity = masterEntity.DetailEntities.First(x => x.Entity.EntityCreatedDate == lastEntity);
                childEntity.Entity.GUID_ACTION = latestChildEntity.Entity.GUID_ACTION;
                childEntity.Entity.GUID_ACTION_USER = latestChildEntity.Entity.GUID_ACTION_USER;
                childEntity.Entity.GUID_RAISE_USER = latestChildEntity.Entity.GUID_RAISE_USER;
                childEntity.Entity.DUE_DATE = latestChildEntity.Entity.DUE_DATE;
            }
            else
            {
                childEntity.Entity.GUID_ACTION = masterEntity.Entity.GUID_ACTION;
                childEntity.Entity.GUID_ACTION_USER = masterEntity.Entity.GUID_ACTION_USER;
                childEntity.Entity.GUID_RAISE_USER = masterEntity.Entity.GUID_RAISE_USER;
                childEntity.Entity.DUE_DATE = masterEntity.Entity.DUE_DATE;
            }
        }

        private void initializeMasterProperties(MINUTE_AGENDAMasterDetailProjection entity)
        {
            if (entity.Entity.GUID == Guid.Empty && entity.Entity.GUID_MINUTE_TITLE == null)
            {
                entity.Entity.GUID_MINUTE_TITLE = MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity.GUID;
                IEnumerable<MINUTE_AGENDAMasterDetailProjection> agenda_collection = displayEntities.Where(x => x.Entity.GUID_MINUTE_TITLE == MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity.GUID);
                int count_attached_agenda = agenda_collection.Count();

                entity.Entity.NUMBER = MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity.DisplayNumber + "." + count_attached_agenda.ToString();
            }

            entity.Entity.RAISE_DATE = loadMEETING.MEETING_DATE;
            entity.Entity.GUID_PROJECT = loadPROJECT.GUID;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<MINUTE_AGENDAMasterDetailProjection> entities)
        {
            MINUTE_TITLECollectionViewModelWrapper.OnDisplaySelectedEntityChangedCallBack = onMINUTE_TITLEChanged;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            calculateTitleSummary();
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        private void calculateTitleSummary()
        {
            RefreshDisplayEntities();
            var entities = MINUTE_TITLECollectionViewModelWrapper.DisplayEntities;
            foreach(MINUTE_TITLE entity in entities)
            {
                IEnumerable<MINUTE_AGENDAMasterDetailProjection> current_agendas = DisplayEntities.Where(x => x.Entity.GUID_MINUTE_TITLE == entity.GUID);
                entity.Summary_Total_Agendas = current_agendas.Count();

                int dueAgendaCount = 0;
                foreach(MINUTE_AGENDAMasterDetailProjection agenda in current_agendas)
                {
                    foreach(MINUTE_AGENDAMasterDetailProjection comment in agenda.DetailEntities)
                    {
                        if(comment.Entity.DUE_DATE != null && ((DateTime)comment.Entity.DUE_DATE).Date <= DateTime.Now.Date)
                        {
                            dueAgendaCount += 1;
                            break;
                        }
                    }
                }

                entity.Summary_Due_Agendas = dueAgendaCount;

                int closedAgendaCount = 0;
                foreach (MINUTE_AGENDAMasterDetailProjection agenda in current_agendas)
                {
                    foreach (MINUTE_AGENDAMasterDetailProjection comment in agenda.DetailEntities)
                    {
                        if (comment.Entity.GUID_ACTION != null)
                        {
                            MEETING_ACTION meeting_action = MEETING_ACTIONCollection.FirstOrDefault(x => x.GUID == comment.Entity.GUID_ACTION);
                            if(meeting_action != null && meeting_action.IS_HIDE)
                            {
                                closedAgendaCount += 1;
                                break;
                            }
                        }
                    }
                }

                entity.Summary_Closed_Agendas = closedAgendaCount;
            }

            MINUTE_TITLECollectionViewModelWrapper.TreeGridControlService.RefreshData();
        }

        #region Collection Call Backs
        private void onMINUTE_TITLEChanged(MINUTE_TITLE minute_title_entity)
        {
            filterAgenda();
        }

        private void filterAgenda()
        {
            MINUTE_TITLE selected_minute_title = MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity;
            if (selected_minute_title == null)
                return;

            GridControlService.SetFilterCriteria(@"[Entity.GUID_MINUTE_TITLE] = '" + selected_minute_title.GUID.ToString() + "'");
            mainThreadDispatcher.BeginInvoke(new Action(() => refreshAgendaInstruction()));
        }

        private void refreshAgendaInstruction()
        {
            this.RaisePropertyChanged(x => x.IsArrowVisible);
            this.RaisePropertyChanged(x => x.NewAgendaInstruction);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(MINUTE_AGENDAMasterDetailProjection entity)
        {
            if (MINUTE_TITLECollectionViewModelWrapper == null || MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity == null)
            {
                MessageBoxService.ShowMessage("Please select a title before adding an agenda");
                return false;
            }

            initializeMasterProperties(entity);
            return true;
        }
        #endregion

        #endregion

        #region View Behaviour
        public void InitNewRow_Master(InitNewRowEventArgs e)
        {
            var gridView = (TableView)e.OriginalSource;
            var grid = gridView.Grid;
            MINUTE_AGENDAMasterDetailProjection data = (MINUTE_AGENDAMasterDetailProjection)grid.GetRow(e.RowHandle);
            initializeMasterProperties(data);
        }

        public void InitNewRow_Detail(InitNewRowEventArgs e)
        {
            var detailGridView = (TableView)e.OriginalSource;
            var detailGrid = detailGridView.Grid;
            var masterGrid = (detailGrid.OwnerDetailDescriptor as DataControlDetailDescriptor).Parent as GridControl;
            int masterRowHandle = ((TableView)e.OriginalSource).Grid.GetMasterRowHandle();
            if (masterGrid != null)
            {
                MINUTE_AGENDAMasterDetailProjection masterData = (MINUTE_AGENDAMasterDetailProjection)masterGrid.GetRow(masterRowHandle);
                MINUTE_AGENDAMasterDetailProjection detailData = (MINUTE_AGENDAMasterDetailProjection)detailGrid.GetRow(e.RowHandle);
                initializeDetailProperties(masterData, detailData);
            }
        }

        public void ExpandAll()
        {
            GridControlService.MasterDetail_ExpandAll();
        }

        public void CollapseAllButThis()
        {
            GridControlService.MasterDetail_CollapseAllButThis();
        }

        public void CollapseAll()
        {
            GridControlService.MasterDetail_CollapseAll();
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(MEETING_ACTION))
                this.RaisePropertyChanged(x => x.MEETING_ACTIONCollection);
            if (changedType == typeof(MINUTE_AGENDA))
                mainThreadDispatcher.BeginInvoke(new Action(() => calculateTitleSummary()));

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        public void ProcessNewValue(ProcessNewValueEventArgs e)
        {
            if(!e.Handled)
            {
                MEETING_ACTION meeting_action = MEETING_ACTIONCollection.FirstOrDefault(x => x.NAME == e.DisplayText);
                if (meeting_action == null)
                {
                    MEETING_ACTION new_meeting_action = new MEETING_ACTION();
                    new_meeting_action.NAME = e.DisplayText;
                    new_meeting_action.IS_HIDE = false;
                    new_meeting_action.CREATED = DateTime.Now;
                    new_meeting_action.CREATEDBY = LoginCredentials.CurrentUserGuid;
                    MEETING_ACTIONViewModel.Save(new_meeting_action);
                }
            }
        }
        #endregion

        #region Reporting
        public bool CanEditReport()
        {
            if (MINUTE_TITLECollectionViewModelWrapper == null || MINUTE_TITLECollectionViewModelWrapper.DisplayEntities == null)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (MINUTE_TITLECollectionViewModelWrapper == null || MINUTE_TITLECollectionViewModelWrapper.DisplayEntities == null)
                return false;

            return true;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Meeting_Minute);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        XtraReportMeeting_Minute meeting_minute;
        public void ViewReport()
        {
            meeting_minute = new XtraReportMeeting_Minute();
            PROJECT_REPORT dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    meeting_minute.LoadLayout(sw.BaseStream);
                }
            }

            RefreshDisplayEntities();
            meeting_minute.AssignProperties(loadMEETING, MINUTE_TITLECollectionViewModelWrapper.DisplayEntities.ToList(), MeetingUserCollection, DisplayEntities.ToList(), MEETING_ACTIONCollection);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = meeting_minute;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            meeting_minute.RequestParameters = false;
            meeting_minute.CreateDocument(true);
            previewWindow.Show();
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "MINUTE_AGENDACollectionViewModelWrapper_V2" + view_project_specific_affix; }
            get { return "MINUTE_AGENDACollectionViewModelWrapper_v2"; }
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

        public void ShowAllAgendas()
        {
            GridControlService.SetFilterCriteria(string.Empty);
            mainThreadDispatcher.BeginInvoke(new Action(() => refreshAgendaInstruction()));
        }

        public IEnumerable<MINUTE_TITLE> MINUTE_TITLECollection
        {
            get
            {
                var collection = GetEntities<MINUTE_TITLE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.Full_Name);
                return collection;
            }
        }

        public IEnumerable<MEETING_ACTION> MEETING_ACTIONCollection
        {
            get
            {
                var collection = GetEntities<MEETING_ACTION>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public CollectionViewModel<MEETING_ACTION, MEETING_ACTION, Guid, IBluePrintsEntitiesUnitOfWork> MEETING_ACTIONViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<MEETING_ACTION, MEETING_ACTION, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<MEETING_ACTION>();
            }
        }

        public string NewAgendaInstruction
        {
            get
            {
                if (MINUTE_TITLECollectionViewModelWrapper == null || MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity == null)
                    return "Please select a title before entering new agenda";

                return "Type here to add new agenda, push enter when complete";
            }
        }

        public Visibility IsArrowVisible
        {
            get
            {
                if (MINUTE_TITLECollectionViewModelWrapper == null || MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity == null)
                    return Visibility.Visible;

                return Visibility.Hidden;
            }
        }

        protected override string expand_key_field_name => BindableBase.GetPropertyName(() => new MINUTE_AGENDAMasterDetailProjection().Entity) + "." + BindableBase.GetPropertyName(() => new MINUTE_AGENDA().GUID);
        protected override bool parentEntitiesFilter(MINUTE_AGENDAMasterDetailProjection x)
        {
            return filterEntities(x);
        }

        protected override bool childEntitiesFilter(MINUTE_AGENDAMasterDetailProjection x)
        {
            return filterEntities(x);
        }

        private bool filterEntities(MINUTE_AGENDAMasterDetailProjection x)
        {
            if (x.Entity.RAISE_DATE == null)
                return true;
            else if (((DateTime)x.Entity.RAISE_DATE).Date <= loadMEETING.MEETING_DATE.Date)
                return true;
            else
                return false;
        }

        protected override object parentEntitiesOrder(MINUTE_AGENDAMasterDetailProjection x)
        {
            return x.Entity.NUMBER;
        }

        protected override object childEntitiesOrder(MINUTE_AGENDAMasterDetailProjection x)
        {
            return x.Entity.RAISE_DATE;
        }

        public override string UnifiedRowValidation(MINUTE_AGENDAMasterDetailProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(MINUTE_AGENDAMasterDetailProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }
        #endregion
    }
}