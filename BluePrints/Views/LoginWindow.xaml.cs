using BluePrints.ViewModels;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Validation;
using DevExpress.XtraEditors.DXErrorProvider;
using System.Windows;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LoginViewModel viewModel = LoginViewModel.Create();
            viewModel.ShowErrorCallBack = ShowErrorCallBack;
            viewModel.HideControlCallBack = Hide;
            viewModel.ShowControlCallBack = Show;
            DataContext = viewModel;

            if (viewModel.UserName == string.Empty)
                txtUsername.Focus();
            else
                txtPassword.Focus();
        }

        public void ShowErrorCallBack(bool isPasswordField, string errorMessage)
        {
            BaseValidationError error = null;
            if (errorMessage != null)
                error = new BaseValidationError(errorMessage, null, ErrorType.Warning);

            if (isPasswordField)
                BaseEditHelper.SetValidationError(txtPassword, error);
            else
                BaseEditHelper.SetValidationError(txtUsername, error);
        }
    }
}