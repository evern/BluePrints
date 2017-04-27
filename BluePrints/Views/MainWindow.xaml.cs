using BluePrints.Common;
using BluePrints.ViewModels;
using BluePrints.Views;
using DevExpress.Xpf.Core;
using System;

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
            LayoutSettings.Default.ThemeName = ApplicationThemeHelper.ApplicationThemeName;
            LayoutSettings.Default.Save();
            ((BluePrintsEntitiesViewModel)((BluePrintsEntitiesView)Content).DataContext).Unloaded();
            base.OnClosed(e);
            Environment.Exit(1);
            //if (ShowLoginWindow != null)
            //    ShowLoginWindow();
        }
    }
}