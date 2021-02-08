using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace BruteForceCopy.Converters
{
    public abstract class BaseConverter<TFrom, TTarget> : IValueConverter
    {
        public abstract TTarget Convert(TFrom value, object parameter, CultureInfo culture);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TFrom casted = value == null ? default(TFrom) : (TFrom)value;
            return Convert(casted, parameter, culture);
        }

        public abstract TFrom ConvertBack(TTarget value, object parameter, CultureInfo culture);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var casted = value == null ? default(TTarget) : (TTarget)value;
            return ConvertBack(casted, parameter, culture);
        }
    }
}