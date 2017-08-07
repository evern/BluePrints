using BluePrints.Common;
using BluePrints.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class PROJECTDashboardPreloadUserControl : UserControl
    {
        public PROJECTDashboardPreloadUserControl()
        {
            InitializeComponent();
            this.DataContext = PROJECTDashboardViewModelWrapper.Create(new Common.ViewModel.ActionObject(nullifyDataContext));

        }

        private void nullifyDataContext()
        {
            ((PROJECTDashboardViewModelWrapper)this.DataContext).cleanUpEntitiesLoader();
            this.DataContext = null;
        }

    }
}