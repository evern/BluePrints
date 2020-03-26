using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
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
    public class REGISTER_HOLDCollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_HOLD, REGISTER_HOLD, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_HOLDCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_HOLDCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_HOLDCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS, BASELINE_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
            loaderCollection.AddLoaderDescription<REGISTER_HOLD_REF, REGISTER_HOLD_REF, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.REGISTER_HOLD_REF);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEM>> BASELINE_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_HOLD);
        }

        protected override Func<IRepositoryQuery<REGISTER_HOLD>, IQueryable<REGISTER_HOLD>> specifyMainViewModelProjection()
        {
            return query => registerHoldProjection(query);
        }

        private IQueryable<REGISTER_HOLD> registerHoldProjection(IRepositoryQuery<REGISTER_HOLD> query)
        {
            List<REGISTER_HOLD> registerHoldCollection = query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NUMBER).ToList();
            registerHoldCollection.ForEach(x => x.SetDeliverables(BASELINE_ITEMCollection, REGISTER_HOLD_REFCollection));

            return registerHoldCollection.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_HOLD> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = OnEntitiesSavedCallBack;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }


        #region Collection Call Backs
        protected override bool onBeforeEntitySavedIsContinue(REGISTER_HOLD projection)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            if (projection.GUID == Guid.Empty && projection.DATE_RAISED == null)
                projection.DATE_RAISED = DateTime.Now.Date;
            return base.onBeforeEntitySavedIsContinue(projection);
        }

        public override string UnifiedRowValidation(REGISTER_HOLD projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(REGISTER_HOLD projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_HOLD().DATE_CLOSED))
            {
                DateTime? dateClosed = (DateTime?)new_value;
                if (projection.DATE_RAISED != null && dateClosed != null && projection.DATE_RAISED > dateClosed)
                    return "Date closed cannot be earlier than date raised";
            }

            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_HOLD().DATE_RAISED))
            {
                DateTime? dateRaised = (DateTime?)new_value;
                if (projection.DATE_CLOSED != null && dateRaised != null && dateRaised > projection.DATE_CLOSED)
                    return "Date raised cannot be later than date closed";
            }

            return string.Empty;
        }

        public void OnEntitiesSavedCallBack(REGISTER_HOLD projectionEntity, REGISTER_HOLD entity, bool isNewEntity)
        {
            save_register_ref(projectionEntity);
        }

        private void save_register_ref(REGISTER_HOLD entity)
        {
            List<REGISTER_HOLD_REF> remove_register_hold_ref = new List<REGISTER_HOLD_REF>();

            if (entity.AssignDeliverables != null)
            {
                foreach (REGISTER_HOLD_REF assignment in REGISTER_HOLD_REFCollection.Where(x => x.GUID_HOLD == entity.GUID))
                {
                    if (!entity.AssignDeliverables.Any(x => x.GUID_ORIGINAL == assignment.GUID_BASELINE_ITEM))
                        remove_register_hold_ref.Add(assignment);
                }

                REGISTER_HOLD_REFCollectionViewModel.BulkDelete(remove_register_hold_ref);
                List<REGISTER_HOLD_REF> add_register_holds = new List<REGISTER_HOLD_REF>();
                foreach (BASELINE_ITEM deliverable in entity.AssignDeliverables)
                {
                    if (!REGISTER_HOLD_REFCollection.Any(x => x.GUID_BASELINE_ITEM == deliverable.GUID_ORIGINAL && x.GUID_HOLD == entity.GUID))
                    {
                        add_register_holds.Add(new REGISTER_HOLD_REF() { GUID_BASELINE_ITEM = deliverable.GUID_ORIGINAL, GUID_HOLD = entity.GUID });
                    }

                }

                REGISTER_HOLD_REFCollectionViewModel.BulkSave(add_register_holds);
            }
            else
            {
                foreach (REGISTER_HOLD_REF assignment in REGISTER_HOLD_REFCollection.Where(x => x.GUID_HOLD == entity.GUID))
                {
                    remove_register_hold_ref.Add(assignment);
                }

                REGISTER_HOLD_REFCollectionViewModel.BulkDelete(remove_register_hold_ref);
            }
        }
        #endregion

        #endregion

        #region IEntityNumber
        protected override string GetEntityNumberFieldName()
        {
            return BindableBase.GetPropertyName(() => new REGISTER_CHANGE().NUMBER);
        }

        protected override int DefaultNumericFieldLength()
        {
            return Int32.Parse(BluePrintsResources.Default_Register_Numeric_Length);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "REGISTER_HOLDCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "REGISTER_HOLDCollectionViewModelWrapper_v1"; }
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

        protected override string ExportFilename()
        {
            return loadPROJECT.NUMBER + "_Register_Hold";
        }


        public CollectionViewModel<REGISTER_HOLD_REF, REGISTER_HOLD_REF, Guid, IBluePrintsEntitiesUnitOfWork> REGISTER_HOLD_REFCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<REGISTER_HOLD_REF, REGISTER_HOLD_REF, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<REGISTER_HOLD_REF>();
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<BASELINE_ITEM> BASELINE_ITEMCollection
        {
            get
            {
                var collection = GetEntities<BASELINE_ITEM>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<REGISTER_HOLD_REF> REGISTER_HOLD_REFCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_HOLD_REF>();
                return collection;
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
        #endregion
    }
}