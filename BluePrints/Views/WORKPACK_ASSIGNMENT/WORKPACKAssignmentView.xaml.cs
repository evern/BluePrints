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
    public partial class WORKPACKAssignmentView : DXWindow, IDisposable
    {
        public WORKPACKAssignmentView(
            IEnumerable<TASK_AppointmentInfo> ALLTASK_Appointments,
            IEnumerable<WORKPACK_Dashboard> WORKPACKS,
            CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                WORKPACK_ASSIGNMENTSViewModel,
            bool IsModified,
            Appointment SelectedTASK_Appointment = null,
            WORKPACK_Dashboard SelectedWORKPACK = null)
        {
            InitializeComponent();
            DataContext = WORKPACK_ASSIGNMENTViewModel.Create(ALLTASK_Appointments, WORKPACKS,
                WORKPACK_ASSIGNMENTSViewModel, IsModified, SelectedTASK_Appointment, SelectedWORKPACK);
            ((WORKPACK_ASSIGNMENTViewModel) DataContext).RefreshWORKPACK_ASSIGNMENTCallBack =
                RefreshWORKPACK_ASSIGNMENTCallBack;
        }

        public void RefreshWORKPACK_ASSIGNMENTCallBack()
        {
            gridControlWORKPACK_ASSIGNMENTS.RefreshData();
        }

        public void Dispose()
        {
            ((WORKPACK_ASSIGNMENTViewModel) DataContext).Dispose();
        }
    }
}