using BluePrints.Common;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
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
        //public Action ShowLoginWindow { get; set; }
        protected override void OnClosed(EventArgs e)
        {
            SignalR.Disconnect();
            base.OnClosed(e);
            System.Environment.Exit(1);
            //if (ShowLoginWindow != null)
            //    ShowLoginWindow();
        }
    }
}
