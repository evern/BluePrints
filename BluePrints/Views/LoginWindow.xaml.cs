using BluePrints.ViewModels;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Validation;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BluePrints.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow : DXWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            if (txtUsername.Text == string.Empty)
                txtUsername.Focus();
            else
                txtPassword.Focus();

            imagePrimeroIcon.Source = new BitmapImage(new Uri(@"/Common/Images/PRIMERO.jpg", UriKind.Relative));

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