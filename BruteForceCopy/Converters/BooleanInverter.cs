using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BruteForceCopy.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanInverter : BaseConverter<bool, bool>
    {
        public override bool Convert(bool value, object parameter, CultureInfo culture)
        {
            return !value;
        }

        public override bool ConvertBack(bool value, object parameter, CultureInfo culture)
        {
            return !value;
        }
    }
}
