using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace pacstallion.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return Brush.Parse("#3399FF"); 
        }
        
        return Brush.Parse("#808080"); 
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}