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
using System.Threading.Tasks;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for BluePrintsSplashScreen.xaml
    /// </summary>
    public partial class BluePrintsSplashScreen : Window
    {
        public BluePrintsSplashScreen()
        {
            InitializeComponent();
            preloadMainWindow();
        }

        private void preloadMainWindow()
        {
            MainWindow preloadWindow = new MainWindow();
            //PROJECTDashboardPreloadUserControl projectDashboardPreloadUserControl = new PROJECTDashboardPreloadUserControl();
            //preloadWindow = null;
            //projectDashboardPreloadUserControl = null;

            LoginWindow newLoginWindow = new LoginWindow();
            newLoginWindow.Show();
            this.Close();
        }
    }
}
