using BluePrints.ViewModels;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Validation;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            var viewModel = LoginViewModel.Create();
            viewModel.ShowErrorCallBack = this.ShowErrorCallBack;
            viewModel.HideControlCallBack = this.Hide;
            viewModel.ShowControlCallBack = this.Show;
            this.DataContext = viewModel;

            if (txtUsername.Text == string.Empty)
                txtUsername.Focus();
            else
                txtPassword.Focus();
        }

        public void ShowErrorCallBack(bool isPasswordField, string errorMessage)
        {
            BaseValidationError error = null;
            if (errorMessage != null)
                error = new BaseValidationError(errorMessage, null, ErrorType.Warning);

            if(isPasswordField)
                BaseEditHelper.SetValidationError(txtPassword, error);
            else
                BaseEditHelper.SetValidationError(txtUsername, error);
        }
    }
}
