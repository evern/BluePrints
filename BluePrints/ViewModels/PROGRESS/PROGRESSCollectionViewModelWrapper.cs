using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using PROJECT = BluePrints.Data.PROJECT;

namespace BluePrints.ViewModels
{
    public class PROGRESSCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PROGRESS, PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESSCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROGRESSCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROGRESSCollectionViewModelWrapper(unitOfWorkFactory));
        }

        
        /// <summary>
        /// Initializes a new instance of the PROGRESSCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROGRESSCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROGRESSCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS> entities)
        {
            MainViewModel.FuncManualRowPastingIsContinue = this.ManualPasteAction;
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public bool OnBeforeEntitySaved(PROGRESS entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            entity.PROGRESS_START = entity.PROGRESS_START.Date;
            entity.DATA_DATE = entity.DATA_DATE.Date.AddDays(1).AddSeconds(-1);
            if(entity.REPORT_DATE != null)
            {
                DateTime qualifiedReportDate = ((DateTime)entity.REPORT_DATE).Date.AddDays(1).AddSeconds(-1);
                entity.REPORT_DATE = qualifiedReportDate;
            }

            return true;
        }

        #endregion

        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "PROGRESSCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "PROGRESSCollectionViewModelWrapper_v1" + view_project_specific_affix; }
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

        public bool CanBackup()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        public void Backup()
        {
            if (MessageBoxService.ShowMessage("This will created a backup of your selected progress with current data date, do you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            PROGRESS selectedPROGRESS = DisplaySelectedEntity;
            if (selectedPROGRESS == null)
                return;

            PROGRESS backupPROGRESS = new PROGRESS();
            DataUtils.ShallowCopy(backupPROGRESS, selectedPROGRESS);
            backupPROGRESS.GUID = Guid.Empty;
            backupPROGRESS.NAME = "BACKUP " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString();
            backupPROGRESS.STATUS = ProgressStatus.Superseded;
            bluePrintsUOW.PROGRESSES.Add(backupPROGRESS);
            //need to save progress to get GUID
            bluePrintsUOW.SaveChanges();

            LoadingScreenManager.ShowLoadingScreen(selectedPROGRESS.PROGRESS_ITEM.Count());
            decimal totalBackupUnits = 0;
            if (selectedPROGRESS.PROGRESS_ITEM != null)
                foreach (PROGRESS_ITEM progress_item in selectedPROGRESS.PROGRESS_ITEM)
                {
                    totalBackupUnits += progress_item.EARNED_UNITS;
                    LoadingScreenManager.SetMessage("Total Backed Up Units: " + totalBackupUnits);

                    PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                    DataUtils.ShallowCopy(newPROGRESS_ITEM, progress_item);
                    newPROGRESS_ITEM.GUID = Guid.Empty;
                    newPROGRESS_ITEM.GUID_PROGRESS = backupPROGRESS.GUID;
                    bluePrintsUOW.PROGRESS_ITEMS.Add(newPROGRESS_ITEM);
                    LoadingScreenManager.Progress();
                }

            bluePrintsUOW.SaveChanges();
            LoadingScreenManager.CloseLoadingScreen();

            FullRefresh();
        }


        public bool CanEdit()
        {
            if (DisplaySelectedEntity == null)
                return false;

            return true;
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void Edit()
        {
            if (DisplaySelectedEntity == null)
                return;

            DocumentInfo DocumentInfo;
            if(DisplaySelectedEntity.TYPE == PhaseType.Design)
                DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, PROGRESS>(null, DisplaySelectedEntity), "OffsiteDirectProgressCollectionView", "[" + loadPROJECT.NUMBER + "] Progress");
            else
                DocumentInfo = new DocumentInfo(DisplaySelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, PROGRESS>(null, DisplaySelectedEntity), "SiteDirectProgressCollectionView", "[" + loadPROJECT.NUMBER + "] Progress");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public override string UnifiedRowValidation(PROGRESS projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PROGRESS projection, string field_name, object new_value)
        {
            if (field_name == BindableBase.GetPropertyName(() => new PROGRESS().STATUS) && new_value != null)
            {
                IEnumerable<PROGRESS> otherPROGRESSES = DisplayEntities.Where(x => x.GUID != projection.GUID && x.TYPE == projection.TYPE);
                ProgressStatus newStatus = (ProgressStatus)new_value;
                if (otherPROGRESSES.Any(x => x.STATUS == ProgressStatus.Live) && newStatus == ProgressStatus.Live)
                    return "There can be only one live progress";
            }
            //else if(field_name == BindableBase.GetPropertyName(() => new PROGRESS().DATA_DATE) && new_value != null)
            //{
            //    DayOfWeek progressStartDayOfWeek = projection.PROGRESS_START.DayOfWeek;
            //    DayOfWeek dataDateDayOfWeek = ((DateTime)new_value).DayOfWeek;

            //    if (progressStartDayOfWeek != dataDateDayOfWeek)
            //        return "Data date day of week must be the same as start date";
            //}
            //else if (field_name == BindableBase.GetPropertyName(() => new PROGRESS().PROGRESS_START) && new_value != null)
            //{
            //    DayOfWeek progressStartDayOfWeek = ((DateTime)new_value).DayOfWeek; 
            //    DayOfWeek dataDateDayOfWeek = projection.DATA_DATE.DayOfWeek;

            //    if (progressStartDayOfWeek != dataDateDayOfWeek)
            //        return "Data date day of week must be the same as start date";
            //}

            return string.Empty;
        }

        public bool ManualPasteAction(List<KeyValuePair<ColumnBase, string>> pasteData, PROGRESS pasteEntity)
        {
            //pasteEntity.STATUS = ProgressStatus.Working;
            return true;
        }
        #endregion
    }
}