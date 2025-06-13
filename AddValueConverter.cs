using System;
using System.Globalization;
using System.Windows.Data;

namespace AgendaDashboard;

// AddValueConverter is a value converter that adds a specified value to the input.
public class AddValueConverter : IValueConverter
{
    public double Addend { get; set; } // Size values in WPF are double

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return d + Addend;
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}