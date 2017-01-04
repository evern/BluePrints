using BluePrints.ViewModels;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Scheduler;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AREADetailsCollectionView.xaml
    /// </summary>
    public partial class PROJECTWORKPACKDetailsMappingView_Unused : UserControl
    {
        public PROJECTWORKPACKDetailsMappingView_Unused()
        {
            InitializeComponent();
            var viewModel = ((WORKPACKSchedulingViewModelWrapper)this.DataContext);
            //viewModel.GridWorkpack = this.gridControl;
            //viewModel.SchedulerActivities = this.scheduler;
            //PopulateSchedulerViewList(this.scheduler);
            //ApplyHorizontalResourceHeaderStyle();
        }
    }
}
