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
    public class ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>, IHaveCanvasWidth
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper());
        }

        #region Used as Dependency Delegate

        public Action<IEnumerable<ESTIMATION_DIRECT_ITEMProjection>> OnMappingViewModelLoaded { get; set; }

        private bool isFromPROGRESS
        {
            get { return OnMappingViewModelLoaded != null; }
        }

        #endregion

        #region Database Operation

        private Data.PROJECT loadPROJECT;
        private P6Data.PROJECT loadP6PROJECT;
        private PROGRESS loadPROGRESS;
        private ESTIMATION_DIRECT loadESTIMATION_DIRECT;
        private BaselineMappingSelectionType mappingType;
        private DEPARTMENT defaultConstructionDEPARTMENT;
        private Data.PHASE defaultConstructionPHASE;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory =
            P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        private IDialogService ActivityDetailDialogService
        {
            get { return this.GetRequiredService<IDialogService>("ActivityIdDialog"); }
        }

        protected override void InitializeParameters(object parameter)
        {
            var obj = (object[])parameter;

            if (isFromPROGRESS)
                loadPROGRESS = (PROGRESS)obj[0];
            else
                loadESTIMATION_DIRECT = (ESTIMATION_DIRECT)obj[0];

            mappingType = (BaselineMappingSelectionType)obj[1];
            Selected_Deliverables = new ObservableCollection<ICanAssignP6>();
            Selected_P6_Assignments = new ObservableCollection<P6_ASSIGNMENTProjection>();
            Selected_Deliverables.CollectionChanged += Selected_Deliverables_CollectionChanged;
            p6_baseline_entity = loadESTIMATION_DIRECT;
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS, DEPARTMENTProjectionFunc, x => defaultConstructionDEPARTMENT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, x => defaultConstructionPHASE = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECTS, ESTIMATION_DIRECTProjectionFunc, x => loadESTIMATION_DIRECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.COMMODITY_CODES, COMMODITY_CODEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.STOCK_CODES, STOCK_CODEProjectionFunc);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => loadPROGRESS = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.P6_ASSIGNMENTS, P6_ASSIGNMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS, ESTIMATION_DIRECT_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJECT, P6PROJECTProjectionFunc, x => loadP6PROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.TASK, P6TASKProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, PROJWBSProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<Data.PROJECT>, IQueryable<Data.PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadESTIMATION_DIRECT.GUID_PROJECT);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT>, IQueryable<ESTIMATION_DIRECT>> ESTIMATION_DIRECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == EstimationStatus.Live);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.SiteDirect);
        }

        private Func<IRepositoryQuery<STOCK_CODE>, IQueryable<STOCK_CODE>> STOCK_CODEProjectionFunc()
        {
            return query => query.Where(x => x.STOCK_CODE_TYPE == StockCodeType.Direct).Include(x => x.PROJECT);
        }

        private Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> DEPARTMENTProjectionFunc()
        {
            return query => query.Where(x => x.NAME == BluePrintsResources.DefaultConstructionDepartment);
        }

        private Func<IRepositoryQuery<Data.PHASE>, IQueryable<Data.PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.INTERNAL_NUM == BluePrintsResources.WorkpackDefaultConstructionPhase);
        }

        private Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID && x.GUID_DEPARTMENT == defaultConstructionDEPARTMENT.GUID && x.COST_GROUP == CostGroup.Site);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadESTIMATION_DIRECT.PROJECT.GUID);
        }

        private Func<IRepositoryQuery<COMMODITY_CODE>, IQueryable<COMMODITY_CODE>> COMMODITY_CODEProjectionFunc()
        {
            return query => query.Where(x => (x.GUID_PROJECT == loadPROJECT.GUID));
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == ProgressType.Construct);
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>> P6_ASSIGNMENTProjectionFunc()
        {
            return
                query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEM>> ESTIMATION_DIRECT_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_ESTIMATION_DIRECT == loadESTIMATION_DIRECT.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == loadPROGRESS.GUID);
        }

        private Func<IRepositoryQuery<P6Data.PROJECT>, IQueryable<P6Data.PROJECT>> P6PROJECTProjectionFunc
            ()
        {
            string projectName;
            if (isFromPROGRESS)
                projectName = loadPROGRESS.P6PROGRESS_NAME;
            else if (mappingType == BaselineMappingSelectionType.Modified)
                projectName = loadESTIMATION_DIRECT.P6MODBASELINE_NAME;
            else
                projectName = loadESTIMATION_DIRECT.P6BASELINE_NAME;

            return query => query.Where(x => x.proj_short_name == projectName);
        }

        private Func<IRepositoryQuery<TASK>, IQueryable<TASK>> P6TASKProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id);
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> PROJWBSProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.ESTIMATION_DIRECT_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<ESTIMATION_DIRECT_ITEM>, IQueryable<ESTIMATION_DIRECT_ITEMProjection>>
            ConstructMainViewModelProjection()
        {
            IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = GetEntities<P6_ASSIGNMENT>();
            return query => ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Rates_Transformation(query, loaderCollection.GetCollection<RATE>(), STOCK_CODECollection, loaderCollection.GetCollection<COMMODITY_CODE>(), P6_ASSIGNMENTS);
        }

        //Used by deliverable scheduling view model to fix assignment
        public Func<object> OnEntitiesLoadedParameterCallBack;
        public Action<IEnumerable<ICanAssignP6>, object> OnEntitiesLoadedWithParameterCallBack;

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<ESTIMATION_DIRECT_ITEMProjection> entities)
        {
            if (OnEntitiesLoadedWithParameterCallBack != null)
            {
                object onLoadedParameter = OnEntitiesLoadedParameterCallBack?.Invoke();
                OnEntitiesLoadedWithParameterCallBack?.Invoke(entities, onLoadedParameter);

                //Self destruct after entities has been returned
                CleanUpEntitiesLoader();
                return;
            }

            Beg = P6TASKCollection.Where(x => x.target_start_date != null).Min(x => (DateTime)x.target_start_date);
            End = P6TASKCollection.Where(x => x.target_start_date != null).Max(x => (DateTime)x.target_end_date);

            VisBeg = new DateTime(Beg.Ticks);
            VisEnd = new DateTime(End.Ticks);

            SelBeg = new DateTime(Beg.Ticks);
            SelEnd = new DateTime(End.Ticks);
            
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void FullRefresh()
        {
            InitializeAndLoadEntitiesLoaderDescription();
        }
        #endregion


        #region Assignment View Properties
        protected IHaveP6Baselines p6_baseline_entity { get; set; }

        private GanttData selected_activity;
        public GanttData Selected_Activity { get => selected_activity; set { selected_activity = value; this.RaisePropertyChanged(x => x.Selected_Activity);  } }

        public IEnumerable<ICanAssignP6> Deliverables_Source => MainViewModel == null || MainViewModel.Entities == null ? null : MainViewModel.Entities;

        ObservableCollection<ICanAssignP6> selected_deliverables;
        public ObservableCollection<ICanAssignP6> Selected_Deliverables { get => selected_deliverables; set { selected_deliverables = value; this.RaisePropertyChanged(x => x.Selected_Deliverables); } }

        ICanAssignP6 selected_deliverable;
        public ICanAssignP6 Selected_Deliverable { get => selected_deliverable; set { selected_deliverable = value; this.RaisePropertyChanged(x => x.Selected_Deliverable); } }

        public P6_ASSIGNMENTProjection Selected_P6_Assignment { get; set; }
        public ObservableCollection<P6_ASSIGNMENTProjection> Selected_P6_Assignments { get; set; }

        List<P6_ASSIGNMENTProjection> p6_assignment;
        public IEnumerable<P6_ASSIGNMENTProjection> P6_Assignments
        {
            get
            {
                if (p6_assignment == null)
                {
                    p6_assignment = new List<P6_ASSIGNMENTProjection>();
                    IEnumerable<ICanAssignP6> process_deliverables = Selected_Deliverables.Count == 0 ? Deliverables_Source : Selected_Deliverables;
                    foreach (ICanAssignP6 process_deliverable in process_deliverables)
                    {
                        foreach (P6_ASSIGNMENT p6_assignments in process_deliverable.P6_Assignments)
                        {
                            if (Selected_Activity == null || p6_assignments.P6_ACTIVITYID == Selected_Activity.P6_ActivityId)
                                p6_assignment.Add(new P6_ASSIGNMENTProjection() { Deliverable_OriginalEntityKey = process_deliverable.OriginalEntityKey, Deliverable_Name = process_deliverable.Deliverable_Name, Entity = p6_assignments });
                        }
                    }
                }

                return p6_assignment.OrderBy(x => x.Deliverable_Name).ThenBy(x => x.Entity.LOW_VALUE);
            }
        }

        private void refresh_p6_assignments()
        {
            p6_assignment = null;
            this.RaisePropertyChanged(x => x.P6_Assignments);
        }

        private decimal assignment_value { get; set; }

        public decimal Assignment_Value
        {
            get { return assignment_value; }
            set
            {
                assignment_value = value;
                this.RaiseCanExecuteChanged(x => x.Add_Assignments());
            }
        }

        public decimal Assignment_MinValue
        {
            get
            {
                if (Selected_Deliverables == null || Selected_Deliverables.Count == 0)
                    return 0;

                decimal assigned_Percentage = Selected_Deliverables.Min(x => x.Assigned_Percentage);
                return assigned_Percentage > 1 ? 1 : assigned_Percentage;
            }
        }

        public decimal Assignment_MaxValue
        {
            get
            {
                return 1;
            }
        }
        #endregion

        #region Assignment Selected Items
        private void Selected_Deliverables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => SetMaxUnits()));
            refresh_p6_assignments();
        }

        /// <summary>
        /// Updates the total assigned percentages on each deliverable
        /// </summary>
        private void raise_deliverable_assignment_changes()
        {
            foreach (ICanAssignP6 deliverable in Deliverables_Source)
            {
                deliverable.Update();
            }

            refresh_p6_assignments();
        }
        #endregion

        #region Assignment View Commands
        public void SetMaxUnits()
        {
            Assignment_Value = Assignment_MaxValue;
            this.RaisePropertyChanged(x => x.Assignment_Value);
        }

        public bool CanSetMaxUnits()
        {
            return CanAdd_Assignments();
        }

        public Action<P6_ASSIGNMENTProjection> Set_SelectedItem_CallBack { get; set; }

        public void Add_Assignments()
        {
            foreach (ICanAssignP6 deliverable in Selected_Deliverables)
            {
                if (deliverable.Assigned_Percentage == Assignment_Value)
                    continue;

                deliverable.P6_Assignments.Add(new P6_ASSIGNMENT()
                {
                    GUID = Guid.Empty,
                    GUID_PROJECT = loadPROJECT.GUID,
                    HIGH_VALUE = Assignment_Value,
                    LOW_VALUE = deliverable.Assigned_Percentage + 0.01m,
                    P6_ACTIVITYID = Selected_Activity.P6_ActivityId,
                    GUID_ORIGINAL = deliverable.OriginalEntityKey,
                    ISMODIFIEDBASELINE = false
                });
            }

            P6_ASSIGNMENTSCollectionViewModel.BulkSave(Selected_Deliverables.SelectMany(x => x.P6_Assignments.Where(y => y.GUID == Guid.Empty)));
            SetMaxUnits();
            raise_deliverable_assignment_changes();
        }

        public bool CanAdd_Assignments()
        {
            if (Selected_Deliverables == null || Selected_Deliverables.Count == 0)
                return false;

            if (Selected_Activity == null)
                return false;

            return true;
        }

        public bool CanDelete_Assignments()
        {
            if (Selected_P6_Assignments == null || Selected_P6_Assignments.Count == 0)
                return false;

            return true;
        }

        public void Delete_Assignments()
        {
            if (Selected_P6_Assignments.Count == 0)
                return;

            remove_assignments(Selected_P6_Assignments.ToList());
            SetMaxUnits();
            raise_deliverable_assignment_changes();
        }

        private void remove_assignments(IEnumerable<P6_ASSIGNMENTProjection> remove_p6_assignments)
        {
            foreach (P6_ASSIGNMENTProjection remove_p6_assignment in remove_p6_assignments)
            {
                var low_value = remove_p6_assignment.Entity.LOW_VALUE;
                var active_deliverable = Deliverables_Source.FirstOrDefault(x => x.OriginalEntityKey == remove_p6_assignment.Deliverable_OriginalEntityKey);
                if (active_deliverable == null)
                    return;

                active_deliverable.P6_Assignments.RemoveAll(x => x.GUID == remove_p6_assignment.GUID);
                P6_ASSIGNMENTSCollectionViewModel.Delete(remove_p6_assignment.Entity);

                ObservableCollection<P6_ASSIGNMENT> p6_assignments_in_order = new ObservableCollection<P6_ASSIGNMENT>(active_deliverable.P6_Assignments.Where(x => x.LOW_VALUE > low_value).OrderBy(x => x.LOW_VALUE).ToList());

                for (var i = 0; i < p6_assignments_in_order.Count(); i++)
                {
                    var current_assignment_amount = p6_assignments_in_order[i].HIGH_VALUE - p6_assignments_in_order[i].LOW_VALUE;
                    p6_assignments_in_order[i].LOW_VALUE = low_value;
                    p6_assignments_in_order[i].HIGH_VALUE = low_value + current_assignment_amount;
                    low_value = p6_assignments_in_order[i].HIGH_VALUE + 0.01m;
                }

                P6_ASSIGNMENTSCollectionViewModel.BulkSave(p6_assignments_in_order);
            }
        }


        private IEnumerable<P6_ASSIGNMENT> MovePriority(bool isUp, P6_ASSIGNMENTProjection selected_p6_assignment, out P6_ASSIGNMENTProjection swap_selected_p6_assignment)
        {
            ICanAssignP6 context_deliverable = Deliverables_Source.First(x => x.OriginalEntityKey == selected_p6_assignment.Deliverable_OriginalEntityKey);
            P6_ASSIGNMENT context_p6_assignment = selected_p6_assignment.Entity;

            var p6_assignments_in_order =
                context_deliverable.P6_Assignments.OrderBy(x => x.LOW_VALUE).ToList();

            P6_ASSIGNMENT swap_p6_assignment;
            //look for next assignment in sequence
            if (!isUp)
                swap_p6_assignment = Selected_Deliverables.Where(x => x.EntityKey == context_deliverable.EntityKey)
                    .SelectMany(x => x.P6_Assignments).FirstOrDefault(x => x.LOW_VALUE == (context_p6_assignment.HIGH_VALUE + 0.01m));
            else
                swap_p6_assignment = Selected_Deliverables.Where(x => x.EntityKey == context_deliverable.EntityKey)
                    .SelectMany(x => x.P6_Assignments).FirstOrDefault(x => x.HIGH_VALUE == (context_p6_assignment.LOW_VALUE - 0.01m));

            if (swap_p6_assignment != null)
            {
                var swap_assignment_id = swap_p6_assignment.P6_ACTIVITYID;
                swap_p6_assignment.P6_ACTIVITYID = context_p6_assignment.P6_ACTIVITYID;
                context_p6_assignment.P6_ACTIVITYID = swap_assignment_id;
                swap_selected_p6_assignment = P6_Assignments.First(x => x.EntityKey == swap_p6_assignment.EntityKey);
                return p6_assignments_in_order;
            }

            swap_selected_p6_assignment = null;
            return null;
        }

        public bool CanPriorityUp()
        {
            if (Selected_P6_Assignment == null)
                return false;

            return true;
        }

        public bool CanPriorityDown()
        {
            if (Selected_P6_Assignment == null)
                return false;

            return true;
        }

        public void PriorityUp()
        {
            List<P6_ASSIGNMENT> save_assignments = new List<P6_ASSIGNMENT>();
            P6_ASSIGNMENTProjection swap_assignment_selection;
            IEnumerable<P6_ASSIGNMENT> edited_assignments = MovePriority(true, Selected_P6_Assignment, out swap_assignment_selection);
            if (edited_assignments != null)
                save_assignments.AddRange(edited_assignments);

            save_assignments_and_restore_selection(save_assignments, swap_assignment_selection);
        }

        public void PriorityDown()
        {
            List<P6_ASSIGNMENT> save_assignments = new List<P6_ASSIGNMENT>();
            P6_ASSIGNMENTProjection swap_assignment_selection;
            IEnumerable<P6_ASSIGNMENT> editedAssignments = MovePriority(false, Selected_P6_Assignment, out swap_assignment_selection);
            if (editedAssignments != null)
                save_assignments.AddRange(editedAssignments);

            save_assignments_and_restore_selection(save_assignments, swap_assignment_selection);
        }

        private void save_assignments_and_restore_selection(IEnumerable<P6_ASSIGNMENT> edited_assignments, P6_ASSIGNMENTProjection swap_assignment_selection)
        {
            P6_ASSIGNMENTProjection store_selected_p6_assignment = Selected_P6_Assignment;
            P6_ASSIGNMENTSCollectionViewModel.BulkSave(new ObservableCollection<P6_ASSIGNMENT>(edited_assignments));
            Selected_P6_Assignments.Clear();
            Selected_P6_Assignments.Add(swap_assignment_selection);
            Selected_P6_Assignment = swap_assignment_selection;
            this.RaisePropertyChanged(x => x.Selected_P6_Assignment);
            this.RaisePropertyChanged(x => x.Selected_P6_Assignments);
        }

        public bool CanLookUpAssignment_Activity()
        {
            return Selected_P6_Assignment != null;
        }

        public void LookUpAssignment_Activity()
        {
            if (Selected_P6_Assignment == null)
                return;

            GanttData activity = Activities_Source.FirstOrDefault(x => x.P6_ActivityId == Selected_P6_Assignment.Entity.P6_ACTIVITYID);
            if (activity != null)
                Selected_Activity = activity;
        }

        /// <summary>
        /// Don't allow users to choose WBS items
        /// </summary>
        public void lookupActivity_EditValueChanging(EditValueChangingEventArgs e)
        {
            if (e.NewValue != null)
            {
                var changingValue = (GanttData)e.NewValue;
                if (changingValue.ActivityType != AppointmentActivityType.Activity)
                {
                    e.IsCancel = true;
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Assignment Interaction
        public void Deliverables_MouseDown(MouseButtonEventArgs e)
        {
            Selected_Activity = null;
            refresh_p6_assignments();
        }

        public void Activities_MouseDown(MouseButtonEventArgs e)
        {
            Selected_Deliverables.Clear();
            refresh_p6_assignments();
        }

        private void select_all_deliverables()
        {
            Selected_Deliverables.Clear();
            foreach (ICanAssignP6 contextDeliverable in MainViewModel.Entities)
                Selected_Deliverables.Add(contextDeliverable);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "ESTIMATION_DIRECT_ITEMSchedulingViewModelWrapper"; }
        }

        private IDialogService P6_Assignment_DialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("P6_Assignment_Dialog"); }
        }

        public IEnumerable<TASK> P6TASKCollection
        {
            get
            {
                var collection = GetEntities<TASK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.task_name);
                return collection;
            }
        }

        public IEnumerable<PROJWBS> P6PROJWBSCollection
        {
            get
            {
                var collection = GetEntities<PROJWBS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.wbs_name);
                return collection;
            }
        }

        public IEnumerable<Data.PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<Data.PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
                return collection;
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

        public IEnumerable<COMMODITY_CODE> ProjectCOMMODITY_CODECollection
        {
            get
            {
                if (loadPROJECT == null)
                    return null;

                return COMMODITY_CODECollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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

        public ICollectionViewModel<BluePrints.P6Data.TASK> P6TASKCollectionViewModel
        {
            get { return (ICollectionViewModel<BluePrints.P6Data.TASK>)loaderCollection.GetViewModel<TASK>(); }
        }

        public CollectionViewModel<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> ESTIMATION_DIRECT_ITEMSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<ESTIMATION_DIRECT_ITEM, ESTIMATION_DIRECT_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<ESTIMATION_DIRECT_ITEM>();
            }
        }

        public CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> P6_ASSIGNMENTSCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<P6_ASSIGNMENT>();
            }
        }

        public void PushToP6()
        {
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string ProjectName;
            if (mappingType == BaselineMappingSelectionType.Modified)
                ProjectName = p6_baseline_entity.P6_Baseline_Name;
            else
                ProjectName = p6_baseline_entity.P6_Mod_Baseline_Name;


            BluePrints.P6Data.PROJECT P6PROJECT = IP6EntitiesUnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == ProjectName && x.delete_date == null);
            if (P6PROJECT != null)
            {
                IEnumerable<TASK> P6Tasks = P6PROJECT.TASK.ToArray().AsEnumerable();
                foreach (TASK Task in P6Tasks)
                {
                    Task.act_work_qty = 0;
                    Task.remain_work_qty = 0;
                    Task.target_work_qty = 0;
                }

                IEnumerable<TASKRSRC> ExistingTaskResource = P6PROJECT.TASKRSRC.ToArray().AsEnumerable();

                double taskrsrcCount = ExistingTaskResource.Count();
                foreach (var TaskRsrc in ExistingTaskResource)
                {
                    IP6EntitiesUnitOfWork.TASKRSRC.Remove(TaskRsrc);
                }

                List<P6ActivityAssignment> missing_activities = new List<P6ActivityAssignment>();
                foreach (ICanAssignP6 deliverable in Deliverables_Source)
                {
                    IEnumerable<P6_ASSIGNMENT> deliverable_assignments = deliverable.P6_Assignments;

                    foreach (P6_ASSIGNMENT deliverable_assignment in deliverable_assignments)
                    {
                        TASK existingTask = P6Tasks.FirstOrDefault(x => x.task_code == deliverable_assignment.P6_ACTIVITYID);
                        P6ActivityAssignment p6_assignment = new P6ActivityAssignment((IDeliverable_Rates)deliverable, deliverable_assignment);

                        if (existingTask != null && existingTask.delete_date == null)
                        {
                            existingTask.target_work_qty += p6_assignment.UNITS;
                            existingTask.remain_work_qty += p6_assignment.UNITS;
                        }
                        else
                        {
                            missing_activities.Add(p6_assignment);
                        }
                    }
                }

                ((P6EntitiesUnitOfWork)IP6EntitiesUnitOfWork).Context.SaveChanges();
                if (missing_activities.Count > 0)
                {
                    DialogCollectionViewModel<P6ActivityAssignment> missing_activities_viewmodel = DialogCollectionViewModel<P6ActivityAssignment>.Create(missing_activities);
                    ActivityDetailDialogService.ShowDialog(MessageButton.OK, "Missing P6 Activities", "MissingAssignments", missing_activities_viewmodel);
                }
                else
                    MessageBoxService.ShowMessage(BluePrintsResources.P6AssignmentWriteComplete);
            }
        }

        public void Remap_P6_Ids()
        {
            IEnumerable<TASK> valid_tasks;
            List<P6ActivityAssignment> missing_activities = get_missing_p6_activities(out valid_tasks, true);
            List<P6ActivityRemap> p6_remap_activities = new List<P6ActivityRemap>();

            if (missing_activities.Count > 0)
            {
                foreach (P6ActivityAssignment missing_activity in missing_activities)
                {
                    if (!p6_remap_activities.Any(x => x.P6_OLD_ACTIVITY == missing_activity.P6_ACTIVITY))
                        p6_remap_activities.Add(ViewModelSource.Create(() => new P6ActivityRemap() { P6_OLD_ACTIVITY = missing_activity.P6_ACTIVITY }));
                }

                P6ActivityAssignmentDialogViewModel<P6ActivityRemap> activities_remap_viewmodel = P6ActivityAssignmentDialogViewModel<P6ActivityRemap>.CreateViewModel(p6_remap_activities, loadPROJECT.NUMBER, valid_tasks);
                if (ActivityDetailDialogService.ShowDialog(MessageButton.OKCancel, "Re-Assign", "MissingAssignmentsRemap", activities_remap_viewmodel) == MessageResult.OK)
                {
                    IEnumerable<P6ActivityRemap> user_remapped_activities = p6_remap_activities.Where(x => x.P6_NEW_ACTIVITY != null && x.P6_NEW_ACTIVITY != string.Empty);
                    List<P6ActivityRemap> valid_user_remapeed_activities = new List<P6ActivityRemap>();

                    foreach (P6ActivityRemap userRemappedActivity in user_remapped_activities)
                    {
                        if (valid_tasks.Any(x => x.task_code == userRemappedActivity.P6_NEW_ACTIVITY))
                        {
                            valid_user_remapeed_activities.Add(userRemappedActivity);
                        }
                    }

                    List<P6ActivityAssignment> reassign_activities = new List<P6ActivityAssignment>();
                    if (user_remapped_activities.Count() > 0)
                    {
                        List<P6ActivityAssignment> valid_reassignments = new List<P6ActivityAssignment>();
                        foreach (P6ActivityAssignment missing_activity in missing_activities)
                        {
                            P6ActivityRemap user_remapped_activity = valid_user_remapeed_activities.FirstOrDefault(x => x.P6_OLD_ACTIVITY == missing_activity.P6_ACTIVITY);
                            if (user_remapped_activity != null)
                            {
                                missing_activity.Reassign(user_remapped_activity.P6_NEW_ACTIVITY);
                                reassign_activities.Add(missing_activity);
                            }
                        }
                    }

                    if (reassign_activities.Count > 0)
                    {
                        P6_ASSIGNMENTSCollectionViewModel.BulkSave(reassign_activities.Select(x => x.deliverable_assignment));
                        MessageBoxService.ShowMessage(reassign_activities.Count + " activities re-assigned");
                    }
                }
            }
            else
                MessageBoxService.ShowMessage("All Assignments Valid");
        }

        public void Check_Assignments()
        {
            IEnumerable<TASK> valid_tasks;
            List<P6ActivityAssignment> missing_activities = get_missing_p6_activities(out valid_tasks);
            if (missing_activities.Count > 0)
            {
                DialogCollectionViewModel<P6ActivityAssignment> missing_activities_viewmodel = DialogCollectionViewModel<P6ActivityAssignment>.Create(missing_activities);
                ActivityDetailDialogService.ShowDialog(MessageButton.OK, "Invalid Assignments", "MissingAssignments", missing_activities_viewmodel);

                if (MessageBoxService.ShowMessage("Do you wish to delete these invalid assignments?", "Warning", MessageButton.OKCancel) == MessageResult.OK)
                {
                    P6_ASSIGNMENTSCollectionViewModel.BaseBulkDelete(missing_activities.Select(x => x.deliverable_assignment));
                    FullRefresh();
                }
            }
            else
                MessageBoxService.ShowMessage("All Assignments Valid");
        }

        private List<P6ActivityAssignment> get_missing_p6_activities(out IEnumerable<TASK> valid_tasks, bool getAllActivities = false)
        {
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string project_name;
            if (mappingType == BaselineMappingSelectionType.Modified)
                project_name = p6_baseline_entity.P6_Baseline_Name;
            else
                project_name = p6_baseline_entity.P6_Mod_Baseline_Name;

            List<P6ActivityAssignment> missing_activities = new List<P6ActivityAssignment>();
            BluePrints.P6Data.PROJECT P6PROJECT = IP6EntitiesUnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == project_name && x.delete_date == null);
            if (P6PROJECT != null)
            {
                valid_tasks = P6PROJECT.TASK.ToArray().AsEnumerable();
                foreach (ICanAssignP6 deliverable in Deliverables_Source)
                {
                    IEnumerable<P6_ASSIGNMENT> deliverable_assignments = deliverable.P6_Assignments;
                    foreach (P6_ASSIGNMENT deliverable_assignment in deliverable_assignments)
                    {
                        if (getAllActivities)
                            missing_activities.Add(new P6ActivityAssignment((IDeliverable_Rates)deliverable, deliverable_assignment));
                        else
                        {
                            TASK existingTask = valid_tasks.FirstOrDefault(x => x.task_code == deliverable_assignment.P6_ACTIVITYID);
                            if (existingTask == null || existingTask.delete_date != null)
                            {
                                missing_activities.Add(new P6ActivityAssignment((IDeliverable_Rates)deliverable, deliverable_assignment));
                            }
                        }
                    }
                }
            }
            else
                valid_tasks = null;

            return missing_activities;
        }

        public void Refresh()
        {
            //RefreshWinformView?.Invoke();
        }
        #endregion

        #region DragDrop
        public void Scheduler_Drop(TreeListDropEventArgs e)
        {
            e.Handled = true;
        }

        public void Scheduler_Dropped(TreeListDroppedEventArgs e)
        {
            IEnumerable<ICanAssignP6> dropped_deliverables = ((IEnumerable<object>)e.DraggedRows).Select(x => (ICanAssignP6)x).AsEnumerable();
            GanttData target_activity = (GanttData)e.TargetNode.Content;

            if(target_activity.ActivityType != AppointmentActivityType.Activity)
            {
                MessageBoxService.ShowMessage("Cannot assign to " + target_activity.ActivityType.ToString(), "Error", MessageButton.OK, MessageIcon.Error);
                return;
            }

            Selected_Activity = target_activity;
            Selected_Deliverables.Clear();
            foreach (ICanAssignP6 contextDeliverable in dropped_deliverables)
                Selected_Deliverables.Add(contextDeliverable);
        }
        #endregion

        #region GanttChart Properties
        private List<GanttData> activities_source;
        public virtual List<GanttData> Activities_Source
        {
            get
            {
                if(MainViewModel != null && activities_source == null)
                {
                    activities_source = new List<GanttData>();
                    activities_source.AddRange(P6TASKCollection.OrderBy(x => x.target_start_date).Select(x => GanttData.Create(x, this)).ToArray().AsEnumerable());
                    activities_source.AddRange(P6PROJWBSCollection.Select(x => GanttData.Create(x, this)).ToArray().AsEnumerable());
                    summarizeActivities(activities_source);
                }

                return activities_source;
            }
        }

        private void summarizeActivities(IEnumerable<GanttData> activities)
        {
            foreach (var activity in activities)
            {
                if (activity.ActivityType == AppointmentActivityType.WBS)
                {
                    activity.AssignedUnits = 0;
                }
            }

            foreach (var activity in activities)
                if (activity.ActivityType == AppointmentActivityType.WBS)
                {
                    List<GanttData> allChildrenActivities = new List<GanttData>();
                    getAllChildrens(activities, activity, allChildrenActivities);
                    //return childTASKInfos.Sum(x => x.AssignedUnits);
                    if (allChildrenActivities.Count() != 0)
                    {
                        activity.Start = allChildrenActivities.Min(x => x.Start);
                        activity.End = allChildrenActivities.Max(x => x.End);
                    }
                }
        }

        private void getAllChildrens(IEnumerable<GanttData> allActivities, GanttData parentActivity, List<GanttData> childrenCollection)
        {
            IEnumerable<GanttData> childActivities = allActivities.Where(x => x.ParentId == parentActivity.Id);

            if(childActivities.Count() > 0)
                parentActivity.WBSLevel += 1;

            foreach (var childActivity in childActivities)
            {
                childrenCollection.Add(childActivity);
                getAllChildrens(allActivities, childActivity, childrenCollection);
            }
        }

        public virtual DateTime Beg { get; set; }
        public virtual DateTime End { get; set; }
        public virtual DateTime VisBeg { get; set; }
        public virtual DateTime VisEnd { get; set; }
        public virtual DateTime SelBeg { get; set; }
        public virtual DateTime SelEnd { get; set; }
        public virtual double CanvasWidth { get; set; }

        public void NodeExpanded()
        {
            Task.Factory
                .StartNew(() => Thread.Sleep(100))
                .ContinueWith(t => { this.RaisePropertyChanged(x => x.SelBeg); });
        }
        #endregion
    }
}