using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseModel.ViewModel.Document;

namespace BluePrints.ViewModels
{
    public class TENDER_PROFILE_ITEMCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <TENDER_PROFILE_ITEM, TENDER_PROFILE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of TENDER_PROFILE_ITEMCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TENDER_PROFILE_ITEMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new TENDER_PROFILE_ITEMCollectionViewModelWrapper(unitOfWorkFactory));
        }

        TENDER_PROFILE loadTENDER_PROFILE;
        /// <summary>
        /// Initializes a new instance of the TENDER_PROFILE_ITEMCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the TENDER_PROFILE_ITEMCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected TENDER_PROFILE_ITEMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            EntitiesParameter<TENDER_PROFILE> entityParameter = (EntitiesParameter<TENDER_PROFILE>)parameter;
            loadTENDER_PROFILE = entityParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
        }
        
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.TENDER_PROFILE_ITEMS);
        }

        protected override Func<IRepositoryQuery<TENDER_PROFILE_ITEM>, IQueryable<TENDER_PROFILE_ITEM>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_TENDER_PROFILE == loadTENDER_PROFILE.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TENDER_PROFILE_ITEM> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(TENDER_PROFILE_ITEM projection, out bool isNew)
        {
            if(loadTENDER_PROFILE != null)
                projection.GUID_TENDER_PROFILE = loadTENDER_PROFILE.GUID;

            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(TENDER_PROFILE_ITEM projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(TENDER_PROFILE_ITEM projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().SCHEDULE_START_PERCENTAGE))
            {
                decimal startPercentage = (decimal)new_value;
                if (projection.SCHEDULE_FINISH_PERCENTAGE != 0 && startPercentage > projection.SCHEDULE_FINISH_PERCENTAGE)
                    return "Start % must be lower than finish %";
            }
            else if (field_name == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().SCHEDULE_FINISH_PERCENTAGE))
            {
                decimal finishPercentage = (decimal)new_value;
                if (finishPercentage < projection.SCHEDULE_START_PERCENTAGE)
                    return "Finish % must be higher than start %";
            }
            else if (field_name == BindableBase.GetPropertyName(() => new TENDER_PROFILE_ITEM().HOURS_PERCENTAGE))
            {
                decimal tenderPercentage = (decimal)new_value;
                decimal remainingPercentage = 1 - (Entities.Where(x => x.GUID != projection.GUID).Sum(x => x.HOURS_PERCENTAGE));

                if (tenderPercentage > remainingPercentage)
                    return "Total % cannot be higher than 100%";
            }

            return string.Empty;
        }
        #endregion

        #endregion

        #region View Properties
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

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "TENDER_PROFILE_ITEMCollectionViewModelWrapper_v2"; }
        }

        #endregion
    }
}