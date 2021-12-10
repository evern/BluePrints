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
using BluePrints.Common;
using BaseModel.Data.Helpers;

namespace BluePrints.ViewModels
{
    public class AREACollectionViewModelWrapper :
        BluePrintsEntitiesMasterDetailCollectionsWrapper
        <AREA, AREAMasterDetailProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of AREACollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static AREACollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new AREACollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the AREACollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the AREACollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected AREACollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            IsReadOnly = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Areas)) == LoginCredentials.PermissionStatus.ReadOnly;
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
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.AREAS);
        }

        protected override Func<IRepositoryQuery<AREA>, IQueryable<AREAMasterDetailProjection>> specifyMainViewModelProjection()
        {
            return query => AREAMasterDetailProjectionQueries.Area_Master_Detail_Transformation(query, loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<AREAMasterDetailProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(AREAMasterDetailProjection projection, out bool isNew)
        {
            projection.Entity.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }
        #endregion

        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "AREACollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "AREACollectionViewModelWrapper_v2" + view_project_specific_affix; }
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

        public IEnumerable<AREA> AllAREACollection
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                List<AREA> allAreas = new List<AREA>();
                allAreas.AddRange(MainViewModel.Entities.Select(x => x.Entity));
                allAreas.AddRange(MainViewModel.Entities.SelectMany(x => x.DetailEntities.Select(z => z.Entity)));
                    
                return allAreas;
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

        protected override string expand_key_field_name => BindableBase.GetPropertyName(() => new AREAMasterDetailProjection().Entity) + "." + BindableBase.GetPropertyName(() => new AREA().GUID);
        #endregion

        #region View Behavior
        protected override void OnClose(CancelEventArgs e)
        {
            //if (STOCK_CODECollectionViewModel != null && DisplayEntities != null)
            //{
            //    List<STOCK_CODE> addStockCodes = new List<STOCK_CODE>();
            //    foreach (AREAMasterDetailProjection area in DisplayEntities)
            //    {
            //        //if default subarea already exists
            //        if (!ProjectSTOCK_CODECollection.Any(x => x.GUID_AREA == area.GUID))
            //        {
            //            addStockCodes.AddRange(getAreaStockCodes(area.Entity));
            //        }

            //        foreach (AREAMasterDetailProjection subArea in area.DetailEntities)
            //        {
            //            addStockCodes.AddRange(getAreaStockCodes(area.Entity, subArea.Entity));
            //        }
            //    }

            //    STOCK_CODECollectionViewModel.BulkSave(addStockCodes);
            //}

            base.OnClose(e);
        }

        public override string UnifiedRowValidation(AREAMasterDetailProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(AREAMasterDetailProjection projection, string field_name, object new_value, bool isPaste)
        {
            string internalNumberFieldName = BindableBase.GetPropertyName(() => new AREAMasterDetailProjection().Entity.INTERNAL_NUM);
            if(field_name.Contains(internalNumberFieldName) && projection.Entity.GUID_PARENT != null)
            {
                return "Subarea number must only contain 2 characters";
            }

            return string.Empty;
        }

        //private List<STOCK_CODE> getAreaStockCodes(AREA area, AREA subArea = null)
        //{
        //    List<STOCK_CODE> areaStockCodes = new List<STOCK_CODE>();
        //    Guid? subAreaGuid = null;
        //    if (subArea != null)
        //        subAreaGuid = subArea.GUID;

        //    foreach (STOCK_CODE globalStockCode in GlobalSTOCK_CODECollection)
        //    {
        //        if (!ProjectSTOCK_CODECollection.Any(x => x.CODE == globalStockCode.CODE && x.GUID_AREA == area.GUID && x.GUID_SUBAREA == subAreaGuid && x.GUID_DISCIPLINE == globalStockCode.GUID_DISCIPLINE))
        //            areaStockCodes.Add(new STOCK_CODE() { GUID_AREA = area.GUID, GUID_DISCIPLINE = globalStockCode.GUID_DISCIPLINE, GUID_PROJECT = loadPROJECT.GUID, GUID_SUBAREA = subAreaGuid, CODE = globalStockCode.CODE });
        //    }

        //    return areaStockCodes;
        //}
        #endregion
    }
}