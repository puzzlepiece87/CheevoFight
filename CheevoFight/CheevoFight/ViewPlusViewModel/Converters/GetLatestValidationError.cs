using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CheevoFight.ViewPlusViewModel.Converters
{
    public class GetLatestValidationError : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) { return null; }

            var allErrors = value as ReadOnlyObservableCollection<ValidationError>;

            if (allErrors.Count == 0) { return null; }

            return allErrors[allErrors.Count - 1].ErrorContent.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
