using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CheevoFight.ViewPlusViewModel.Converters
{
    public class GetProductGivenFactors : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var product = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
            return product.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
