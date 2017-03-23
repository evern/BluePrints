using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.ViewModels;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Scheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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