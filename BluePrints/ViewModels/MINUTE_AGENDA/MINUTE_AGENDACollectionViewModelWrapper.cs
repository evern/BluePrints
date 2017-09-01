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

            MeetingUserCollection = new List<MeetingUser>();
            foreach (MeetingUser meetingUser in AllMeetingUsers.OrderBy(x => x.Full_Name))
            {
                if (!MeetingUserCollection.Any(x => x.Guid == meetingUser.Guid))
                    MeetingUserCollection.Add(meetingUser);
            }

            disable_immediate_post = true;
            MINUTE_TITLECollectionViewModelWrapper.OnParameterChanged(new EntitiesParameter<MEETING_TYPE>(loadMEETING.MEETING_TYPE));
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<MEETING_ACTION, MEETING_ACTION, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.MEETING_ACTIONS);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.MINUTE_TITLES, MINUTE_TITLEProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<MINUTE_TITLE>, IQueryable<MINUTE_TITLE>> MINUTE_TITLEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_MEETING_TYPE == loadMEETING.GUID_MEETING_TYPE);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.MINUTE_AGENDAS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<MINUTE_AGENDA>, IQueryable<MINUTE_AGENDAMasterDetailProjection>> specifyMainViewModelProjection()
        {
            return query => MINUTE_AGENDAMasterDetailProjectionQueries.MINUTE_AGENDA_Master_Detail_Transformation(query, loadPROJECT.GUID, minute_title_guid);
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
            childEntity.Entity.GUID_MINUTE_TITLE = masterEntity.Entity.GUID_MINUTE_TITLE;
            childEntity.Entity.GUID_ACTION_USER = masterEntity.Entity.GUID_ACTION_USER;
            childEntity.Entity.GUID_RAISE_USER = masterEntity.Entity.GUID_RAISE_USER;
            childEntity.Entity.RAISE_DATE = loadMEETING.MEETING_DATE;
            childEntity.Entity.DUE_DATE = masterEntity.Entity.DUE_DATE;

            return base.OnBeforeParentAssigned(masterEntity, childEntity);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<MINUTE_AGENDAMasterDetailProjection> entities)
        {
            MINUTE_TITLECollectionViewModelWrapper.OnDisplaySelectedEntityChangedCallBack = onMINUTE_TITLEChanged;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        private void mainViewModelRefreshed(IEnumerable<MINUTE_AGENDAMasterDetailProjection> entities)
        {
            MainViewModel.OnEntitiesLoadedCallBack = null;
            mainThreadDispatcher.BeginInvoke(new Action(() => RefreshDisplayEntities()));
        }

        private void onMINUTE_TITLEChanged(MINUTE_TITLE minute_title_entity)
        {
            MainViewModel.OnEntitiesLoadedCallBack = mainViewModelRefreshed;
            MainViewModel.Refresh();
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(MINUTE_AGENDAMasterDetailProjection entity)
        {
            if(entity.Entity.GUID == Guid.Empty && entity.Entity.GUID_MINUTE_TITLE == null)
            {
                entity.Entity.GUID_MINUTE_TITLE = MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity.GUID;
                entity.Entity.NUMBER = MINUTE_TITLECollectionViewModelWrapper.DisplaySelectedEntity.NUMBER;
            }

            entity.Entity.RAISE_DATE = loadMEETING.MEETING_DATE;
            entity.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }
        #endregion

        #endregion

        #region View Behaviour
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(MEETING_ACTION))
                this.RaisePropertyChanged(x => x.MEETING_ACTIONCollection);

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

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "MINUTE_AGENDACollectionViewModelWrapper" + view_project_specific_affix; }
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

        protected override string expand_key_field_name => BindableBase.GetPropertyName(() => new MINUTE_AGENDAMasterDetailProjection().Entity) + "." + BindableBase.GetPropertyName(() => new MINUTE_AGENDA().GUID);
        #endregion
    }
}