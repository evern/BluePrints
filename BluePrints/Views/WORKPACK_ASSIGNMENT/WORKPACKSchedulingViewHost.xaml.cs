using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
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
    public partial class WORKPACKSchedulingViewHost : UserControl
    {
        public WORKPACKSchedulingViewHost()
        {
            InitializeComponent();
            ((WORKPACKSchedulingViewModelWrapper) DataContext).windowsFormHostViewInitialization =
                windowsFormHostViewInitialization;
        }

        public void windowsFormHostViewInitialization(Func<IEnumerable<TASK>> getTASKsFunc,
            Func<IEnumerable<PROJWBS>> getWBSSFunc,
            Func<IEnumerable<WORKPACK_Dashboard>> getWORKPACK_DashboardFunc,
            CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                WORKPACKS_ASSIGNMENTSViewModel,
            bool IsModified)
        {
            winFormHost.Child = new PROJECTWORKPACKDetailsMappingView(getTASKsFunc, getWBSSFunc,
                getWORKPACK_DashboardFunc, WORKPACKS_ASSIGNMENTSViewModel, IsModified);
        }
    }
}