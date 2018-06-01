using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BluePrints.Common.ViewModel.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapImage returnImage;

            if (value == DependencyProperty.UnsetValue || value == null)
            {
                var a = (DXImageInfo) new DXImageConverter().ConvertFromString("Cancel_16x16.png");
                returnImage = new BitmapImage(a.MakeUri());
            }
            else
            {
                var a = (DXImageInfo) new DXImageConverter().ConvertFromString(value + "_16x16.png");
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