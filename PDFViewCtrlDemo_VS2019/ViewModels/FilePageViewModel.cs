using pdftron.Filters;
using pdftron.PDF;
using pdftron.SDF;
using PDFViewCtrlDemo_Windows10.Resources;
using PDFViewCtrlDemo_Windows10.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace PDFViewCtrlDemo_Windows10.ViewModels
{
    public class FilePageViewModel : Common.ViewModelBase
    {
        private static List<string> _NON_PDF_FILE_TYPES = null;
        private static List<string> NON_PDF_FILE_TYPES
        {
            get
            {
                if (_NON_PDF_FILE_TYPES == null)
                {
                    _NON_PDF_FILE_TYPES = new List<string>();
                    _NON_PDF_FILE_TYPES.Add(".pptx");
                    _NON_PDF_FILE_TYPES.Add(".docx");
                    _NON_PDF_FILE_TYPES.Add(".doc");
                    _NON_PDF_FILE_TYPES.Add(".cbz");
                    _NON_PDF_FILE_TYPES.Add(".jpg");
                    _NON_PDF_FILE_TYPES.Add(".gif");
                    _NON_PDF_FILE_TYPES.Add(".jpeg");
                    _NON_PDF_FILE_TYPES.Add(".png");
                    _NON_PDF_FILE_TYPES.Add(".txt");
                    _NON_PDF_FILE_TYPES.Add(".xml");
                    _NON_PDF_FILE_TYPES.Add(".md");
                    _NON_PDF_FILE_TYPES.Add(".xlsx");
                    _NON_PDF_FILE_TYPES.Add(".xls");
                }
                return _NON_PDF_FILE_TYPES;
            }
        }

        public FilePageViewModel()
        {
            InitCommands();
        }


        #region Properties

        private string _UrlString = "http://pdftron.s3.amazonaws.com/downloads/pdfref.pdf";
        public string UrlString
        {
            get { return _UrlString; }
            set { Set(ref _UrlString, value); }
        }

        private bool _IsURLDialogOpen = false;
        public bool IsURLDialogOpen
        {
            get { return _IsURLDialogOpen; }
            set
            {
                if (Set(ref _IsURLDialogOpen, value))
                {
                    if (_IsURLDialogOpen)
                    {
                        IsPasswordDialogOpen = false;
                    }
                    RaisePropertyChanged("AreAdditonalDialogsClosed");
                }
            }
        }

        public bool AreAdditonalDialogsClosed
        {
            get { return !IsURLDialogOpen && !IsPasswordDialogOpen; }
        }

        #endregion Properties


        #region Commands

        private void InitCommands()
        {
            BrowseCommand = new RelayCommand(BrowseCommandImpl);
            GettingStartedCommand = new RelayCommand(GettingStartedCommandImpl);
            ShowOpenURLDialogCommand = new RelayCommand(ShowOpenURLDialogCommandImpl);
            OpenURLCommand = new RelayCommand(OpenURLCommandImpl);
        }

        public RelayCommand BrowseCommand { get; private set; }
        public RelayCommand GettingStartedCommand { get; private set; }
        public RelayCommand ShowOpenURLDialogCommand { get; private set; }
        public RelayCommand OpenURLCommand { get; private set; }

        private void BrowseCommandImpl(object parameter)
        {
            IsURLDialogOpen = false;
            string param = parameter as string;
            if (!string.IsNullOrEmpty(param) && param.Equals("NonPDF", StringComparison.OrdinalIgnoreCase))
            {
                Browse(false);
            }
            else
            {
                Browse(true);
            }
        }

        private void GettingStartedCommandImpl(object parameter)
        {
            IsURLDialogOpen = false;
            OpenGettingStarted();
        }

        private void ShowOpenURLDialogCommandImpl(object parameter)
        {
            IsURLDialogOpen = true;
        }

        private void OpenURLCommandImpl(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(UrlString))
            {
                NavigationHelper.GoToViewerPage(UrlString);
            }
        }

        #endregion Commands


        #region Impl

        private bool _FilePickerOpen = false;
        private async void Browse(bool pdfs)
        {
            if (_FilePickerOpen)
            {
                return;
            }

            try
            {
                _FilePickerOpen = true;
                ClosePasswordDialog(false);
                FileOpenPicker picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.List;
                if (pdfs)
                {
                    picker.FileTypeFilter.Add(".pdf");
                }
                else
                {
                    foreach (string fileType in NON_PDF_FILE_TYPES)
                    {
                        picker.FileTypeFilter.Add(fileType);
                    }
                }
                StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    if (pdfs)
                    {
                        Open(file);
                    }
                    else
                    {
                        if (file.FileType == ".txt" || file.FileType == ".xml" || file.FileType == ".md")
                        {
                            await OpenTextToPDF(file);
                        }
                        else
                        {
                            IRandomAccessStream inStream = await file.OpenReadAsync();
                            OpenNonPDF(inStream);                            
                        }
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                _FilePickerOpen = false;
            }

        }

        private void Open(StorageFile file)
        {
            if (file == null)
            {
                return;
            }

            PDFDoc doc = new PDFDoc(file);
            if (!doc.InitSecurityHandler())
            {
                IsPasswordDialogOpen = true;
                PasswordViewModel = new PasswordViewModel(doc);
                PasswordViewModel.PasswordFinished += PasswordViewModel_PasswordFinished;
            }
            else
            {
                NavigationHelper.GoToViewerPage(doc);
            }
        }

        private void OpenNonPDF(IRandomAccessStream inStream)
        {
            IFilter filter = new RandomAccessStreamFilter(inStream);

            // Also consider adding this in App.xaml.cs at startup instead:
            // pdftron.PDFNet.AddResourceSearchPath(System.IO.Path.Combine(Package.Current.InstalledLocation.Path, "Resources"));
            OfficeToPDFOptions opts = new OfficeToPDFOptions();
            opts.SetLayoutResourcesPluginPath(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Resources"));
            opts.SetSmartSubstitutionPluginPath(System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Resources"));

            DocumentConversion conversion = pdftron.PDF.Convert.StreamingPDFConversion(filter, opts);

            NavigationHelper.GoToViewerPage(conversion);
        }

        private async Task OpenTextToPDF(StorageFile storageFile)
        {
            // Create objects to add Conversion Options
            var mObjSet = new ObjSet();
            var mObj = mObjSet.CreateDict();

            // Add formating options
            mObj.PutNumber("FontSize", 12);
            mObj.PutBool("UseSourceCodeFormatting", true);
            mObj.PutNumber("PageWidth", 12);
            mObj.PutNumber("PageHeight", 12);

            PDFDoc doc = new PDFDoc();
            await pdftron.PDF.Convert.FromTextAsync(doc, storageFile, mObj);

            NavigationHelper.GoToViewerPage(doc);
        }

        private void PasswordViewModel_PasswordFinished(PDFDoc doc, bool success)
        {
            if (success)
            {
                NavigationHelper.GoToViewerPage(doc);
            }
            ClosePasswordDialog(success);
        }

        private void ClosePasswordDialog(bool docOpened)
        {
            if (PasswordViewModel != null)
            {
                if (!docOpened)
                {
                    PasswordViewModel.Doc.Dispose();
                }
                IsPasswordDialogOpen = false;
                PasswordViewModel.PasswordFinished -= PasswordViewModel_PasswordFinished;
                PasswordViewModel = null;
            }
        }

        private async void OpenGettingStarted()
        {
            ClosePasswordDialog(false);
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Resources");
            if (folder != null)
            {
                StorageFile file = await folder.GetFileAsync("GettingStarted.pdf");
                if (file != null)
                {
                    Open(file);
                }
            }
        }

        #endregion Impl

        #region Password

        private bool _IsPasswordDialogOpen = false;
        public bool IsPasswordDialogOpen
        {
            get { return _IsPasswordDialogOpen; }
            set
            {
                Set(ref _IsPasswordDialogOpen, value);
                if (_IsPasswordDialogOpen)
                {
                    IsURLDialogOpen = false;
                }
                RaisePropertyChanged("AreAdditonalDialogsClosed");
            }
        }

        private PasswordViewModel _PasswordViewModel = null;
        public PasswordViewModel PasswordViewModel
        {
            get { return _PasswordViewModel; }
            set { Set(ref _PasswordViewModel, value); }
        }

        #endregion Password

    }
}
