using BluePrints.Common;
using DevExpress.Xpf.Core;
using System;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //DevExpress 20 implementation
        //protected async override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);
        //    await ThemeManager.PreloadThemeResourceAsync("Win10Light");
        //}

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            //disable prompt for expression binding when editing reports
            DevExpress.XtraReports.Configuration.Settings.Default.UserDesignerOptions.DataBindingMode = DevExpress.XtraReports.UI.DataBindingMode.Bindings;
            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(SignalRExceptionLogging);
        }

        static void SignalRExceptionLogging(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            //SignalR.HubLogMessage(LoginCredentials.CurrentUser.NAME + " app crashed: " + e.Message);
        }
    }
}