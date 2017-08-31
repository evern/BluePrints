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
using BluePrints.Common.Reports;
using BluePrints.Common;
using BluePrints.Reports;
using System.IO;
using DevExpress.Xpf.Printing;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class MINUTE_AGENDACollectionViewModelWrapper : BluePrintsEntitiesStaticMasterOtherDetailCollectionsWrapper<MINUTE_TITLE, MINUTE_AGENDA, MINUTE_COMMENT, Guid, IBluePrintsEntitiesUnitOfWork>
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

        protected override void resolveParameters(object parameter)
        {
            var meetingParameter = (DualEntitiesParameter<PROJECT, MEETING>) parameter;
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
            foreach(MeetingUser meetingUser in AllMeetingUsers.OrderBy(x => x.Full_Name))
            {
                if (!MeetingUserCollection.Any(x => x.Guid == meetingUser.Guid))
                    MeetingUserCollection.Add(meetingUser);
            }
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.MINUTE_AGENDAS, MINUTE_AGENDAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.MINUTE_COMMENTS, MINUTE_COMMENTProjectionFunc);
        }

        private Func<IRepositoryQuery<MINUTE_AGENDA>, IQueryable<MINUTE_AGENDA>> MINUTE_AGENDAProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<MINUTE_COMMENT>, IQueryable<MINUTE_COMMENT>> MINUTE_COMMENTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.MINUTE_TITLES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<MINUTE_TITLE>, IQueryable<MINUTE_TITLE>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_MEETING_TYPE == loadMEETING.MEETING_TYPE.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<MINUTE_TITLE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override bool onMainBeforeSavedIsContinue(MINUTE_AGENDA mainEntity)
        {
            mainEntity.GUID_PROJECT = loadPROJECT.GUID;
            return base.onMainBeforeSavedIsContinue(mainEntity);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "MINUTE_AGENDACollectionViewModelWrapper"; }
        }

        protected override string expand_key_field_name => BindableBase.GetPropertyName(() => new MINUTE_AGENDA().GUID);
        protected override IEnumerable<MINUTE_COMMENT> child_entities => MINUTE_COMMENTCollection;
        public override CollectionViewModel<MINUTE_COMMENT, MINUTE_COMMENT, Guid, IBluePrintsEntitiesUnitOfWork> ChildEntitiesViewModel => MINUTE_COMMENTCollectionViewModel;
        protected override IEnumerable<MINUTE_AGENDA> main_entities => MINUTE_AGENDACollection;
        public override CollectionViewModel<MINUTE_AGENDA, MINUTE_AGENDA, Guid, IBluePrintsEntitiesUnitOfWork> MainEntitiesViewModel => MINUTE_AGENDACollectionViewModel;

        public IEnumerable<MINUTE_TITLE> MINUTE_TITLECollection
        {
            get
            {
                var collection = GetEntities<MINUTE_TITLE>();
                if (collection == null)
                    return new List<MINUTE_TITLE>();

                return collection.OrderBy(x => x.Full_Name);
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

        public IEnumerable<MINUTE_AGENDA> MINUTE_AGENDACollection
        {
            get
            {
                var collection = GetEntities<MINUTE_AGENDA>();
                if (collection == null)
                    return new List<MINUTE_AGENDA>();

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


        public CollectionViewModel<MINUTE_AGENDA, MINUTE_AGENDA, Guid, IBluePrintsEntitiesUnitOfWork> MINUTE_AGENDACollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<MINUTE_AGENDA, MINUTE_AGENDA, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<MINUTE_AGENDA>();
            }
        }

        #endregion
    }
}