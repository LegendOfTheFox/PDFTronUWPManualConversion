using PDFNetUniversalSamples.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace PDFNetUniversalSamples.ViewModels
{
    public class SampleListItem
    {
        public string Title { get; private set; }

        public string Description { get; private set; }

        public Sample SampleCode { get; private set; }

        public SampleListItem(Sample sample)
        {
            SampleCode = sample;
            Title = sample.Name;
            Description = sample.Description;
        }
    }


    class SampleSelectionViewModel : ViewModelBase
    {
        private ObservableCollection<Sample> _Samples;
        public ObservableCollection<Sample> Samples { get { return _Samples; } }

        public System.Windows.Input.ICommand SampleClickedCommand { get; private set; }

        public RelayCommand SelectionCommand { get; private set; }

        public SampleSelectionViewModel()
        {
            _Samples = new ObservableCollection<Sample>();
            SampleClickedCommand = new RelayCommand(OpenSample);
            SelectionCommand = new RelayCommand(SelectionChanged);
            CreateSamples();
        }

        private void CreateSamples()
        {
            _Samples.Add(new PDFNetSamples.AddImageTest());
            _Samples.Add(new PDFNetSamples.AnnotationTest());
            _Samples.Add(new PDFNetSamples.BookmarkTest());
            _Samples.Add(new PDFNetSamples.ContentReplacerTest());
            _Samples.Add(new PDFNetSamples.ConvertTest());
            _Samples.Add(new PDFNetSamples.DigitalSignaturesTest());
            _Samples.Add(new PDFNetSamples.ElementBuilderTest());
            _Samples.Add(new PDFNetSamples.ElementEditTest());
            _Samples.Add(new PDFNetSamples.ElementReaderAdvTest());
            _Samples.Add(new PDFNetSamples.ElementReaderTest());
            _Samples.Add(new PDFNetSamples.EncTestCS());
            _Samples.Add(new PDFNetSamples.FDFTest());
            _Samples.Add(new PDFNetSamples.ImageExtractTest());
            _Samples.Add(new PDFNetSamples.ImpositionTest());
            _Samples.Add(new PDFNetSamples.InteractiveFormsTest());
            _Samples.Add(new PDFNetSamples.JBIG2Test());
            _Samples.Add(new PDFNetSamples.OfficeToPDFTest());
            _Samples.Add(new PDFNetSamples.OptimizerTest());
            _Samples.Add(new PDFNetSamples.PageLabelsTest());
            _Samples.Add(new PDFNetSamples.PatternTest());
            _Samples.Add(new PDFNetSamples.PDFATest());
            _Samples.Add(new PDFNetSamples.PDFDocLockTest());
            _Samples.Add(new PDFNetSamples.PDFDocMemoryTest());
            _Samples.Add(new PDFNetSamples.PDFDrawTest());
            _Samples.Add(new PDFNetSamples.PDFLayersTest());
			_Samples.Add(new PDFNetSamples.PDFPackageTest());
			_Samples.Add(new PDFNetSamples.PDFPageTest());
			_Samples.Add(new PDFNetSamples.PDFRedactTest());
			_Samples.Add(new PDFNetSamples.RectTest());
			_Samples.Add(new PDFNetSamples.SDFTest());
			_Samples.Add(new PDFNetSamples.StamperTest());
			_Samples.Add(new PDFNetSamples.TextExtractTest());
			_Samples.Add(new PDFNetSamples.TextSearchTest());
            _Samples.Add(new PDFNetSamples.UndoRedoTest());
            _Samples.Add(new PDFNetSamples.U3DTest());
            _Samples.Add(new PDFNetSamples.UnicodeWriteTest());
            _Samples.Add(new PDFNetSamples.VisualComparisionTest());            
        }

        public void OpenSample(object clickedItem)
        {
            Sample sample = clickedItem as Sample;
            if (sample != null)
            {
#if WINDOWS_PHONE_APP
                Common.NavigationService service = new NavigationService();
                service.Navigate(typeof(SamplePage), sample);
#endif
            }
        }

        public void SelectionChanged(object selectionArgs)
        {
            //SelectionChangedEventArgs args = selectionArgs as SelectionChangedEventArgs;
            //if (args.RemovedItems.Count > 0)
            //{
            //    Sample sample = args.RemovedItems[0] as Sample;
            //    if (sample != null)
            //    {
            //        sample.ClearOutput();
            //    }
            //}
            //if (args.AddedItems.Count > 0)
            //{
            //    Sample sample = args.AddedItems[0] as Sample;
            //    if (sample != null)
            //    {
            //        sample.ClearOutput();
            //    }
            //}
        }
    }
}
