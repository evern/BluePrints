using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class BASELINECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE, BASELINE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASELINECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private Data.PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
        }
        
        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINES);
        }

        protected override Func<IRepositoryQuery<BASELINE>, IQueryable<BASELINE>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderByDescending(x => x.REVISION);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(BASELINE projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
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
            //get { return "BASELINECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "BASELINECollectionViewModelWrapper_v2" + view_project_specific_affix; }
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

        public IEnumerable<PROJWBS> P6PROJECTSCollection
        {
            get
            {
                var collection = GetEntities<PROJWBS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.wbs_short_name);
                return collection;
            }
        }

        #endregion

        #region ISupportCustomDocumentTypeAndParameter

        public bool CanEdit()
        {
            if (SelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public bool CanBackup()
        {
            return CanEdit();
        }

        public void Backup()
        {
            if (MessageBoxService.ShowMessage("This will created a backup of your selected baseline, do you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            BASELINE selectedBASELINE = SelectedEntity;
            if (selectedBASELINE == null)
                return;

            BASELINE backupBASELINE = new BASELINE();
            DataUtils.ShallowCopy(backupBASELINE, selectedBASELINE);
            backupBASELINE.GUID = Guid.Empty;
            backupBASELINE.NAME = "BACKUP " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString();
            backupBASELINE.STATUS = BaselineStatus.Superseded;
            backupBASELINE.REVISION = selectedBASELINE.REVISION;
            bluePrintsUOW.BASELINES.Add(backupBASELINE);
            //need to save progress to get GUID
            bluePrintsUOW.SaveChanges();

            LoadingScreenManager.ShowLoadingScreen(selectedBASELINE.BASELINE_ITEM.Count());
            decimal totalDeliverables = 0;
            if (selectedBASELINE.BASELINE_ITEM != null)
                foreach (BASELINE_ITEM baseline_item in selectedBASELINE.BASELINE_ITEM)
                {
                    totalDeliverables += 1;
                    LoadingScreenManager.SetMessage("Total Backed Up Deliverable(s): " + totalDeliverables);

                    BASELINE_ITEM newBASELINE_ITEM = new BASELINE_ITEM();
                    DataUtils.ShallowCopy(newBASELINE_ITEM, baseline_item);
                    newBASELINE_ITEM.GUID = Guid.Empty;
                    newBASELINE_ITEM.GUID_BASELINE = backupBASELINE.GUID;
                    bluePrintsUOW.BASELINE_ITEMS.Add(newBASELINE_ITEM);
                    LoadingScreenManager.Progress();
                }

            bluePrintsUOW.SaveChanges();
            LoadingScreenManager.CloseLoadingScreen();

            FullRefresh();
        }

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new TripleEntitiesParameter<Data.PROJECT, IAmBaseline, object>(loadPROJECT, SelectedEntity, DeliverablesViewType.Both), "BASELINE_ITEMCollectionView", "[" + loadPROJECT.NUMBER + "] Baseline Rev " + SelectedEntity.REVISION);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public bool CanP6BASELINE_ASSIGN()
        {
            return SelectedEntity != null && SelectedEntity.P6BASELINE_NAME != null && SelectedEntity.P6BASELINE_NAME != string.Empty;
        }

        public void P6BASELINE_ASSIGN()
        {
            string viewName;
            if (loadPROJECT.USE_WORKPACKS)
                viewName = "BASELINE_ITEMWorkpackSchedulingView";
            else
                viewName = "BASELINE_ITEMSchedulingView";

            string tabName = SelectedEntity.NAME + " - " + SelectedEntity.P6BASELINE_NAME + " Mapping";
            DocumentInfo DocumentInfo = new DocumentInfo(tabName, new object[] { SelectedEntity, BaselineMappingSelectionType.Original, loadPROJECT, true }, viewName, tabName);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public override string UnifiedRowValidation(BASELINE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(BASELINE projection, string field_name, object new_value, bool isPaste)
        {
            if(field_name == BindableBase.GetPropertyName(() => new BASELINE().STATUS) && new_value != null)
            {
                IEnumerable<BASELINE> otherBASELINES = Entities.Where(x => x.GUID != projection.GUID);
                BaselineStatus newStatus = (BaselineStatus)new_value;
                if (otherBASELINES.Any(x => x.STATUS == BaselineStatus.Live) && newStatus == BaselineStatus.Live)
                    return "There can be only one live baseline";
            }

            return string.Empty;
        }
        #endregion
    }
}