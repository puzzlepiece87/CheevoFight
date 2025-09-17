using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CheevoFight.ViewPlusViewModel.Converters
{
    public class GetResultsProgressBarValueColumn0 : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueAsInt = System.Convert.ToInt32(value);

            if (valueAsInt >= 50)
            {
                return (valueAsInt - 50).ToString();
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
