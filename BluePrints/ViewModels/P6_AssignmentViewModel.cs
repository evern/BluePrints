using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class P6_AssignmentViewModel : BindableBase, IDisposable
    {
        public static P6_AssignmentViewModel Create(PROJECT PROJECT, IEnumerable<GanttData> activities, IEnumerable<ICanAssignP6> deliverables,
            CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> P6_ASSIGNMENTCollectionViewModel, bool is_stock_code, GanttData selected_activity = null,
            IEnumerable<ICanAssignP6> dropped_deliverables = null, Action on_close = null, Action summarize_activities_assigned_units = null, bool isModified = false)
        {
            return ViewModelSource.Create(() => new P6_AssignmentViewModel(PROJECT, activities, deliverables, P6_ASSIGNMENTCollectionViewModel, is_stock_code, selected_activity, dropped_deliverables, on_close, summarize_activities_assigned_units, isModified));
        }

        private Dispatcher current_dispatcher = Application.Current.Dispatcher;
        private PROJECT loadPROJECT;
        private bool is_modified { get; set; }
        private CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> P6_ASSIGNMENTCollectionViewModel { get; set; }
        private Action on_close;
        private Action summarize_activities_assigned_units;
        public bool Is_Stock_Code { get; set; }
        protected P6_AssignmentViewModel(PROJECT PROJECT, IEnumerable<GanttData> activities, IEnumerable<ICanAssignP6> deliverables,
            CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> P6_ASSIGNMENTCollectionViewModel, bool is_stock_code, GanttData selected_activity = null,
            IEnumerable<ICanAssignP6> dropped_deliverables = null, Action on_close = null, Action summarize_activities_assigned_units = null, bool is_modified = false)
        {
            loadPROJECT = PROJECT;
            Activities_Source = activities;
            Deliverables_Source = deliverables;
            this.on_close = on_close;
            this.summarize_activities_assigned_units = summarize_activities_assigned_units;
            this.P6_ASSIGNMENTCollectionViewModel = P6_ASSIGNMENTCollectionViewModel;
            this.is_modified = is_modified;
            this.Is_Stock_Code = is_stock_code;
            Selected_Activity = selected_activity != null ? activities.First(x => x.Id == selected_activity.Id) : null;

            InitializeContext(dropped_deliverables);
            RegisterMessage();
        }

        private void InitializeContext(IEnumerable<ICanAssignP6> dropped_deliverables)
        {
            Context_Deliverables = dropped_deliverables != null ? new ObservableCollection<ICanAssignP6>(dropped_deliverables) : new ObservableCollection<ICanAssignP6>();

            Selected_Deliverables = new ObservableCollection<ICanAssignP6>();
            Selected_P6_Assignments = new ObservableCollection<P6_ASSIGNMENTProjection>();
            Selected_Deliverables.CollectionChanged += Selected_Deliverable_CollectionChanged;

            current_dispatcher.BeginInvoke(new Action(() => select_all_deliverables()));
            current_dispatcher.BeginInvoke(new Action(() => SetMaxUnits()));
        }

        private void select_all_deliverables()
        {
            Selected_Deliverables.Clear();
            foreach (ICanAssignP6 contextDeliverable in Context_Deliverables)
                Selected_Deliverables.Add(contextDeliverable);
        }

        #region Messaging
        private void RegisterMessage()
        {
            Messenger.Default.Register<P6_Deliverable_Assignment_Message>(this, x => OnMessage(x));
        }

        private void UnregisterMessageHandler()
        {
            Messenger.Default.Unregister(this);
        }

        private void OnMessage(P6_Deliverable_Assignment_Message message)
        {
            Context_Deliverables = message.Selected_Deliverables != null ? new ObservableCollection<ICanAssignP6>(message.Selected_Deliverables) : new ObservableCollection<ICanAssignP6>();
            select_all_deliverables();

            this.RaisePropertyChanged(x => x.Selected_Deliverables);
            Selected_Activity = message.Selected_Activity;
        }
        #endregion

        #region Assignment Selected Items
        private void Selected_Deliverable_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            current_dispatcher.BeginInvoke(new Action(() => SetMaxUnits()));
            this.RaisePropertyChanged(x => x.Deliverables_P6_Assignments);
        }

        /// <summary>
        /// Updates the total assigned percentages on each deliverable
        /// </summary>
        private void raise_deliverable_assignment_changes()
        {
            foreach(ICanAssignP6 deliverable in Deliverables_Source)
            {
                deliverable.Update();
            }

            this.RaisePropertyChanged(x => x.Deliverables_P6_Assignments);
        }
        #endregion

        #region Assignment View Properties
        public IEnumerable<GanttData> Activities_Source { get; set; }

        private GanttData selected_activity;
        public GanttData Selected_Activity { get => selected_activity; set { selected_activity = value; this.RaisePropertiesChanged(); } }

        public IEnumerable<ICanAssignP6> Deliverables_Source { get; set; }

        public List<P6_ASSIGNMENT> P6_Assignments
        {
            get
            {
                if (Deliverables_Source == null || Selected_Activity == null)
                    return null;

                return Deliverables_Source.SelectMany(x => x.P6_Assignments).Where(x => x.P6_ACTIVITYID == Selected_Activity.P6_ActivityId).ToList();
            }
        }


        private ObservableCollection<ICanAssignP6> context_deliverables;
        public ObservableCollection<ICanAssignP6> Context_Deliverables { get => context_deliverables; set { context_deliverables = value; this.RaisePropertyChanged(x => x.Context_Deliverables); } }

        public ObservableCollection<ICanAssignP6> Selected_Deliverables { get; set; }

        public ICanAssignP6 Selected_Deliverable { get; set; }

        public P6_ASSIGNMENTProjection Selected_P6_Assignment { get; set; }
        public ObservableCollection<P6_ASSIGNMENTProjection> Selected_P6_Assignments { get; set; }
        public IEnumerable<P6_ASSIGNMENTProjection> Deliverables_P6_Assignments
        {
            get
            {
                List<P6_ASSIGNMENTProjection> baseline_item_assignments = new List<P6_ASSIGNMENTProjection>();
                foreach (ICanAssignP6 selected_deliverable in Selected_Deliverables)
                {
                    foreach (P6_ASSIGNMENT p6_assignments in selected_deliverable.P6_Assignments)
                    {
                        if (Selected_Activity == null || p6_assignments.P6_ACTIVITYID == Selected_Activity.P6_ActivityId)
                            baseline_item_assignments.Add(new P6_ASSIGNMENTProjection() { Deliverable_OriginalEntityKey = selected_deliverable.OriginalEntityKey, Deliverable_Name = selected_deliverable.Deliverable_Name, Entity = p6_assignments });
                    }
                }

                return baseline_item_assignments.OrderBy(x => x.Deliverable_Name);
            }
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
                if (Selected_Deliverables.Count == 0)
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
            foreach(ICanAssignP6 deliverable in Selected_Deliverables)
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
                    ISMODIFIEDBASELINE = is_modified
                });
            }

            P6_ASSIGNMENTCollectionViewModel.BulkSave(Selected_Deliverables.SelectMany(x => x.P6_Assignments.Where(y => y.GUID == Guid.Empty)));
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

            foreach(P6_ASSIGNMENTProjection selectedASSIGNMENT in Selected_P6_Assignments)
            {
                RemoveWorkpackAssignment(selectedASSIGNMENT);
            }

            SetMaxUnits();
            raise_deliverable_assignment_changes();
        }

        private void RemoveWorkpackAssignment(P6_ASSIGNMENTProjection remove_p6_assignments)
        {
            var low_value = remove_p6_assignments.Entity.LOW_VALUE;
            var active_deliverable = Deliverables_Source.FirstOrDefault(x => x.OriginalEntityKey == remove_p6_assignments.Deliverable_OriginalEntityKey);
            if (active_deliverable == null)
                return;

            active_deliverable.P6_Assignments.RemoveAll(x => x.GUID == remove_p6_assignments.GUID);
            P6_ASSIGNMENTCollectionViewModel.Delete(remove_p6_assignments.Entity);

            ObservableCollection<P6_ASSIGNMENT> p6_assignments_in_order =  new ObservableCollection<P6_ASSIGNMENT>(active_deliverable.P6_Assignments.Where(x => x.LOW_VALUE > low_value).OrderBy(x => x.LOW_VALUE).ToList());

            for (var i = 0; i < p6_assignments_in_order.Count(); i++)
            {
                var currentBASELINE_ITEM_ASSIGNMENTAmount = p6_assignments_in_order[i].HIGH_VALUE - p6_assignments_in_order[i].LOW_VALUE + 1;
                p6_assignments_in_order[i].LOW_VALUE = low_value;
                p6_assignments_in_order[i].HIGH_VALUE = low_value + currentBASELINE_ITEM_ASSIGNMENTAmount - 1;
                low_value = p6_assignments_in_order[i].HIGH_VALUE + 1;
            }

            P6_ASSIGNMENTCollectionViewModel.BulkSave(p6_assignments_in_order);
        }


        private IEnumerable<P6_ASSIGNMENT> MovePriority(bool isUp, P6_ASSIGNMENTProjection selectedAssignment)
        {
            ICanAssignP6 context_deliverable = Deliverables_Source.First(x => x.OriginalEntityKey == selectedAssignment.Deliverable_OriginalEntityKey);
            P6_ASSIGNMENT contextASSIGNMENT = selectedAssignment.Entity;

            var BASELINE_ITEM_ASSIGNMENTSInOrder =
                context_deliverable.P6_Assignments.OrderBy(x => x.LOW_VALUE).ToList();

            P6_ASSIGNMENT swapBASELINE_ITEM_ASSIGNMENT;
            //look for next assignment in sequence
            if(!isUp)
                swapBASELINE_ITEM_ASSIGNMENT = Context_Deliverables.Where(x => x.EntityKey == context_deliverable.EntityKey)
                    .SelectMany(x => x.P6_Assignments).FirstOrDefault(x => x.LOW_VALUE == (contextASSIGNMENT.HIGH_VALUE + 0.01m));
            else
                swapBASELINE_ITEM_ASSIGNMENT = Context_Deliverables.Where(x => x.EntityKey == context_deliverable.EntityKey)
                    .SelectMany(x => x.P6_Assignments).FirstOrDefault(x => x.HIGH_VALUE == (contextASSIGNMENT.LOW_VALUE - 0.01m));

            if(swapBASELINE_ITEM_ASSIGNMENT != null)
            {
                var swapBASELINE_ITEM_ASSIGNMENTId = swapBASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID;
                swapBASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID = contextASSIGNMENT.P6_ACTIVITYID;
                contextASSIGNMENT.P6_ACTIVITYID = swapBASELINE_ITEM_ASSIGNMENTId;
                return BASELINE_ITEM_ASSIGNMENTSInOrder;
            }

            return null;
        }

        public bool CanPriorityUp()
        {
            if (Selected_P6_Assignments == null || Selected_P6_Assignments.Count == 0)
                return false;

            return true;
        }

        public bool CanPriorityDown()
        {
            if (Selected_P6_Assignments == null || Selected_P6_Assignments.Count == 0)
                return false;

            return true;
        }

        public void PriorityUp()
        {
            List<P6_ASSIGNMENT> saveAssignments = new List<P6_ASSIGNMENT>();

            foreach (var selectedAssignment in Selected_P6_Assignments)
            {
                IEnumerable<P6_ASSIGNMENT> editedAssignments = MovePriority(true, selectedAssignment);
                if (editedAssignments != null)
                    saveAssignments.AddRange(editedAssignments);
            }

            P6_ASSIGNMENTCollectionViewModel.BulkSave(new ObservableCollection<P6_ASSIGNMENT>(saveAssignments));
            this.RaisePropertiesChanged();
        }

        public void PriorityDown()
        {
            List<P6_ASSIGNMENT> saveAssignments = new List<P6_ASSIGNMENT>();

            foreach (var selectedAssignment in Selected_P6_Assignments)
            {
                IEnumerable<P6_ASSIGNMENT> editedAssignments = MovePriority(false, selectedAssignment);
                if (editedAssignments != null)
                    saveAssignments.AddRange(editedAssignments);
            }

            P6_ASSIGNMENTCollectionViewModel.BulkSave(new ObservableCollection<P6_ASSIGNMENT>(saveAssignments));
            this.RaisePropertiesChanged();
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
            if(e.NewValue != null)
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

        public void Dispose()
        {
            UnregisterMessageHandler();
            summarize_activities_assigned_units?.Invoke();
            summarize_activities_assigned_units = null;
            Activities_Source = null;
            Deliverables_Source = null;
            P6_ASSIGNMENTCollectionViewModel.OnDestroy();
            on_close?.Invoke();
        }
    }
}