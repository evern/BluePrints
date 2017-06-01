using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for WORKPACKSchedulingViewHost.xaml
    /// </summary>
    public partial class BASELINE_ITEMSchedulingViewHost : UserControl
    {
        public BASELINE_ITEMSchedulingViewHost()
        {
            InitializeComponent();
            ((BASELINE_ITEMSchedulingViewModelWrapper)DataContext).WinformFormHostInitialization = windowsFormHostViewInitialization;
            ((BASELINE_ITEMSchedulingViewModelWrapper)DataContext).RefreshWinformView = RefreshWinformView;
        }

        public void windowsFormHostViewInitialization(BluePrints.Data.PROJECT PROJECT, IEnumerable<TASK> TASKS,
            IEnumerable<PROJWBS> P6WBSs,
            IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMProjections,
            CollectionViewModel<BASELINE_ITEM_ASSIGNMENT, BASELINE_ITEM_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                BASELINE_ITEM_ASSIGNMENTSViewModel,
            bool IsModified)
        {
            winFormHost.Child = new BASELINE_ITEMSchedulingView(PROJECT, TASKS, P6WBSs, BASELINE_ITEMProjections, BASELINE_ITEM_ASSIGNMENTSViewModel, IsModified);
        }

        public void RefreshWinformView()
        {
            if (winFormHost.Child == null)
                return;

            ((BASELINE_ITEMSchedulingView)winFormHost.Child).RefreshAllDataSource();
        }
    }
}