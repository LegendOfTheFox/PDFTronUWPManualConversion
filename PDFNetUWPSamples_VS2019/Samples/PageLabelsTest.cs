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
    public sealed class PageLabelsTest : Sample
    {
        public PageLabelsTest() :
            base("PageLabels", "This example illustrates how to work with PDF page labels. PDF page labels can be used to describe a page. This is used to allow for non-sequential page numbering or the addition of arbitrary labels for a page (such as the inclusion of Roman numerals at the beginning of a book).")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Annotation Test...");
                WriteLine("--------------------------------\n");
			    try
			    {
				    //-----------------------------------------------------------
				    // Example 1: Add page labels to an existing or newly created PDF
				    // document.
				    //-----------------------------------------------------------
                    {
                        string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                        WriteLine("Opening input file " + input_file_path);
                        PDFDoc doc = new PDFDoc(input_file_path);
					    doc.InitSecurityHandler();

					    // Create a page labeling scheme that starts with the first page in 
					    // the document (page 1) and is using uppercase roman numbering 
					    // style. 
					    doc.SetPageLabel(1, PageLabel.Create(doc.GetSDFDoc(), PageLabelStyle.e_roman_uppercase, "My Prefix ", 1));

					    // Create a page labeling scheme that starts with the fourth page in 
					    // the document and is using decimal arabic numbering style. 
					    // Also the numeric portion of the first label should start with number 
					    // 4 (otherwise the first label would be "My Prefix 1"). 
					    PageLabel L2 = PageLabel.Create(doc.GetSDFDoc(), PageLabelStyle.e_decimal, "My Prefix ", 4);
					    doc.SetPageLabel(4, L2);

					    // Create a page labeling scheme that starts with the seventh page in 
					    // the document and is using alphabetic numbering style. The numeric 
					    // portion of the first label should start with number 1. 
					    PageLabel L3 = PageLabel.Create(doc.GetSDFDoc(), PageLabelStyle.e_alphabetic_uppercase, "My Prefix ", 1);
					    doc.SetPageLabel(7, L3);

                        String output_file_path = Path.Combine(OutputPath, "newsletter_with_pagelabels.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
					    doc.Destroy();
				    }

				    //-----------------------------------------------------------
				    // Example 2: Read page labels from an existing PDF document.
				    //-----------------------------------------------------------
                    {
                        string input_file_path = Path.Combine(OutputPath, "newsletter_with_pagelabels.pdf");
                        WriteLine("Opening file " + input_file_path);
                        PDFDoc doc = new PDFDoc(input_file_path);
					    doc.InitSecurityHandler();

					    PageLabel label;
					    int page_num = doc.GetPageCount();
					    for (int i=1; i<=page_num; ++i) 
					    {
						    WriteLine(string.Format("Page number: {0}", i));
						    label = doc.GetPageLabel(i);
						    if (label.IsValid()) {
							    WriteLine(string.Format(" Label: {0}", label.GetLabelTitle(i)));
						    }
						    else {
							    WriteLine(" No Label."); 
						    }
					    }
					    doc.Destroy();
				    }

				    //-----------------------------------------------------------
				    // Example 3: Modify page labels from an existing PDF document.
				    //-----------------------------------------------------------
                    {
                        string input_file_path = Path.Combine(OutputPath, "newsletter_with_pagelabels.pdf");
                        WriteLine("Opening file " + input_file_path);
                        PDFDoc doc = new PDFDoc(input_file_path);
					    doc.InitSecurityHandler();

					    // Remove the alphabetic labels from example 1.
					    doc.RemovePageLabel(7); 

					    // Replace the Prefix in the decimal lables (from example 1).
					    PageLabel label = doc.GetPageLabel(4);
					    if (label.IsValid()) {
						    label.SetPrefix("A");
						    label.SetStart(1);
					    }

					    // Add a new label
					    PageLabel new_label = PageLabel.Create(doc.GetSDFDoc(), PageLabelStyle.e_decimal, "B", 1);
					    doc.SetPageLabel(10, new_label);  // starting from page 10.

                        String output_file_path = Path.Combine(OutputPath, "newsletter_with_pagelabels_modified.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);

					    int page_num = doc.GetPageCount();
					    for (int i=1; i<=page_num; ++i) 
					    {
						    WriteLine(string.Format("Page number: {0}", i));
						    label = doc.GetPageLabel(i);
						    if (label.IsValid()) {
                                WriteLine(string.Format(" Label: {0}", label.GetLabelTitle(i)));
						    }
						    else {
							    WriteLine(" No Label."); 
						    }
					    }
					    doc.Destroy();
				    }

				    //-----------------------------------------------------------
				    // Example 4: Delete all page labels in an existing PDF document.
				    //-----------------------------------------------------------
				    {
					    PDFDoc doc = new PDFDoc(Path.Combine(OutputPath, "newsletter_with_pagelabels.pdf"));
					    //doc.GetRoot().Erase("PageLabels");
					    // ...
					    doc.Destroy();
				    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done PageLabels Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
