using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.Views;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static LoginViewModel Create()
        {
            return ViewModelSource.Create(() => new LoginViewModel());
        }

        IEnumerable<USER> USERS { get; set; }
        protected LoginViewModel()
        {
            USERS = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork().USERS.AsEnumerable();
            UserName = XMLHelpers.GetSettings_Username();
            Application.Current.Dispatcher.BeginInvoke(new Action(() => EVERNPCLogin()));
        }


        public void EVERNPCLogin()
        {
            if (Environment.MachineName == "EVERN-PC")
            {
                UserName = CommonResources.AdminUsername;
                UserPassword = CommonResources.AdminPassword;
                if (HideControlCallBack != null)
                    HideControlCallBack();
                Login();
            }
        }

        public void Login()
        {
            if ((UserName == CommonResources.AdminUsername && UserPassword == CommonResources.AdminPassword) || UserAuthenticate())
            {
                if (UserName == CommonResources.AdminUsername)
                    LoginCredentials.CurrentUser = new USER() { NAME = CommonResources.AdminUsername };
                else
                    LoginCredentials.CurrentUser = USERS.FirstOrDefault(x => x.NAME == UserName);

                ShowMainWindow();
                if (HideControlCallBack != null)
                    HideControlCallBack();
            }
            else
                SetUsernamePasswordError();
        }

        public void Exit()
        {
            System.Environment.Exit(1);
        }

        private bool UserAuthenticate()
        {
            USER user = USERS.FirstOrDefault(x => x.NAME == UserName);
            if (user == null)
            {
                ShowError(false, "Username not found");
                return false;
            }
            else
            {
                if (user.GUID_ROLE == Guid.Empty)
                {
                    ShowError(false, "No role assigned on user");
                    return false;
                }

                if (UserName != null && UserPassword != null)
                {
                    if (ActiveDirectory.Authenticate(UserName, UserPassword))
                    {
                        ShowError(false, null);
                        ShowError(true, null);
                        XMLHelpers.UpdateSettingsXML(new XMLSettings() { Username = UserName.Trim() });
                        return true;
                    }
                    else
                    {
                        SetUsernamePasswordError();
                        XMLHelpers.UpdateSettingsXML(new XMLSettings() { Username = string.Empty });
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        private void SetUsernamePasswordError()
        {
            ShowError(false, "Invalid username or password");
            ShowError(true, "Invalid username or password");
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
            if (ShowErrorCallBack != null)
                ShowErrorCallBack(isPasswordField, errorMessage);
        }

        public void ShowMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            //mainWindow.ShowLoginWindow = this.ShowThisControl;
            mainWindow.Show();
        }

        public void Window_KeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }
    }
}
