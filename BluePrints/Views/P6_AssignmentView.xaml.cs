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
    public partial class P6_AssignmentView : DXWindow, IDisposable
    {
        public P6_AssignmentView(
            PROJECT PROJECT, IEnumerable<GanttData> activities,
            IEnumerable<ICanAssignP6> deliverables,
            CollectionViewModel<P6_ASSIGNMENT, P6_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> P6_ASSIGNMENTCollectionViewModel, bool isModified, GanttData selected_activity = null,
            IEnumerable<ICanAssignP6> dropped_deliverables = null, Action recalculateUnits = null)
        {
            InitializeComponent();
            DataContext = P6_AssignmentViewModel.Create(PROJECT, activities, deliverables, P6_ASSIGNMENTCollectionViewModel, isModified, selected_activity, dropped_deliverables, recalculateUnits);
        }

        //public void RefreshWORKPACK_ASSIGNMENTCallBack()
        //{
        //    gridControlBASELINE_ITEMS.RefreshData();
        //    gridControlBASELINE_ITEM_ASSIGNMENTS.RefreshData();
        //}

        //public void SetSelectedItem(object selectedItem)
        //{
        //    gridControlBASELINE_ITEM_ASSIGNMENTS.CurrentItem = selectedItem;
        //}

        public void Dispose()
        {
            ((BASELINE_ITEM_ASSIGNMENTViewModel) DataContext).Dispose();
        }

        private void userControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }
    }
}