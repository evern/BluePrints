using DevExpress.Xpf.Core;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

#if RELEASE
            this.Title = "BluePrints";
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Common/Images/Logo.ico", UriKind.Absolute));
#endif

#if DEBUG
            this.Title = "BluePrints DEBUG";
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Common/Images/LogoBeta.ico", UriKind.Absolute));
#endif

#if BETA
            this.Title = "BluePrints BETA";
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Common/Images/LogoBeta.ico", UriKind.Absolute));
#endif
        }
    }
}