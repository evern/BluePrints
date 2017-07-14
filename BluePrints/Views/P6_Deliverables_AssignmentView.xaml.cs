using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
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
    public partial class P6_Deliverables_AssignmentView : DXWindow, IDisposable
    {
        public P6_Deliverables_AssignmentView()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            ((P6_AssignmentViewModel)DataContext).Dispose();
        }

        private void userControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }
    }
}