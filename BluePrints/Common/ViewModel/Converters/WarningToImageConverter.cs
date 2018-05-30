using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BluePrints.Common.ViewModel.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class WarningToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapImage returnImage;

            if (value == DependencyProperty.UnsetValue || value == null)
            {
                var a = (DXImageInfo) new DXImageConverter().ConvertFromString("Cancel_16x16.png");
                returnImage = new BitmapImage(a.MakeUri());
            }

            if (value.ToString().ToUpper() == "CRITICAL")
            {
                var a = (DXImageInfo) new DXImageConverter().ConvertFromString("Cancel_16x16.png");
                returnImage = new BitmapImage(a.MakeUri());
            }
            else
            {
                var a = (DXImageInfo) new DXImageConverter().ConvertFromString("Warning_16x16.png");
                returnImage = new BitmapImage(a.MakeUri());
            }

            return returnImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}