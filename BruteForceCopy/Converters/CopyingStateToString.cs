using BruteForceCopy.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BruteForceCopy.Converters
{
    [ValueConversion(typeof(CopyingState), typeof(string))]
    public class CopyingStateToString : BaseConverter<CopyingState, string>
    {
        Dictionary<CopyingState, string> messages;
        public CopyingStateToString()
        {
            messages = new Dictionary<CopyingState, string>();
            messages.Add(CopyingState.Waiting, "Waiting for start...");
            messages.Add(CopyingState.CoutingFiles, "Recursively counting files...");
            messages.Add(CopyingState.Copying, "Copying files...");
            messages.Add(CopyingState.Success, "Successfully copied files, please see the logs...");
            messages.Add(CopyingState.Error, "Error on copy operation. Some files cannot be copied after several tries!");
        }

        public override string Convert(CopyingState value, object parameter, CultureInfo culture)
        {
            return messages.TryGetValue(value, out string msg) ? msg : "Unknown";   
        }

        public override CopyingState ConvertBack(string value, object parameter, CultureInfo culture)
        {
            for(int i = 0, n = messages.Count; i < n; i++)
            {
                if (value == messages.ElementAt(i).Value)
                    return messages.ElementAt(i).Key;
            }
            return CopyingState.Unknown;
        }
    }
}
