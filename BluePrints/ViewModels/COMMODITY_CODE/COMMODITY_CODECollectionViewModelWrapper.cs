using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class COMMODITY_CODECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <COMMODITY_CODE, COMMODITY_CODEProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of COMMODITY_CODECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITY_CODECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the COMMODITY_CODECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITY_CODECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITY_CODECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

#if MONTREAL
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(true).CreateUnitOfWork();
#else
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
#endif

        public bool IsProjectSpecific
        {
            get { return loadPROJECT != null; }
        }

        public List<STOCK_ITEMS> STOCK_ITEMS { get; set; }
        public List<JOB_COSTTYPES> JOB_COSTTYPES { get; set; }
        public List<JOB_COSTGROUPS> JOB_COSTGROUPS { get; set; }
        protected override void resolveParameters(object parameter)
        {
            if (parameter != null)
            {
                var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
                loadPROJECT = PROJECTParameter.GetEntity();
            }

            STOCK_ITEMS = primeroUnitOfWork.STOCK_ITEMS.Where(x => !x.DESCRIPTION.ToUpper().Contains("VARIATION")).OrderBy(x => x.STOCKCODE).ToList();
            JOB_COSTTYPES = primeroUnitOfWork.JOB_COSTTYPES.Where(x => !x.COSTDESC.ToUpper().Contains("VARIATION")).OrderBy(x => x.SHORTCODE).ToList();
            JOB_COSTGROUPS = primeroUnitOfWork.JOB_COSTGROUPS.OrderBy(x => x.SHORTCODE).ToList();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            //need to add another viewmodel so that all stock codes are loaded for stock codes generation
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            if (IsProjectSpecific)
                return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            else
                //not necessary to load anything when it's global commodity code
                return query => query.Where(x => x.GUID_PROJECT == Guid.Empty);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            if (IsProjectSpecific)
                return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID));
            else
                return query => query.Where(x => x.GUID == null);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES);
        }

        protected override Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODEProjection>> specifyMainViewModelProjection()
        {
            if(IsProjectSpecific)
                return query => COMMODITY_CODEProjectionQueries.COMMODITY_CODE_Transformation(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID), primeroUnitOfWork);
            else
                return query => COMMODITY_CODEProjectionQueries.COMMODITY_CODE_Transformation(query.Where(x => x.GUID_PROJECT == null), primeroUnitOfWork);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<COMMODITY_CODEProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(COMMODITY_CODEProjection projection, out bool isNew)
        {
            if (IsProjectSpecific)
                projection.Entity.GUID_PROJECT = loadPROJECT.GUID;

            if (projection.GUID == Guid.Empty)
            {
                if (projection.Entity.DEFAULT_STOCKCODE == null || projection.Entity.DEFAULT_STOCKCODE == string.Empty)
                    projection.Entity.DEFAULT_STOCKCODE = projection.Entity.CODE;

                if (projection.Entity.DEFAULT_COSTGROUP == null || projection.Entity.DEFAULT_COSTGROUP == string.Empty)
                {
                    projection.Entity.DEFAULT_COSTGROUP = getCostGroupCode(projection.Entity.GUID_DISCIPLINE);
                }
            }

            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, COMMODITY_CODEProjection projection, bool isNew)
        {
            if(isNew && field_name.Contains(BindableBase.GetPropertyName(() => new COMMODITY_CODEProjection().Entity.CODE)))
            {
                if (projection.Entity.DEFAULT_STOCKCODE == string.Empty)
                    projection.Entity.DEFAULT_STOCKCODE = projection.Entity.CODE;
            }
            else if (isNew && field_name.Contains(BindableBase.GetPropertyName(() => new COMMODITY_CODEProjection().Entity.GUID_DISCIPLINE)))
            {
                if (projection.Entity.DEFAULT_COSTGROUP == string.Empty)
                    projection.Entity.DEFAULT_COSTGROUP = getCostGroupCode((Guid?)new_value);
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        private string getCostGroupCode(Guid? guid_discipline)
        {
            if (guid_discipline == null)
                return string.Empty;

            DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == guid_discipline);
            if (findDISCIPLINE != null)
                return string.Concat(findDISCIPLINE.CODE, "01");

            return string.Empty;
        }

        public override string UnifiedRowValidation(COMMODITY_CODEProjection projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(COMMODITY_CODEProjection projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public bool CanAlignExoData()
        {
            return !IsLoading;
        }

        private string validateProjection(COMMODITY_CODEProjection projection, bool isFix)
        {
            string message = string.Empty;
            if (projection.EXO_COSTTYPE == null)
                message += "Cost Type: " + projection.Entity.CODE + " and ";
            if (projection.EXO_STOCKITEM == null)
                message += "Stock Code: " + projection.Entity.DEFAULT_STOCKCODE + " and ";

            if (message != string.Empty)
            {
                message = string.Concat(message.Substring(0, message.Length - 4), "doesn't exist in exo");
                return message; 
            }

            if (projection.Entity.GUID_DISCIPLINE != null)
            {
                DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == projection.Entity.GUID_DISCIPLINE);
                if (findDISCIPLINE != null)
                {
                    string validDisciplineCode = string.Concat(findDISCIPLINE.CODE, BluePrintsResources.DefaultCostGroupAffix);
                    JOB_COSTGROUPS validCostGroup = JOB_COSTGROUPS.FirstOrDefault(x => x.SHORTCODE == validDisciplineCode);
                    if (validCostGroup == null)
                        return "Cost Group:" + validDisciplineCode + " doesn't exists in exo";
                }

            }

            return string.Empty;
        }

        public void CustomUnboundColumnData(GridColumnDataEventArgs e)
        {
            if (Entities != null && Entities.Count > 0)
            {
                if (e.Column.FieldName == "ErrorMessage")
                {
                    COMMODITY_CODEProjection row = Entities[e.ListSourceRowIndex];
                    e.Value = validateProjection(row, false);
                }
            }
        }
#endregion

#endregion

#region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "COMMODITY_CODECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "COMMODITY_CODECollectionViewModelWrapper_v2" + view_project_specific_affix; }
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

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> SUBAREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT != null).OrderBy(x => x.INTERNAL_NUM);
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

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPCollection
        {
            get
            {
                return JOB_COSTGROUPS;
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_TYPECollection
        {
            get
            {
                return JOB_COSTTYPES;
            }
        }

        public IEnumerable<UOM> UOMCollection
        {
            get
            {
                var collection = GetEntities<UOM>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.UOM1);
                return collection;
            }
        }
       
        public IEnumerable<COMMODITY_CODE> GlobalCOMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITY_CODECollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<COMMODITY_CODE, COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<COMMODITY_CODE>();
            }
        }
#endregion
    }
}