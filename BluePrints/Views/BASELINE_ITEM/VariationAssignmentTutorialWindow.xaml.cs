using System.Linq;
using BaseModel.Misc;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class VariationAssignmentTutorialWindow : DXWindow
    {
        public VariationAssignmentTutorialWindow()
        {
            InitializeComponent();
            this.Loaded += VariationAssignmentTutorialWindow_Loaded;
        }

        private void VariationAssignmentTutorialWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DevExpress.Xpf.PdfViewer.DXScrollViewer viewer = LayoutTreeHelper.GetVisualChildren(this).OfType<DevExpress.Xpf.PdfViewer.DXScrollViewer>().FirstOrDefault();
            viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void PdfViewerControl_UriOpening(System.Windows.DependencyObject d, DevExpress.Xpf.PdfViewer.UriOpeningEventArgs e)
        {
            
        }
    }
}