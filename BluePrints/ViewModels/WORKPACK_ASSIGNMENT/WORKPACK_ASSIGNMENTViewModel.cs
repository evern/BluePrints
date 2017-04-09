using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.P6Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class WORKPACK_ASSIGNMENTViewModel : IDisposable
    {
        public static WORKPACK_ASSIGNMENTViewModel Create(IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments,
            IEnumerable<WORKPACK_Dashboard> WORKPACKS,
            CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                WORKPACK_ASSIGNMENTSViewModel, bool IsModified, Appointment SelectedTASK_Appointment = null,
            WORKPACK_Dashboard SelectedWORKPACK = null)
        {
            return
                ViewModelSource.Create(
                    () =>
                        new WORKPACK_ASSIGNMENTViewModel(ALLTASK_Appointments, WORKPACKS, WORKPACK_ASSIGNMENTSViewModel,
                            IsModified, SelectedTASK_Appointment, SelectedWORKPACK));
        }

        private bool IsModified { get; set; }

        protected WORKPACK_ASSIGNMENTViewModel(IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments,
            IEnumerable<WORKPACK_Dashboard> WORKPACKS,
            CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                WORKPACK_ASSIGNMENTSViewModel, bool IsModified, Appointment SelectedTASK_Appointment = null,
            WORKPACK_Dashboard SelectedWORKPACK = null)
        {
            TASKSItemSource = ALLTASK_Appointments.ToArray().AsEnumerable();
            WORKPACKSItemSource = WORKPACKS;
            this.WORKPACK_ASSIGNMENTSViewModel = WORKPACK_ASSIGNMENTSViewModel;
            this.IsModified = IsModified;
            SelectedTASK = SelectedTASK_Appointment != null
                ? ALLTASK_Appointments.First(x => x.task_id == (int) SelectedTASK_Appointment.Id)
                : null;
            this.SelectedWORKPACK = SelectedWORKPACK != null ? SelectedWORKPACK : null;
        }

        #region Public Properties

        private CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
            WORKPACK_ASSIGNMENTSViewModel { get; set; }

        private decimal assignmenthighvalue { get; set; }

        public decimal AssignmentHighValue
        {
            get { return assignmenthighvalue; }
            set
            {
                assignmenthighvalue = value;
                this.RaiseCanExecuteChanged(x => x.AddAssignment());
            }
        }

        public decimal AssignmentLowValue
        {
            get
            {
                if (SelectedWORKPACK == null)
                    return 0;

                var assignedValue = SelectedWORKPACK.ASSIGNED_UNITS;
                if (assignedValue >= AssignmentMaxValue)
                    return 0;
                else
                    return assignedValue + 1;
            }
        }

        public decimal AssignmentMinValue
        {
            get
            {
                if (AssignmentLowValue == 0)
                    return 0;
                else if (AssignmentLowValue >= AssignmentMaxValue)
                    return AssignmentMaxValue;
                else
                    return AssignmentLowValue + 1;
            }
        }

        public decimal AssignmentMaxValue
        {
            get
            {
                if (SelectedWORKPACK == null)
                    return 0;

                return SelectedWORKPACK.Stats.totalUnits;
            }
        }

        public List<WORKPACK_ASSIGNMENT> TASK_ASSIGNMENTS
        {
            get
            {
                if (WORKPACKSItemSource == null || SelectedTASK == null)
                    return null;

                return
                    WORKPACKSItemSource.SelectMany(x => x.ObservableWORKPACK_ASSIGNMENTS)
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
                this.RaisePropertyChanged(x => x.SelectedTASK);
            }
        }

        private WORKPACK_Dashboard selectedWORKPACK { get; set; }

        public WORKPACK_Dashboard SelectedWORKPACK
        {
            get { return selectedWORKPACK; }
            set
            {
                if (value != null)
                {
                    selectedWORKPACK = value;
                    if (AssignmentMinValue == 0)
                        AssignmentHighValue = 0;
                    else if (AssignmentMinValue > AssignmentMaxValue)
                        AssignmentHighValue = AssignmentMaxValue;
                    else
                        AssignmentHighValue = AssignmentMinValue;

                    this.RaisePropertyChanged(x => x.SelectedWORKPACK);
                }
            }
        }

        private WORKPACK_ASSIGNMENT selectedWORKPACK_ASSIGNMENT { get; set; }

        public virtual WORKPACK_ASSIGNMENT SelectedWORKPACK_ASSIGNMENT { get; set; }

        public bool CanMatchSelectedWORKPACK_ASSIGNMENT()
        {
            return SelectedWORKPACK_ASSIGNMENT != null;
        }

        public void MatchSelectedWORKPACK_ASSIGNMENT()
        {
            TASK_AppointmentInfo taskAppointment = TASKSItemSource.FirstOrDefault(x => x.Subject == SelectedWORKPACK_ASSIGNMENT.P6_ACTIVITYID);
            if (taskAppointment != null)
                SelectedTASK = taskAppointment;
        }
        #endregion

        #region Item Source

        public IEnumerable<TASK_AppointmentInfo> TASKSItemSource { get; set; }
        public IEnumerable<WORKPACK_Dashboard> WORKPACKSItemSource { get; set; }

        #endregion

        #region Commands

        public void MaxUnits()
        {
            AssignmentHighValue = AssignmentMaxValue;
            this.RaisePropertiesChanged();
        }

        public bool CanMaxUnits()
        {
            if (!CanAddAssignment())
                return false;

            if (AssignmentHighValue == AssignmentMaxValue)
                return false;
            else
                return true;
        }

        public Action RefreshWORKPACK_ASSIGNMENTCallBack { get; set; }

        public void Refresh()
        {
            if (RefreshWORKPACK_ASSIGNMENTCallBack != null)
                RefreshWORKPACK_ASSIGNMENTCallBack();

            this.RaisePropertiesChanged();
        }

        public void AddAssignment()
        {
            var newWORKPACK_ASSIGNMENT = new WORKPACK_ASSIGNMENT()
            {
                GUID = Guid.Empty,
                HIGH_VALUE = AssignmentHighValue,
                LOW_VALUE = AssignmentLowValue,
                P6_ACTIVITYID = SelectedTASK.Subject,
                PRIORITY = SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Count + 1,
                GUID_WORKPACK = SelectedWORKPACK.GUID,
                ISMODIFIEDBASELINE = IsModified
            };

            WORKPACK_ASSIGNMENTSViewModel.Save(newWORKPACK_ASSIGNMENT);
            SelectedWORKPACK.Entity.WORKPACK_ASSIGNMENT.Add(newWORKPACK_ASSIGNMENT);
            SelectedWORKPACK_ASSIGNMENT = newWORKPACK_ASSIGNMENT;

            if (AssignmentMinValue == 0)
                AssignmentHighValue = 0;
            else if (AssignmentMinValue > AssignmentMaxValue)
                AssignmentHighValue = AssignmentMaxValue;
            else
                AssignmentHighValue = AssignmentMinValue;

            Refresh();
        }

        public bool CanAddAssignment()
        {
            if (SelectedWORKPACK == null)
                return false;

            var assignedUnits = SelectedWORKPACK.ASSIGNED_UNITS;

            if (AssignmentLowValue == 0)
                return false;

            if (assignmenthighvalue < AssignmentLowValue)
                return false;

            if (assignmenthighvalue > AssignmentMaxValue)
                return false;

            if (SelectedTASK == null)
                return false;

            return true;
        }

        public void DeleteAssignment()
        {
            if (SelectedWORKPACK_ASSIGNMENT == null)
                return;

            RemoveWorkpackAssignment(SelectedWORKPACK_ASSIGNMENT);
            AssignmentHighValue = AssignmentMinValue;

            Refresh();
        }

        private void RemoveWorkpackAssignment(WORKPACK_ASSIGNMENT removeWORKPACK_ASSIGNMENT)
        {
            var removingWORKPACK_ASSIGNMENTLowValue = removeWORKPACK_ASSIGNMENT.LOW_VALUE;
            var activeWORKPACK =
                WORKPACKSItemSource.FirstOrDefault(x => x.Entity.GUID == removeWORKPACK_ASSIGNMENT.GUID_WORKPACK);
            if (activeWORKPACK == null)
                return;

            activeWORKPACK.Entity.WORKPACK_ASSIGNMENT.Remove(removeWORKPACK_ASSIGNMENT);
            WORKPACK_ASSIGNMENTSViewModel.Delete(removeWORKPACK_ASSIGNMENT);

            var workpackAssignmentsInOrder =
                activeWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Where(
                    x => x.LOW_VALUE > removingWORKPACK_ASSIGNMENTLowValue).OrderBy(x => x.PRIORITY).ToList();
            for (var i = 0; i < workpackAssignmentsInOrder.Count; i++)
            {
                var currentWORKPACK_ASSIGNMENTAmount = workpackAssignmentsInOrder[i].HIGH_VALUE -
                                                           workpackAssignmentsInOrder[i].LOW_VALUE + 1;
                workpackAssignmentsInOrder[i].LOW_VALUE = removingWORKPACK_ASSIGNMENTLowValue;
                workpackAssignmentsInOrder[i].HIGH_VALUE = removingWORKPACK_ASSIGNMENTLowValue +
                                                           currentWORKPACK_ASSIGNMENTAmount - 1;
                removingWORKPACK_ASSIGNMENTLowValue = workpackAssignmentsInOrder[i].HIGH_VALUE + 1;
                workpackAssignmentsInOrder[i].PRIORITY = workpackAssignmentsInOrder[i].PRIORITY - 1;
            }

            WORKPACK_ASSIGNMENTSViewModel.BulkSave(
                new ObservableCollection<WORKPACK_ASSIGNMENT>(workpackAssignmentsInOrder));
        }

        public bool CanDeleteAssignment()
        {
            if (TASK_ASSIGNMENTS != null && TASK_ASSIGNMENTS.Count > 0)
                return true;

            if (SelectedWORKPACK == null || SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Count() == 0)
                return false;

            if (SelectedWORKPACK_ASSIGNMENT == null)
                return false;

            return true;
        }

        public void PriorityUp()
        {
            MovePriority(true);
        }

        private void MovePriority(bool isUp)
        {
            var WORKPACK_ASSIGNMENTSInOrder =
                SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.OrderBy(x => x.PRIORITY).ToList();
            var selectionIndex = WORKPACK_ASSIGNMENTSInOrder.IndexOf(SelectedWORKPACK_ASSIGNMENT);
            var swapWORKPACK_ASSIGNMENT =
                WORKPACK_ASSIGNMENTSInOrder[selectionIndex + (isUp == true ? -1 : 1)];
            var swapWORKPACK_ASSIGNMENTId = swapWORKPACK_ASSIGNMENT.P6_ACTIVITYID;
            swapWORKPACK_ASSIGNMENT.P6_ACTIVITYID = SelectedWORKPACK_ASSIGNMENT.P6_ACTIVITYID;
            SelectedWORKPACK_ASSIGNMENT.P6_ACTIVITYID = swapWORKPACK_ASSIGNMENTId;
            WORKPACK_ASSIGNMENTSViewModel.BulkSave(
                new ObservableCollection<WORKPACK_ASSIGNMENT>(WORKPACK_ASSIGNMENTSInOrder));

            SelectedWORKPACK_ASSIGNMENT = swapWORKPACK_ASSIGNMENT;
            Refresh();
        }

        public bool CanPriorityUp()
        {
            if (selectedWORKPACK_ASSIGNMENT == null || SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Count == 0 ||
                selectedWORKPACK_ASSIGNMENT == SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.First())
                return false;

            return true;
        }

        public void PriorityDown()
        {
            MovePriority(false);
        }

        public bool CanPriorityDown()
        {
            if (selectedWORKPACK_ASSIGNMENT == null || SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Count == 0 ||
                selectedWORKPACK_ASSIGNMENT == SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Last())
                return false;

            return true;
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
            WORKPACKSItemSource = null;
            WORKPACK_ASSIGNMENTSViewModel.OnDestroy();
        }
    }
}