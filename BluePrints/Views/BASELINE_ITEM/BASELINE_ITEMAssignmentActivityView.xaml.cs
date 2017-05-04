using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.ViewModels;
using DevExpress.Xpf.Core;
using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for PROJECTWORKPACKDetailsWorkpackAssignment.xaml
    /// </summary>
    public partial class BASELINE_ITEMAssignmentActivityView : DXWindow, IDisposable
    {
        public BASELINE_ITEMAssignmentActivityView(
            PROJECT PROJECT, 
            IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments,
            IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMS,
            CollectionViewModel<BASELINE_ITEM_ASSIGNMENT, BASELINE_ITEM_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTSViewModel,
            bool IsModified,
            Appointment SelectedTASK_Appointment = null,
            IEnumerable<BASELINE_ITEMProjection> selectedBASELINE_ITEMProjections = null)
        {
            InitializeComponent();
            DataContext = BASELINE_ITEM_ASSIGNMENTViewModel.Create(PROJECT, ALLTASK_Appointments, BASELINE_ITEMS,
                WORKPACK_ASSIGNMENTSViewModel, IsModified, SelectedTASK_Appointment, selectedBASELINE_ITEMProjections);
            ((BASELINE_ITEM_ASSIGNMENTViewModel)DataContext).RefreshBASELINE_ITEM_ASSIGNMENTCallBack = RefreshWORKPACK_ASSIGNMENTCallBack;
            ((BASELINE_ITEM_ASSIGNMENTViewModel)DataContext).SetSelectedItemCallBack = SetSelectedItem;
        }

        public void RefreshWORKPACK_ASSIGNMENTCallBack()
        {
            gridControlBASELINE_ITEM_ASSIGNMENTS.RefreshData();
        }

        public void SetSelectedItem(object selectedItem)
        {
            gridControlBASELINE_ITEM_ASSIGNMENTS.CurrentItem = selectedItem;
        }

        public void Dispose()
        {
            ((BASELINE_ITEM_ASSIGNMENTViewModel) DataContext).Dispose();
        }
    }
}