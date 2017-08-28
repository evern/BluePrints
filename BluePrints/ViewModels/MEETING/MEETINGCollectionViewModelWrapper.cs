using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class MEETINGCollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <MEETING, MEETING, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of MEETINGCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static MEETINGCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new MEETINGCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the MEETINGCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the MEETINGCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected MEETINGCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public PROJECT loadPROJECT { get; set; }
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.CLIENTS, clientQueryProjection);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription<MEETING_USER, MEETING_USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.MEETING_USERS);
        }

        bool user_client_loaded;
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            user_client_loaded = true;
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.MEETINGS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected Func<IRepositoryQuery<CLIENT>, IQueryable<CLIENT>> clientQueryProjection()
        {
            return query => query.Where(x => x.CLIENT_PROJECT.Any(client_project => client_project.GUID_PROJECT == loadPROJECT.GUID));
        }

        protected override Func<IRepositoryQuery<MEETING>, IQueryable<MEETING>> specifyMainViewModelProjection()
        {
            return query => populateMEETINGProject(query);
        }

        private IQueryable<MEETING> populateMEETINGProject(IQueryable<MEETING> query)
        {
            List<MEETING> meetings = query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).ToList();
            //need to call ToList for tokenComboBoxEditSettings to work
            meetings.ForEach(x => x.Attendees = AllUserCollection.Where(user => MEETING_USERCollection.Any(meeting_user => meeting_user.GUID_MEETING == x.GUID && meeting_user.GUID_USER == user.Guid && meeting_user.TYPE == Common.MeetingUserSection.Attendees)).ToList());
            meetings.ForEach(x => x.Apologies = AllUserCollection.Where(user => MEETING_USERCollection.Any(meeting_user => meeting_user.GUID_MEETING == x.GUID && meeting_user.GUID_USER == user.Guid && meeting_user.TYPE == Common.MeetingUserSection.Apologies)).ToList());
            meetings.ForEach(x => x.Distribution = AllUserCollection.Where(user => MEETING_USERCollection.Any(meeting_user => meeting_user.GUID_MEETING == x.GUID && meeting_user.GUID_USER == user.Guid && meeting_user.TYPE == Common.MeetingUserSection.Distribution)).ToList());
            meetings.ForEach(x => x.Signoff = AllUserCollection.Where(user => MEETING_USERCollection.Any(meeting_user => meeting_user.GUID_MEETING == x.GUID && meeting_user.GUID_USER == user.Guid && meeting_user.TYPE == Common.MeetingUserSection.SignOff)).ToList());

            return meetings.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<MEETING> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = onBeforeEntitySavedIsContinueCallBack;
            MainViewModel.OnAfterEntitySavedCallBack = onAfterEntitySaved;
            MainViewModel.AdditionalValidateCellCallBack = validateCellCallBack;
            MainViewModel.ValidateSetValueIsContinueCallBack = validateSetValueCallBack;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private bool onBeforeEntitySavedIsContinueCallBack(MEETING entity)
        {
            if (entity.CREATED.Year == 1)
                entity.CREATED = DateTime.Now;
            entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }

        private void validateCellCallBack(GridCellValidationEventArgs e)
        {
            MEETING edit_meeting = (MEETING)e.Row;
            if(e.Column.FieldName == BindableBase.GetPropertyName(() => new MEETING().MEETING_START))
            {
                DateTime start_DateTime = (DateTime)e.Value;
                if(start_DateTime > edit_meeting.MEETING_END)
                {
                    e.ErrorContent = "Start time cannot be later than end time";
                    e.IsValid = false;
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new MEETING().MEETING_END))
            {
                DateTime end_DateTime = (DateTime)e.Value;
                if (end_DateTime < edit_meeting.MEETING_START)
                {
                    e.ErrorContent = "End time cannot be earlier than start time";
                    e.IsValid = false;
                }
            }
        }

        public bool validateSetValueCallBack(MEETING entity, string column_name, object newValue)
        {
            string fieldName = DataUtils.FormatColumnFieldname(column_name);
            //estimated hours field is disabled but just in case
            if (fieldName == BindableBase.GetPropertyName(() => new MEETING().MEETING_START))
            {
                if (((DateTime)newValue) > entity.MEETING_END)
                    return false;
            }
            else if (fieldName == BindableBase.GetPropertyName(() => new MEETING().MEETING_END))
            {
                if (((DateTime)newValue) < entity.MEETING_START)
                    return false;
            }

            return true;
        }
        #endregion

        #region IEntityNumber
        protected override string GetEntityNumberFieldName()
        {
            return BindableBase.GetPropertyName(() => new MEETING().NUMBER);
        }

        protected override int DefaultNumericFieldLength()
        {
            return Int32.Parse(BluePrintsResources.Default_Register_Numeric_Length);
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(MEETING entity, bool isNewEntity)
        {
            save_meeting_users(entity, MeetingUserSection.Attendees);
            save_meeting_users(entity, MeetingUserSection.Apologies);
            save_meeting_users(entity, MeetingUserSection.Distribution);
            save_meeting_users(entity, MeetingUserSection.SignOff);
        }

        private void save_meeting_users(MEETING entity, MeetingUserSection section)
        {
            if (entity.Attendees != null)
            {
                List<MEETING_USER> remove_meeting_users = new List<MEETING_USER>();
                IEnumerable<MeetingUser> section_users;

                if (section == MeetingUserSection.Attendees)
                    section_users = entity.Meeting_Attendees;
                else if (section == MeetingUserSection.Apologies)
                    section_users = entity.Meeting_Apologies;
                else if (section == MeetingUserSection.Distribution)
                    section_users = entity.Meeting_Distribution;
                else
                    section_users = entity.Meeting_Signoff;

                if(section_users != null)
                {
                    foreach (MEETING_USER assignment in MEETING_USERCollection.Where(x => x.GUID_MEETING == entity.GUID && x.TYPE == section))
                    {
                        if (!section_users.Any(x => x.Guid == assignment.GUID))
                            remove_meeting_users.Add(assignment);
                    }

                    MEETING_USERCollectionViewModel.BaseBulkDelete(remove_meeting_users);

                    List<MEETING_USER> add_attendees = new List<MEETING_USER>();
                    foreach (MeetingUser user in section_users)
                    {
                        if (!MEETING_USERCollection.Any(x => x.GUID == user.Guid && x.GUID_MEETING == entity.GUID && x.TYPE == Common.MeetingUserSection.Attendees))
                            add_attendees.Add(new MEETING_USER() { GUID_USER = user.Guid, GUID_MEETING = entity.GUID, TYPE = section, USER_TYPE = user.User_Type });
                    }

                    MEETING_USERCollectionViewModel.BulkSave(add_attendees);
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
            get { return "MEETINGCollectionViewModelWrapper"; }
        }

        //need to use private alluser because this can only be initialized once for shuffling data with same hash into attendance, apologies, signoff for token combobox edit to work
        List<MeetingUser> allusercollection;
        public IEnumerable<MeetingUser> AllUserCollection
        {
            get
            {
                if(allusercollection == null && user_client_loaded)
                {
                    allusercollection = new List<MeetingUser>();
                    List<MeetingUser> meeting_local_user = USERCollection.Select(x => new MeetingUser() { Guid = x.GUID, Full_Name = x.Full_Name, User_Type = Common.MeetingUserType.Internal }).ToList();
                    List<MeetingUser> meeting_client = CLIENTCollection.Select(x => new MeetingUser() { Guid = x.GUID, Full_Name = x.Full_Name, User_Type = Common.MeetingUserType.Client }).ToList();

                    allusercollection.AddRange(meeting_local_user);
                    allusercollection.AddRange(meeting_client);
                }

                return allusercollection;
            }
        }

        public IEnumerable<CLIENT> CLIENTCollection
        {
            get
            {
                var collection = GetEntities<CLIENT>();
                if (collection == null)
                    return new List<CLIENT>();

                return collection.OrderBy(x => x.Full_Name);
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection == null)
                    return new List<USER>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection.OrderBy(x => x.NAME).ToList();
            }
        }

        public IEnumerable<MEETING_USER> MEETING_USERCollection
        {
            get
            {
                var collection = GetEntities<MEETING_USER>();
                if (collection == null)
                    return new List<MEETING_USER>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection;
            }
        }

        public CollectionViewModel<MEETING_USER, MEETING_USER, Guid, IBluePrintsEntitiesUnitOfWork> MEETING_USERCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<MEETING_USER, MEETING_USER, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<MEETING_USER>();
            }
        }
        #endregion

        #region ISupportCustomDocumentTypeAndParameter

        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(),
                new EntitiesParameter<MEETING>(
                    DisplaySelectedEntity),
                    "MINUTE_AGENDACollectionView",
                    "[" + DisplaySelectedEntity.EntityNumber + "] Agenda");

            DocumentManagerService.ShowExistingEntityDocument(DocumentInfo, this);
        }
        #endregion
    }
}