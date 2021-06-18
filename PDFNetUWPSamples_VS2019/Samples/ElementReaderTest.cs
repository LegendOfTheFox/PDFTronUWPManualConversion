//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class ElementReaderTest : Sample
    {
        public ElementReaderTest() :
            base("ElementReader", "Illustrates how to traverse page display list using ElementReader.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(() => {
                WriteLine("--------------------------------");
                WriteLine("Starting ElementReader Test...");
                WriteLine("--------------------------------\n");
                try {
                    WriteLine("-------------------------------------------------");
                    WriteLine("Extract text data from all pages in the document.");

                    // Open the test file
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file: " + input_file_path);
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();

                    PageIterator itr;
                    pdftron.PDF.ElementReader page_reader = new pdftron.PDF.ElementReader();

                    //int i = 0;
                    for (itr = doc.GetPageIterator(); itr.HasNext(); itr.Next())		//  Read every page
                    {
                        int pageNo = itr.GetPageNumber();
                        WriteLine(String.Format("Page {0:d} ----------------------------------------", pageNo));

                        page_reader.Begin(itr.Current());
                        String result = ProcessElements(page_reader);
                        WriteLine(result);
                        page_reader.End();
                    }
                    WriteLine("Done.");
                    doc.Destroy();
                }
                catch (Exception e) {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done ElementReader Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }
        
        String ProcessElements(ElementReader reader)
        {
            String result = "";
            Element element;
            //int i = 0;
            while ((element = reader.Next()) != null) 	// Read page contents
            {
                switch (element.GetType())
                {
                    case ElementType.e_path:						// Process path data...
                        {
                            result += "Process Element.Type.e_path\n";
                            //PathData data = element.GetPathData();
                            //double[] points = data.get_pts();// points;
                            break;
                        }
                    case ElementType.e_image:                       // Process images...
                    case ElementType.e_inline_image:
                        {
                            result += "Process Element.Type.e_image\n";
                            break;
                        }
                    case ElementType.e_text: 				// Process text strings...
                        {
                            result += "Process Element.Type.e_text\n";
                            //String txt = element.GetTextString();
                            // Message+=(txt);
                            break;
                        }
                    case ElementType.e_form:				// Process form XObjects
                        {
                            result += "Process Element.Type.e_form\n";
                            reader.FormBegin();
                            result += ProcessElements(reader);
                            reader.End();
                            break;
                        }
                }
            }
            return result;
        }
    }
}
