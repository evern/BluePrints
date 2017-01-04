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

namespace BluePrints.ViewModels
{
    public class WORKPACK_ASSIGNMENTViewModel : IDisposable
    {
        public static WORKPACK_ASSIGNMENTViewModel Create(IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments, IEnumerable<WORKPACK_Dashboard> WORKPACKS, CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTSViewModel, bool IsModified, Appointment SelectedTASK_Appointment = null, WORKPACK_Dashboard SelectedWORKPACK = null)
        {
            return ViewModelSource.Create(() => new WORKPACK_ASSIGNMENTViewModel(ALLTASK_Appointments, WORKPACKS, WORKPACK_ASSIGNMENTSViewModel, IsModified, SelectedTASK_Appointment, SelectedWORKPACK));
        }

        bool IsModified { get; set; }
        protected WORKPACK_ASSIGNMENTViewModel(IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments, IEnumerable<WORKPACK_Dashboard> WORKPACKS, CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTSViewModel, bool IsModified, Appointment SelectedTASK_Appointment = null, WORKPACK_Dashboard SelectedWORKPACK = null)
        {
            this.TASKSItemSource = ALLTASK_Appointments.ToArray().AsEnumerable();
            this.WORKPACKSItemSource = WORKPACKS;
            this.WORKPACK_ASSIGNMENTSViewModel = WORKPACK_ASSIGNMENTSViewModel;
            this.IsModified = IsModified;
            this.SelectedTASK = SelectedTASK_Appointment != null ? ALLTASK_Appointments.First(x => x.task_id == (int)SelectedTASK_Appointment.Id) : null;
            this.SelectedWORKPACK = SelectedWORKPACK != null ? SelectedWORKPACK : null;
        }

        #region Public Properties
        CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTSViewModel { get; set; }
        decimal assignmenthighvalue { get; set; }
        public decimal AssignmentHighValue
        {
            get
            {
                return assignmenthighvalue;
            }
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

                decimal assignedValue = SelectedWORKPACK.ASSIGNED_UNITS;
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

                return SelectedWORKPACK.Final_BudgetedUnits; 
            }
        }

        public List<WORKPACK_ASSIGNMENT> TASK_ASSIGNMENTS
        {
            get
            {
                if (WORKPACKSItemSource == null || SelectedTASK == null)
                    return null;

                return WORKPACKSItemSource.SelectMany(x => x.ObservableWORKPACK_ASSIGNMENTS).Where(x => x.P6_ACTIVITYID == SelectedTASK.Subject).ToList();
            }
        }
        #endregion

        #region Selected Items
        TASK_AppointmentInfo selectedTASK { get; set; }
        public TASK_AppointmentInfo SelectedTASK
        {
            get { return selectedTASK; }
            set 
            { 
                selectedTASK = value;
                this.RaisePropertiesChanged();
            }
        }

        WORKPACK_Dashboard selectedWORKPACK { get; set; }
        public WORKPACK_Dashboard SelectedWORKPACK
        {
            get { return selectedWORKPACK; }
            set
            {
                if (value != null)
                {
                    selectedWORKPACK = value;
                    if (this.AssignmentMinValue == 0)
                        this.AssignmentHighValue = 0;
                    else if (this.AssignmentMinValue > this.AssignmentMaxValue)
                        this.AssignmentHighValue = this.AssignmentMaxValue;
                    else
                        this.AssignmentHighValue = this.AssignmentMinValue;

                    this.RaisePropertiesChanged();
                }
            }
        }

        WORKPACK_ASSIGNMENT selectedWORKPACK_ASSIGNMENT { get; set; }
        public WORKPACK_ASSIGNMENT SelectedWORKPACK_ASSIGNMENT
        {
            get { return selectedWORKPACK_ASSIGNMENT; }
            set
            {
                selectedWORKPACK_ASSIGNMENT = value;
            }
        }
        #endregion

        #region Item Source
        public IEnumerable<TASK_AppointmentInfo> TASKSItemSource { get; set; }
        public IEnumerable<WORKPACK_Dashboard> WORKPACKSItemSource { get; set; }
        #endregion

        #region Commands
        public void MaxUnits()
        {
            AssignmentHighValue = this.AssignmentMaxValue;
            this.RaisePropertiesChanged();
        }

        public bool CanMaxUnits()
        {
            if (!CanAddAssignment())
                return false;

            if (AssignmentHighValue == this.AssignmentMaxValue)
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
            WORKPACK_ASSIGNMENT newWORKPACK_ASSIGNMENT = new WORKPACK_ASSIGNMENT()
            {
                GUID = Guid.Empty,
                HIGH_VALUE = this.AssignmentHighValue,
                LOW_VALUE = this.AssignmentLowValue,
                P6_ACTIVITYID = this.SelectedTASK.Subject,
                PRIORITY = this.SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Count + 1,
                GUID_WORKPACK = this.SelectedWORKPACK.GUID,
                ISMODIFIEDBASELINE = this.IsModified
            };

            WORKPACK_ASSIGNMENTSViewModel.Save(newWORKPACK_ASSIGNMENT);
            this.SelectedWORKPACK.WORKPACK.WORKPACK_ASSIGNMENT.Add(newWORKPACK_ASSIGNMENT);
            this.SelectedWORKPACK_ASSIGNMENT = newWORKPACK_ASSIGNMENT;

            if (this.AssignmentMinValue == 0)
                this.AssignmentHighValue = 0;
            else if (this.AssignmentMinValue > AssignmentMaxValue)
                this.AssignmentHighValue = AssignmentMaxValue;
            else
                this.AssignmentHighValue = this.AssignmentMinValue;

            Refresh();
        }

        public bool CanAddAssignment()
        {
            if (SelectedWORKPACK == null)
                return false;

            decimal assignedUnits = SelectedWORKPACK.ASSIGNED_UNITS;

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
            if (this.SelectedWORKPACK_ASSIGNMENT == null)
                return;

            RemoveWorkpackAssignment(this.SelectedWORKPACK_ASSIGNMENT);
            this.AssignmentHighValue = this.AssignmentMinValue;

            Refresh();
        }

        private void RemoveWorkpackAssignment(WORKPACK_ASSIGNMENT removeWORKPACK_ASSIGNMENT)
        {
            decimal removingWORKPACK_ASSIGNMENTLowValue = removeWORKPACK_ASSIGNMENT.LOW_VALUE;
            WORKPACK_Dashboard activeWORKPACK = WORKPACKSItemSource.FirstOrDefault(x => x.WORKPACK.GUID == removeWORKPACK_ASSIGNMENT.GUID_WORKPACK);
            if (activeWORKPACK == null)
                return;

            activeWORKPACK.WORKPACK.WORKPACK_ASSIGNMENT.Remove(removeWORKPACK_ASSIGNMENT);
            WORKPACK_ASSIGNMENTSViewModel.Delete(removeWORKPACK_ASSIGNMENT);

            List<WORKPACK_ASSIGNMENT> workpackAssignmentsInOrder = activeWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Where(x => x.LOW_VALUE > removingWORKPACK_ASSIGNMENTLowValue).OrderBy(x => x.PRIORITY).ToList();
            for (int i = 0; i < workpackAssignmentsInOrder.Count; i++)
            {
                decimal currentWORKPACK_ASSIGNMENTAmount = (workpackAssignmentsInOrder[i].HIGH_VALUE - workpackAssignmentsInOrder[i].LOW_VALUE) + 1;
                workpackAssignmentsInOrder[i].LOW_VALUE = removingWORKPACK_ASSIGNMENTLowValue;
                workpackAssignmentsInOrder[i].HIGH_VALUE = (removingWORKPACK_ASSIGNMENTLowValue + currentWORKPACK_ASSIGNMENTAmount) - 1;
                removingWORKPACK_ASSIGNMENTLowValue = workpackAssignmentsInOrder[i].HIGH_VALUE + 1;
                workpackAssignmentsInOrder[i].PRIORITY = workpackAssignmentsInOrder[i].PRIORITY - 1;
            }

            WORKPACK_ASSIGNMENTSViewModel.BulkSave(new ObservableCollection<WORKPACK_ASSIGNMENT>(workpackAssignmentsInOrder));
        }

        public bool CanDeleteAssignment()
        {
            if(TASK_ASSIGNMENTS != null && TASK_ASSIGNMENTS.Count > 0)
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
            List<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENTSInOrder = this.SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.OrderBy(x => x.PRIORITY).ToList();
            int selectionIndex = WORKPACK_ASSIGNMENTSInOrder.IndexOf(SelectedWORKPACK_ASSIGNMENT);
            WORKPACK_ASSIGNMENT swapWORKPACK_ASSIGNMENT = WORKPACK_ASSIGNMENTSInOrder[selectionIndex + (isUp == true ? -1 : 1)];
            string swapWORKPACK_ASSIGNMENTId = swapWORKPACK_ASSIGNMENT.P6_ACTIVITYID;
            swapWORKPACK_ASSIGNMENT.P6_ACTIVITYID = SelectedWORKPACK_ASSIGNMENT.P6_ACTIVITYID;
            SelectedWORKPACK_ASSIGNMENT.P6_ACTIVITYID = swapWORKPACK_ASSIGNMENTId;
            WORKPACK_ASSIGNMENTSViewModel.BulkSave(new ObservableCollection<WORKPACK_ASSIGNMENT>(WORKPACK_ASSIGNMENTSInOrder));

            this.SelectedWORKPACK_ASSIGNMENT = swapWORKPACK_ASSIGNMENT;
            Refresh();
        }

        public bool CanPriorityUp()
        {
            if (selectedWORKPACK_ASSIGNMENT == null || SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Count == 0 || selectedWORKPACK_ASSIGNMENT == SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.First())
                return false;

            return true;
        }

        public void PriorityDown()
        {
            MovePriority(false);
        }

        public bool CanPriorityDown()
        {
            if (selectedWORKPACK_ASSIGNMENT == null || SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Count == 0 || selectedWORKPACK_ASSIGNMENT == this.SelectedWORKPACK.ObservableWORKPACK_ASSIGNMENTS.Last())
                return false;

            return true;
        }

        /// <summary>
        /// Don't allow users to choose WBS items
        /// </summary>
        public void lookupActivity_EditValueChanging(EditValueChangingEventArgs e)
        {
            TASK_AppointmentInfo changingValue = (TASK_AppointmentInfo)e.NewValue;
            if (changingValue.Status != AppointmentActivityType.Activity)
            {
                e.IsCancel = true;
                e.Handled = true;
            }
        }
        #endregion

        public void Dispose()
        {
            this.TASKSItemSource = null;
            this.WORKPACKSItemSource = null;
            this.WORKPACK_ASSIGNMENTSViewModel.OnDestroy();
        }
    }
}
