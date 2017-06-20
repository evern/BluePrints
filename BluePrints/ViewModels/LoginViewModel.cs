using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class LoginViewModel
    {
        public string UserName { get; set; }
        public string UserPassword { get; set; }
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
        bool isUsernameLoadedFromXML;
        public LoginViewModel()
        {
            CacheMainWindow();
            delayedHideDispatcher = new DispatcherTimer();
            delayedHideDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedHideDispatcher.Tick += delayedHideDispatcher_Tick;
            USERS = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork().USERS.AsEnumerable();
            UserName = XMLHelpers.GetSettings_Username();
            if (UserName != string.Empty)
                isUsernameLoadedFromXML = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => EVERNPCLogin()));
        }

        private void delayedHideDispatcher_Tick(object sender, EventArgs e)
        {
            delayedHideDispatcher.Stop();
            if (HideControlCallBack != null)
                HideControlCallBack();
        }

        public void EVERNPCLogin()
        {
#if DEBUG
            if (Environment.MachineName == "EVERN-PC")
            {
                UserName = BluePrintsResources.AdminUsername;
                UserPassword = BluePrintsResources.AdminPassword;
                delayedHideDispatcher.Start();
                Login();
            }
#endif
        }

        public void Login()
        {
            UserAuthenticationResult authenticationResult = UserAuthenticate;

            if (authenticationResult == UserAuthenticationResult.Authenticated || UserName == BluePrintsResources.AdminUsername && UserPassword == BluePrintsResources.AdminPassword)
            {
                if (UserName == BluePrintsResources.AdminUsername)
                    LoginCredentials.CurrentUser = new USER() { NAME = BluePrintsResources.AdminUsername };
                    //LoginCredentials.CurrentUser = USERS.FirstOrDefault(x => x.NAME.ToUpper() == "SU.BING-WEN");
                else
                    LoginCredentials.CurrentUser = USERS.FirstOrDefault(x => x.NAME.ToUpper() == UserName.ToUpper());

                LoginCredentials.CurrentHWID = CommonMethods.GetHWID();
                SignalR.ConnectAsync();
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

                    if (UserName != null && UserPassword != null)
                    {
                        if(!isUsernameLoadedFromXML)
                        {
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
                        }

                        bool? result = ActiveDirectory.Authenticate(UserName, UserPassword);
                        if (result == null)
                            return UserAuthenticationResult.ActiveDirectoryError;

                        if (((bool)result))
                        {
                            ShowError(false, null);
                            ShowError(true, null);
                            XMLHelpers.UpdateSettingsXML(new XMLSettings() { Username = UserName.Trim() });
                            return UserAuthenticationResult.Authenticated;
                        }
                        else
                        {
                            XMLHelpers.UpdateSettingsXML(new XMLSettings() { Username = string.Empty });
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
                ShowError(false, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.RoleNotAssigned)
            {
                errorText = "Please ask pete to assign a role to you";
                ShowError(false, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.UsernameNotAdded)
            {
                errorText = "Please ask pete to add you as a BluePrint user";
                ShowError(false, errorText);
            }
            else if (authenticationResult == UserAuthenticationResult.ActiveDirectoryError)
            {
                errorText = "Active directory not responding";
                ShowError(false, errorText);
            }
        }

        public Action ShowControlCallBack;
        public Action HideControlCallBack;
        public Action<bool, string> ShowErrorCallBack;

        public void ShowThisControl()
        {
            if (ShowControlCallBack != null)
                ShowControlCallBack();
        }

        public void ShowError(bool isPasswordField, string errorMessage)
        {
            ShowErrorCallBack?.Invoke(isPasswordField, errorMessage);
        }
        
        private async void CacheMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow = null;
        }

        public void ShowMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        public void Window_KeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }
    }
}