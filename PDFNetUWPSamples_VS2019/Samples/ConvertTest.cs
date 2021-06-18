//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class ConvertTest : Sample
    {
        public ConvertTest() :
            base("Convert", "This sample shows how to use PDFNet Convert Add-on (i.e. 'pdftron.PDF.Convert' namespace) for direct, high-quality conversion between PDF, XPS, EMF, SVG, TIFF, PNG, JPEG, and other image formats.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Convert Test...");
                WriteLine("--------------------------------\n");

                bool err = false;

                err = await ConvertToPdfFromFile();
                if (err)
                {
                    WriteLine("ConvertFile failed");
                }
                else
                {
                    WriteLine("ConvertFile succeeded");
                }

                err = await ConvertSpecificFormats();
                if (err)
                {
                    WriteLine("ConvertSpecificFormats failed");
                }
                else
                {
                    WriteLine("ConvertSpecificFormats succeeded");
                }

                err = await ConvertToXpsFromFile().ConfigureAwait(false);
                if (err)
                {
                    WriteLine("ConvertToXpsFromFile failed");
                }
                else
                {
                    WriteLine("ConvertToXpsFromFile succeeded");
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Convert Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        async Task<bool> ConvertSpecificFormats()
		{
            bool err = false;
			try
            {
                PDFDoc pdfdoc = new PDFDoc();
                String input_file_path = Path.Combine(InputPath, "simple-xps.xps");
                String output_file_path = Path.Combine(OutputPath, "ConvertTest_fromXps.pdf");
                pdftron.PDF.Convert.FromXps(pdfdoc, input_file_path);
                await pdfdoc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                pdfdoc.Destroy();
                WriteLine("Done. Result saved in " + output_file_path);
                await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			}
			catch (Exception e)
			{
                WriteLine(GetExceptionMessage(e));
				err = true;
			}
            try
            {
                String input_file_path = Path.Combine(InputPath, "simple-word_2007.docx");
                String output_file_path = Path.Combine(OutputPath, "ConvertTest_WordToPDF.pdf");
                DocumentConversion conversion = pdftron.PDF.Convert.WordToPDFConversion(input_file_path, null);
                conversion.Convert();
                uint num_warnings = conversion.GetNumWarnings();
                if (num_warnings > 0)
                {
                    WriteLine("WordToPDF conversion warnings: ");
                    for (uint i = 0; i < num_warnings; ++i)
                    {
                        WriteLine(conversion.GetWarningString(i));
                    }
                }
                PDFDoc pdfdoc = conversion.GetDoc();
                await pdfdoc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                pdfdoc.Destroy();
                WriteLine("Done. Result saved in " + output_file_path);
                await AddFileToOutputList(output_file_path).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                WriteLine(GetExceptionMessage(e));
                err = true;
            }
			return err;
		}

        private struct TestFile
        {
            public TestFile(String inputFile, String outputFile)
            {
                this.InputFile = Path.Combine(InputPath, inputFile);
                this.OutputFile = Path.Combine(OutputPath, outputFile);
            }
            public String InputFile;
            public String OutputFile;
        }

        async Task<bool> ConvertToPdfFromFile()
		{
            List<TestFile> testfiles = new List<TestFile>(
                new TestFile[] {
                    new TestFile("butterfly.png", "ConvertTest_png2pdf.pdf"),
                    new TestFile("pdftron.bmp", "ConvertTest_bmp2pdf.pdf"),
                    new TestFile("simple-xps.xps", "ConvertTest_xps2pdf.pdf")
                }
            );

            bool err = false;

			foreach (var testfile in testfiles)
			{
				try
				{
					pdftron.PDF.PDFDoc pdfdoc = new PDFDoc();
					pdftron.PDF.Convert.ToPdf(pdfdoc, testfile.InputFile);
                    await pdfdoc.SaveAsync(testfile.OutputFile, SDFDocSaveOptions.e_linearized);
                    pdfdoc.Destroy();
					pdfdoc = null;
                    WriteLine("Converted file: " + testfile.InputFile);
                    WriteLine("        to pdf: " + testfile.OutputFile);
                    await AddFileToOutputList(testfile.OutputFile).ConfigureAwait(false);
				}
				catch (Exception e)
				{
                    WriteLine("ERROR: on input file " + testfile.InputFile);
					WriteLine(GetExceptionMessage(e));
					err = true;
				}
			}
			return err;
		}

        async Task<bool> ConvertToXpsFromFile()
        {
            List<TestFile> testfiles = new List<TestFile>(
                new TestFile[] {
                    new TestFile("butterfly.png", "butterfly.xps"),
                    new TestFile("newsletter.pdf", "newsletter.xps")
                }
            );

            bool err = false;

            foreach (var testfile in testfiles)
            {
			    try
			    {
				    pdftron.PDF.Convert.ToXps(testfile.InputFile, testfile.OutputFile);
                    WriteLine("Converted file: " + testfile.InputFile);
                    WriteLine("        to xps: " + testfile.OutputFile);
                    await AddFileToOutputList(testfile.OutputFile).ConfigureAwait(false);
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
				    err = true;
			    }
            }
			return err;
		}

	}
}
