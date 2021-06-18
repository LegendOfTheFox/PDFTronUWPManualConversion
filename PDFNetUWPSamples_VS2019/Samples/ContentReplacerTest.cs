//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.PDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class ContentReplacerTest : Sample
    {
        public ContentReplacerTest() :
            base("ContentReplacer", "This sample shows how to use 'pdftron.PDF.ContentReplacer' to search and replace text strings and images in existing PDF (e.g. business cards and other PDF templates).")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting ContentReplacer Test...");
                WriteLine("--------------------------------\n");
			    // The following example illustrates how to replace an image in a certain region,
			    // and how to change template text.
			    try
			    {
                    String input_file_path = Path.Combine(InputPath, "BusinessCardTemplate.pdf");
                    WriteLine("Opening input file " + input_file_path );
				    PDFDoc doc = new PDFDoc(input_file_path);
				    doc.InitSecurityHandler();

				    // first, replace the image on the first page
				    ContentReplacer replacer = new ContentReplacer();
                    pdftron.PDF.Page page = doc.GetPage(1);
                    //pdftron.SDF.SDFDoc sdoc = new pdftron.SDF.SDFDoc(doc);
                    pdftron.PDF.Image img = pdftron.PDF.Image.Create(doc.GetSDFDoc(), Path.Combine(InputPath, "peppers.jpg"));
				    replacer.AddImage(page.GetMediaBox(), img.GetSDFObj());
				    // next, replace the text place holders on the second page
				    replacer.AddString("NAME", "John Smith");
				    replacer.AddString("QUALIFICATIONS", "Philosophy Doctor"); 
				    replacer.AddString("JOB_TITLE", "Software Developer"); 
				    replacer.AddString("ADDRESS_LINE1", "#100 123 Software Rd"); 
				    replacer.AddString("ADDRESS_LINE2", "Vancouver, BC"); 
				    replacer.AddString("PHONE_OFFICE", "604-730-8989"); 
				    replacer.AddString("PHONE_MOBILE", "604-765-4321"); 
				    replacer.AddString("EMAIL", "info@pdftron.com"); 
				    replacer.AddString("WEBSITE_URL", "http://www.pdftron.com"); 
				    // finally, apply
				    replacer.Process(page);

                    String output_file_path = Path.Combine(OutputPath, "BusinessCard.pdf");
                    await doc.SaveAsync(output_file_path, 0);
				    doc.Destroy();
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
                }

			    // The following example illustrates how to replace text in a given region
			    try {
                    String input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    PDFDoc doc = new PDFDoc(input_file_path);
				    doc.InitSecurityHandler();

				    ContentReplacer replacer = new ContentReplacer();
                    pdftron.PDF.Page page = doc.GetPage(1);
                    pdftron.PDF.Rect target_region = page.GetMediaBox();
				    string replacement_text = "hello hello hello hello hello hello hello hello hello hello";
				    replacer.AddText(target_region, replacement_text);
				    replacer.Process(page);

                    String output_file_path = Path.Combine(OutputPath, "ContentReplaced.pdf");
                    await doc.SaveAsync(output_file_path, 0);
				    doc.Destroy();
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
                }
                WriteLine("\n--------------------------------");
                WriteLine("Done ContentReplacer Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
