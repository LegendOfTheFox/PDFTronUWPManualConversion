using pdftron.PDF;
using pdftron.PDF.Tools.Controls;
using PDFViewCtrlDemo_Windows10.ViewModels.Common;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using UIRect = Windows.Foundation.Rect;

namespace PDFViewCtrlDemo_Windows10.ViewModels
{
    public class FindTextViewModel : ViewModelBase
    {
        public FindTextViewModel(PDFViewCtrl pdfView)
        {
            _PDFViewCtrl = pdfView;
            FindCommand = new RelayCommand(FindCommandImpl);
            PrevNextCommand = new RelayCommand(PrevNextCommandImpl);
            ExitCommand = new RelayCommand(ExitCommandImpl);
            TextChangedCommand = new RelayCommand(TextChangedCommandImpl);
            SearchTextBoxKeyUpCommand = new RelayCommand(SearchTextBoxKeyUpCommandImpl);

            if (_PDFViewCtrl != null)
            {
                _PDFViewCtrl.OnScale += PDFViewCtrl_OnScale;
                _PDFViewCtrl.OnSize += PDFViewCtrl_OnSize;
                _PDFViewCtrl.OnPageNumberChanged += PDFViewCtrl_OnPageNumberChanged;
            }
        }

        #region View Model Interactions

        public delegate void OnFindTextClosed();

        public event OnFindTextClosed FindTextClosed;

        public void CloseFindText()
        {
            CancelFindText();
            ClearSelection();
            SearchTerm = "";
        }

        #endregion View Model Interactions

        #region Commands

        public RelayCommand FindCommand { get; private set; }
        public RelayCommand PrevNextCommand { get; private set; }
        public RelayCommand ExitCommand { get; private set; }
        public RelayCommand TextChangedCommand { get; private set; }
        public RelayCommand SearchTextBoxKeyUpCommand { get; private set; }

        private void FindCommandImpl(object sender)
        {
            FindText(false);
        }

        private void PrevNextCommandImpl(object direction)
        {
            string dir = direction as string;
            if (string.Equals(dir, "prev", StringComparison.OrdinalIgnoreCase))
            {
                FindText(true);
            }
            else
            {
                FindText(false);
            }
        }

        private void ExitCommandImpl(object sender)
        {
            if (FindTextClosed != null)
            {
                FindTextClosed();
            }
            CloseFindText();
        }

        private void TextChangedCommandImpl(object textChangedArgs)
        {
            CancelFindText();
        }

        private void SearchTextBoxKeyUpCommandImpl(object parameter)
        {
            KeyRoutedEventArgs args = parameter as KeyRoutedEventArgs;
            if (args != null)
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    FindText(false);
                }
            }
        }

        #endregion Commands

        #region Properties

        private double _SearchProgress = 0;
        public double SearchProgress {
            get { return _SearchProgress; }
            private set
            {
                if (value != _SearchProgress)
                {
                    _SearchProgress = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SearchTerm = "";
        public string SearchTerm
        {
            get { return _SearchTerm; }
            set
            {
                if (!string.Equals(value, _SearchTerm))
                {
                    _SearchTerm = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsSearchTermFocused = false;
        public bool IsSearchTermFocused
        {
            get { return _IsSearchTermFocused; }
            set
            {
                if (value != _IsSearchTermFocused)
                {
                    _IsSearchTermFocused = value;
                    RaisePropertyChanged();
                    IsSearchTermFocused = false;
                }
            }
        }

        private string _ErrorText = "";
        public string ErrorText
        {
            get { return _ErrorText; }
            private set
            {
                if (!string.Equals(value, _ErrorText))
                {
                    _ErrorText = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _ShowPrevNextButtons = true;
        public bool ShowPrevNextButtons
        {
            get { return _ShowPrevNextButtons; }
            private set
            {
                if (value != _ShowPrevNextButtons)
                {
                    _ShowPrevNextButtons = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _EnablePrevNext = false;
        public bool EnablePrevNext
        {
            get { return _EnablePrevNext; }
            private set
            {
                if (value != _EnablePrevNext)
                {
                    _EnablePrevNext = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ControlWidth;
        public double ControlWidth
        {
            get { return _ControlWidth; }
            set
            {
                if (value != _ControlWidth)
                {
                    _ControlWidth = value;
                    RaisePropertyChanged();
                    ShowPrevNextButtons = (_ControlWidth > 650);
                }
            }
        }
             

        #endregion Properties

        private PDFViewCtrl _PDFViewCtrl;


        #region Impl

        private TextHighlighter _TextHighLighter;
        private List<Rectangle> _OnScreenSelection = new List<Rectangle>();

        private const string _TextNotFoundErrorMessage = "Text not found.";

        private IAsyncOperation<PDFViewCtrlSelection> _SearchOperation;
        private bool _CancelSearch = false;

        private SolidColorBrush _HighlightColor = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 100, 0, 255));
        private pdftron.PDF.PDFViewCtrlSelection _CurrentSearchSelection;
        private int _SelectionPageNumber;

        private const int _MillisecsPerTick = 50;
        private const int _TicksBeforeProgress = 20;
        private DispatcherTimer _SearchProgressTimer;
        private static int _TicksRemaining;

        void PDFViewCtrl_OnScale()
        {
            ClearSelection();
        }

        void PDFViewCtrl_OnSize()
        {
            ClearSelection();
        }

        void PDFViewCtrl_OnPageNumberChanged(int current_page, int num_pages)
        {
            if (_OnScreenSelection != null && _OnScreenSelection.Count > 0)
            {
                Canvas annotCanvas = _PDFViewCtrl.GetAnnotationCanvas();
                Canvas rectCanvas = _OnScreenSelection[0].Parent as Canvas;
                if (rectCanvas != null)
                {
                    if (!rectCanvas.Equals(annotCanvas))
                    {
                        ClearSelection();
                    }
                }
            }
        }

        private void FindText(bool searchUp = false)
        {
            if (_PDFViewCtrl == null || _PDFViewCtrl.GetDoc() == null || string.IsNullOrWhiteSpace(SearchTerm))
            {
                return;
            }

            if (_SearchOperation != null)
            {
                return;
            }
            _SearchOperation = StartSearchAsync(SearchTerm, false, false, searchUp, false);
            _SearchOperation.Completed = SearchDelegate;

        }

        private IAsyncOperation<PDFViewCtrlSelection> StartSearchAsync(string searchString, bool matchCase, bool matchWholeWord, bool searchUp, bool regExp)
        {
            _CancelSearch = false;
            ErrorText = "";
            _TicksRemaining = _TicksBeforeProgress;
            if (_SearchProgressTimer != null)
            {
                _SearchProgressTimer.Stop();
            }
            _SearchProgressTimer = new DispatcherTimer();
            _SearchProgressTimer.Tick += SearchProgressTimer_Tick;
            _SearchProgressTimer.Interval = TimeSpan.FromMilliseconds(50);
            _SearchProgressTimer.Start();



            IAsyncOperation<PDFViewCtrlSelection> findTextOperation = _PDFViewCtrl.FindTextAsync(searchString, matchCase, matchWholeWord, searchUp, regExp);

            if (_TextHighLighter == null)
            {
                _TextHighLighter = new TextHighlighter(_PDFViewCtrl, searchString);
                _TextHighLighter.HighlightColor = Windows.UI.Color.FromArgb(100, 255, 255, 0);
            }
            
            return findTextOperation;
        }

        public async void SearchDelegate(IAsyncOperation<pdftron.PDF.PDFViewCtrlSelection> asyncAction, AsyncStatus asyncStatus)
        {
            try
            {
                _SearchOperation = null;
                await _PDFViewCtrl.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, delegate
                {
                    switch (asyncStatus)
                    {
                        case AsyncStatus.Error:
                            //SearchResultOutput.Text = "Error";
                            break;
                        case AsyncStatus.Completed:
                            if (_CancelSearch)
                            {
                                ResetSearchProgress();
                                return;
                            }
                            _CurrentSearchSelection = asyncAction.GetResults();
                            if (HighlightSelection(_CurrentSearchSelection))
                            {
                                ResetSearchProgress();
                                EnablePrevNext = true;
                            }
                            else
                            {
                                EnablePrevNext = false;
                                ErrorText = _TextNotFoundErrorMessage;
                                IsSearchTermFocused = true;
                                MaxSeachProgress();
                            }
                            break;
                    }
                });
                
            }
            catch (Exception)
            {
            }
            finally
            {
                
            }
            
        }

        private void CancelFindText()
        {
            _CancelSearch = true;
            ErrorText = "";
            if (_SearchOperation != null)
            {
                _SearchOperation = null;
            }
            _PDFViewCtrl.CancelFindText();
            ClearSelection();
            if (_TextHighLighter != null)
            {
                _TextHighLighter.Detach();
                _TextHighLighter = null;
            }
            ResetSearchProgress();
        }

        /// <summary>
        /// Highlights the search result if any
        /// </summary>
        /// <param name="result">A text selection acquired by mPDFView.FindText</param>
        /// <returns>true if and only if the selections contains at least one highlight</returns>
        private bool HighlightSelection(pdftron.PDF.PDFViewCtrlSelection result)
        {
            if (result == null)
            {
                return false;
            }

            double[] quads = result.GetQuads();
            int numQuads = result.GetQuadArrayLength() / 8;
            _SelectionPageNumber = result.GetPageNum();
            int quadNumber = 0;
            List<UIRect> rects = new List<UIRect>();

            // get highlights in control (screen) space
            for (int i = 0; i < numQuads; i++)
            {
                quadNumber = i * 8;

                pdftron.Common.DoubleRef x1 = new pdftron.Common.DoubleRef(quads[quadNumber + 0]);
                pdftron.Common.DoubleRef y1 = new pdftron.Common.DoubleRef(quads[quadNumber + 1]);

                pdftron.Common.DoubleRef x2 = new pdftron.Common.DoubleRef(quads[quadNumber + 2]);
                pdftron.Common.DoubleRef y2 = new pdftron.Common.DoubleRef(quads[quadNumber + 3]);

                pdftron.Common.DoubleRef x3 = new pdftron.Common.DoubleRef(quads[quadNumber + 4]);
                pdftron.Common.DoubleRef y3 = new pdftron.Common.DoubleRef(quads[quadNumber + 5]);

                pdftron.Common.DoubleRef x4 = new pdftron.Common.DoubleRef(quads[quadNumber + 6]);
                pdftron.Common.DoubleRef y4 = new pdftron.Common.DoubleRef(quads[quadNumber + 7]);

                _PDFViewCtrl.ConvPagePtToScreenPt(x1, y1, _SelectionPageNumber);
                _PDFViewCtrl.ConvPagePtToScreenPt(x2, y2, _SelectionPageNumber);
                _PDFViewCtrl.ConvPagePtToScreenPt(x3, y3, _SelectionPageNumber);
                _PDFViewCtrl.ConvPagePtToScreenPt(x4, y4, _SelectionPageNumber);
                _PDFViewCtrl.ConvScreenPtToAnnotationCanvasPt(x1, y1);
                _PDFViewCtrl.ConvScreenPtToAnnotationCanvasPt(x2, y2);
                _PDFViewCtrl.ConvScreenPtToAnnotationCanvasPt(x3, y3);
                _PDFViewCtrl.ConvScreenPtToAnnotationCanvasPt(x4, y4);

                double left, right, top, bottom;

                left = Math.Min(x1.Value, Math.Min(x2.Value, Math.Min(x3.Value, x4.Value)));
                right = Math.Max(x1.Value, Math.Max(x2.Value, Math.Max(x3.Value, x4.Value)));
                top = Math.Min(y1.Value, Math.Min(y2.Value, Math.Min(y3.Value, y4.Value)));
                bottom = Math.Max(y1.Value, Math.Max(y2.Value, Math.Max(y3.Value, y4.Value)));

                rects.Add(new UIRect(left, top, right - left, bottom - top));
            }

            Canvas annotCanvas = _PDFViewCtrl.GetAnnotationCanvas();

            ClearSelection();

            // add highlight(s) to annotation canvas
            foreach (UIRect rect in rects)
            {
                Rectangle highlight = new Rectangle();
                highlight.Fill = _HighlightColor;
                highlight.Width = rect.Width;
                highlight.Height = rect.Height;
                Canvas.SetLeft(highlight, rect.Left);
                Canvas.SetTop(highlight, rect.Top);
                annotCanvas.Children.Add(highlight);
                _OnScreenSelection.Add(highlight);
            }

            return numQuads > 0;
        }

        private void ClearSelection()
        {
            if (_OnScreenSelection == null)
            {
                return;
            }
            foreach (Rectangle rect in _OnScreenSelection)
            {
                Canvas parent = rect.Parent as Canvas;
                if (parent != null)
                {
                    parent.Children.Remove(rect);
                }
            }
            _OnScreenSelection.Clear();
        }

        void ResetSearchProgress()
        {
            if (_SearchProgressTimer != null)
            {
                _SearchProgressTimer.Stop();
            }
            SearchProgress = 0;
        }

        void MaxSeachProgress()
        {
            if (_SearchProgressTimer != null)
            {
                _SearchProgressTimer.Stop();
            }
            SearchProgress = 100;
        }

        void SearchProgressTimer_Tick(object sender, object e)
        {
            if (_TicksRemaining > 0)
            {
                _TicksRemaining--;
            }
            else
            {
                int progress = _PDFViewCtrl.GetFindTextProgress();
                SearchProgress = Math.Min(Math.Max(progress, 0), 100.0);
            }
        }

        #endregion Impl

    }

}
