using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.Reports;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

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

            disable_immediate_post = true;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.CLIENTS, clientQueryProjection);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription<MEETING_USER, MEETING_USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.MEETING_USERS);
            loaderCollection.AddLoaderDescription<MEETING_TYPE, MEETING_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.MEETING_TYPES);
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
            meetings.ForEach(x => x.Meeting_ChairUser = AllUserCollection.FirstOrDefault(user => user.Guid == x.CHAIRED_BY));

            return meetings.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<MEETING> entities)
        {
            MainViewModel.OnAfterEntitySavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool onBeforeEntitySavedIsContinue(MEETING projection)
        {
            if (projection.CREATED.Year == 1)
                projection.CREATED = DateTime.Now;
            projection.GUID_PROJECT = loadPROJECT.GUID;

            return base.onBeforeEntitySavedIsContinue(projection);
        }

        public override string UnifiedRowValidation(MEETING projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(MEETING projection, string field_name, object new_value)
        {
            if (field_name == BindableBase.GetPropertyName(() => new MEETING().MEETING_START))
            {
                DateTime start_DateTime = (DateTime)new_value;
                if (start_DateTime > projection.MEETING_END)
                {
                    return "Start time cannot be later than end time";
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new MEETING().MEETING_END))
            {
                DateTime end_DateTime = (DateTime)new_value;
                if (end_DateTime < projection.MEETING_START)
                {
                    return "End time cannot be earlier than start time";
                }
            }

            return string.Empty;
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
        private void onAfterEntitySaved(MEETING entity, MEETING projection, bool isNewEntity)
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
                        if (!section_users.Any(x => x.Guid == assignment.GUID_USER))
                            remove_meeting_users.Add(assignment);
                    }

                    MEETING_USERCollectionViewModel.BaseBulkDelete(remove_meeting_users);

                    List<MEETING_USER> add_attendees = new List<MEETING_USER>();
                    foreach (MeetingUser user in section_users)
                    {
                        if (!MEETING_USERCollection.Any(x => x.GUID_USER == user.Guid && x.GUID_MEETING == entity.GUID && x.TYPE == section))
                            add_attendees.Add(new MEETING_USER() { GUID_USER = user.Guid, GUID_MEETING = entity.GUID, TYPE = section, USER_TYPE = user.User_Type });
                    }

                    MEETING_USERCollectionViewModel.BulkSave(add_attendees);
                }
                else
                {
                    foreach (MEETING_USER assignment in MEETING_USERCollection.Where(x => x.GUID_MEETING == entity.GUID && x.TYPE == section))
                    {
                        remove_meeting_users.Add(assignment);
                    }

                    MEETING_USERCollectionViewModel.BaseBulkDelete(remove_meeting_users);
                }
            }
        }
        #endregion

        #region Meeting Type 
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            if (changedType == typeof(MEETING_TYPE))
                this.RaisePropertyChanged(x => x.MEETING_TYPECollection);

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, isBulkRefresh);
        }

        public void ProcessNewValue(ProcessNewValueEventArgs e)
        {
            if (!e.Handled)
            {
                MEETING_TYPE meeting_type = MEETING_TYPECollection.FirstOrDefault(x => x.NAME == e.DisplayText);
                if (meeting_type == null)
                {
                    MEETING_TYPE new_meeting_type = new MEETING_TYPE();
                    new_meeting_type.NAME = e.DisplayText;
                    new_meeting_type.GUID_PROJECT = loadPROJECT.GUID;
                    new_meeting_type.CREATED = DateTime.Now;
                    new_meeting_type.CREATEDBY = LoginCredentials.CurrentUserGuid;
                    MEETING_TYPEViewModel.Save(new_meeting_type);
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
                    List<MeetingUser> unsorted_meeting_users = new List<MeetingUser>();
                    List<MeetingUser> meeting_local_user = USERCollection.Select(x => new MeetingUser() { Guid = x.GUID, Full_Name = x.Full_Name, Company_Name = "Primero", User_Type = Common.MeetingUserType.Internal }).ToList();
                    List<MeetingUser> meeting_client = CLIENTCollection.Select(x => new MeetingUser() { Guid = x.GUID, Full_Name = x.Full_Name, Company_Name = loadPROJECT.CLIENT, Email = x.EMAIL, Phone_Number = x.PHONE, User_Type = Common.MeetingUserType.Client }).ToList();

                    unsorted_meeting_users.AddRange(meeting_local_user);
                    unsorted_meeting_users.AddRange(meeting_client);
                    foreach (MeetingUser unsorted_meeting_user in unsorted_meeting_users.OrderBy(x => x.Full_Name))
                    {
                        allusercollection.Add(unsorted_meeting_user);
                    }
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

        public IEnumerable<MEETING_TYPE> MEETING_TYPECollection
        {
            get
            {
                var collection = GetEntities<MEETING_TYPE>();
                if (collection == null)
                    return new List<MEETING_TYPE>();

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

        public CollectionViewModel<MEETING_TYPE, MEETING_TYPE, Guid, IBluePrintsEntitiesUnitOfWork> MEETING_TYPEViewModel
        {
            get
            {
                if (loaderCollection == null)
                    return null;

                return
                    (CollectionViewModel<MEETING_TYPE, MEETING_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<MEETING_TYPE>();
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

            if (DisplaySelectedEntity.MEETING_TYPE == null && DisplaySelectedEntity.GUID_MEETING_TYPE != null)
                DisplaySelectedEntity.MEETING_TYPE = MEETING_TYPECollection.FirstOrDefault(x => x.GUID == DisplaySelectedEntity.GUID_MEETING_TYPE);


            if(DisplaySelectedEntity.MEETING_TYPE == null)
            {
                MessageBoxService.ShowMessage("Please assign a meeting type for current meeting before continuing");
                return;
            }

            DocumentInfo DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(),
                new DualEntitiesParameter<PROJECT, MEETING>(loadPROJECT, DisplaySelectedEntity),
                    "MINUTE_AGENDACollectionView",
                    "[" + DisplaySelectedEntity.EntityNumber + "] Agenda");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        #endregion
    }
}