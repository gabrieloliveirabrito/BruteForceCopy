using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ookii.Dialogs.Wpf;

namespace BruteForceCopy.Models
{
    using Enums;
    public partial class MainModel
    {
        public RelayCommand<MainWindow> SelectFromFolderCommand { get; set; }
        public RelayCommand<MainWindow> SelectToFolderCommand { get; set; }
        public RelayCommand<MainWindow> StartCopyCommand { get; set; }
        public RelayCommand<MainWindow> TryReadBufferCommand { get; set; }

        protected void InitializeMethods()
        {
            State = CopyingState.Waiting;
            SelectFromFolderCommand = new RelayCommand<MainWindow>(SelectFromFolder);
            SelectToFolderCommand = new RelayCommand<MainWindow>(SelectToFolder);
            StartCopyCommand = new RelayCommand<MainWindow>((mw) => Task.Run(() => StartCopy(mw)));
            TryReadBufferCommand = new RelayCommand<MainWindow>((mw) => Task.Run(() => TryReadBuffer(mw)));
        }

        private void SelectFromFolder(MainWindow obj)
        {
            var dialog = new VistaFolderBrowserDialog();

            if (dialog.ShowDialog(obj).GetValueOrDefault())
            {
                FromFolder = dialog.SelectedPath;
            }
        }

        private void SelectToFolder(MainWindow obj)
        {
            var dialog = new VistaFolderBrowserDialog();

            if (dialog.ShowDialog(obj).GetValueOrDefault())
            {
                ToFolder = dialog.SelectedPath;
            }
        }

        void ShowWarning(MainWindow window, string msg)
        {
            using (var dialog = new TaskDialog())
            {
                dialog.MainIcon = TaskDialogIcon.Warning;
                dialog.WindowTitle = "AVISO!!";
                dialog.Content = msg;
                dialog.CenterParent = true;
                dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));

                dialog.ShowDialog(window);
            }
        }

        void AppendLog(string msg)
        {
            Log += msg + Environment.NewLine;
        }

        private async Task StartCopy(MainWindow arg)
        {
            if (string.IsNullOrEmpty(FromFolder))
                ShowWarning(arg, "The \"From Folder\" input is empty!");
            else if (!Directory.Exists(FromFolder))
                ShowWarning(arg, "The \"From Folder\" hasn't exists!");
            else if (string.IsNullOrEmpty(ToFolder))
                ShowWarning(arg, "The \"To Folder\" input is empty!");
            else
            {
                if (!Directory.Exists(ToFolder))
                    Directory.CreateDirectory(ToFolder);

                IsCopying = true;
                Total = 0;
                CurrentIndex = 0;
                BufferPosition = 0;
                BufferLength = 0;

                var files = new List<FileInfo>();

                State = CopyingState.CoutingFiles;
                AppendLog("Counting files...");
                await CountAsync(FromFolder, files);

                AppendLog("Waiting 2 seconds for cooling...");
                await Task.Delay(2000);

                AppendLog("Ordering from small to big file...");
                files = files.OrderBy(f => f.Length).ToList();

                AppendLog($"Total of {files.Count} fetched!");
                State = CopyingState.Copying;
                foreach (var file in files.Select(f => f.FullName))
                {
                    CurrentIndex++;

                    if (!await TryCopyFile(file))
                    {
                        AppendLog($"ERROR on copying file {Path.GetFileName(file)}!");
                        AppendLog("Waiting 2 seconds for cooling...");
                        await Task.Delay(2000);
                    }

                    if (CurrentIndex % 10 == 0)
                    {
                        AppendLog("Waiting 2 seconds for cooling...");
                        await Task.Delay(2000);
                    }
                }

                if (State != CopyingState.Error)
                    State = CopyingState.Success;
                IsCopying = false;
            }
        }

        private async Task CountAsync(string path, List<FileInfo> files)
        {
            AppendLog($"Counting files of directory {path}..");

            var dirFiles = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => !_IgnoreSmallFiles || Path.GetFileName(f)[0] != '.')
                .Select(f => new FileInfo(f))
                .Where(fi => !_IgnoreSmallFiles || fi.Length > 1024);
            files.AddRange(dirFiles);

            var directories = Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var directoryPath in directories)
            {
                var dirName = Path.GetFileName(directoryPath);
                if (!_IgnoreDirs.Split(',').Any(d => d == dirName) && (!_IgnoreDotDirs || dirName[0] != '.'))
                {
                    var targetFile = directoryPath.Remove(0, FromFolder.Length);
                    var outputPath = Path.Combine(ToFolder, targetFile);

                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(outputPath);

                    await CountAsync(directoryPath, files);
                }
            }

            Total += dirFiles.Count();
            if (dirFiles.Count() > 0)
                await Task.Delay(10);
        }

        private async Task<bool> TryCopyFile(string filename)
        {
            byte[] buffer = new byte[512 * 1024];

            var targetFile = filename.Remove(0, FromFolder.Length);
            var outputPath = Path.Combine(ToFolder, targetFile);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    AppendLog($"Try to copy file {targetFile}");

                    if (File.Exists(outputPath))
                    {
                        AppendLog($"File {targetFile} already exists, overwriting...");
                        File.Delete(outputPath);
                    }

                    BufferPosition = 0;
                    BufferLength = 0;

                    using (var input = File.OpenRead(filename))
                    using (var output = File.Create(outputPath))
                    {
                        BufferLength = input.Length;
                        AppendLog($"Buffering file {targetFile}...");

                        int readed = 0;
                        while ((readed = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, readed);
                            BufferPosition += readed;
                            await Task.Delay(15);
                        }

                        AppendLog($"File {targetFile} copied successfully!");
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    AppendLog(ex.Message + Environment.NewLine + ex.StackTrace);
                    AppendLog($"Error on copying file {targetFile}, try again after 30 seconds for cooling storage!");
                    await Task.Delay(30000);
                }
            }

            AppendLog($"Failed on copy file {targetFile} after 10 tries!");
            return false;
        }

        private async Task TryReadBuffer(MainWindow arg)
        {
            try
            {
                using (var dialog = new TaskDialog())
                {
                    dialog.MainIcon = TaskDialogIcon.Warning;
                    dialog.WindowTitle = "AVISO!!";
                    dialog.Content = "Copy all files that cannot be recovered from software to an folder, and select on next dialog. Are you ready?";
                    dialog.CenterParent = true;
                    dialog.Buttons.Add(new TaskDialogButton(ButtonType.Yes));
                    dialog.Buttons.Add(new TaskDialogButton(ButtonType.No));

                    var btn = dialog.ShowDialog(arg);
                    if (btn.ButtonType == ButtonType.No)
                        return;
                }

                var folderDialog = new VistaFolderBrowserDialog();
                if (folderDialog.ShowDialog(arg).GetValueOrDefault())
                {
                    IsCopying = true;
                    State = CopyingState.CoutingFiles;
                    CurrentIndex = 0;
                    Total = 0;
                    BufferLength = 0;
                    BufferPosition = 0;

                    var recoverTryFolder = folderDialog.SelectedPath;
                    var recoveredPath = Path.Combine(recoverTryFolder, "Recovered");
                    var nonRecoveredFiles = Directory.GetFiles(recoverTryFolder, "*.*", SearchOption.TopDirectoryOnly);

                    AppendLog($"Search for files on folder...");
                    var storageFiles = new List<FileInfo>();
                    await FindNonRecoveredFiles(_FromFolder, nonRecoveredFiles, storageFiles);

                    AppendLog("Ordering from small to big file...");
                    storageFiles = storageFiles.OrderBy(f => f.Length).ToList();

                    if (!Directory.Exists(recoveredPath))
                        Directory.CreateDirectory(recoveredPath);

                    var chunk = new byte[256];

                    State = CopyingState.Copying;
                    Total = storageFiles.Count;
                    foreach (var filename in storageFiles.Select(f => f.FullName))
                    {
                        var targetFile = filename.Remove(0, FromFolder.Length);
                        var outputPath = Path.Combine(recoveredPath, targetFile);

                        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                        AppendLog($"Try to read file {targetFile}");

                        if (File.Exists(outputPath))
                        {
                            AppendLog($"File {targetFile} already exists, overwriting...");
                            File.Delete(outputPath);
                        }

                        BufferPosition = 0;
                        BufferLength = 0;
                        
                        var withError = false;
                        var errorChunk = false;

                        using (var input = File.OpenRead(filename))
                        using (var output = File.Create(outputPath))
                        {
                            BufferLength = input.Length;
                            AppendLog($"Buffering file {targetFile}...");

                            while (true)
                            {
                                int readed = 0;
                                try
                                {
                                    if (errorChunk)
                                    {
                                        readed = input.ReadByte();
                                        if (readed != -1)
                                        {
                                            errorChunk = false;
                                            output.WriteByte(Convert.ToByte(readed));
                                            await output.FlushAsync();
                                        }
                                        else break;
                                    }
                                    else
                                    {
                                        readed = await input.ReadAsync(chunk, 0, chunk.Length);
                                        if (readed > 0)
                                        {
                                            await output.WriteAsync(chunk, 0, readed);
                                            await output.FlushAsync();
                                        }
                                        else break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorChunk = true;
                                    AppendLog(ex.Message + Environment.NewLine + ex.StackTrace);
                                    AppendLog($"Failed to read a byte {_BufferPosition} of {targetFile}, appending 0x0 and try again after 5 seconds for cooling storage!");
                                    await Task.Delay(100);
                                }
                                finally
                                {
                                    BufferPosition += errorChunk ? 1 : readed;
                                    if (BufferPosition % (1024 * 1024) == 0)
                                        await Task.Delay(100);
                                }
                            }

                            AppendLog($"File {targetFile} copied {(withError ? "with errors/corrupted" : "successfully")}!");
                        }
                        CurrentIndex++;
                    }

                    State = CopyingState.Success;
                    IsCopying = false;
                }
                else
                    ShowWarning(arg, "Folder browser dialog has been canceled!");
            }
            catch(Exception ex)
            {
                AppendLog(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private async Task FindNonRecoveredFiles(string path, string[] nonRecoveredFiles, List<FileInfo> files)
        {
            AppendLog($"Counting files of directory {path}..");

            var dirFiles = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => !_IgnoreSmallFiles || Path.GetFileName(f)[0] != '.')
                .Where(f => nonRecoveredFiles.Any(r => Path.GetFileName(r) == Path.GetFileName(f)))
                .Select(f => new FileInfo(f))
                .Where(fi => !_IgnoreSmallFiles || fi.Length > 1024);
            files.AddRange(dirFiles);

            var directories = Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var directoryPath in directories)
            {
                var dirName = Path.GetFileName(directoryPath);
                if (!_IgnoreDirs.Split(',').Any(d => d == dirName) && (!_IgnoreDotDirs || dirName[0] != '.'))
                {
                    var targetFile = directoryPath.Remove(0, FromFolder.Length);
                    var outputPath = Path.Combine(ToFolder, targetFile);

                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(outputPath);

                    await FindNonRecoveredFiles(directoryPath, nonRecoveredFiles, files);
                }
            }
        }
    }
}