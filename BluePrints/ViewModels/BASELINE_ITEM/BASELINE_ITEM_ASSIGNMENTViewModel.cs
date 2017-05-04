using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
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
    public class BASELINE_ITEM_ASSIGNMENTViewModel : BindableBase, IDisposable
    {
        public static BASELINE_ITEM_ASSIGNMENTViewModel Create(PROJECT PROJECT, IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments,
            IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMS,
            CollectionViewModel<BASELINE_ITEM_ASSIGNMENT, BASELINE_ITEM_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                BASELINE_ITEM_ASSIGNMENTSViewModel, bool IsModified, Appointment SelectedTASK_Appointment = null,
            IEnumerable<BASELINE_ITEMProjection> dropBASELINE_ITEMS = null)
        {
            return
                ViewModelSource.Create(
                    () =>
                        new BASELINE_ITEM_ASSIGNMENTViewModel(PROJECT, ALLTASK_Appointments, BASELINE_ITEMS, BASELINE_ITEM_ASSIGNMENTSViewModel,
                            IsModified, SelectedTASK_Appointment, dropBASELINE_ITEMS));
        }

        private bool IsModified { get; set; }
        private DispatcherTimer dispatchTimer;
        private DispatcherTimer selectAllDispatcherTimer;

        protected BASELINE_ITEM_ASSIGNMENTViewModel(PROJECT PROJECT, IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments,
            IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMS,
            CollectionViewModel<BASELINE_ITEM_ASSIGNMENT, BASELINE_ITEM_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                BASELINE_ITEM_ASSIGNMENTSViewModel, bool IsModified, Appointment SelectedTASK_Appointment = null,
            IEnumerable<BASELINE_ITEMProjection> droppedBASELINE_ITEMS = null)
        {
            TASKSItemSource = ALLTASK_Appointments.ToArray().AsEnumerable();
            BASELINE_ITEMSource = BASELINE_ITEMS;
            this.BASELINE_ITEM_ASSIGNMENTSViewModel = BASELINE_ITEM_ASSIGNMENTSViewModel;
            this.IsModified = IsModified;
            SelectedTASK = SelectedTASK_Appointment != null
                ? ALLTASK_Appointments.First(x => x.task_id == (int) SelectedTASK_Appointment.Id)
                : null;
            this.ContextBASELINE_ITEMS = droppedBASELINE_ITEMS != null ? new ObservableCollection<BASELINE_ITEMProjection>(droppedBASELINE_ITEMS) : new ObservableCollection<BASELINE_ITEMProjection>();

            this.SelectedBASELINE_ITEMS = new ObservableCollection<BASELINE_ITEMProjection>();
            this.SelectedASSIGNMENTS = new ObservableCollection<BASELINE_ITEM_ASSIGNMENTSProjection>();
            this.SelectedBASELINE_ITEMS.CollectionChanged += SelectedBASELINE_ITEMS_CollectionChanged;
            this.loadPROJECT = PROJECT;

            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            selectAllDispatcherTimer = new DispatcherTimer();
            selectAllDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            selectAllDispatcherTimer.Tick += SelectAllDispatcherTimer_Tick;
            selectAllDispatcherTimer.Start();
        }

        private void SelectAllDispatcherTimer_Tick(object sender, EventArgs e)
        {
            selectAllDispatcherTimer.Stop();
            SelectedBASELINE_ITEMS.Clear();
            foreach (BASELINE_ITEMProjection contextBASELINE_ITEM in ContextBASELINE_ITEMS)
                SelectedBASELINE_ITEMS.Add(contextBASELINE_ITEM);
        }

        #region Public Properties

        private CollectionViewModel<BASELINE_ITEM_ASSIGNMENT, BASELINE_ITEM_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
            BASELINE_ITEM_ASSIGNMENTSViewModel { get; set; }

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
                if (SelectedBASELINE_ITEMS.Count == 0)
                    return 0;

                decimal assignedPercentage = SelectedBASELINE_ITEMS.Min(x => x.ASSIGNED_PERCENTAGE);
                return assignedPercentage > 1 ? 1 : assignedPercentage;
            }
        }

        public decimal AssignmentMaxValue
        {
            get
            {
                return 1;
            }
        }

        public List<BASELINE_ITEM_ASSIGNMENT> TASK_ASSIGNMENTS
        {
            get
            {
                if (BASELINE_ITEMSource == null || SelectedTASK == null)
                    return null;

                return
                    BASELINE_ITEMSource.SelectMany(x => x.BASELINE_ITEM_ASSIGNMENTS)
                        .Where(x => x.P6_ACTIVITYID == SelectedTASK.Subject)
                        .ToList();
            }
        }

        #endregion

        #region Selected Items

        private TASK_AppointmentInfo selectedTASK { get; set; }

        public TASK_AppointmentInfo SelectedTASK
        {
            get { return selectedTASK; }
            set
            {
                selectedTASK = value;
                this.RaisePropertiesChanged();
            }
        }

        public ObservableCollection<BASELINE_ITEMProjection> SelectedBASELINE_ITEMS { get; set; }
        public BASELINE_ITEMProjection SelectedBASELINE_ITEM { get; set; }

        private ObservableCollection<BASELINE_ITEMProjection> contextBASELINE_ITEMS { get; set; }
        public ObservableCollection<BASELINE_ITEMProjection> ContextBASELINE_ITEMS
        {
            get { return contextBASELINE_ITEMS; }
            set
            {
                contextBASELINE_ITEMS = value;
                this.RaisePropertyChanged(x => x.ContextBASELINE_ITEMS);
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
            return true;
        }

        public void MatchSelectedBASELINE_ITEM_ASSIGNMENT()
        {
            TASK_AppointmentInfo taskAppointment = TASKSItemSource.FirstOrDefault(x => x.Subject == SelectedASSIGNMENT.Entity.P6_ACTIVITYID);
            if (taskAppointment != null)
                SelectedTASK = taskAppointment;
        }
        #endregion

        #region Item Source

        public IEnumerable<TASK_AppointmentInfo> TASKSItemSource { get; set; }
        public IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMSource { get; set; }
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
        public Action<BASELINE_ITEM_ASSIGNMENTSProjection> SetSelectedItemCallBack { get; set; }
        public void Refresh()
        {
            RefreshBASELINE_ITEM_ASSIGNMENTCallBack?.Invoke();
            this.RaisePropertiesChanged();
        }

        public void AddAssignment()
        {
            foreach(BASELINE_ITEMProjection baseline_item in SelectedBASELINE_ITEMS)
            {
                if (baseline_item.ASSIGNED_PERCENTAGE == AssignmentValue)
                    continue;

                baseline_item.BASELINE_ITEM_ASSIGNMENTS.Add(new BASELINE_ITEM_ASSIGNMENT()
                {
                    GUID = Guid.Empty,
                    GUID_PROJECT = loadPROJECT.GUID,
                    HIGH_VALUE = AssignmentValue,
                    LOW_VALUE = baseline_item.ASSIGNED_PERCENTAGE + 0.01m,
                    P6_ACTIVITYID = SelectedTASK.Subject,
                    GUID_ORIGINAL = baseline_item.Entity.GUID_ORIGINAL,
                    ISMODIFIEDBASELINE = IsModified
                });
            }

            BASELINE_ITEM_ASSIGNMENTSViewModel.BulkSave(SelectedBASELINE_ITEMS.SelectMany(x => x.BASELINE_ITEM_ASSIGNMENTS.Where(y => y.GUID == Guid.Empty)));
            ResetAssignmentValue();
            this.RaisePropertiesChanged();
        }

        public bool CanAddAssignment()
        {
            if (SelectedBASELINE_ITEMS == null)
                return false;

            if (SelectedTASK == null)
                return false;

            return true;
        }

        public BASELINE_ITEM_ASSIGNMENTSProjection SelectedASSIGNMENT { get; set; }
        public ObservableCollection<BASELINE_ITEM_ASSIGNMENTSProjection> SelectedASSIGNMENTS { get; set; }

        public IEnumerable<BASELINE_ITEM_ASSIGNMENTSProjection> ContextBASELINE_ITEM_ASSIGNMENTS
        {
            get
            {
                List<BASELINE_ITEM_ASSIGNMENTSProjection> baseline_item_assignments = new List<BASELINE_ITEM_ASSIGNMENTSProjection>();
                foreach(BASELINE_ITEMProjection baseline_item in SelectedBASELINE_ITEMS)
                {
                    foreach(BASELINE_ITEM_ASSIGNMENT baseline_item_assignment in baseline_item.BASELINE_ITEM_ASSIGNMENTS)
                    {
                        if(SelectedTASK == null || baseline_item_assignment.P6_ACTIVITYID == SelectedTASK.Subject)
                            baseline_item_assignments.Add(new BASELINE_ITEM_ASSIGNMENTSProjection() { GUID_ORIGINAL = baseline_item.Entity.GUID_ORIGINAL, INTERNAL_NUM = baseline_item.Entity.INTERNAL_NUM, Entity = baseline_item_assignment });
                    }
                }

                return baseline_item_assignments.OrderBy(x => x.INTERNAL_NUM).OrderBy(x => x.Entity.LOW_VALUE);
            }
        }

        public bool CanDeleteAssignment()
        {
            if (SelectedASSIGNMENTS.Count == 0)
                return false;

            return true;
        }

        public void DeleteAssignment()
        {
            if (SelectedASSIGNMENTS.Count == 0)
                return;

            foreach(BASELINE_ITEM_ASSIGNMENTSProjection selectedASSIGNMENT in SelectedASSIGNMENTS)
            {
                RemoveWorkpackAssignment(selectedASSIGNMENT);
            }

            ResetAssignmentValue();
            this.RaisePropertiesChanged();
        }

        private void RemoveWorkpackAssignment(BASELINE_ITEM_ASSIGNMENTSProjection removeBASELINE_ITEM_ASSIGNMENT)
        {
            var removingBASELINE_ITEM_ASSIGNMENTLowValue = removeBASELINE_ITEM_ASSIGNMENT.Entity.LOW_VALUE;
            var activeBASELINE_ITEM =
                BASELINE_ITEMSource.FirstOrDefault(x => x.Entity.GUID_ORIGINAL == removeBASELINE_ITEM_ASSIGNMENT.GUID_ORIGINAL);
            if (activeBASELINE_ITEM == null)
                return;

            activeBASELINE_ITEM.BASELINE_ITEM_ASSIGNMENTS.RemoveAll(x => x.GUID == removeBASELINE_ITEM_ASSIGNMENT.GUID);
            BASELINE_ITEM_ASSIGNMENTSViewModel.Delete(removeBASELINE_ITEM_ASSIGNMENT.Entity);

            ObservableCollection<BASELINE_ITEM_ASSIGNMENT> workpackAssignmentsInOrder = 
                new ObservableCollection<BASELINE_ITEM_ASSIGNMENT>(activeBASELINE_ITEM.BASELINE_ITEM_ASSIGNMENTS
                .Where(x => x.LOW_VALUE > removingBASELINE_ITEM_ASSIGNMENTLowValue).OrderBy(x => x.LOW_VALUE).ToList());

            for (var i = 0; i < workpackAssignmentsInOrder.Count(); i++)
            {
                var currentBASELINE_ITEM_ASSIGNMENTAmount = workpackAssignmentsInOrder[i].HIGH_VALUE -
                                                           workpackAssignmentsInOrder[i].LOW_VALUE + 1;
                workpackAssignmentsInOrder[i].LOW_VALUE = removingBASELINE_ITEM_ASSIGNMENTLowValue;
                workpackAssignmentsInOrder[i].HIGH_VALUE = removingBASELINE_ITEM_ASSIGNMENTLowValue +
                                                           currentBASELINE_ITEM_ASSIGNMENTAmount - 1;
                removingBASELINE_ITEM_ASSIGNMENTLowValue = workpackAssignmentsInOrder[i].HIGH_VALUE + 1;
            }

            BASELINE_ITEM_ASSIGNMENTSViewModel.BulkSave(workpackAssignmentsInOrder);
        }


        private void MovePriority(bool isUp)
        {
            BASELINE_ITEMProjection contextBASELINE_ITEM = BASELINE_ITEMSource.First(x => x.Entity.GUID_ORIGINAL == SelectedASSIGNMENT.GUID_ORIGINAL);
            BASELINE_ITEM_ASSIGNMENT contextASSIGNMENT = SelectedASSIGNMENT.Entity;

            var BASELINE_ITEM_ASSIGNMENTSInOrder =
                contextBASELINE_ITEM.BASELINE_ITEM_ASSIGNMENTS.OrderBy(x => x.LOW_VALUE).ToList();

            BASELINE_ITEM_ASSIGNMENT swapBASELINE_ITEM_ASSIGNMENT;
            //look for next assignment in sequence
            if(!isUp)
                swapBASELINE_ITEM_ASSIGNMENT = ContextBASELINE_ITEMS.Where(x => x.GUID == contextBASELINE_ITEM.GUID)
                    .SelectMany(x => x.BASELINE_ITEM_ASSIGNMENTS).FirstOrDefault(x => x.LOW_VALUE == (contextASSIGNMENT.HIGH_VALUE + 0.01m));
            else
                swapBASELINE_ITEM_ASSIGNMENT = ContextBASELINE_ITEMS.Where(x => x.GUID == contextBASELINE_ITEM.GUID)
                    .SelectMany(x => x.BASELINE_ITEM_ASSIGNMENTS).FirstOrDefault(x => x.HIGH_VALUE == (contextASSIGNMENT.LOW_VALUE - 0.01m));

            if(swapBASELINE_ITEM_ASSIGNMENT != null)
            {
                //var selectionIndex = BASELINE_ITEM_ASSIGNMENTSInOrder.IndexOf(contextASSIGNMENT);
                //var swapBASELINE_ITEM_ASSIGNMENT =
                //    BASELINE_ITEM_ASSIGNMENTSInOrder[selectionIndex + (isUp == true ? -1 : 1)];
                var swapBASELINE_ITEM_ASSIGNMENTId = swapBASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID;
                swapBASELINE_ITEM_ASSIGNMENT.P6_ACTIVITYID = contextASSIGNMENT.P6_ACTIVITYID;
                contextASSIGNMENT.P6_ACTIVITYID = swapBASELINE_ITEM_ASSIGNMENTId;
                BASELINE_ITEM_ASSIGNMENTSViewModel.BulkSave(
                    new ObservableCollection<BASELINE_ITEM_ASSIGNMENT>(BASELINE_ITEM_ASSIGNMENTSInOrder));

                //Refresh();
                //SelectedASSIGNMENT = ContextBASELINE_ITEM_ASSIGNMENTS.FirstOrDefault(x => x.Entity.GetHashCode() == swapBASELINE_ITEM_ASSIGNMENT.GetHashCode());
                //SetSelectedItemCallBack?.Invoke(SelectedASSIGNMENT);
                this.RaisePropertiesChanged();
            }
        }

        public bool CanPriorityUp()
        {
            if (SelectedASSIGNMENT == null || SelectedASSIGNMENT.Entity.LOW_VALUE <= 0.01m)
                return false;

            return true;
        }

        public bool CanPriorityDown()
        {
            if (SelectedASSIGNMENT == null || SelectedASSIGNMENT.Entity.HIGH_VALUE >= 1)
                return false;

            return true;
        }

        public void PriorityUp()
        {
            MovePriority(true);
        }

        public void PriorityDown()
        {
            MovePriority(false);
        }

        /// <summary>
        /// Don't allow users to choose WBS items
        /// </summary>
        public void lookupActivity_EditValueChanging(EditValueChangingEventArgs e)
        {
            var changingValue = (TASK_AppointmentInfo) e.NewValue;
            if (changingValue.Status != AppointmentActivityType.Activity)
            {
                e.IsCancel = true;
                e.Handled = true;
            }
        }

        #endregion

        public void Dispose()
        {
            TASKSItemSource = null;
            BASELINE_ITEMSource = null;
            BASELINE_ITEM_ASSIGNMENTSViewModel.OnDestroy();
        }
    }
}