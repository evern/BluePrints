using BluePrints.Common;
using BluePrints.Common.ViewModel.Converters;
using BluePrints.Common.ViewModel.Misc;
using BluePrints.ViewModels;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace BluePrints.Views
{
    public partial class PROJECTView : UserControl
    {
        public PROJECTView()
        {
            InitializeComponent();
            //HwndSource source = (HwndSource)HwndSource.FromVisual(this);
            //HwndTarget target = source?.CompositionTarget;
            //if (target != null)
            //    target.RenderMode = RenderMode.SoftwareOnly;
        }

        private void TableView_ShowFilterPopup(object sender, DevExpress.Xpf.Grid.FilterPopupEventArgs e)
        {
            FilterPopupBehavior.TryAttach(e.PopupBaseEdit);
        }
    }
}