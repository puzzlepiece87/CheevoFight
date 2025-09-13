using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CheevoFight.ViewPlusViewModel.Converters
{
    public class GetResultsProgressBarColor : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) >= 50 ? Brushes.Blue : Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
