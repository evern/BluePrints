using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;


namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for PROJECTViewWindow.xaml
    /// </summary>
    public partial class PROJECTViewWindow : DXWindow
    {
        public Action<string> WindowClosed;
        public PROJECTViewWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            WindowClosed?.Invoke(this.Tag.ToString());
            base.OnClosed(e);
        }
    }
}
