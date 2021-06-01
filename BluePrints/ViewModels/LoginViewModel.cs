using BaseModel.Data.Helpers;
using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Views;
using DevExpress.LookAndFeel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class LoginViewModel
    {
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public bool RememberPassword { get; set; }
        private Guid authenticateKey;
        enum UserAuthenticationResult
        {
            UsernameNotAdded,
            UsernameInactive,
            RoleNotAssigned,
            InvalidUsernameOrPassword,
            Authenticated,
            ActiveDirectoryError
        }

        public static LoginViewModel Create()
        {
            return ViewModelSource.Create(() => new LoginViewModel());
        }

        private IEnumerable<USER> USERS { get; set; }
        private DispatcherTimer delayedHideDispatcher;
        private DispatcherTimer delayedMainWindowCloseDispatcher;
        private DispatcherTimer delayedConnectDispatcher;
        protected virtual BaseModel.ViewModel.Services.IWindowService WindowService { get { return this.GetService<BaseModel.ViewModel.Services.IWindowService>(); } }

        public LoginViewModel()
        {
            delayedMainWindowCloseDispatcher = new DispatcherTimer();
            delayedMainWindowCloseDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            delayedMainWindowCloseDispatcher.Tick += DelayedMainWindowCloseDispatcher_Tick;

            //preloadMainWindow();
            delayedHideDispatcher = new DispatcherTimer();
            delayedHideDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedHideDispatcher.Tick += delayedHideDispatcher_Tick;

            delayedConnectDispatcher = new DispatcherTimer();
            delayedConnectDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            delayedConnectDispatcher.Tick += DelayedConnectDispatcher_Tick;
            USERS = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork().USERS.AsEnumerable();
            UserName = XMLHelpers.GetSettings_Username();
            UserPassword = XMLHelpers.GetSettings_Password();

            Messenger.Default.Register<AuthenticationResult>(this, (AuthenticationResult) => signalRLoadWindow(AuthenticationResult));
            authenticateKey = Guid.NewGuid();
#if DEBUG
            Application.Current.Dispatcher.BeginInvoke(new Action(() => immediateLogin()));
#else
            if (UserName != string.Empty && UserPassword != string.Empty)
            {
                RememberPassword = true;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Login()));
            }
#endif
        }

        private void DelayedConnectDispatcher_Tick(object sender, EventArgs e)
        {
            delayedConnectDispatcher.Stop();
            SignalR.ConnectAsync(UserName);
        }

        public void OnLoaded()
        {
            string themeName = Properties.Settings.Default["ThemeName"] as string;
            if (themeName == "")
                themeName = "Win10Light";
            ApplicationThemeHelper.ApplicationThemeName = themeName;
            delayedConnectDispatcher.Start();
        }

        private void delayedHideDispatcher_Tick(object sender, EventArgs e)
        {
            delayedHideDispatcher.Stop();
            WindowService.Hide();
        }

        private void immediateLogin()
        {
            //if (Environment.MachineName == "EVERN-PC")
            //{
            LoginCredentials.IsAdmin = true;
            UserName = BluePrintsResources.Default_AdminUsername;
            UserPassword = BluePrintsResources.Default_AdminPassword;
            delayedHideDispatcher.Start();
            Login();
            //}
        }

        public void Login()
        {
            try
            {
                if (UserName == BluePrintsResources.Default_AdminUsername && UserPassword == BluePrintsResources.Default_AdminPassword)
                {
                    loadWindow(UserAuthenticationResult.Authenticated.ToString());
                    return;
                }

                UserAuthenticationResult authenticationResult = UserAuthenticate;
                if (authenticationResult == UserAuthenticationResult.ActiveDirectoryError)
                    signalRLogin();
                else
                    loadWindow(authenticationResult.ToString());
            }
            //when Microsoft.DirectoryServices fail to load on some device
            catch (Exception ex)
            {
                string s = ex.ToString();
                signalRLogin();
            }
        }

        private void signalRLogin()
        {
            if (SignalR.Connection == null)
            {
                SignalR.ConnectAsync(UserName);
                do
                {
                    Thread.Sleep(100);
                } while (SignalR.Connection.State != Microsoft.AspNet.SignalR.Client.ConnectionState.Connected);
            }

            SignalR.HubAuthenticate(UserName, UserPassword, authenticateKey.ToString());
        }

        private void signalRLoadWindow(AuthenticationResult authenticationResult)
        {
            if(authenticationResult.Key == authenticateKey.ToString())
            {
                loadWindow(authenticationResult.Result);
            }
        }

        private void loadWindow(string authenticationResult)
        {
            if (authenticationResult == UserAuthenticationResult.Authenticated.ToString())
            {
                XMLSettings newXMLSettings = new XMLSettings();
                if (RememberPassword)
                    newXMLSettings.Password = BluePrintsUtils.Encrypt(UserPassword.Trim(), true);
                else
                    newXMLSettings.Password = string.Empty;

                newXMLSettings.Username = UserName.Trim();
                XMLHelpers.UpdateSettingsXMLCredentials(newXMLSettings);

                if (UserName == BluePrintsResources.Default_AdminUsername && UserPassword == BluePrintsResources.Default_AdminPassword)
                    LoginCredentials.IsAdmin = true;
                else
                    LoginCredentials.IsAdmin = false;

                if (LoginCredentials.IsAdmin)
                {
                    LoginCredentials.CurrentUser = USERS.First(x => x.NAME.ToUpper() == "BEN.DAVIES");
                    //LoginCredentials.CurrentUser = new USER() { NAME = BluePrintsResources.Default_AdminUsername };
                    USER_PREFERENCE forecastActualPreference = new USER_PREFERENCE();
                    //forecastActualPreference.PREFERENCE_NAME = DataUtils.GetNameOf(() => UserPreferences.Forecast_ShowActuals);
                    //forecastActualPreference.PREFERENCE_VALUE = UserPreferences.PreferenceTrueValue;
                    //LoginCredentials.CurrentUser.UserPreferences.Add(forecastActualPreference);
                    //Task.Run(() => ActiveDirectory.ExchangeLoginAsync(LoginCredentials.CurrentUser.NAME, "NEWpass14."));
                }
                else
                {
                    LoginCredentials.CurrentUser = USERS.FirstOrDefault(x => x.NAME.ToUpper() == UserName.ToUpper());
                    //Task.Run(() => ActiveDirectory.ExchangeLoginAsync(LoginCredentials.CurrentUser.NAME, UserPassword));
                }

                LoginCredentials.CurrentHWID = CommonMethods.GetHWID();
                ShowMainWindow();
                delayedHideDispatcher.Start();
            }
            else
                SetUsernamePasswordError(authenticationResult);
        }

        protected IMessageBoxService MessageBoxService
        {
            get { return this.GetRequiredService<IMessageBoxService>(); }
        }

        public void Exit()
        {
            Environment.Exit(1);
        }

        private UserAuthenticationResult UserAuthenticate
        {
            get
            {
                var user = USERS.FirstOrDefault(x => x.NAME.ToLower() == UserName.ToLower());
                if (user == null)
                    return UserAuthenticationResult.UsernameNotAdded;
                else
                {
                    if (user.GUID_ROLE == Guid.Empty)
                        return UserAuthenticationResult.RoleNotAssigned;

                    if (user.LEAVE_DATE != null)
                        return UserAuthenticationResult.UsernameInactive;

                    if (UserName != null && UserPassword != null)
                    {
                        IEnumerable<USER> activeDirectoryUSERS = null;
                        try
                        {
                            activeDirectoryUSERS = EmailServices.GetUSERS();
                        }
                        catch
                        {
                            return UserAuthenticationResult.ActiveDirectoryError;
                        }

                        if (activeDirectoryUSERS != null)
                        {
                            USER CaseSensitiveUser = activeDirectoryUSERS.FirstOrDefault(x => x.NAME.ToLower() == UserName.ToLower());
                            if (CaseSensitiveUser != null)
                                UserName = CaseSensitiveUser.NAME;
                        }

                        bool? result = ActiveDirectory.ActiveDirectory.Authenticate(UserName, UserPassword);
                        
                        if (result == null)
                            return UserAuthenticationResult.ActiveDirectoryError;

                        if (((bool)result))
                        {
                            return UserAuthenticationResult.Authenticated;
                        }
                        else
                        {
                            XMLHelpers.UpdateSettingsXMLCredentials(new XMLSettings() { Username = string.Empty, Password = string.Empty });
                            return UserAuthenticationResult.InvalidUsernameOrPassword;
                        }
                    }

                    else
                        return UserAuthenticationResult.InvalidUsernameOrPassword;
                }
            }
        }

        private void SetUsernamePasswordError(string authenticationResult)
        {
            String errorText = string.Empty;
            if(authenticationResult == UserAuthenticationResult.InvalidUsernameOrPassword.ToString())
            {
                errorText = "Invalid username or password";
                ShowError(true, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.UsernameInactive.ToString())
            {
                errorText = "User has left the company";
                ShowError(true, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.RoleNotAssigned.ToString())
            {
                errorText = "Please email " + BluePrintsResources.ITEmail + " to assign a role to you";
                ShowError(false, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.UsernameNotAdded.ToString())
            {
                errorText = "Please email " + BluePrintsResources.ITEmail + " to add you as a BluePrint user";
                ShowError(false, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.ActiveDirectoryError.ToString())
            {
                errorText = "Active directory not responding";
                ShowError(false, errorText);
            }
        }

        public void ShowError(bool isPasswordField, string errorMessage)
        {
            MessageBoxService.ShowMessage(errorMessage);
            //ShowErrorCallBack?.Invoke(isPasswordField, errorMessage);
        }

        private void preloadMainWindow()
        {
            BluePrintsEntitiesWindow preloadMainWindow = new BluePrintsEntitiesWindow();
            preloadMainWindow = null;
        }

        BluePrintsEntitiesWindow mainWindow;
        public void ShowMainWindow()
        {
            mainWindow = new BluePrintsEntitiesWindow();
            mainWindow.Show();
        }

        public void SignalRShutdown(string message)
        {
            WindowService.Show();
            delayedMainWindowCloseDispatcher.Start();

            UserName = string.Empty;
            UserPassword = string.Empty;
            RememberPassword = false;

            this.RaisePropertiesChanged();
            if (message != string.Empty)
            {
                MessageBoxService.ShowMessage(message);
                Environment.Exit(1);
            }
        }

        private void DelayedMainWindowCloseDispatcher_Tick(object sender, EventArgs e)
        {
            delayedMainWindowCloseDispatcher.Stop();
            if (mainWindow != null)
            {
                ((BluePrintsEntitiesViewModel)mainWindow.DataContext).isLoggingOut = false;
                mainWindow.Close();
            }
        }

        public void Window_KeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }
    }
}