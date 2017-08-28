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

namespace BluePrints.ViewModels
{
    public class MINUTE_AGENDACollectionViewModelWrapper : BluePrintsEntitiesMasterOtherDetailCollectionsWrapper<MINUTE_AGENDA, MINUTE_COMMENT, MINUTE_AGENDAMasterDetailProjection, Guid, IBluePrintsEntitiesUnitOfWork>
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

        private MEETING loadMEETING;
        public List<MeetingUser> MeetingUserCollection { get; set; }

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var meetingParameter = (EntitiesParameter<MEETING>) parameter;
            loadMEETING = meetingParameter.GetEntity();
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
            foreach(MeetingUser meetingUser in AllMeetingUsers)
            {
                if (!MeetingUserCollection.Any(x => x.Guid == meetingUser.Guid))
                    MeetingUserCollection.Add(meetingUser);
            }
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription<MINUTE_COMMENT, MINUTE_COMMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.MINUTE_COMMENTS);
            loaderCollection.AddLoaderDescription<MINUTE_TITLE, MINUTE_TITLE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.MINUTE_TITLES);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.MINUTE_AGENDAS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<MINUTE_AGENDA>, IQueryable<MINUTE_AGENDAMasterDetailProjection>> specifyMainViewModelProjection()
        {
            return query => MINUTE_AGENDAMasterDetailProjectionQueries.MINUTE_AGENDA_Master_Detail_Transformation(query, loadMEETING.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<MINUTE_AGENDAMasterDetailProjection> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(MINUTE_AGENDAMasterDetailProjection entity)
        {
            entity.Entity.GUID_MEETING = loadMEETING.GUID;
            return true;
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "MINUTE_AGENDACollectionViewModelWrapper"; }
        }

        protected override string expand_key_field_name => BindableBase.GetPropertyName(() => new MINUTE_AGENDAMasterDetailProjection().Entity) + "." + BindableBase.GetPropertyName(() => new MINUTE_AGENDA().GUID);
        protected override IEnumerable<MINUTE_COMMENT> child_entities => MINUTE_COMMENTCollection;
        public override CollectionViewModel<MINUTE_COMMENT, MINUTE_COMMENT, Guid, IBluePrintsEntitiesUnitOfWork> ChildEntitiesViewModel => MINUTE_COMMENTCollectionViewModel;

        public IEnumerable<MINUTE_TITLE> MINUTE_TITLECollection
        {
            get
            {
                var collection = GetEntities<MINUTE_TITLE>();
                if (collection == null)
                    return new List<MINUTE_TITLE>();

                return collection;
            }
        }

        public IEnumerable<MINUTE_COMMENT> MINUTE_COMMENTCollection
        {
            get
            {
                var collection = GetEntities<MINUTE_COMMENT>();
                if (collection == null)
                    return new List<MINUTE_COMMENT>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection;
            }
        }

        public CollectionViewModel<MINUTE_COMMENT, MINUTE_COMMENT, Guid, IBluePrintsEntitiesUnitOfWork> MINUTE_COMMENTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<MINUTE_COMMENT, MINUTE_COMMENT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<MINUTE_COMMENT>();
            }
        }
        #endregion
    }
}