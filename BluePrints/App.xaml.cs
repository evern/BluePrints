using BluePrints.Common;
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
        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {
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