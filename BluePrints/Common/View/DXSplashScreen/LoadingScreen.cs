using BluePrints.View;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using System.Windows;
using System.Windows.Media;

namespace BluePrints.Common
{
    public static class LoadingScreenManager
    {
        public static bool DisableLoadingScreen { get; set; }

        public static void ShowLoadingScreen(int maxProgress, bool alwaysOnTop = false)
        {
                return;

            if (DXSplashScreen.IsActive || maxProgress == 0)
                return;

            ResetCurrentProgress();
            SetMaxProgress(maxProgress);

            if(!alwaysOnTop)
            {
                DXSplashScreen.Show(x => {
                    Window res = new Window()
                    {
                        ShowActivated = false,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        AllowsTransparency = true,
                        Background = new SolidColorBrush(Colors.Transparent),
                        ShowInTaskbar = false,
                        Topmost = true,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    };
                    WindowFadeAnimationBehavior.SetEnableAnimation(res, true);
                    res.Topmost = false;
                    return res;
                }, x => {
                    return new LoadingScreen() { DataContext = new SplashScreenViewModel() };
                }, null, null);
            }
            else
                DXSplashScreen.Show<LoadingScreen>();
        }

        public static string DefaultState
        {
            get { return "Loading..."; }
        }

        public static int MaxProgress { get; set; }
        public static int CurrentProgress { get; set; }

        public static void CloseLoadingScreen()
        {
            if(DXSplashScreen.IsActive)
                DXSplashScreen.Close();
        }

        public static void SetMessage(string message)
        {
            if (DXSplashScreen.IsActive)
                DXSplashScreen.SetState(message);
        }

        public static void ResetCurrentProgress()
        {
            CurrentProgress = 0;
        }

        public static void SetMaxProgress(int maxProgress)
        {
            MaxProgress = maxProgress;
        }

        public static void Progress()
        {
            if(DXSplashScreen.IsActive && MaxProgress > 0)
            {
                DXSplashScreen.Progress(CurrentProgress++, MaxProgress);
                if (CurrentProgress == MaxProgress)
                    CloseLoadingScreen();
            }
        }
    }
}
