//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron;
using pdftron.PDF;
using pdftron.PDF.PDFA;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class PDFATest : Sample
    {
        public PDFATest() :
            base("PDF/A", "This sample illustrates how to use PDF/A add-on to validate existing PDF documents for PDF/A compliance as well as to convert generic PDF documents to PDF/A format.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () =>
            {
                WriteLine("--------------------------------");
                WriteLine("Starting PDF/A Test...");
                WriteLine("--------------------------------\n");
			
                PDFNet.SetColorManagement(CMSType.e_lcms);  // Required for PDFA validation.

			    //-----------------------------------------------------------
			    // Example 1: PDF/A Validation
			    //-----------------------------------------------------------
			    try
			    {
				    string filename = Path.Combine(InputPath, "newsletter.pdf");
				    PDFACompliance pdf_a = new PDFACompliance(false, filename, "", PDFAComplianceConformance.e_Level1B, null, 10, false);
				    PrintResults(pdf_a, filename);
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

			    //-----------------------------------------------------------
			    // Example 2: PDF/A Conversion
			    //-----------------------------------------------------------
			    try
			    {
				    string filename = Path.Combine(InputPath, "fish.pdf");
                    PDFACompliance pdf_a = new PDFACompliance(true, filename, "", PDFAComplianceConformance.e_Level1B, null, 10, false);
				    filename = Path.Combine(OutputPath, "pdfa.pdf");
				    pdf_a.SaveAs(filename, true);
                    WriteLine("Saved PDF/A converted doc to" + filename);
                    await AddFileToOutputList(filename).ConfigureAwait(false);

				    // Re-validate the document after the conversion...
				    pdf_a = new PDFACompliance(false, filename, "", PDFAComplianceConformance.e_Level1B, null, 10, false);
				    PrintResults(pdf_a, filename);				
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done PDF/A Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}

        void PrintResults(PDFACompliance pdf_a, String filename) 
		{
			int err_cnt = pdf_a.GetErrorCount();
			if (err_cnt == 0) 
			{
                WriteLine(string.Format("{0}: OK.", filename));
			}
			else 
			{
                WriteLine(string.Format("{0} is NOT a valid PDFA.", filename));
				for (int i=0; i<err_cnt; ++i) 
				{
					PDFAComplianceErrorCode c = pdf_a.GetError(i);
					WriteLine(string.Format(" - e_PDFA{0}: {1}.", c, PDFACompliance.GetPDFAErrorMessage(c)));

					if (true) 
					{
						int num_refs = pdf_a.GetRefObjCount(c);
						if (num_refs > 0)  
						{
							Write("   Objects:");
							for (int j=0; j<num_refs; ) 
							{
                                Write(string.Format("{0}", pdf_a.GetRefObj(c, j)));
                                if (++j != num_refs) Write(", ");
							}
                            WriteLine("");
						}
					}
				}
			}
		}
	}
}
