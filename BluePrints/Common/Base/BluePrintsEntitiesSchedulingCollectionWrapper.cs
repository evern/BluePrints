using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BluePrints.Common.Base
{
    public interface IEntitiesSchedulingCollectionWrapper
    {
        Action<IEnumerable<ICanAssignP6>> OnViewModelLoaded { get; set; }
        IEnumerable<TASK> TASK_Source { get; }
        void Save_Task(TASK task);
    }

    public abstract class BluePrintsEntitiesSchedulingCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : BluePrintsEntitiesCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>, IHaveCanvasWidth, IEntitiesSchedulingCollectionWrapper
        where TMainEntity : class, IGuidEntityKey, new()
        where TMainProjectionEntity : class, IGuidEntityKey, ICanAssignP6, new()
        where TMainEntityUnitOfWork : IUnitOfWork
    {
        protected IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESSES, PROGRESSProjectionFunc, x => live_PROGRESS = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS, PROGRESS_ITEMProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.P6_ASSIGNMENTS, P6_ASSIGNMENTProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJECT, P6PROJECTProjectionFunc, x => loadP6PROJECT = x);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.TASK, P6TASKProjectionFunc);
            loaderCollection.AddLoaderDescription(p6UnitOfWorkFactory, x => x.PROJWBS, PROJWBSProjectionFunc);

            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        protected abstract ProgressType progress_type { get; }

        private Func<IRepositoryQuery<P6Data.PROJECT>, IQueryable<P6Data.PROJECT>> P6PROJECTProjectionFunc()
        {
            string projectName;
            if (isFromPROGRESS)
                projectName = live_PROGRESS.P6PROGRESS_NAME;
            else if (mappingType == BaselineMappingSelectionType.Modified)
                projectName = p6_baseline_entity.P6_Mod_Baseline_Name;
            else
                projectName = p6_baseline_entity.P6_Baseline_Name;

            return query => query.Where(x => x.proj_short_name == projectName);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == WorkpackType.SiteDirect);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<TASK>, IQueryable<TASK>> P6TASKProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id).Where(x => x.TASKACTV.Count > 0).Where(x => x.TASKACTV.Any(taskact => taskact.ACTVCODE != null && taskact.ACTVCODE.actv_code_name.ToUpper() == progress_type.ToString().ToUpper()));
        }

        private Func<IRepositoryQuery<PROJWBS>, IQueryable<PROJWBS>> PROJWBSProjectionFunc()
        {
            return query => query.Where(x => x.proj_id == loadP6PROJECT.proj_id);
        }

        private Func<IRepositoryQuery<PROGRESS_ITEM>, IQueryable<PROGRESS_ITEM>> PROGRESS_ITEMProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROGRESS == live_PROGRESS.GUID);
        }

        private Func<IRepositoryQuery<PROGRESS>, IQueryable<PROGRESS>> PROGRESSProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.STATUS == ProgressStatus.Live && x.TYPE == progress_type);
        }

        private Func<IRepositoryQuery<P6_ASSIGNMENT>, IQueryable<P6_ASSIGNMENT>> P6_ASSIGNMENTProjectionFunc()
        {
            return
                query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.TYPE == progress_type);
        }
        #region Used as Dependency Delegate
        public Action<IEnumerable<ICanAssignP6>> OnViewModelLoaded { get; set; }

        protected bool isFromPROGRESS
        {
            get { return OnViewModelLoaded != null; }
        }
        #endregion

        protected P6Data.PROJECT loadP6PROJECT;
        protected PROGRESS live_PROGRESS;
        protected IHaveP6Baselines p6_baseline_entity { get; set; }
        protected Data.PROJECT loadPROJECT;
        protected BaselineMappingSelectionType mappingType;

        protected IDialogService ActivityDetailDialogService
        {
            get { return this.GetRequiredService<IDialogService>("ActivityIdDialog"); }
        }

        protected override void InitializeParameters(object parameter)
        {
            var obj = (object[])parameter;

            if (isFromPROGRESS)
                live_PROGRESS = (PROGRESS)obj[0];
            else
                p6_baseline_entity = (IHaveP6Baselines)obj[0];

            mappingType = (BaselineMappingSelectionType)obj[1];

            Selected_Deliverables = new ObservableCollection<ICanAssignP6>();
            Selected_P6_Assignments = new ObservableCollection<P6_ASSIGNMENTProjection>();
            Selected_Deliverables.CollectionChanged += Selected_Deliverables_CollectionChanged;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<TMainProjectionEntity> entities)
        {
            if (isFromPROGRESS)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => OnViewModelLoaded(entities)));
                return;
            }

            if (Activities_Source.Count > 0)
            {
                IEnumerable<P6_Activity> activities_with_start_date = Activities_Source.Where(x => x.Start != null);
                IEnumerable<P6_Activity> activities_with_end_date = Activities_Source.Where(x => x.End != null);

                if(activities_with_start_date.Count() > 0 && activities_with_end_date.Count() > 0)
                {
                    Beg = activities_with_start_date.Min(x => (DateTime)x.Start);
                    End = activities_with_end_date.Max(x => (DateTime)x.End);

                    VisBeg = new DateTime(Beg.Ticks);
                    VisEnd = new DateTime(End.Ticks);

                    SelBeg = new DateTime(Beg.Ticks);
                    SelEnd = new DateTime(End.Ticks);
                }
            }

            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => summarize_activities_units(Activities_Source, entities)));
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void FullRefresh()
        {
            InitializeAndLoadEntitiesLoaderDescription();
        }

        #region View Refreshing
        private void summarize_activities_dates(IEnumerable<P6_Activity> activities, bool trim = false)
        {
            List<P6_Activity> remove_activities = new List<P6_Activity>();
            foreach (var activity in activities)
                if (activity.ActivityType == AppointmentActivityType.WBS)
                {
                    List<P6_Activity> allChildrenActivities = new List<P6_Activity>();
                    get_all_childrens(activities, activity, allChildrenActivities);
                    //return childTASKInfos.Sum(x => x.AssignedUnits);
                    if (allChildrenActivities.Count() != 0)
                    {
                        activity.Start = allChildrenActivities.Min(x => x.Start);
                        activity.End = allChildrenActivities.Max(x => x.End);
                    }
                    else if (trim)
                        remove_activities.Add(activity);
                }

            if (trim)
            {
                foreach (P6_Activity activity in remove_activities)
                    activities_source.Remove(activity);
            }
        }

        private void get_all_childrens(IEnumerable<P6_Activity> allActivities, P6_Activity parentActivity, List<P6_Activity> childrenCollection)
        {
            IEnumerable<P6_Activity> childActivities = allActivities.Where(x => x.ParentId == parentActivity.Id);

            if (childActivities.Count() > 0)
                parentActivity.WBSLevel += 1;

            foreach (var childActivity in childActivities)
            {
                if (childActivity.ActivityType == AppointmentActivityType.Activity)
                    childrenCollection.Add(childActivity);

                get_all_childrens(allActivities, childActivity, childrenCollection);
            }
        }

        protected void summarize_activities_units(IEnumerable<P6_Activity> activities, IEnumerable<ICanAssignP6> deliverables)
        {
            //first we calculate activity level
            foreach (var activity in activities.Where(x => x.ActivityType == AppointmentActivityType.Activity))
            {
                decimal total_activity_assigned_units = deliverables.Sum(x => x.P6_Assignments.Where(assignment => assignment.P6_ACTIVITYID == activity.P6_ActivityId)
                                                        .Sum(assignment => ((assignment.HIGH_VALUE - assignment.LOW_VALUE) + 0.01m) * x.Total_Units));
                activity.Assigned_Units = total_activity_assigned_units;
                activity.RaisePropertiesChanged();
            }

            summarize_wbs_units(Activities_Source);
        }

        protected void summarize_wbs_parent_unit(P6_Activity activity)
        {
            if (activity == null)
                return;

            decimal total_activity_assigned_units = Deliverables_Source.Sum(x => x.P6_Assignments.Where(assignment => assignment.P6_ACTIVITYID == activity.P6_ActivityId)
                                                    .Sum(assignment => ((assignment.HIGH_VALUE - assignment.LOW_VALUE) + 0.01m) * x.Total_Units));

            activity.Assigned_Units = total_activity_assigned_units;
            activity.RaisePropertiesChanged();
            IEnumerable<P6_Activity> activity_parents = get_activity_wbs_parents(activity);
            summarize_wbs_units(activity_parents);
        }

        protected IEnumerable<P6_Activity> get_activity_wbs_parents(P6_Activity activity)
        {
            P6_Activity activity_parent = Activities_Source.FirstOrDefault(x => x.Id == activity.ParentId);
            if (activity_parent != null)
            {
                yield return activity_parent;
                foreach (P6_Activity activity_recurse_parent in get_activity_wbs_parents(activity_parent))
                    yield return activity_recurse_parent;
            }
        }

        //Recurse childrens to sum budgeted units
        protected void summarize_wbs_units(IEnumerable<P6_Activity> activities)
        {
            //reset wbs total units
            foreach (var activity in activities)
            {
                if (activity.ActivityType == AppointmentActivityType.WBS)
                    activity.Assigned_Units = 0;
            }

            foreach (var activity in activities)
                if (activity.ActivityType == AppointmentActivityType.WBS)
                {
                    List<P6_Activity> iteration_activities = new List<P6_Activity>();
                    recurse_collect_child_activities(Activities_Source, activity, iteration_activities);
                    activity.Assigned_Units = iteration_activities.Sum(x => x.Assigned_Units);
                    activity.RaisePropertiesChanged();
                }
        }

        protected void recurse_collect_child_activities(IEnumerable<P6_Activity> child_activities, P6_Activity parent_activity, List<P6_Activity> iteration_activities)
        {
            IEnumerable<P6_Activity> current_parent_child_activities = child_activities.Where(x => x.ParentId == parent_activity.Id);

            foreach (P6_Activity child_activity in current_parent_child_activities)
            {
                if (child_activity.ActivityType == AppointmentActivityType.Activity)
                    iteration_activities.Add(child_activity);

                recurse_collect_child_activities(child_activities, child_activity, iteration_activities);
            }
        }
        #endregion

        #region Assignment View Properties
        private P6_Activity selected_activity;
        public P6_Activity Selected_Activity { get => selected_activity; set { selected_activity = value; this.RaisePropertyChanged(x => x.Selected_Activity); } }

        public abstract IEnumerable<ICanAssignP6> Deliverables_Source { get; }

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
                if (MainViewModel == null)
                    return new List<P6_ASSIGNMENTProjection>();

                if (p6_assignment == null)
                {
                    p6_assignment = new List<P6_ASSIGNMENTProjection>();
                    IEnumerable<ICanAssignP6> process_deliverables = (Selected_Deliverables == null || Selected_Deliverables.Count == 0) ? Deliverables_Source : Selected_Deliverables;
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
            this.RaisePropertyChanged(x => x.Assignment_MinValue);
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
            bool show_already_assigned_message = false;
            foreach (ICanAssignP6 deliverable in Selected_Deliverables)
            {
                if (deliverable.Assigned_Percentage == Assignment_Value)
                {
                    show_already_assigned_message = true;
                    continue;
                }

                deliverable.P6_Assignments.Add(new P6_ASSIGNMENT()
                {
                    GUID = Guid.Empty,
                    GUID_PROJECT = loadPROJECT.GUID,
                    HIGH_VALUE = Assignment_Value,
                    LOW_VALUE = deliverable.Assigned_Percentage + 0.01m,
                    P6_ACTIVITYID = Selected_Activity.P6_ActivityId,
                    GUID_ORIGINAL = deliverable.OriginalEntityKey,
                    TYPE = progress_type,
                    ISMODIFIEDBASELINE = false
                });
            }

            if(show_already_assigned_message)
                MessageBoxService.ShowMessage("Current percentage is already assigned to an activity");

            summarize_wbs_parent_unit(Selected_Activity);
            IEnumerable<P6_ASSIGNMENT> save_assignments = Selected_Deliverables.SelectMany(x => x.P6_Assignments.Where(y => y.GUID == Guid.Empty));
            foreach(P6_ASSIGNMENT save_assignment in save_assignments)
            {
                P6_ASSIGNMENTSCollectionViewModel.EntitiesUndoRedoManager.AddUndo(save_assignment, null, null, null, EntityMessageType.Added);
            }

            P6_ASSIGNMENTSCollectionViewModel.BulkSave(save_assignments);
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
            List<string> affected_activity_ids = new List<string>();

            foreach (P6_ASSIGNMENTProjection remove_p6_assignment in remove_p6_assignments)
            {
                affected_activity_ids.Add(remove_p6_assignment.Entity.P6_ACTIVITYID);
                var low_value = remove_p6_assignment.Entity.LOW_VALUE;
                var active_deliverable = Deliverables_Source.FirstOrDefault(x => x.OriginalEntityKey == remove_p6_assignment.Deliverable_OriginalEntityKey);
                if (active_deliverable == null)
                    return;

                active_deliverable.P6_Assignments.RemoveAll(x => x.GUID == remove_p6_assignment.GUID);
                P6_ASSIGNMENTSCollectionViewModel.Delete(remove_p6_assignment.Entity);

                ObservableCollection<P6_ASSIGNMENT> p6_assignments_in_order = new ObservableCollection<P6_ASSIGNMENT>(active_deliverable.P6_Assignments.Where(x => x.LOW_VALUE > low_value).OrderBy(x => x.LOW_VALUE).ToList());
                for (var i = 0; i < p6_assignments_in_order.Count(); i++)
                {
                    affected_activity_ids.Add(p6_assignments_in_order[i].P6_ACTIVITYID);
                    var current_assignment_amount = p6_assignments_in_order[i].HIGH_VALUE - p6_assignments_in_order[i].LOW_VALUE;
                    p6_assignments_in_order[i].LOW_VALUE = low_value;
                    p6_assignments_in_order[i].HIGH_VALUE = low_value + current_assignment_amount;
                    low_value = p6_assignments_in_order[i].HIGH_VALUE + 0.01m;
                }

                foreach (P6_Activity activity in Activities_Source.Where(x => affected_activity_ids.Any(str => str == x.P6_ActivityId)))
                    summarize_wbs_parent_unit(activity);

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
            IEnumerable<string> edited_activities_id = edited_assignments.Select(x => x.P6_ACTIVITYID);
            IEnumerable<P6_Activity> edited_activities = Activities_Source.Where(x => edited_activities_id.Any(str => str == x.P6_ActivityId));
            foreach (P6_Activity edited_activity in edited_activities)
                summarize_wbs_parent_unit(edited_activity);

            P6_ASSIGNMENTSCollectionViewModel.BulkSave(new ObservableCollection<P6_ASSIGNMENT>(edited_assignments));
            Selected_P6_Assignments.Clear();
            Selected_P6_Assignments.Add(swap_assignment_selection);
            Selected_P6_Assignment = swap_assignment_selection;
            this.RaisePropertyChanged(x => x.Selected_P6_Assignment);
            this.RaisePropertyChanged(x => x.Selected_P6_Assignments);
        }

        /// <summary>
        /// Don't allow users to choose WBS items
        /// </summary>
        public void lookupActivity_EditValueChanging(EditValueChangingEventArgs e)
        {
            if (e.NewValue != null)
            {
                var changingValue = (P6_Activity)e.NewValue;
                if (changingValue.ActivityType != AppointmentActivityType.Activity)
                {
                    MessageBoxService.ShowMessage("WBS or Milestones are not permitted for assignment, please choose another activity");
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

        public void P6_Assignment_MouseDown(MouseButtonEventArgs e)
        {
            Selected_Deliverable = Deliverables_Source.FirstOrDefault(x => x.OriginalEntityKey == Selected_P6_Assignment.Deliverable_OriginalEntityKey);
            Selected_Activity = Activities_Source.FirstOrDefault(x => x.P6_ActivityId == Selected_P6_Assignment.Entity.P6_ACTIVITYID);
        }

        private void select_all_deliverables()
        {
            Selected_Deliverables.Clear();
            foreach (ICanAssignP6 contextDeliverable in MainViewModel.Entities)
                Selected_Deliverables.Add(contextDeliverable);
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
            P6_Activity target_activity = (P6_Activity)e.TargetNode.Content;

            if (target_activity.ActivityType != AppointmentActivityType.Activity)
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
        private List<P6_Activity> activities_source;
        public virtual List<P6_Activity> Activities_Source
        {
            get
            {
                if (MainViewModel != null && activities_source == null)
                {
                    activities_source = new List<P6_Activity>();
                    if(P6TASKCollection.Count() > 0)
                        activities_source.AddRange(P6TASKCollection.OrderBy(x => x.target_start_date).Select(x => P6_Activity.Create(x, this)).ToArray().AsEnumerable());

                    if(P6PROJWBSCollection.Count() > 0)
                        activities_source.AddRange(P6PROJWBSCollection.OrderBy(x => x.wbs_short_name).Select(x => P6_Activity.Create(x, this)).ToArray().AsEnumerable());

                    summarize_activities_dates(activities_source, true);
                }

                return activities_source;
            }
        }

        public IEnumerable<TASK> TASK_Source
        {
            get
            {
                if (Activities_Source != null)
                    return Activities_Source.Where(x => x.IsTask).Select(x => x.Task);
                else
                    return new List<TASK>();
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


        #region P6 Interaction
        public void PushToP6()
        {
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string ProjectName;
            if (mappingType == BaselineMappingSelectionType.Modified)
                ProjectName = p6_baseline_entity.P6_Mod_Baseline_Name;
            else
                ProjectName = p6_baseline_entity.P6_Baseline_Name;

            P6Data.PROJECT P6_PROJECT = IP6EntitiesUnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == ProjectName && x.delete_date == null);

            IEnumerable<TASK> actual_tasks = P6_PROJECT.TASK.Where(x => TASK_Source.Any(task => task.task_code == x.task_code)).AsEnumerable();
            foreach (TASK Task in actual_tasks)
            {
                Task.act_work_qty = 0;
                Task.remain_work_qty = 0;
                Task.target_work_qty = 0;
            }

            IEnumerable<TASKRSRC> ExistingTaskResource = P6_PROJECT.TASKRSRC.ToArray().AsEnumerable();

            double taskrsrcCount = ExistingTaskResource.Count();
            foreach (var TaskRsrc in ExistingTaskResource)
            {
                IP6EntitiesUnitOfWork.TASKRSRC.Remove(TaskRsrc);
            }

            List<P6_AssignmentProjection> missing_activities = new List<P6_AssignmentProjection>();
            foreach (ICanAssignP6 deliverable in Deliverables_Source)
            {
                IEnumerable<P6_ASSIGNMENT> deliverable_assignments = deliverable.P6_Assignments;
                foreach (P6_ASSIGNMENT deliverable_assignment in deliverable_assignments)
                {
                    TASK actual_context_task = actual_tasks.FirstOrDefault(x => x.task_code == deliverable_assignment.P6_ACTIVITYID);
                    P6_AssignmentProjection p6_assignment = new P6_AssignmentProjection((IDeliverable_Rates)deliverable, deliverable_assignment);

                    if (actual_context_task != null && actual_context_task.delete_date == null)
                    {
                        actual_context_task.target_work_qty += p6_assignment.UNITS;
                        actual_context_task.remain_work_qty += p6_assignment.UNITS;
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
                DialogCollectionViewModel<P6_AssignmentProjection> missing_activities_viewmodel = DialogCollectionViewModel<P6_AssignmentProjection>.Create(missing_activities);
                ActivityDetailDialogService.ShowDialog(MessageButton.OK, "Missing P6 Activities", "MissingAssignments", missing_activities_viewmodel);
            }
            else
                MessageBoxService.ShowMessage(BluePrintsResources.Notify_P6_Assignment_Write_Complete);
        }

        public void Remap_P6_Ids()
        {
            List<P6_AssignmentProjection> missing_activities = get_missing_p6_activities(true);
            List<P6ActivityRemap> p6_remap_activities = new List<P6ActivityRemap>();

            if (missing_activities.Count > 0)
            {
                foreach (P6_AssignmentProjection missing_activity in missing_activities)
                {
                    if (!p6_remap_activities.Any(x => x.P6_OLD_ACTIVITY == missing_activity.P6_ACTIVITY))
                        p6_remap_activities.Add(ViewModelSource.Create(() => new P6ActivityRemap() { P6_OLD_ACTIVITY = missing_activity.P6_ACTIVITY }));
                }

                P6ActivityAssignmentDialogViewModel<P6ActivityRemap> activities_remap_viewmodel = P6ActivityAssignmentDialogViewModel<P6ActivityRemap>.CreateViewModel(p6_remap_activities, loadPROJECT.NUMBER, Activities_Source.Where(x => x.IsTask).Select(x => x.Task));
                if (ActivityDetailDialogService.ShowDialog(MessageButton.OKCancel, "Re-Assign", "MissingAssignmentsRemap", activities_remap_viewmodel) == MessageResult.OK)
                {
                    IEnumerable<P6ActivityRemap> user_remapped_activities = p6_remap_activities.Where(x => x.P6_NEW_ACTIVITY != null && x.P6_NEW_ACTIVITY != string.Empty);
                    List<P6ActivityRemap> valid_user_remapeed_activities = new List<P6ActivityRemap>();

                    foreach (P6ActivityRemap userRemappedActivity in user_remapped_activities)
                    {
                        if (Activities_Source.Any(x => x.P6_ActivityId == userRemappedActivity.P6_NEW_ACTIVITY))
                        {
                            valid_user_remapeed_activities.Add(userRemappedActivity);
                        }
                    }

                    List<P6_AssignmentProjection> reassign_activities = new List<P6_AssignmentProjection>();
                    if (user_remapped_activities.Count() > 0)
                    {
                        List<P6_AssignmentProjection> valid_reassignments = new List<P6_AssignmentProjection>();
                        foreach (P6_AssignmentProjection missing_activity in missing_activities)
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
            List<P6_AssignmentProjection> missing_activities = get_missing_p6_activities();
            if (missing_activities.Count > 0)
            {
                DialogCollectionViewModel<P6_AssignmentProjection> missing_activities_viewmodel = DialogCollectionViewModel<P6_AssignmentProjection>.Create(missing_activities);
                ActivityDetailDialogService.ShowDialog(MessageButton.OK, "Invalid Assignments", "MissingAssignments", missing_activities_viewmodel);

                if (MessageBoxService.ShowMessage("Do you wish to delete these invalid assignments?", BluePrintsResources.Warning_Caption, MessageButton.OKCancel) == MessageResult.OK)
                {
                    P6_ASSIGNMENTSCollectionViewModel.BaseBulkDelete(missing_activities.Select(x => x.deliverable_assignment));
                    FullRefresh();
                }
            }
            else
                MessageBoxService.ShowMessage("All Assignments Valid");
        }

        private List<P6_AssignmentProjection> get_missing_p6_activities(bool getAllActivities = false)
        {
            var IP6EntitiesUnitOfWork = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            string project_name;
            if (mappingType == BaselineMappingSelectionType.Modified)
                project_name = p6_baseline_entity.P6_Baseline_Name;
            else
                project_name = p6_baseline_entity.P6_Mod_Baseline_Name;

            List<P6_AssignmentProjection> missing_activities = new List<P6_AssignmentProjection>();
            foreach (ICanAssignP6 deliverable in Deliverables_Source)
            {
                IEnumerable<P6_ASSIGNMENT> deliverable_assignments = deliverable.P6_Assignments;
                foreach (P6_ASSIGNMENT deliverable_assignment in deliverable_assignments)
                {
                    if (getAllActivities)
                        missing_activities.Add(new P6_AssignmentProjection((IDeliverable_Rates)deliverable, deliverable_assignment));
                    else
                    {
                        P6_Activity existingTask = Activities_Source.FirstOrDefault(x => x.P6_ActivityId == deliverable_assignment.P6_ACTIVITYID);
                        if (existingTask == null)
                        {
                            missing_activities.Add(new P6_AssignmentProjection((IDeliverable_Rates)deliverable, deliverable_assignment));
                        }
                    }
                }
            }

            return missing_activities;
        }

        public void Save_Task(TASK task)
        {
            P6TASKCollectionViewModel.Save(task);
        }

        protected CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> P6_ASSIGNMENTSCollectionViewModel
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

        protected CollectionViewModel<TASK, TASK, int, IP6EntitiesUnitOfWork> P6TASKCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<TASK, TASK, int, IP6EntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<TASK>();
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

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMCollection
        {
            get
            {
                return GetEntities<PROGRESS_ITEM>();
            }
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
        #endregion
    }
}
