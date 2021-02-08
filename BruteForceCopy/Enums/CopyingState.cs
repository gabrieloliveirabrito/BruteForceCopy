using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BruteForceCopy.Enums
{
    public enum CopyingState
    {
        Unknown,
        Waiting,
        CoutingFiles,
        Copying,
        Error,
        Success
    }
}
