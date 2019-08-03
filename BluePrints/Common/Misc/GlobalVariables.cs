using BluePrints.Common.Helpers;
using BluePrints.ViewModels;
using BluePrints.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.Common
{
    public static class GlobalVariables
    {
        public static bool IsNotificationShown { get; set; }
        public static bool IsProjectViewNotificationShown { get; set; }
        public static bool IsProjectCollectionViewNotificationShown { get; set; }
        public static bool IsBaselineItemNotificationShown { get; set; }
    }

    public static class BluePrintsGlobalMethods
    {
        public static void LogOut()
        {
            XMLHelpers.ClearSettings();
            Window active_window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.ToString().Contains("LoginWindow"));
            if (active_window == null)
                return;

            ((LoginViewModel)((LoginWindow)active_window).DataContext).SignalRShutdown(string.Empty);
        }

        public static void ApplicationShutDown()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
