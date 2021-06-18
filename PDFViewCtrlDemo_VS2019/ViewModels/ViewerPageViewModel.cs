using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pdftron.PDF;
using PDFViewCtrlDemo_Windows10.ViewModels.Common;
using Windows.Storage.Pickers;
using Windows.Storage;
using pdftron.PDF.Tools;
using Windows.UI.Core;
using Windows.UI.Popups;
using pdftron.PDF.Tools.Controls;

namespace PDFViewCtrlDemo_Windows10.ViewModels
{
    public class ViewerPageViewModel : Common.ViewModelBase
    {
        public ViewerPageViewModel()
        {
            InitCommands();

            _IsOnPhone = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
        }

        public void Activate()
        {
            InitPDFViewCtrl();
            if (IsOnPhone)
            {
                if (PageNumberIndicator != null)
                {
                    PageNumberIndicator.SmallView = true;
                }
            }
            _BackButtonHandler = new EventHandler<BackRequestedEventArgs>(BackButtonHandler_BackPressed);
            SystemNavigationManager.GetForCurrentView().BackRequested += _BackButtonHandler;

            FindTextViewModel = new FindTextViewModel(PDFViewCtrl);
        }

        public void Deactivate()
        {
            UnregisterForConversion();

            if (_BackButtonHandler != null)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested -= _BackButtonHandler;
            }
            if (ToolManager != null)
            {
                ToolManager.CreateDefaultTool();
                ToolManager.Dispose();
                ToolManager = null;
            }
            if (PDFViewCtrl != null)
            {
                PDFViewCtrl.IsEnabledChanged -= PDFViewCtrl_IsEnabledChanged;
                PDFViewCtrl = null;
            }

            PageNumberIndicator.PDFViewCtrl = null;
        }

        private void InitPDFViewCtrl()
        {
            PDFViewCtrl = new PDFViewCtrl();
            PDFViewCtrl.SetupThumbnails(false, true, false, 250, 100 * 1024 * 1024, 0.1);
            PDFViewCtrl.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_single_page);
            PDFViewCtrl.SetBackgroundColor(Windows.UI.Colors.DarkGray);
            PDFViewCtrl.IsEnabledChanged += PDFViewCtrl_IsEnabledChanged;
            PDFViewCtrl.SetPageSpacing(3, 3, 1, 1);
            PDFViewCtrl.SetRelativeZoomLimits(PDFViewCtrlPageViewMode.e_fit_page, 0.7, 5);
            PDFViewCtrl.SetPageRefViewMode(PDFViewCtrlPageViewMode.e_zoom);
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.MemoryManager"))
            {
                ulong mem = Windows.System.MemoryManager.AppMemoryUsageLimit / (5000000 * 2);
                PDFViewCtrl.SetRenderedContentCacheSize(Math.Min(mem, 96)); // 96 MB is plenty
            }

            ToolManager = new ToolManager(PDFViewCtrl);
            ToolManager.EnablePopupMenuOnLongPress = true;
            ToolManager.PanToolTextSelectionMode = ToolManager.TextSelectionBehaviour.Mixed;
            ToolManager.TextMarkupAdobeHack = true;

            PageNumberIndicator = new pdftron.PDF.Tools.Controls.ClickablePageNumberIndicator(PDFViewCtrl);
            if (_IsOnPhone)
            {
                PageNumberIndicator.SmallView = true;
            }
        }
        
        private void PDFViewCtrl_IsEnabledChanged(object sender, Windows.UI.Xaml.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                IsModalDialogOpen = false;
                IsModalGrayout = false;
            }
            else
            {
                IsModalDialogOpen = true;
                IsModalGrayout = true;
            }
        }


        #region Properties

        private bool _IsOnPhone = false;
        public bool IsOnPhone { get { return _IsOnPhone; } }

        private PDFViewCtrl _PDFViewCtrl;
        public PDFViewCtrl PDFViewCtrl
        {
            get { return _PDFViewCtrl; }
            private set { Set(ref _PDFViewCtrl, value); }
        }

        private ToolManager _ToolManager;
        public ToolManager ToolManager
        {
            get { return _ToolManager; }
            set { _ToolManager = value; }
        }

        private MagnifyingControl _MagnifyingControl;
        public MagnifyingControl MagnifyingControl
        {
            get { return _MagnifyingControl; }
            set
            {
                if (Set(ref _MagnifyingControl, value))
                {
                    RaisePropertyChanged("IsMagnifying");
                }

            }
        }

        private pdftron.PDF.Tools.Controls.ViewModels.MagnifyingToolbarViewModel _MagnifyingToolbarViewModel;
        public pdftron.PDF.Tools.Controls.ViewModels.MagnifyingToolbarViewModel MagnifyingToolbarViewModel
        {
            get { return _MagnifyingToolbarViewModel; }
            set { Set(ref _MagnifyingToolbarViewModel, value); }
        }

        public bool IsMagnifying
        {
            get { return _MagnifyingControl != null; }
        }

        private bool _IsAppBarVisible = true;
        public bool IsAppBarVisible
        {
            get { return _IsAppBarVisible; }
            set { Set(ref _IsAppBarVisible, value); }
        }

        private void ResolveAppBarVisibility()
        {
            IsAppBarVisible = !ShowAnnotToolbar && !ShowThumbnails && !ShowReflow;
        }

        private bool _IsTapDismissableDialogOpen = false;
        public bool IsTapDismissableDialogOpen
        {
            get { return _IsTapDismissableDialogOpen; }
            set { Set(ref _IsTapDismissableDialogOpen, value); }
        }

        private bool _IsModalDialogOpen = false;
        public bool IsModalDialogOpen
        {
            get { return _IsModalDialogOpen; }
            set
            {
                if (Set(ref _IsModalDialogOpen, value))
                {
                    if (_IsModalDialogOpen)
                    {
                        PDFViewCtrl.IsEnabled = false;
                    }
                    else
                    {
                        PDFViewCtrl.IsEnabled = true;
                        _IsModalGrayout = false;
                    }
                }
            }
        }

        private bool _IsModalGrayout = false;
        public bool IsModalGrayout
        {
            get { return _IsModalGrayout; }
            set
            {
                if (Set(ref _IsModalGrayout, value))
                {
                    if (_IsModalGrayout)
                    {
                        IsModalDialogOpen = true;
                    }
                }
            }
        }

        private bool _IsSummaryFlyoutOpen = false;
        public bool IsSummaryFlyoutOpen
        {
            get { return _IsSummaryFlyoutOpen; }
            set { Set(ref _IsSummaryFlyoutOpen, value); }
        }

        private bool _IsPageLayoutFlyoutOpen = false;
        public bool IsPageLayoutFlyoutOpen
        {
            get { return _IsPageLayoutFlyoutOpen; }
            set { Set(ref _IsPageLayoutFlyoutOpen, value); }
        }

        private bool _IsConverting = false;
        /// <summary>
        /// While the PDFViewCtrl is converting a document, we can not read content actively.
        /// This includes interacting with Annotations and searching for text.
        /// </summary>
        public bool IsConverting
        {
            get { return _IsConverting; }
            set { Set(ref _IsConverting, value); }
        }


        private bool _ShowAnnotToolbar = false;
        public bool ShowAnnotToolbar
        {
            get { return _ShowAnnotToolbar; }
            set
            {
                if (Set(ref _ShowAnnotToolbar, value))
                {
                    if (_ShowAnnotToolbar)
                    {
                        AnnotationToolbar = new pdftron.PDF.Tools.Controls.AnnotationCommandBar(ToolManager);
                        AnnotationToolbar.ControlClosed += AnnotationToolbar_ControlClosed;
                    }
                    else
                    {
                        if (AnnotationToolbar != null)
                        {
                            AnnotationToolbar.ControlClosed -= AnnotationToolbar_ControlClosed;
                        }
                        AnnotationToolbar = null;
                    }
                }
                ResolveAppBarVisibility();
            }
        }

        private void AnnotationToolbar_ControlClosed()
        {
            ShowAnnotToolbar = false;
        }

        private pdftron.PDF.Tools.Controls.AnnotationCommandBar _AnnotationToolbar;
        public pdftron.PDF.Tools.Controls.AnnotationCommandBar AnnotationToolbar
        {
            get { return _AnnotationToolbar; }
            set { Set(ref _AnnotationToolbar, value); }
        }

        private bool _ShowOutline = false;
        public bool ShowOutline
        {
            get { return _ShowOutline; }
            set
            {
                if (Set(ref _ShowOutline, value))
                {
                    if (_ShowOutline)
                    {
                        Outline = new pdftron.PDF.Tools.Controls.Outline(PDFViewCtrl);
                        Outline.ItemClicked += ModalDialog_ItemClicked;
                        if (IsOnPhone)
                        {
                            IsModalDialogOpen = true;
                            IsModalGrayout = true;
                        }
                        else
                        {
                            IsTapDismissableDialogOpen = true;
                        }
                    }
                    else
                    {
                        if (Outline != null)
                        {
                            Outline.PDFViewCtrl = null;
                        }
                        Outline = null;
                        if (IsOnPhone)
                        {
                            IsModalDialogOpen = false;
                            IsModalGrayout = false;
                        }
                    }
                }
            }
        }

        private pdftron.PDF.Tools.Controls.Outline _Outline;
        public pdftron.PDF.Tools.Controls.Outline Outline
        {
            get { return _Outline; }
            set { Set(ref _Outline, value); }
        }


        private bool _ShowAnnotationList = false;
        public bool ShowAnnotationList
        {
            get { return _ShowAnnotationList; }
            set
            {
                if (Set(ref _ShowAnnotationList, value))
                {
                    if (_ShowAnnotationList)
                    {
                        AnnotationList = new pdftron.PDF.Tools.Controls.AnnotationList(PDFViewCtrl);
                        if (IsOnPhone)
                        {
                            IsModalDialogOpen = true;
                            IsModalGrayout = true;
                            AnnotationList.ItemClicked += ModalDialog_ItemClicked;
                        }
                        else
                        {
                            IsTapDismissableDialogOpen = true;
                        }
                    }
                    else
                    {
                        if (AnnotationList != null)
                        {
                            AnnotationList.PDFViewCtrl = null;
                        }
                        AnnotationList = null;
                        if (IsOnPhone)
                        {
                            IsModalDialogOpen = false;
                            IsModalGrayout = false;
                        }
                    }
                }
            }
        }

        private pdftron.PDF.Tools.Controls.AnnotationList _AnnotationList;
        public pdftron.PDF.Tools.Controls.AnnotationList AnnotationList
        {
            get { return _AnnotationList; }
            set { Set(ref _AnnotationList, value); }
        }


        private bool _ShowUserBookmarks = false;
        public bool ShowUserBookmarks
        {
            get { return _ShowUserBookmarks; }
            set
            {
                if (Set(ref _ShowUserBookmarks, value))
                {
                    if (_ShowUserBookmarks)
                    {
                        string docPath = string.Empty;
                        if (PDFViewCtrl != null && PDFViewCtrl.GetDoc() != null)
                        {
                            docPath = PDFViewCtrl.GetDoc().GetFileName();
                            if (string.IsNullOrEmpty(docPath))
                            {
                                PDFDocInfo info = PDFViewCtrl.GetDoc().GetDocInfo();
                                if (info != null)
                                {
                                    docPath = info.GetTitle();
                                }
                            }
                        }
                        UserBookmarks = new pdftron.PDF.Tools.Controls.UserBookmarkControl(PDFViewCtrl, docPath);
                        UserBookmarks.ItemClicked += ModalDialog_ItemClicked;
                        if (IsOnPhone)
                        {
                            IsModalDialogOpen = true;
                            IsModalGrayout = true;
                        }
                        else
                        {
                            IsTapDismissableDialogOpen = true;
                        }
                    }
                    else
                    {
                        if (UserBookmarks != null)
                        {
                            UserBookmarks.SaveBookmarks();
                        }
                        UserBookmarks = null;
                        if (IsOnPhone)
                        {
                            IsModalDialogOpen = false;
                            IsModalGrayout = false;
                        }
                    }
                }
            }
        }

        private pdftron.PDF.Tools.Controls.UserBookmarkControl _UserBookmarks;
        public pdftron.PDF.Tools.Controls.UserBookmarkControl UserBookmarks
        {
            get { return _UserBookmarks; }
            set { Set(ref _UserBookmarks, value); }
        }

        private bool _ShowFindText = false;
        public bool ShowFindText
        {
            get { return _ShowFindText; }
            set
            {
                if (Set(ref _ShowFindText, value))
                {
                    if (!_ShowFindText)
                    {
                        FindTextViewModel.CloseFindText();
                    }
                }
            }
        }

        private FindTextViewModel _FindTextViewModel;
        public FindTextViewModel FindTextViewModel
        {
            get { return _FindTextViewModel; }
            private set { Set(ref _FindTextViewModel, value); }
        }


        private bool _ShowThumbnails = false;
        public bool ShowThumbnails
        {
            get { return _ShowThumbnails; }
            set
            {
                if (Set(ref _ShowThumbnails, value))
                {
                    IsModalDialogOpen = _ShowThumbnails;
                    if (_ShowThumbnails)
                    {
                        string docPath = string.Empty;
                        if (PDFViewCtrl != null && PDFViewCtrl.GetDoc() != null)
                        {
                            docPath = PDFViewCtrl.GetDoc().GetFileName();
                            if (string.IsNullOrEmpty(docPath))
                            {
                                PDFDocInfo info = PDFViewCtrl.GetDoc().GetDocInfo();
                                if (info != null)
                                {
                                    docPath = info.GetTitle();
                                }
                            }
                        }
                        _DocumentRearrangedSinceOpening = false;
                        ThumbnailViewer = new pdftron.PDF.Tools.Controls.ThumbnailViewer(PDFViewCtrl, docPath);
                        ThumbnailViewer.ViewModel.PageMoved += ViewModel_PageMoved;
                        ThumbnailViewer.ViewModel.PageDeleted += ViewModel_PageDeleted;
                        ThumbnailViewer.ViewModel.PageAdded += ViewModel_PageAdded;
                        ThumbnailViewer.ControlClosed += ThumbnailViewer_ControlClosed;
                    }
                    else
                    {
                        if (ThumbnailViewer != null)
                        {
                            ThumbnailViewer.ViewModel.CleanUp();
                            ThumbnailViewer.ViewModel.PageMoved -= ViewModel_PageMoved;
                            ThumbnailViewer.ViewModel.PageDeleted -= ViewModel_PageDeleted;
                            ThumbnailViewer.ViewModel.PageAdded -= ViewModel_PageAdded;
                            ThumbnailViewer.ControlClosed -= ThumbnailViewer_ControlClosed;
                            ThumbnailViewer.PDFViewCtrl = null;
                        }
                        ThumbnailViewer = null;
                    }
                    ResolveAppBarVisibility();
                }
            }
        }

        private bool _ShowReflow = false;
        public bool ShowReflow
        {
            get { return _ShowReflow; }
            set
            {
                if (Set(ref _ShowReflow, value))
                {
                    IsModalDialogOpen = _ShowReflow;
                    if (_ShowReflow)
                    {
                        ReflowView = new pdftron.PDF.Tools.Controls.ReflowView(PDFViewCtrl.GetDoc(), PDFViewCtrl.GetCurrentPage());
                        ReflowView.ReflowViewModel.PageChanged += ReflowViewModel_PageChanged;
                        ReflowPageString = string.Format("Page {0} / {1}", ReflowView.CurrentPage, PDFViewCtrl.GetPageCount());
                    }
                    else
                    {
                        if (ReflowView != null)
                        {
                            ReflowView.ReflowViewModel.PageChanged -= ReflowViewModel_PageChanged;
                            PDFViewCtrl.SetCurrentPage(ReflowView.CurrentPage);
                        }
                        ReflowView = null;
                    }
                    ResolveAppBarVisibility();
                }
            }
        }

        private string _ReflowPageString;
        public string ReflowPageString
        {
            get { return _ReflowPageString; }
            set { Set(ref _ReflowPageString, value); }
        }

        private void ReflowViewModel_PageChanged(int pageNumber, int totalPages)
        {
            ReflowPageString = string.Format("Page {0} / {1}", pageNumber, totalPages);
        }

        private bool _DocumentRearrangedSinceOpening = false;
        private void ThumbnailViewer_ControlClosed()
        {
            ShowThumbnails = false;
            if (_DocumentRearrangedSinceOpening)
            {
                PDFViewCtrl.UpdatePageLayout();
            }
        }

        private void ViewModel_PageDeleted(int pageNumber)
        {
            _DocumentRearrangedSinceOpening = true;
        }

        private void ViewModel_PageAdded(int pageNumber, pdftron.PDF.Tools.Controls.ViewModels.ThumbnailItem item)
        {
            _DocumentRearrangedSinceOpening = true;
        }

        private void ViewModel_PageMoved(int pageNumber, int newLocation)
        {
            _DocumentRearrangedSinceOpening = true;
        }

        private pdftron.PDF.Tools.Controls.ThumbnailViewer _ThumbnailViewer;
        public pdftron.PDF.Tools.Controls.ThumbnailViewer ThumbnailViewer
        {
            get { return _ThumbnailViewer; }
            set { Set(ref _ThumbnailViewer, value); }
        }

        private pdftron.PDF.Tools.Controls.ReflowView _ReflowView;
        public pdftron.PDF.Tools.Controls.ReflowView ReflowView
        {
            get { return _ReflowView; }
            set { Set(ref _ReflowView, value); }
        }

        private pdftron.PDF.Tools.Controls.ClickablePageNumberIndicator _PageNumberIndicator;
        public pdftron.PDF.Tools.Controls.ClickablePageNumberIndicator PageNumberIndicator
        {
            get { return _PageNumberIndicator; }
            private set
            {
                Set(ref _PageNumberIndicator, value);
            }
        }

        #endregion Properties


        #region Public Functions

        public void OpenDoc(PDFDoc doc)
        {
            Open(doc);
        }

        public void Open(DocumentConversion conversion)
        {
            OpenNonPDF(conversion);
        }

        public async void OpenURL(string url)
        {
            try
            {
                await _PDFViewCtrl.OpenURLAsync(url);
            }
            catch (Exception e)
            {
                pdftron.Common.PDFNetException ex = new pdftron.Common.PDFNetException(e.HResult);
                if (ex.IsPDFNetException)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }

        #endregion Public Functions


        #region Commands

        public RelayCommand GoBackCommand { get; private set; }
        public RelayCommand MagnifyCommand { get; private set; }
        public RelayCommand FindTextCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand TapOverlayPressedCommand { get; private set; }
        public RelayCommand PageLayoutCommand { get; private set; }
        public RelayCommand SummaryCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }

        private void InitCommands()
        {
            GoBackCommand = new RelayCommand(GoBackCommandImpl);
            MagnifyCommand = new RelayCommand(MagnifyCommandImpl);
            FindTextCommand = new RelayCommand(FindTextCommandImpl);
            EditCommand = new RelayCommand(EditCommandImpl);
            TapOverlayPressedCommand = new RelayCommand(TapOverlayPressedCommandImpl);
            PageLayoutCommand = new RelayCommand(PageLayoutCommandImpl);
            SummaryCommand = new RelayCommand(SummaryCommandImpl);
            SaveCommand = new RelayCommand(SaveCommandImpl);
        }


        private void GoBackCommandImpl(object parameter)
        {
            bool handled = HandleBack();
            if (!handled)
            {
                Resources.NavigationHelper.GoBack();
            }
        }

        private void MagnifyCommandImpl(object parameter)
        {
            if (IsMagnifying)
            {
                MagnifyingToolbarViewModel.CommitCurrentInk();
                MagnifyingControl.ViewModel.CleanUp();
                MagnifyingControl = null;
                ToolManager.CreateDefaultTool();
                PDFViewCtrl.SetRelativeZoomLimits(PDFViewCtrlPageViewMode.e_fit_page, 0.1, 100);
            }
            else
            {
                MagnifyingTool magTool = ToolManager.CreateMagnifyingTool();
                double refZoom = PDFViewCtrl.GetZoomForViewMode(PDFViewCtrlPageViewMode.e_fit_page);
                if (PDFViewCtrl.GetZoom() < refZoom)
                {
                    PDFViewCtrl.SetZoom(refZoom);
                }
                if (PDFViewCtrl.GetZoom() > refZoom * 4)
                {
                    PDFViewCtrl.SetZoom(refZoom * 4);
                }
                PDFViewCtrl.SetRelativeZoomLimits(PDFViewCtrlPageViewMode.e_fit_page, 1, 4);

                PDFViewCtrl.SetPageViewMode(PDFViewCtrlPageViewMode.e_zoom); // prevents it from zooming.
                MagnifyingControl magControl = new MagnifyingControl();
                magControl.SetPDFViewCtrl(PDFViewCtrl);
                magControl.SetMagnifyingTool(magTool);
                magControl.ViewModel.EffectiveHeight = PDFViewCtrl.ActualHeight * 0.4;
                magControl.ViewModel.MagnifiedPDFViewCtrl.SetScrollBarVisibility(Windows.UI.Xaml.Controls.ScrollBarVisibility.Hidden);
                magControl.ViewModel.MagnifiedPDFViewCtrl.SetPageSpacing(3, 3, 0, 0);
                magControl.ViewModel.MagnifiedPDFViewCtrl.SetRelativeZoomLimits(PDFViewCtrlPageViewMode.e_fit_width, 1, 8);
                magControl.ViewModel.MagnifiedPDFViewCtrl.SetPageRefViewMode(PDFViewCtrlPageViewMode.e_zoom);
                magControl.ViewModel.MagnifiedToolManager.AllowedLinkActions = ToolManager.AllowedLinks.GoTo;
                MagnifyingControl = magControl;

                int currentPage = PDFViewCtrl.GetCurrentPage();
                double desiredZoom = PDFViewCtrl.GetZoom() * 4;
                MagnifyingControl.ViewModel.SetStartPosition(new pdftron.PDF.Tools.Controls.ViewModels.MagnifyingControlViewModel.StartPosition(currentPage, new Windows.Foundation.Point(double.NaN, double.NaN), desiredZoom));
                MagnifyingToolbarViewModel = new pdftron.PDF.Tools.Controls.ViewModels.MagnifyingToolbarViewModel(MagnifyingControl.ViewModel, magTool);
            }
        }

        private void FindTextCommandImpl(object parameter)
        {
            ShowFindText = true;
        }

        private void EditCommandImpl(object parameter)
        {
            ShowAnnotToolbar = true;
        }

        private void TapOverlayPressedCommandImpl(object parameter)
        {
            ShowOutline = false;
            ShowAnnotationList = false;
            ShowUserBookmarks = false;
            IsTapDismissableDialogOpen = false;
        }


        private void PageLayoutCommandImpl(object parameter)
        {
            string paramName = parameter as string;
            if (!string.IsNullOrEmpty(paramName))
            {
                if (paramName.Equals("sp", StringComparison.OrdinalIgnoreCase))
                {
                    PDFViewCtrl.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_single_page);
                }
                else if (paramName.Equals("scon", StringComparison.OrdinalIgnoreCase))
                {
                    PDFViewCtrl.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_single_continuous);
                }
                else if (paramName.Equals("f", StringComparison.OrdinalIgnoreCase))
                {
                    PDFViewCtrl.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_facing);
                }
                else if (paramName.Equals("fcon", StringComparison.OrdinalIgnoreCase))
                {
                    PDFViewCtrl.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_facing_continuous);
                }
                else if (paramName.Equals("fcov", StringComparison.OrdinalIgnoreCase))
                {
                    PDFViewCtrl.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_facing_cover);
                }
                else if (paramName.Equals("fconcov", StringComparison.OrdinalIgnoreCase))
                {
                    PDFViewCtrl.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_facing_continuous_cover);
                }
                IsPageLayoutFlyoutOpen = false;
            }
        }

        private void SummaryCommandImpl(object parameter)
        {
            string paramName = parameter as string;
            {
                if (!string.IsNullOrEmpty(paramName))
                {
                    if (paramName.Equals("outline", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowOutline = true;
                    }
                    else if (paramName.Equals("annotationlist", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowAnnotationList = true;
                    }
                    else if (paramName.Equals("userbookmarks", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowUserBookmarks = true;
                    }
                    else if (paramName.Equals("thumbnails", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowThumbnails = true;
                    }
                    else if (paramName.Equals("reflow", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowReflow = true;
                    }
                }
            }
            IsSummaryFlyoutOpen = false;
        }

        private async void SaveCommandImpl(object obj)
        {
            PDFDoc doc = PDFViewCtrl.GetDoc();
            if (doc == null)
            {
                return;
            }

            StorageFile file = null;

            bool isModified = false;
            if (_DocumentIsNonPDF)
            {
                try
                {
                    if (_DocumentIsNonPDF)
                    {
                        file = await GetSaveAsLocationAsync();
                        if (file != null)
                        {
                            isModified = true;
                        }
                    }
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    PDFViewCtrl.DocLockRead();
                    if (doc.IsModified())
                    {
                        isModified = true;
                    }
                }
                catch (Exception) { }
                finally
                {
                    PDFViewCtrl.DocUnlockRead();
                }
            }

            if (isModified)
            {
                bool success = false;
                string saveError = string.Empty;
                try
                {
                    if (_DocumentIsNonPDF && file != null)
                    {
                        await doc.SaveAsync(file, pdftron.SDF.SDFDocSaveOptions.e_remove_unused);
                        _DocumentIsNonPDF = false;
                    }
                    else
                    {
                        await doc.SaveAsync();
                    }
                    success = true;
                }
                catch (Exception e)
                {
                    pdftron.Common.PDFNetException pdfnetEx = new pdftron.Common.PDFNetException(e.HResult);
                    if (pdfnetEx.IsPDFNetException)
                    {
                        saveError = string.Format("Line number: {0}{1}Error message: {2}", pdfnetEx.LineNumber, Environment.NewLine, pdfnetEx.Message);
                    }
                    else
                    {
                        saveError = e.ToString();
                    }
                }
                if (!success)
                {
                    Windows.UI.Popups.MessageDialog md = new Windows.UI.Popups.MessageDialog(
                        string.Format("An error occured when saving:{0}{1}", Environment.NewLine, saveError), "Saving error");
                    await md.ShowAsync();
                }
            }
        }

        #endregion Commands


        #region Impl

        /// <summary>
        /// Used by Outline, AnnotationList, and UserBookmarks to indicate we should close the dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModalDialog_ItemClicked(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            ShowOutline = false;
            ShowAnnotationList = false;
            ShowUserBookmarks = false;
            IsModalDialogOpen = false;
            IsModalGrayout = false;
        }

        private async void Open(PDFDoc doc)
        {
            if (doc == null)
            {
                return;
            }

            bool openSucceeded = false;
            string errorMessage = "";
            try
            {
                _PDFViewCtrl.SetDoc(doc);
                openSucceeded = true;
            }
            catch (Exception e)
            {
                pdftron.Common.PDFNetException ex = new pdftron.Common.PDFNetException(e.HResult);
                if (ex.IsPDFNetException)
                {
                    errorMessage = ex.ToString();
                }
                else
                {
                    errorMessage = e.ToString();
                }
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            if (!openSucceeded)
            {
                MessageDialog md = new MessageDialog("Failed to open the document. Error message:\n" + errorMessage);
                await md.ShowAsync();
                GoBackCommand.Execute(null);
            }
        }

        private void OpenNonPDF(DocumentConversion conversion)
        {
            if (conversion != null)
            {
                try
                {
                    _PDFViewCtrl.OpenUniversalDocument(conversion);
                    RegisterForConversion();
                }
                catch (Exception e)
                {
                    pdftron.Common.PDFNetException ex = new pdftron.Common.PDFNetException(e.HResult);
                    if (ex.IsPDFNetException)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private async Task<StorageFile> GetSaveAsLocationAsync()
        {
            Windows.Storage.Pickers.FileSavePicker fileSavePicker = new Windows.Storage.Pickers.FileSavePicker();
            fileSavePicker.CommitButtonText = "Save";
            fileSavePicker.FileTypeChoices.Add("PDF Document", new List<string>() { ".pdf" });
            string suggestedFileName = string.Empty;
            StorageFile file = await fileSavePicker.PickSaveFileAsync();
            return file;
        }

        #endregion Impl


        #region Handle Non PDFs

        private bool _DocumentIsNonPDF = false;
        private bool _RegisteredForConversion = false;

        private void RegisterForConversion()
        {
            _PDFViewCtrl.OnConversionChanged += PDFViewCtrl_OnConversionChanged;
            _DocumentIsNonPDF = true;
            IsConverting = true;
            ToolManager.IsEnabled = false;
            _RegisteredForConversion = true;
        }

        private void UnregisterForConversion()
        {
            if (!_RegisteredForConversion)
            {
                return;
            }
            _RegisteredForConversion = false;
            _PDFViewCtrl.OnConversionChanged -= PDFViewCtrl_OnConversionChanged;
            IsConverting = false;
            ToolManager.IsEnabled = true;
        }

        private async void PDFViewCtrl_OnConversionChanged(PDFViewCtrlConversionType type, int totalPagesConverted)
        {
            switch (type)
            {
                case PDFViewCtrlConversionType.e_conversion_progress:
                    PageNumberIndicator.UpdatePageNumbers();
                    break;
                case PDFViewCtrlConversionType.e_conversion_failed:
                    UnregisterForConversion();
                    MessageDialog messageDialog = new MessageDialog("Something went wrong when converting the document.");
                    await messageDialog.ShowAsync();
                    GoBackCommand.Execute(null);
                    break;
                case PDFViewCtrlConversionType.e_conversion_finished:
                    UnregisterForConversion();
                    break;
            }
        }

        #endregion Handle Non PDFs


        #region GoBack

        EventHandler<BackRequestedEventArgs> _BackButtonHandler;

        void BackButtonHandler_BackPressed(object sender, BackRequestedEventArgs e)
        {
            e.Handled = HandleBack();
        }

        private bool HandleBack()
        {
            Windows.UI.Xaml.Controls.Frame frame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
            if (frame == null)
            {
                return false;
            }

            if (ShowOutline)
            {
                if (Outline != null && !Outline.GoBack())
                {
                    ShowOutline = false;
                }
                return true;
            }

            if (ShowAnnotationList)
            {
                if (AnnotationList != null && !AnnotationList.GoBack())
                {
                    ShowAnnotationList = false;
                }
                return true;
            }

            if (ShowUserBookmarks)
            {
                if (UserBookmarks != null && !UserBookmarks.GoBack())
                {
                    ShowUserBookmarks = false;
                }
                return true;
            }

            if (ShowFindText)
            {
                ShowFindText = false;
                return true;
            }

            if (ShowThumbnails)
            {
                if (ThumbnailViewer != null && !ThumbnailViewer.GoBack())
                {
                    ShowThumbnails = false;
                }
                return true;
            }

            if (ShowAnnotToolbar)
            {
                if (AnnotationToolbar != null && !AnnotationToolbar.GoBack())
                {
                    ShowAnnotToolbar = false;
                }
                return true;
            }

            if (ToolManager != null && ToolManager.CloseOpenDialog())
            {
                return true;
            }

            if (PageNumberIndicator.GoBack())
            {
                return true;
            }

            if (IsSummaryFlyoutOpen)
            {
                IsSummaryFlyoutOpen = false;
                return true;
            }

            if (IsPageLayoutFlyoutOpen)
            {
                IsPageLayoutFlyoutOpen = false;
                return true;
            }

            if (ShowReflow)
            {
                ShowReflow = false;
                return true;
            }
            return false;
        }


        #endregion GoBack
    }
}
