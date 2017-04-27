using DevExpress.Mvvm.UI;
using System.Windows;

namespace BluePrints.Common.Helpers
{
    public interface ICustomMessageBoxService
    {
        void Show();
    }

    public class CustomMessageBoxService : ServiceBase, ICustomMessageBoxService
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(CustomMessageBoxService),
                new PropertyMetadata("Hello"));

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        void ICustomMessageBoxService.Show()
        {
            MessageBox.Show(Message);
        }
    }
}