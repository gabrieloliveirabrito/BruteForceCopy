using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BruteForceCopy.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class ProgressToString : BaseConverter<long, string>
    {
        public override string Convert(long value, object parameter, CultureInfo culture)
        {
            
            return $"{value}%";
        }

        public override long ConvertBack(string value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
