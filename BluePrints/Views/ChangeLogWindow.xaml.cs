using System.Linq;
using BaseModel.Misc;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class ChangeLogWindow : DXWindow
    {
        public ChangeLogWindow()
        {
            InitializeComponent();
            this.Loaded += ChangeLogWindow_Loaded;
        }

        private void ChangeLogWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DevExpress.Xpf.PdfViewer.DXScrollViewer viewer = LayoutTreeHelper.GetVisualChildren(this).OfType<DevExpress.Xpf.PdfViewer.DXScrollViewer>().FirstOrDefault();
            viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void PdfViewerControl_UriOpening(System.Windows.DependencyObject d, DevExpress.Xpf.PdfViewer.UriOpeningEventArgs e)
        {
            
        }
    }
}