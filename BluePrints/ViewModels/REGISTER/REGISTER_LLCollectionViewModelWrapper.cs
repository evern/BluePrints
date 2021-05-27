using BaseModel.DataModel;
using BaseModel.Misc;
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
    public class REGISTER_LLCollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_LL, REGISTER_LL, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_LLCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_LLCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_LLCollectionViewModelWrapper(
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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_LL);
        }

        protected override Func<IRepositoryQuery<REGISTER_LL>, IQueryable<REGISTER_LL>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NUMBER);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_LL> entities)
        {
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(REGISTER_LL projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            if (projection.GUID == Guid.Empty && projection.DATE_IDENTIFIED == null)
                projection.DATE_IDENTIFIED = DateTime.Now.Date;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(REGISTER_LL projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(REGISTER_LL projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_LL().DATE_CLOSED))
            {
                DateTime? dateClosed = (DateTime?)new_value;
                if (projection.DATE_IDENTIFIED != null && dateClosed != null && projection.DATE_IDENTIFIED > dateClosed)
                    return "Date closed cannot be earlier than date identified";
            }

            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_LL().DATE_IDENTIFIED))
            {
                DateTime? dateRaised = (DateTime?)new_value;
                if (projection.DATE_CLOSED != null && dateRaised != null && dateRaised > projection.DATE_CLOSED)
                    return "Date identified cannot be later than date closed";
            }

            return string.Empty;
        }
        #endregion

        #endregion

        #region IEntityNumber
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
            //get { return "REGISTER_LLCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "REGISTER_LLCollectionViewModelWrapper_v2"; }
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
            return loadPROJECT.NUMBER + "_Register_LL";
        }

        protected override string GetEntityNumberFieldName()
        {
            throw new NotImplementedException();
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
        #endregion
    }
}