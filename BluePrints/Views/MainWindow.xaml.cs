using DevExpress.Xpf.Core;
using System.Windows.Input;

namespace BluePrints
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DXWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    this.DragMove();
                }
                catch
                {

                }
            }
        }
    }
}