using BluePrints.Common;
using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class SUBJOBDashboardView : UserControl
    {
        public SUBJOBDashboardView()
        {
            InitializeComponent();
            ((SUBJOBDashboardViewModelWrapper) DataContext).ChangeViewMemberFieldNames =
                ChangeViewMemberFieldNames;
        }

        public void ChangeViewMemberFieldNames(DashboardViewType DashboardViewType)
        {
            
        }
    }
}