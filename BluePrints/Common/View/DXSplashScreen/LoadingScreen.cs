using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.Core;
using BluePrints.View;

namespace BluePrints.Common
{
    public static class LoadingScreenManager
    {
        public static void ShowLoadingScreen(int maxProgress)
        {
            if (DXSplashScreen.IsActive || maxProgress == 0)
                return;

            ResetCurrentProgress();
            SetMaxProgress(maxProgress);
            DXSplashScreen.Show<LoadingScreen>();
        }

        public static int MaxProgress { get; set; }
        public static int CurrentProgress { get; set; }

        public static void CloseLoadingScreen()
        {
            if(DXSplashScreen.IsActive)
                DXSplashScreen.Close();
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
