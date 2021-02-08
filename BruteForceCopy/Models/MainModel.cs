using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BruteForceCopy.Enums;
using Microsoft.Win32;

namespace BruteForceCopy.Models
{
    public partial class MainModel : BaseModel
    {
        RegistryKey registry;

        public static readonly DependencyProperty MainWindowProperty = DependencyProperty.Register(nameof(MainWindow), typeof(MainWindow), typeof(MainModel));
        public MainWindow MainWindow
        {
            get => (MainWindow)GetValue(MainWindowProperty);
            set => SetValue(MainWindowProperty, value);
        }

        public MainModel()
        {
            InitializeMethods();
            registry = Registry.CurrentUser.OpenSubKey("BruteForceCopy", true) ?? Registry.CurrentUser.CreateSubKey("BruteForceCopy", true);

            var fromFolder = registry.GetValue("FromFolder");
            if (fromFolder != null)
                _FromFolder = fromFolder.ToString();

            var toFolder = registry.GetValue("ToFolder");
            if (toFolder != null)
                _ToFolder = toFolder.ToString();

            var ignoreDirs = registry.GetValue("IgnoreDirs");
            if (ignoreDirs != null)
                _IgnoreDirs = ignoreDirs.ToString();

            var ignoreDotDirs = registry.GetValue("IgnoreDotDirs");
            if (ignoreDotDirs != null)
                _IgnoreDotDirs = Convert.ToInt16(ignoreDotDirs) == 1;

            var ignoreDotFiles = registry.GetValue("IgnoreDotFiles");
            if (ignoreDotFiles != null)
                _IgnoreDotFiles = Convert.ToInt16(ignoreDotFiles) == 1;

            var ignoreSmallFiles = registry.GetValue("IgnoreSmallFiles");
            if (ignoreSmallFiles != null)
                _IgnoreSmallFiles = Convert.ToInt16(ignoreSmallFiles) == 1;
        }

        protected override void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (field == null && value != null || field != null && !field.Equals(value))
            {
                field = value;

                    NotifyChanged(propertyName);
            }
        }

        private CopyingState _State;
        public CopyingState State
        {
            get => _State;
            set => Set(ref _State, value);
        }

        private bool _IsCopying;
        public bool IsCopying
        {
            get => _IsCopying;
            set => Set(ref _IsCopying, value);
        }

        private int _CurrentIndex;
        public int CurrentIndex
        {
            get => _CurrentIndex;
            set
            {
                Set(ref _CurrentIndex, value);
                NotifyChanged(nameof(TotalProgress));
            }
        }

        private int _Total;
        public int Total
        {
            get => _Total;
            set
            {
                Set(ref _Total, value);
                NotifyChanged(nameof(TotalProgress));
            }
        }
        
        public int TotalProgress
        {
            get => _Total == 0 ? 0 : (int)((double)_CurrentIndex / _Total * 100);
        }

        private string _Log;
        public string Log
        {
            get => _Log;
            set => Set(ref _Log, value);
        }

        private string _FromFolder;
        public string FromFolder
        {
            get => _FromFolder;
            set
            {
                Set(ref _FromFolder, value);
                registry.SetValue("FromFolder", value);
            }
        }

        private string _ToFolder;
        public string ToFolder
        {
            get => _ToFolder;
            set
            {
                Set(ref _ToFolder, value);
                registry.SetValue("ToFolder", value);
            }
        }

        private string _IgnoreDirs;
        public string IgnoreDirs
        {
            get => _IgnoreDirs;
            set
            {
                Set(ref _IgnoreDirs, value);
                registry.SetValue("IgnoreDirs", value);
            }
        }

        private long _BufferPosition;
        public long BufferPosition
        {
            get => _BufferPosition;
            set
            {
                Set(ref _BufferPosition, value);
                NotifyChanged(nameof(BufferProgress));
            }
        }

        private long _BufferLength;
        public long BufferLength
        {
            get => _BufferLength;
            set
            {
                Set(ref _BufferLength, value);
                NotifyChanged(nameof(BufferProgress));
            }
        }

        public int BufferProgress
        {
            get => _BufferLength == 0 ? 0 : (int)((double)_BufferPosition / _BufferLength * 100);
        }

        private bool _IgnoreDotDirs;
        public bool IgnoreDotDirs
        {
            get => _IgnoreDotDirs;
            set
            {
                Set(ref _IgnoreDotDirs, value);
                registry.SetValue("IgnoreDotDirs", value ? 1 : 0);
            }
        }

        private bool _IgnoreDotFiles;
        public bool IgnoreDotFiles
        {
            get => _IgnoreDotFiles;
            set
            {
                Set(ref _IgnoreDotFiles, value);
                registry.SetValue("IgnoreDotFiles", value ? 1 : 0);
            }
        }

        private bool _IgnoreSmallFiles;
        public bool IgnoreSmallFiles
        {
            get => _IgnoreSmallFiles;
            set
            {
                Set(ref _IgnoreSmallFiles, value);
                registry.SetValue("IgnoreSmallFiles", value ? 1 : 0);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MainModel();
        }
    }
}
