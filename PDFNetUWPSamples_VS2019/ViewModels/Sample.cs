using PDFNetUniversalSamples.Common;
using pdftron.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;

namespace PDFNetUniversalSamples.ViewModels
{
    public class SampleFile
    {
        public string FileName { get; private set; }
        public string Path { get; private set; }
        public string Folder { get; private set; }

        public SampleFile(string fullPath)
        {
            FileName = System.IO.Path.GetFileName(fullPath);
            Folder = System.IO.Path.GetDirectoryName(fullPath);
            Path = fullPath;
        }
    }


    public class Sample : ViewModelBase
    {
        public static String OutputPath { get { return Windows.Storage.ApplicationData.Current.TemporaryFolder.Path; } } // writable
        public static String InputPath { get { return System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "TestFiles"); } } 

        // Runs the selected Sample
        public RelayCommand RunSampleCommand { get; private set; }

        // Opens a clicked file.
        public System.Windows.Input.ICommand OutputFileClickedCommand { get; private set; }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            private set
            {
                if (string.Compare(value, _Description) != 0)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            private set
            {
                if (string.Compare(value, _Name) != 0)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ConsoleOutput;
        public string ConsoleOutput 
        {
            get { return _ConsoleOutput; }
            private set
            {
                if (string.Compare(value, _ConsoleOutput) != 0)
                {
                    _ConsoleOutput = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<SampleFile> _OutputFiles;
        public ObservableCollection<SampleFile> OutputFiles 
        {
            get { return _OutputFiles; } 
        }

        private bool _IsOutputTextChanged;
        public bool IsOutputTextChanged
        {
            get { return _IsOutputTextChanged; }
            private set
            {
                if (value != _IsOutputTextChanged)
                {
                    _IsOutputTextChanged = value;
                    RaisePropertyChanged();
                }
            }
        }


        #region Text Output
        protected void Flush()
        {
            this.Flushpublic();
        }

        protected async void Flushpublic()
        {
            await this._Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() => {
                if (this._BufferQueue.Count > 0) {
                    String buffer = String.Empty;
                    this._BufferQueue.TryDequeue(out buffer);

                    // ensure the string is not null;
                    if (string.IsNullOrEmpty(ConsoleOutput))
                    {
                        ConsoleOutput = string.Empty;
                    }

                    // limit the contents to 0x100000 characters
                    if (ConsoleOutput.Length >= 0x1000000)
                    {
                        ConsoleOutput = ConsoleOutput.Substring(0, buffer.Length + 0x10);
                    }
                    ConsoleOutput += buffer;

                    IsOutputTextChanged = true;
                    IsOutputTextChanged = false;
                }
            }));
        }

        protected void Write(String message)
        {
            this._BufferQueue.Enqueue(message);
            this.Flushpublic();
        }

        public void WriteLine(String message)
        {
            this.Write(message + Environment.NewLine);
        }

        protected void ClearConsole()
        {
            ConsoleOutput = String.Empty;
        }

        #endregion Text Output

        private ConcurrentQueue<String> _BufferQueue = null;
        private CoreDispatcher _Dispatcher = null;


        public Sample(string name, string description)
        {
            Name = name;
            Description = description;
            ConsoleOutput = string.Empty;
            _OutputFiles = new ObservableCollection<SampleFile>();

            _Dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            _BufferQueue = new ConcurrentQueue<string>();

            RunSampleCommand = new RelayCommand(RunAsyncCommand);
            OutputFileClickedCommand = new RelayCommand(OpenFile);
        }

        public async void RunAsyncCommand(object sample)
        {
            ClearOutput();
            await RunAsync();
        }

        // specific sample should override
        public virtual IAsyncAction RunAsync()
        {
            return System.Threading.Tasks.Task.Delay(10).AsAsyncAction();
        }

        public async void OpenFile(object selectedItem)
        {
            SampleFile sampleFile = selectedItem as SampleFile;
            if (sampleFile != null)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(sampleFile.Path);
                bool success = await Windows.System.Launcher.LaunchFileAsync(file);
            }
        }

        public async System.Threading.Tasks.Task AddFileToOutputList(string fileName)
        {
            await this._Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                OutputFiles.Add(new SampleFile(fileName));
            }));
        }

        public void ClearOutput()
        {
            OutputFiles.Clear();
            ClearConsole();
        }

        public static string GetExceptionMessage(System.Exception e, string extra = "", [CallerFilePath]string fileName = "", [CallerMemberName]string functionName = "")
        {
            pdftron.Common.PDFNetException ex = new PDFNetException(e.HResult);
            string message = "Exception in " + fileName + " inside " + functionName + Environment.NewLine;
            if (!string.IsNullOrWhiteSpace(extra))
            {
                message += extra + Environment.NewLine;
            }
            if (ex.IsPDFNetException)
            {
                message += string.Format("\"{0}\" at line {1} in {2}::{3}", ex.Message, ex.LineNumber, ex.Function, ex.FileName);
            }
            else
            {
                message += string.Format("\"{0}\" at {1}", e.Message, e.StackTrace);
            }

            System.Diagnostics.Debug.WriteLine("***********************************************************\n" + message);
            return message;
        }
    }
}
