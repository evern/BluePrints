using BaseModel.DataModel;
using BaseModel.Misc;
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
    public class RATECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <RATE, RATE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of RATECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RATECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RATECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the RATECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the RATECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RATECollectionViewModelWrapper(
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

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.RATES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<RATE>, IQueryable<RATE>> specifyMainViewModelProjection()
        {
            return query => rateCommodityProjection(query);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        private IQueryable<RATE> rateCommodityProjection(IRepositoryQuery<RATE> rates)
        {
            List<RATE> rateCollection = rates.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).ToList();
            rateCollection.ForEach(x => x.SetCommodityCodes(CombinedCommodityCodeCollection));

            return rateCollection.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<RATE> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(RATE entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            return true;
        }

        public override string UnifiedValueValidation(RATE projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, RATE projection, bool isNew)
        {      
            if (field_name == BindableBase.GetPropertyName(() => new RATE().Phase_Type))
            {
                //rate is not instantiated with commodity codes to be selected, hence initialization begins here
                if(isNew && new_value != null)
                {
                    projection.SetCommodityCodes(CombinedCommodityCodeCollection);
                }

                Guid? oldValue = projection.CommodityCodeId;
                Guid? newValue = null;
                projection.CommodityCodeId = newValue;
                MainViewModel.EntitiesUndoRedoManager.PauseActionId();
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new RATE().CommodityCodeId), oldValue, newValue, EntityMessageType.Changed);
                projection.Update();
            }

            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }
        #endregion

        #endregion

        #region View Properties
        public void CustomColumnDisplayText(CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().CommodityCodeId) ||
                e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_DEPARTMENT) ||
                e.Column.FieldName == BindableBase.GetPropertyName(() => new RATE().GUID_DISCIPLINE))
            {
                if (e.Row != null && e.Value == null)
                {
                    e.DisplayText = "Any";
                }
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "RATECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "RATECollectionViewModelWrapper_v2"; }
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

        public IEnumerable<CombinedCommodityCode> CombinedCommodityCodeCollection
        {
            get
            {
                List<CombinedCommodityCode> combinedCommodityCodes = new List<CombinedCommodityCode>();
                if (DOCTYPECollection != null)
                    combinedCommodityCodes.AddRange(DOCTYPECollection.Select(x => new CombinedCommodityCode() { PhaseType = PhaseType.Design, Code = x.CODE, Key = x.GUID }));

                if (COMMODITY_CODECollection != null)
                    combinedCommodityCodes.AddRange(COMMODITY_CODECollection.Select(x => new CombinedCommodityCode() { PhaseType = PhaseType.Construct, Code = x.CODE, Key = x.GUID }));

                if (COMMODITY_CODECollection != null)
                    combinedCommodityCodes.AddRange(COMMODITY_CODECollection.Select(x => new CombinedCommodityCode() { PhaseType = PhaseType.Procurement, Code = x.CODE, Key = x.GUID }));

                return combinedCommodityCodes;
            }
        }

        public IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();

                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);

                return collection;
            }
        }

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();

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