//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.PDF;
using pdftron.SDF;

using pdftron.Common;
using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    /// <summary>
    ///---------------------------------------------------------------------------------------
    /// The following sample illustrates how to use the PDF::Convert utility class to convert 
    /// .docx files to PDF
    ///
    /// This conversion is performed entirely within the PDFNet and has *no* external or
    /// system dependencies
    ///
    /// Please contact us if you have any questions.	
    ///---------------------------------------------------------------------------------------
    /// </summary>

    public sealed class OfficeToPDFTest : Sample
    {
        public OfficeToPDFTest() :
            base("OfficeToPDF", "The following sample illustrates how to use the PDF::Convert utility class to convert .docx files to PDF")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () =>
            {
                WriteLine("--------------------------------");
                WriteLine("Starting OfficeToPDF Test...");
                WriteLine("--------------------------------\n");

                try
                {
                    // first the one-line conversion method
                    await SimpleConvert("Fishermen.docx", "Fishermen.pdf");

                    // then the more flexible line-by-line conversion API
                    await FlexibleConvert("the_rime_of_the_ancient_mariner.docx", "the_rime_of_the_ancient_mariner.pdf");

                    // conversion of RTL content
                    await FlexibleConvert("factsheet_Arabic.docx", "factsheet_Arabic.pdf");
                }
                catch (Exception e)
                {
                    WriteLine("Unrecognized Exception: " + e.Message);
                }

                WriteLine("--------------------------------");
                WriteLine("Done OfficeToPDF Test.");
                WriteLine("--------------------------------\n");

            })).AsAsyncAction();
        }

        async Task<bool> SimpleConvert(String input_filename, String output_filename)
        {
            // Make sure all files exist
            if (!File.Exists(Path.Combine(InputPath, input_filename)))
            {
                WriteLine("File: " + input_filename + " not found!");
                return false;
            }

                // Start with a PDFDoc (the conversion destination)
                using (PDFDoc doc = new PDFDoc())
            {
                // perform the conversion with no optional parameters
                pdftron.PDF.Convert.OfficeToPDF(doc, Path.Combine(InputPath, input_filename), null);

                // save the result
                await doc.SaveAsync(Path.Combine(OutputPath, output_filename), SDFDocSaveOptions.e_linearized);

                WriteLine("Saved " + output_filename);

                await AddFileToOutputList(Path.Combine(OutputPath, output_filename));

                return true;
            }
        }

        async Task<bool> FlexibleConvert(String input_filename, String output_filename)
        {
            // Make sure all files exist
            if (!File.Exists(Path.Combine(InputPath,input_filename)))
            {                
                WriteLine("File: " + input_filename + " not found!");
                return false;
            }

            // Start with a PDFDoc (the conversion destination)
            using (PDFDoc doc = new PDFDoc())
            {
                // Instanciate convertion options
                OfficeToPDFOptions options = new OfficeToPDFOptions();

                // Smart Substitutions requires a plugin file, here we set the path to it 
                options.SetSmartSubstitutionPluginPath(
                    System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Resources"));
                
                // create a conversion object -- this sets things up but does not yet
                // perform any conversion logic.
                // in a multithreaded environment, this object can be used to monitor
                // the conversion progress and potentially cancel it as well
                DocumentConversion conversion = 
                    pdftron.PDF.Convert.StreamingPDFConversion(doc, Path.Combine(InputPath, input_filename), options);

                // actually perform the conversion
                // this particular method will not throw on conversion failure, but will
                // return an error status instead
                if (conversion.TryConvert() == DocumentConversionResult.e_document_conversion_success)
                {
                    var num_warnings = conversion.GetNumWarnings();

                    // print information about the conversion 
                    for (uint i = 0; i < num_warnings; ++i)
                    {
                        WriteLine("Warning: " + conversion.GetWarningString(i));
                    }

                    // save the result
                    await doc.SaveAsync(Path.Combine(OutputPath, output_filename), SDFDocSaveOptions.e_linearized);

                    WriteLine("Saved " + output_filename);

                    await AddFileToOutputList(Path.Combine(OutputPath, output_filename));

                    return true;
                }

                return false;
            }
        }
    }
}
