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
        enum UserAuthenticationResult
        {
            UsernameNotAdded,
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
        private DispatcherTimer delayedConnectDispatcher;
        protected virtual BaseModel.ViewModel.Services.IWindowService WindowService { get { return this.GetService<BaseModel.ViewModel.Services.IWindowService>(); } }

        public LoginViewModel()
        {
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

            //SignalR.ConnectAsync(UserName);
        }

        public void OnLoaded()
        {
            string themeName = Properties.Settings.Default["ThemeName"] as string;
            if (themeName == "")
                themeName = "Office2016Colorful";
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
            UserAuthenticationResult authenticationResult = UserAuthenticate;
            if (authenticationResult == UserAuthenticationResult.Authenticated || UserName == BluePrintsResources.Default_AdminUsername && UserPassword == BluePrintsResources.Default_AdminPassword)
            {
                if (UserName == BluePrintsResources.Default_AdminUsername && UserPassword == BluePrintsResources.Default_AdminPassword)
                    LoginCredentials.IsAdmin = true;

                if (LoginCredentials.IsAdmin)
                {
                    LoginCredentials.CurrentUser = new USER() { NAME = BluePrintsResources.Default_AdminUsername };
                    //LoginCredentials.CurrentUser = USERS.FirstOrDefault(x => x.NAME.ToUpper() == "GEORGE.EDWARDS");
                    Task.Run(() => ActiveDirectory.ExchangeLoginAsync(LoginCredentials.CurrentUser.NAME, "NEWpass14."));
                }
                else
                {
                    LoginCredentials.CurrentUser = USERS.FirstOrDefault(x => x.NAME.ToUpper() == UserName.ToUpper());
                    Task.Run(() => ActiveDirectory.ExchangeLoginAsync(LoginCredentials.CurrentUser.NAME, UserPassword));
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
                var user = USERS.Where(x => x.LEAVE_DATE == null).FirstOrDefault(x => x.NAME.ToLower() == UserName.ToLower());
                if (user == null)
                    return UserAuthenticationResult.UsernameNotAdded;
                else
                {
                    if (user.GUID_ROLE == Guid.Empty)
                        return UserAuthenticationResult.RoleNotAssigned;

                    if (UserName != null && UserPassword != null)
                    {
                        //if(!isUsernameLoadedFromXML)
                        //{
                            IEnumerable<USER> activeDirectoryUSERS = null;
                            try
                            {
                                activeDirectoryUSERS = ActiveDirectory.GetUSERS();
                            }
                            catch
                            {
                                return UserAuthenticationResult.ActiveDirectoryError;
                            }
                            
                            if(activeDirectoryUSERS != null)
                            {
                                USER CaseSensitiveUser = activeDirectoryUSERS.FirstOrDefault(x => x.NAME.ToLower() == UserName.ToLower());
                                if (CaseSensitiveUser != null)
                                    UserName = CaseSensitiveUser.NAME;
                            }
                        //}

                        bool? result = ActiveDirectory.Authenticate(UserName, UserPassword);
                        if (result == null)
                            return UserAuthenticationResult.ActiveDirectoryError;

                        if (((bool)result))
                        {
                            XMLSettings newXMLSettings = new XMLSettings();
                            if (RememberPassword)
                                newXMLSettings.Password = BluePrintsUtils.Encrypt(UserPassword.Trim(), true);
                            else
                                newXMLSettings.Password = string.Empty;

                            newXMLSettings.Username = UserName.Trim();
                            XMLHelpers.UpdateSettingsXML(newXMLSettings);
                            return UserAuthenticationResult.Authenticated;
                        }
                        else
                        {
                            XMLHelpers.UpdateSettingsXML(new XMLSettings() { Username = string.Empty, Password = string.Empty });
                            return UserAuthenticationResult.InvalidUsernameOrPassword;
                        }
                    }

                    else
                        return UserAuthenticationResult.InvalidUsernameOrPassword;
                }
            }
        }

        private void SetUsernamePasswordError(UserAuthenticationResult authenticationResult)
        {
            String errorText = string.Empty;
            if(authenticationResult == UserAuthenticationResult.InvalidUsernameOrPassword)
            {
                errorText = "Invalid username or password";
                ShowError(true, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.RoleNotAssigned)
            {
                errorText = "Please email " + BluePrintsResources.ITEmail + " to assign a role to you";
                ShowError(false, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.UsernameNotAdded)
            {
                errorText = "Please email " + BluePrintsResources.ITEmail + " to add you as a BluePrint user";
                ShowError(false, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.ActiveDirectoryError)
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

            if(mainWindow != null)
                mainWindow.Hide();

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

        public void Window_KeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }
    }
}