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
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class P6_AssignmentViewModel : BindableBase, IDisposable
    {
        public static P6_AssignmentViewModel Create(PROJECT PROJECT, IEnumerable<GanttData> activities,
            IEnumerable<ICanAssignP6> deliverables,
            CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                P6_ASSIGNMENTCollectionViewModel, bool isModified, GanttData selected_activity = null,
            IEnumerable<ICanAssignP6> dropped_deliverables = null, Action recalculateUnits = null)
        {
            return
                ViewModelSource.Create(
                    () =>
                        new P6_AssignmentViewModel(PROJECT, activities, deliverables, P6_ASSIGNMENTCollectionViewModel,
                            isModified, selected_activity, dropped_deliverables, recalculateUnits));
        }

        private bool IsModified { get; set; }
        public Action RecalculateUnits { get; set; }
        private DispatcherTimer dispatchTimer;
        private DispatcherTimer selectAllDispatcherTimer;
        private DispatcherTimer maxUnitsDispatcherTimer;

        protected P6_AssignmentViewModel(PROJECT PROJECT, IEnumerable<GanttData> activities,
            IEnumerable<ICanAssignP6> deliverables,
            CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                P6_ASSIGNMENTCollectionViewModel, bool isModified, GanttData selected_activity = null,
            IEnumerable<ICanAssignP6> dropped_deliverables = null, Action recalculateUnits = null)
        {
            Activities = activities.ToArray().AsEnumerable();
            Deliverables_Source = deliverables;
            RecalculateUnits = recalculateUnits;
            this.P6_ASSIGNMENTCollectionViewModel = P6_ASSIGNMENTCollectionViewModel;
            IsModified = isModified;
            Selected_Activity = selected_activity != null
                ? activities.First(x => x.Id == selected_activity.Id)
                : null;
            this.loadPROJECT = PROJECT;

            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            selectAllDispatcherTimer = new DispatcherTimer();
            selectAllDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            selectAllDispatcherTimer.Tick += SelectAllDispatcherTimer_Tick;

            maxUnitsDispatcherTimer = new DispatcherTimer();
            maxUnitsDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            maxUnitsDispatcherTimer.Tick += maxUnitsDispatcherTimer_Tick;

            InitializeBASELINE_ITEMSContext(dropped_deliverables);
        }

        private void InitializeBASELINE_ITEMSContext(IEnumerable<ICanAssignP6> dropped_deliverables)
        {
            this.Context_Deliverables = dropped_deliverables != null ? new ObservableCollection<ICanAssignP6>(dropped_deliverables) : new ObservableCollection<ICanAssignP6>();

            this.Selected_Deliverable = new ObservableCollection<ICanAssignP6>();
            this.Selected_Activities = new ObservableCollection<P6_ASSIGNMENTSProjection>();
            this.Selected_Deliverable.CollectionChanged += SelectedBASELINE_ITEMS_CollectionChanged;

            selectAllDispatcherTimer.Start();
        }

        private void maxUnitsDispatcherTimer_Tick(object sender, EventArgs e)
        {
            maxUnitsDispatcherTimer.Stop();
            MaxUnits();
        }

        private void SelectAllDispatcherTimer_Tick(object sender, EventArgs e)
        {
            selectAllDispatcherTimer.Stop();
            Selected_Deliverable.Clear();
            foreach (ICanAssignP6 contextDeliverable in Context_Deliverables)
                Selected_Deliverable.Add(contextDeliverable);

            maxUnitsDispatcherTimer.Start();
        }

        #region Public Properties

        private CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
            P6_ASSIGNMENTCollectionViewModel { get; set; }

        private decimal assignmentValue { get; set; }

        public decimal AssignmentValue
        {
            get { return assignmentValue; }
            set
            {
                assignmentValue = value;
                this.RaiseCanExecuteChanged(x => x.AddAssignment());
            }
        }

        public decimal AssignmentMinValue
        {
            get
            {
                if (Selected_Deliverable.Count == 0)
                    return 0;

                decimal assigned_Percentage = Selected_Deliverable.Min(x => x.Assigned_Percentage);
                return assigned_Percentage > 1 ? 1 : assigned_Percentage;
            }
        }

        public decimal AssignmentMaxValue
        {
            get
            {
                return 1;
            }
        }

        public List<P6_ASSIGNMENT> TASK_ASSIGNMENTS
        {
            get
            {
                if (Deliverables_Source == null || Selected_Activity == null)
                    return null;

                return
                    Deliverables_Source.SelectMany(x => x.P6_Assignments)
                        .Where(x => x.P6_ACTIVITYID == Selected_Activity.P6_ActivityId)
                        .ToList();
            }
        }

        #endregion

        #region Selected Items

        private GanttData selectedTASK { get; set; }

        public GanttData Selected_Activity
        {
            get { return selectedTASK; }
            set
            {
                selectedTASK = value;
                this.RaisePropertiesChanged();
            }
        }

        public ObservableCollection<ICanAssignP6> Selected_Deliverable { get; set; }
        public ICanAssignP6 SelectedBASELINE_ITEM { get; set; }

        private ObservableCollection<ICanAssignP6> contextBASELINE_ITEMS { get; set; }
        public ObservableCollection<ICanAssignP6> Context_Deliverables
        {
            get { return contextBASELINE_ITEMS; }
            set
            {
                contextBASELINE_ITEMS = value;
                this.RaisePropertyChanged(x => x.Context_Deliverables);
            }
        }

        private void SelectedBASELINE_ITEMS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            dispatchTimer.Tick -= dispatchTimer_Tick;
            dispatchTimer.Tick += dispatchTimer_Tick;
            dispatchTimer.Start();
        }

        private void dispatchTimer_Tick(object sender, EventArgs e)
        {
            ResetAssignmentValue();
            this.RaisePropertiesChanged();
            dispatchTimer.Stop();
        }

        private void ResetAssignmentValue()
        {
            AssignmentValue = AssignmentMinValue;
        }

        public bool CanMatchSelectedBASELINE_ITEM_ASSIGNMENT()
        {
            return SelectedASSIGNMENT != null;
        }

        public void MatchSelectedBASELINE_ITEM_ASSIGNMENT()
        {
            if (SelectedASSIGNMENT == null)
                return;

            GanttData taskAppointment = Activities.FirstOrDefault(x => x.P6_ActivityId == SelectedASSIGNMENT.Entity.P6_ACTIVITYID);

            if (taskAppointment != null)
                Selected_Activity = taskAppointment;
        }
        #endregion

        #region Item Source

        public IEnumerable<GanttData> Activities { get; set; }
        public IEnumerable<ICanAssignP6> Deliverables_Source { get; set; }
        public PROJECT loadPROJECT;
        #endregion

        #region Commands

        public void MaxUnits()
        {
            AssignmentValue = AssignmentMaxValue;
            this.RaisePropertiesChanged();
        }

        public bool CanMaxUnits()
        {
            return CanAddAssignment();
        }

        public Action RefreshBASELINE_ITEM_ASSIGNMENTCallBack { get; set; }
        public Action<P6_ASSIGNMENTSProjection> SetSelectedItemCallBack { get; set; }
        public void Refresh()
        {
            RefreshBASELINE_ITEM_ASSIGNMENTCallBack?.Invoke();
            this.RaisePropertiesChanged();
        }

        public void AddAssignment()
        {
            foreach(ICanAssignP6 deliverable in Selected_Deliverable)
            {
                if (deliverable.Assigned_Percentage == AssignmentValue)
                    continue;

                deliverable.P6_Assignments.Add(new P6_ASSIGNMENT()
                {
                    GUID = Guid.Empty,
                    GUID_PROJECT = loadPROJECT.GUID,
                    HIGH_VALUE = AssignmentValue,
                    LOW_VALUE = deliverable.Assigned_Percentage + 0.01m,
                    P6_ACTIVITYID = Selected_Activity.P6_ActivityId,
                    GUID_ORIGINAL = deliverable.OriginalEntityKey,
                    ISMODIFIEDBASELINE = IsModified
                });
            }

            P6_ASSIGNMENTCollectionViewModel.BulkSave(Selected_Deliverable.SelectMany(x => x.P6_Assignments.Where(y => y.GUID == Guid.Empty)));
            ResetAssignmentValue();
            Refresh();
        }

        public bool CanAddAssignment()
        {
            if (Selected_Deliverable == null)
                return false;

            if (Selected_Activity == null)
                return false;

            return true;
        }

        public P6_ASSIGNMENTSProjection SelectedASSIGNMENT { get; set; }
        public ObservableCollection<P6_ASSIGNMENTSProjection> Selected_Activities { get; set; }

        public IEnumerable<P6_ASSIGNMENTSProjection> ContextBASELINE_ITEM_ASSIGNMENTS
        {
            get
            {
                List<P6_ASSIGNMENTSProjection> baseline_item_assignments = new List<P6_ASSIGNMENTSProjection>();
                foreach(ICanAssignP6 selected_deliverable in Selected_Deliverable)
                {
                    foreach(P6_ASSIGNMENT p6_assignments in selected_deliverable.P6_Assignments)
                    {
                        if(Selected_Activity == null || p6_assignments.P6_ACTIVITYID == Selected_Activity.P6_ActivityId)
                            baseline_item_assignments.Add(new P6_ASSIGNMENTSProjection() { Deliverable_OriginalEntityKey = selected_deliverable.OriginalEntityKey, Deliverable_Name = selected_deliverable.Deliverable_Name, Entity = p6_assignments });
                    }
                }

                return baseline_item_assignments.OrderBy(x => x.Deliverable_Name);
            }
        }

        public bool CanDeleteAssignment()
        {
            if (Selected_Activities.Count == 0)
                return false;

            return true;
        }

        public void DeleteAssignment()
        {
            if (Selected_Activities.Count == 0)
                return;

            foreach(P6_ASSIGNMENTSProjection selectedASSIGNMENT in Selected_Activities)
            {
                RemoveWorkpackAssignment(selectedASSIGNMENT);
            }

            ResetAssignmentValue();
            Refresh();
        }

        private void RemoveWorkpackAssignment(P6_ASSIGNMENTSProjection remove_p6_assignments)
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


        private IEnumerable<P6_ASSIGNMENT> MovePriority(bool isUp, P6_ASSIGNMENTSProjection selectedAssignment)
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
            if (Selected_Activities.Count == 0)
                return false;

            return true;
        }

        public bool CanPriorityDown()
        {
            if (Selected_Activities.Count == 0)
                return false;

            return true;
        }

        public void PriorityUp()
        {
            List<P6_ASSIGNMENT> saveAssignments = new List<P6_ASSIGNMENT>();

            foreach (var selectedAssignment in Selected_Activities)
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

            foreach (var selectedAssignment in Selected_Activities)
            {
                IEnumerable<P6_ASSIGNMENT> editedAssignments = MovePriority(false, selectedAssignment);
                if (editedAssignments != null)
                    saveAssignments.AddRange(editedAssignments);
            }

            P6_ASSIGNMENTCollectionViewModel.BulkSave(new ObservableCollection<P6_ASSIGNMENT>(saveAssignments));
            this.RaisePropertiesChanged();
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
            RecalculateUnits?.Invoke();
            RecalculateUnits = null;
            Activities = null;
            Deliverables_Source = null;
            P6_ASSIGNMENTCollectionViewModel.OnDestroy();
        }
    }
}