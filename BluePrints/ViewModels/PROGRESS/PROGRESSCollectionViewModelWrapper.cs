using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Utils;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.P6_DATABASE == P6DatabaseVersion.New);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, P6PROJECTProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.PROGRESS.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> P6PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(loadPROJECT.NUMBER)).OrderBy(proj => proj.wbs_short_name);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES);
        }

        protected override Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        List<Guid> deleteGuids = new List<Guid>();
        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PROGRESS> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        //have to resort to this due to EF error
        //to reproduce the error remove this convention and make a backup of a progress, then edit any field, then delete
        //somehow TreeCreated on softdeleteinterceptor is not triggered
        //the error is: The relationship could not be changed because one or more of the foreign-key properties is non-nullable
        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            if (deleteGuids.Count() > 0)
            {
                foreach (Guid deleteGuid in deleteGuids)
                {
                    PROGRESS findPROGRESS = MainViewModel.Entities.FirstOrDefault(x => x.GUID == deleteGuid);
                    if (findPROGRESS != null)
                        MainViewModel.Delete(findPROGRESS);
                }

                deleteGuids.Clear();
            }

            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        #region Collection Call Backs
        public override void BulkDelete()
        {
            if (MessageBoxService.ShowMessage("Are you sure you want to delete " + SelectedEntities.Count + " selected entries?", "Confirmation", MessageButton.OKCancel) == MessageResult.Cancel)
                return;

            foreach (PROGRESS projection in SelectedEntities)
            {
                deleteGuids.Add(projection.GUID);
            }

            FullRefresh();
        }

        protected override void OnBeforeProjectionsDelete(IEnumerable<PROGRESS> projections)
        {
            base.OnBeforeProjectionsDelete(projections);
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, PROGRESS projection, bool isNew)
        {
            //always save data date as end of day
            if(field_name == BindableBase.GetPropertyName(() => new PROGRESS().DATA_DATE))
            {
                if(new_value != null)
                {
                    DateTime dataDate = (DateTime)new_value;
                    projection.DATA_DATE = dataDate.Date.AddDays(1).AddSeconds(-1);
                }
            }

            base.UnifiedCellValueChanged(field_name, old_value, new_value, projection, isNew);
        }

        public override void UnifiedNewRowInitializationFromView(PROGRESS projection)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            projection.PROGRESS_START = projection.PROGRESS_START.Date;
            projection.DATA_DATE = projection.DATA_DATE.Date.AddDays(1).AddSeconds(-1);
            if (projection.REPORT_DATE != null)
            {
                DateTime qualifiedReportDate = ((DateTime)projection.REPORT_DATE).Date.AddDays(1).AddSeconds(-1);
                projection.REPORT_DATE = qualifiedReportDate;
            }

            base.UnifiedNewRowInitializationFromView(projection);
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
            get { return "PROGRESSCollectionViewModelWrapper_v2" + view_project_specific_affix; }
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
            if (SelectedEntity == null)
                return false;

            return true;
        }

        public void Backup()
        {
            if (MessageBoxService.ShowMessage("This will created a backup of your selected progress with current data date, do you wish to continue?", BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
                return;

            BluePrintsDataUtils.CreateProgressBackup(SelectedEntity);
            FullRefresh();
        }

        public bool CanP6BASELINE_ASSIGN()
        {
            return SelectedEntity != null && SelectedEntity.P6PROGRESS_NAME != null && SelectedEntity.P6PROGRESS_NAME != string.Empty;
        }

        public void P6BASELINE_ASSIGN()
        {
            string viewName;
            if (loadPROJECT.USE_WORKPACKS)
            {
                if (SelectedEntity.TYPE == PhaseType.Construct)
                    viewName = "BUDGET_ITEMWorkpackSchedulingView";
                else
                    viewName = "BASELINE_ITEMWorkpackSchedulingView";
            }
            else
            {
                if (SelectedEntity.TYPE == PhaseType.Construct)
                    viewName = "BUDGET_ITEMSchedulingView";
                else
                    viewName = "BASELINE_ITEMSchedulingView";
            }

            string tabName = SelectedEntity.NAME + " - " + SelectedEntity.P6PROGRESS_NAME + " Mapping";
            DocumentInfo DocumentInfo = new DocumentInfo(tabName, new object[] { SelectedEntity, BaselineMappingSelectionType.Original, loadPROJECT, false }, viewName, tabName);
            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

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

        public void Edit()
        {
            if (SelectedEntity == null)
                return;

            DocumentInfo DocumentInfo;
            if(SelectedEntity.TYPE == PhaseType.Design)
                DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, PROGRESS>(null, SelectedEntity), "OffsiteDirectProgressCollectionView", "[" + loadPROJECT.NUMBER + "] Progress");
            else
                DocumentInfo = new DocumentInfo(SelectedEntity.GUID.ToString(), new DualEntitiesParameter<PROJECT, PROGRESS>(null, SelectedEntity), "SiteDirectProgressCollectionView", "[" + loadPROJECT.NUMBER + "] Progress");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }
        
        public override string UnifiedRowValidation(PROGRESS projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PROGRESS projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new PROGRESS().STATUS) && new_value != null)
            {
                IEnumerable<PROGRESS> otherPROGRESSES = Entities.Where(x => x.GUID != projection.GUID && x.TYPE == projection.TYPE);
                ProgressStatus newStatus = (ProgressStatus)new_value;
                if (otherPROGRESSES.Any(x => x.STATUS == ProgressStatus.Live) && newStatus == ProgressStatus.Live)
                    return "There can be only one live progress";
            }
            else if (projection.TYPE == PhaseType.Design && field_name == BindableBase.GetPropertyName(() => new PROGRESS().DATA_DATE) && new_value != null)
            {
                if(projection.PROGRESS_ITEM != null && projection.PROGRESS_ITEM.Count > 0)
                {
                    DateTime newDateTime = (DateTime)new_value;
                    DateTime currentDateTime = projection.DATA_DATE;

                    if(newDateTime.DayOfWeek != currentDateTime.DayOfWeek)
                    {
                        UICommand backwardCommand = new UICommand()
                        {
                            Id = EarnedDataDateRealignmentAction.Forward,
                            Caption = "Backward",
                            IsCancel = true,
                            IsDefault = false,
                        };

                        UICommand forwardCommand = new UICommand()
                        {
                            Id = EarnedDataDateRealignmentAction.Backward,
                            Caption = "Forward",
                            IsCancel = true,
                            IsDefault = false,
                        };

                        UICommand cancelCommand = new UICommand()
                        {
                            Id = EarnedDataDateRealignmentAction.Cancel,
                            Caption = "Cancel",
                            IsCancel = true,
                            IsDefault = false,
                        };

                        TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(projection);
                        DateTime lastDataDate = ((DateTime)new_value).AddDays(140); //add days just in case there are progresses in future dates
                        DateTime firstAlignedDataDate = ChronologicalHelpers.RewindDataDate(projection.PROGRESS_START.AddYears(-1), lastDataDate, interval);
                        List<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);
                        List<EarnedDataDateRealignModel> earnedDataDateRealignModels = new List<EarnedDataDateRealignModel>();

                        IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();
                        IQueryable<BASELINE_ITEM> baselineItems = bluePrintsUnitOfWork.BASELINE_ITEMS.Where(x => x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.GUID_PROJECT == loadPROJECT.GUID);

                        foreach (PROGRESS_ITEM progress_item in projection.PROGRESS_ITEM)
                        {
                            string s;
                            if (progress_item.EARNED_DATE.Day == 12 && progress_item.EARNED_DATE.Month == 4 && progress_item.EARNED_DATE.Year == 2020)
                                s = string.Empty;
                            DateTime? backwardDate = alignedDataDateCollection.OrderByDescending(x => x).FirstOrDefault(x => progress_item.EARNED_DATE > x);
                            DateTime? forwardDate = alignedDataDateCollection.OrderBy(x => x).FirstOrDefault(x => x > progress_item.EARNED_DATE);

                            if (backwardDate == null || forwardDate == null)
                                return "Some earned dates cannot be readjusted";

                            EarnedDataDateRealignModel earnedDataDateRealignModel = new EarnedDataDateRealignModel() { Guid = progress_item.GUID, EarnedUnits = progress_item.EarnedUnits, CurrentEarnedDate = progress_item.EARNED_DATE, BackwardEarnedDate = (DateTime)backwardDate, ForwardEarnedDate = (DateTime)forwardDate };
                            earnedDataDateRealignModels.Add(earnedDataDateRealignModel);
                        }

                        var culture = new System.Globalization.CultureInfo("de-DE");
                        string label = "All earned units are currently set on the week ending " + CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(currentDateTime.DayOfWeek) + " will be changed to " + CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(newDateTime.DayOfWeek);
                        EarnedDataDateRealignmentViewModel earnedDataDateRealignmentViewModel = EarnedDataDateRealignmentViewModel.Create(earnedDataDateRealignModels, label);
                        UICommand result = EarnedDataDateRealignmentDialogService.ShowDialog(new List<UICommand>() { backwardCommand, forwardCommand, cancelCommand }, "Earned Date Realignment", "EarnedDataDateRealignment", earnedDataDateRealignmentViewModel);
                        if (result == null || result == cancelCommand)
                            return "Date date change cancelled";
                        else
                        {
                            foreach (EarnedDataDateRealignModel earnedDataDateRealignModel in earnedDataDateRealignModels)
                            {
                                PROGRESS_ITEM findPROGRESS_ITEM = bluePrintsUnitOfWork.PROGRESS_ITEMS.FirstOrDefault(x => x.GUID == earnedDataDateRealignModel.Guid);
                                if (findPROGRESS_ITEM != null)
                                {
                                    if (result == backwardCommand)
                                        findPROGRESS_ITEM.EARNED_DATE = earnedDataDateRealignModel.BackwardEarnedDate;
                                    else if (result == forwardCommand)
                                        findPROGRESS_ITEM.EARNED_DATE = earnedDataDateRealignModel.ForwardEarnedDate;
                                }
                            }

                            bluePrintsUnitOfWork.SaveChanges();
                        }
                    }
                }
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

        private DevExpress.Mvvm.IDialogService EarnedDataDateRealignmentDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("EarnedDataDateRealignmentDialogService"); }
        }

        public CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESS_ITEMCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROGRESS_ITEM>();
            }
        }
        #endregion
    }
}