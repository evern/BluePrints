using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.Services;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Helpers;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BluePrints.ViewModels
{
    public class EXO_POCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<PURCHORD_LINES, PURCHORD_LINES, int, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of EXO_POCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_POCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_POCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the EXO_POCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the EXO_POCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_POCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            IsReadOnly = LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_AllPO)) == LoginCredentials.PermissionStatus.ReadOnly;
        }

        #region Database Operations
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        protected PROJECT loadPROJECT;
        public List<ExoDataPoint> ExoMaterials { get; set; }
        public CriteriaOperator FilterCriteria { get; set; }
        protected virtual IGridControlService DetailGridControlService { get { return this.GetService<IGridControlService>("DetailGridControlService"); } }
        protected virtual ITableViewService DetailTableViewService { get { return this.GetService<ITableViewService>("DetailTableViewService"); } }
        bool isFilterActuals;
        public bool IsFilterActuals
        {
            get => isFilterActuals;
            set
            {
                isFilterActuals = value;
                if (!isFilterActuals)
                    clearFilter();

                BluePrintsDataUtils.SaveUserPreference(DataUtils.GetNameOf(() => UserPreferences.EXO_POUseFilter), value ? UserPreferences.PreferenceTrueValue : UserPreferences.PreferenceFalseValue);
            }
        }

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();

            bool? filterActualsPreference = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.EXO_POUseFilter));
            isFilterActuals = filterActualsPreference == null ? false : (bool)filterActualsPreference;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription<PrimeroData.STOCK_ITEMS, PrimeroData.STOCK_ITEMS, string, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.STOCK_ITEMS);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINE_DESCS, DISCIPLINE_DESCProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == null);
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query.Where(x => x.JOBCODE.Contains(loadPROJECT.NUMBER.ToString()));
        }

        private Func<IRepositoryQuery<DISCIPLINE_DESC>, IQueryable<DISCIPLINE_DESC>> DISCIPLINE_DESCProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.PURCHORD_LINES);
        }

        protected override Func<IRepositoryQuery<PURCHORD_LINES>, IQueryable<PURCHORD_LINES>> specifyMainViewModelProjection()
        {
            return query => getExoPOs(query);
        }

        public IQueryable<PURCHORD_LINES> getExoPOs(IRepositoryQuery<PURCHORD_LINES> query)
        {
            DateTime futureDateTime = DateTime.Now.AddYears(10);
            if(ExoMaterials == null)
            {
                ExoMaterials = BluePrintsDataUtils.GetMaterials(MainViewModel.UnitOfWork, loadPROJECT.NUMBER, futureDateTime);
                foreach(ExoDataPoint exoMaterial in ExoMaterials)
                {
                    exoMaterial.PopulateDisciplineDesc(DISCIPLINE_DESCCollection, JOB_COSTGROUPSCollection);
                }
            }

            List<PURCHORD_LINES> exoPos = BluePrintsDataUtils.GetAllNativeEXOPO(MainViewModel.UnitOfWork, query, loadPROJECT.NUMBER);
            List<PURCHORD_LINES> returnDataPoints = new List<PURCHORD_LINES>();

            foreach(PURCHORD_LINES exoPo in exoPos)
            {
                exoPo.PopulateCommodityCodes(COMMODITY_CODECollection);
                exoPo.PopulateCostTypes(JOB_COSTTYPESCollection);
                exoPo.PopulateStockItems(STOCK_ITEMSCollection);
                exoPo.PopulateDisciplineDesc(DISCIPLINE_DESCCollection, JOB_COSTGROUPSCollection);

                returnDataPoints.Add(exoPo);
                exoPo.Update();
            }

            return returnDataPoints.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PURCHORD_LINES> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            DetailTableViewService.ApplyBestFit();
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, PURCHORD_LINES projection, bool isNew)
        {
            if(field_name == BindableBase.GetPropertyName(() => new PURCHORD_LINES().DisciplineDesc))
            {
                projection.FindExistingOrAddDisciplineDesc(DISCIPLINE_DESCCollectionViewModel, loadPROJECT.GUID);

                //update view discipline desc on PO with same discipline code
                foreach (IHaveDisciplineDesc purchordLine in MainViewModel.Entities.Where(x => x.DisciplineCode == projection.DisciplineCode))
                {
                    purchordLine.DisciplineDesc = projection.DisciplineDesc;
                    purchordLine.Update();
                }

                //update view discipline desc on actuals with same discipline code
                foreach (IHaveDisciplineDesc exoMaterial in ExoMaterials.Where(x => x.DisciplineCode == projection.DisciplineCode))
                {
                    exoMaterial.DisciplineDesc = projection.DisciplineDesc;
                    exoMaterial.Update();
                }
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override string UnifiedValueValidation(PURCHORD_LINES projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(PURCHORD_LINES projection)
        {
            return string.Empty;
        }

        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            ExoMaterials = null;
            base.FullRefresh();
        }
        #endregion

        #region Filtering

        protected override void OnSelectedEntitiesChanged()
        {
            setFilter();
            base.OnSelectedEntitiesChanged();
        }

        /// <summary>
        /// Because grid alternate between showing editor and focused row, use showing editor to invoke set filter
        /// </summary>
        public void ShowingEditor(DevExpress.Xpf.Grid.ShowingEditorEventArgs e)
        {
            setFilter();
        }

        private bool isDetailBestFitApplied { get; set; }
        private void setFilter()
        {
            if (!IsFilterActuals)
                return;

            if (SelectedEntities == null || SelectedEntities.Count == 0)
                return;

            IEnumerable<PURCHORD_LINES> projections = SelectedEntities.Where(x => x.Subjob_Name != null && x.Subjob_Name != string.Empty && x.Discipline_Code != null && x.Discipline_Code != string.Empty && x.Commodity_Code != null && x.Commodity_Code != string.Empty);
            var groupedSelections = projections.GroupBy(x => x.Subjob_Name + x.Discipline_Code + x.Commodity_Code).Select(group => new { SelectionKey = group.Key, GroupedProjections = group.ToList() });
            string criteriaString = string.Empty;

            foreach(var groupedSelection in groupedSelections)
            {
                PURCHORD_LINES firstElement = groupedSelection.GroupedProjections.First();
                criteriaString += "([Subjob_Name] = '" + firstElement.Subjob_Name + "' And [Discipline_Code] = '" + firstElement.Discipline_Code + "' And [Variation_Code] = '" + firstElement.X_VARIATIONCODE + "' And [Commodity_Code] = '" + firstElement.Commodity_Code + "' And (";

                var groupedByPOSelections = groupedSelection.GroupedProjections.GroupBy(x => x.HDR_SEQNO).Select(group => new { PONumber = group.Key, GroupedProjections = group.ToList() }); ;
                foreach(var groupedByPOSelection in groupedByPOSelections)
                {
                    criteriaString += "([PONumber] = '" + groupedByPOSelection.PONumber + "' AND (";
                    foreach (PURCHORD_LINES puchordLine in groupedByPOSelection.GroupedProjections)
                    {
                        string sanitizedDescription = puchordLine.DESCRIPTION.Replace("'", "''");
                        criteriaString += "[Description] = '" + sanitizedDescription + "' OR ";
                    }

                    criteriaString = criteriaString.Substring(0, criteriaString.Length - 4);
                    criteriaString += ")) OR ";
                }

                criteriaString = criteriaString.Substring(0, criteriaString.Length - 4);
                criteriaString += ")) OR ";
            }

            criteriaString = criteriaString.Substring(0, criteriaString.Length - 4);
            FilterCriteria = CriteriaOperator.Parse(criteriaString);

            this.RaisePropertyChanged(x => x.FilterCriteria);
            if (!isDetailBestFitApplied)
            {
                DetailTableViewService.ApplyBestFit();
                isDetailBestFitApplied = true;
            }
        }

        public void DetailGridKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.F)
                {
                    clearFilter();
                }
            }
        }

        private void clearFilter()
        {
            //workaround for when detail grid doesn't show anything when it's first loaded, bug on devexpress
            FilterCriteria = CriteriaOperator.Parse("[Subjob_Name] = '000'");
            this.RaisePropertyChanged(x => x.FilterCriteria);

            FilterCriteria = CriteriaOperator.Parse("");
            this.RaisePropertyChanged(x => x.FilterCriteria);
        }

        public void CopyDetailWithHeader()
        {
            DetailGridControlService.CopyWithHeader();
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "EXO_POCollectionViewModelWrapper_v3"; }
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

        public IEnumerable<PrimeroData.STOCK_ITEMS> STOCK_ITEMSCollection
        {
            get
            {
                var collection = GetEntities<PrimeroData.STOCK_ITEMS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.STOCKCODE);
                return collection;
            }
        }

        public IEnumerable<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_HDR>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.JOBCODE);
                return collection;
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<string> COMMODITY_CODEStringCollection
        {
            get
            {
                var collection = GetEntities<COMMODITY_CODE>();
                List<string> allCommodityCodes = new List<string>();
                if (collection != null)
                {
                    allCommodityCodes.AddRange(collection.OrderBy(x => x.CODE).Distinct().Select(x => x.CODE).Distinct());
                }

                return allCommodityCodes.OrderBy(x => x);
            }
        }

        public IEnumerable<string> STOCK_CODEStringCollection
        {
            get
            {
                var collection = GetEntities<PrimeroData.STOCK_ITEMS>();
                List<string> allStockCodeRanges = new List<string>();
                if (collection != null)
                {
                    allStockCodeRanges.AddRange(collection.OrderBy(x => x.STOCKCODE).Distinct().Select(x => x.STOCKCODE).Distinct());
                }

                return allStockCodeRanges.OrderBy(x => x);
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE_DESC> DISCIPLINE_DESCCollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE_DESC>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public CollectionViewModel<DISCIPLINE_DESC, DISCIPLINE_DESC, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINE_DESCCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<DISCIPLINE_DESC, DISCIPLINE_DESC, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<DISCIPLINE_DESC>();
            }
        }
        #endregion
    }
}