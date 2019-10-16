using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.Views;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BluePrints.ViewModels
{
    public class BUDGET_ITEMSchedulingViewModelWrapper :
        BluePrintsEntitiesSchedulingCollectionWrapper<ESTIMATE_ITEM, ESTIMATE_ITEMProgress, Guid, IBluePrintsEntitiesUnitOfWork>, IHaveCanvasWidth
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BUDGET_ITEMSchedulingViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new BUDGET_ITEMSchedulingViewModelWrapper());
        }

        #region Database Operation

        protected override PhaseType phase_type => PhaseType.Construct;
        private ESTIMATE loadESTIMATE;
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATES, ESTIMATEProjectionFunc, assign_estimation);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_GROUPS, STOCK_GROUPProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);

            base.addEntitiesLoader();
        }

        private void assign_estimation(ESTIMATE entity)
        {
            if (entity == null && !SupressCompulsoryEntityNotFoundMessage)
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Live estimation not found")));

            loadESTIMATE = entity;
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == iHaveP6BaselinesEntity.project_guid);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID || x.GUID_PROJECT == null));
        }

        private Func<IRepositoryQuery<ESTIMATE>, IQueryable<ESTIMATE>> ESTIMATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == iHaveP6BaselinesEntity.project_guid && x.STATUS == BaselineStatus.Live);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Construct);
        }

        private Func<IRepositoryQuery<STOCK_GROUP>, IQueryable<STOCK_GROUP>> STOCK_GROUPProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID));
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.PHASE_TYPE == PhaseType.Construct);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATE_ITEM>, IQueryable<ESTIMATE_ITEMProgress>>
            specifyMainViewModelProjection()
        {
            IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = GetEntities<P6_ASSIGNMENT>();
            return query => ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(query.Where(x => x.GUID_ESTIMATE == loadESTIMATE.GUID), loadPROJECT, loaderCollection.GetCollection<RATE>(), live_PROGRESS, PROGRESS_ITEMCollection, false, STOCK_CODECollection, loaderCollection.GetCollection<STOCK_GROUP>(), null, false, P6_ASSIGNMENTS, false, COMMODITY_CODECollection);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            disableMultipleDeliverablesToOneActivityAssignment = true;
            MainViewModel.AlwaysSkipMessage = true;
            P6_ASSIGNMENTSCollectionViewModel.AlwaysSkipMessage = false;
            P6_ASSIGNMENTSCollectionViewModel.AfterBulkOperationRefreshCallBack = onAfterBulkOperationRefresh;
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        private void onAfterBulkOperationRefresh()
        {

        }

        public override string UnifiedRowValidation(ESTIMATE_ITEMProgress projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(ESTIMATE_ITEMProgress projection, string field_name, object new_value)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        //Auto assign P6 activity by activity ID
        public void AutoAssignActName()
        {
            if (MessageBoxService.ShowMessage("This will reset all assignment and attempt to auto assign deliverables to activity\n\nDo you wish to continue?", "Warning", MessageButton.YesNo) == MessageResult.No)
                return;

            IEnumerable<P6_ASSIGNMENT> delete_assignments = DisplayEntities.SelectMany(x => x.P6_Assignments);
            P6_ASSIGNMENTSCollectionViewModel.BaseBulkDelete(delete_assignments);

            foreach(ESTIMATE_ITEMProgress displayEntity in DisplayEntities)
            {
                displayEntity.P6_Assignments.Clear();
            }

            foreach (var task in TASK_Source)
            {
                string activity_id = task.task_code;
                IEnumerable<ESTIMATE_ITEMProgress> estimateItemsBySubArea = DisplayEntities.Where(x => x.Entity != null && x.Entity.Entity != null && x.Entity.Entity.P6ACTIVITYMAP == activity_id);
                decimal lowValue = 0.01m;
                foreach (var estimateItem in estimateItemsBySubArea)
                {
                    if (estimateItem.Assigned_Percentage == 1)
                        continue;

                    //decimal assignmentValue = (1m / estimateItemsBySubArea.Count());
                    //decimal highValue = (lowValue - 0.01m) + assignmentValue;
                    decimal highValue = 1;
                    P6_ASSIGNMENT newP6_ASSIGNMENT = new P6_ASSIGNMENT();
                    newP6_ASSIGNMENT.GUID = Guid.Empty;
                    newP6_ASSIGNMENT.GUID_PROJECT = loadPROJECT.GUID;
                    newP6_ASSIGNMENT.HIGH_VALUE = highValue;
                    newP6_ASSIGNMENT.LOW_VALUE = lowValue;
                    newP6_ASSIGNMENT.P6_ACTIVITYID = task.task_code;
                    newP6_ASSIGNMENT.GUID_ORIGINAL = estimateItem.OriginalEntityKey;
                    newP6_ASSIGNMENT.TYPE = phase_type;
                    newP6_ASSIGNMENT.ISMODIFIEDBASELINE = false;

                    estimateItem.P6_Assignments.Add(newP6_ASSIGNMENT);
                    //lowValue += assignmentValue;
                }
            }

            IEnumerable<P6_ASSIGNMENT> save_assignments = DisplayEntities.SelectMany(x => x.P6_Assignments.Where(y => y.GUID == Guid.Empty));
            foreach (P6_ASSIGNMENT save_assignment in save_assignments)
            {
                P6_ASSIGNMENTSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(save_assignment, null, null, null, EntityMessageType.Added);
            }

            P6_ASSIGNMENTSCollectionViewModel.BulkSave(save_assignments);
            FullRefresh();
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "BUDGET_ITEMSchedulingViewModelWrapper" + view_project_specific_affix; }
            get { return "BUDGET_ITEMSchedulingViewModelWrapper_v1" + view_project_specific_affix; }
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

        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<STOCK_GROUP> STOCK_GROUPCollection
        {
            get
            {
                var collection = GetEntities<STOCK_GROUP>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_GROUP> ProjectSTOCK_GROUPCollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return STOCK_GROUPCollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            }
        }

        public IEnumerable<STOCK_CODE> STOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> GlobalSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == null).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<STOCK_CODE> ProjectSTOCK_CODECollection
        {
            get
            {
                var collection = GetEntities<STOCK_CODE>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTCollection
        {
            get
            {
                var collection = GetEntities<P6_ASSIGNMENT>();
                return collection;
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

        public override IEnumerable<ICanAssignP6> Deliverables_Source => DisplayEntities;
        #endregion
    }
}