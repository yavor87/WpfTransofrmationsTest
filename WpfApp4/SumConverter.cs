using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WpfApp4
{
    public class SumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double sum = values.Sum(c => System.Convert.ToDouble(c));
            return sum;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
