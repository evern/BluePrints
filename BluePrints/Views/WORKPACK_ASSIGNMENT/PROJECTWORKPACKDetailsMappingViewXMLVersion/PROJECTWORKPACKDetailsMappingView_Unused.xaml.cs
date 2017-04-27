using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for AREADetailsCollectionView.xaml
    /// </summary>
    public partial class PROJECTWORKPACKDetailsMappingView_Unused : UserControl
    {
        public PROJECTWORKPACKDetailsMappingView_Unused()
        {
            InitializeComponent();
            var viewModel = (WORKPACKSchedulingViewModelWrapper) DataContext;
            //viewModel.GridWorkpack = this.gridControl;
            //viewModel.SchedulerActivities = this.scheduler;
            //PopulateSchedulerViewList(this.scheduler);
            //ApplyHorizontalResourceHeaderStyle();
        }
    }
}