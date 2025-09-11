using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CheevoFight.ViewPlusViewModel.Converters
{
    public class GetResultsProgressBarAngle : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueAsInt = System.Convert.ToInt32(value);
            return valueAsInt >= 50 ? 180.ToString() : 0.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
