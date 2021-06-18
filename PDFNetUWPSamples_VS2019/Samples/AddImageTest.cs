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
    //-----------------------------------------------------------------------------------
    // This sample illustrates how to embed various raster image formats 
    // (e.g. TIFF, JPEG, JPEG2000, JBIG2, GIF, PNG, BMP, etc.) in a PDF document.
    //-----------------------------------------------------------------------------------
    /// </summary>
    public sealed class AddImageTest : Sample
    {
        public AddImageTest() :
            base("AddImage", "This sample illustrates how to embed various raster image formats (e.g. TIFF, JPEG, JPEG2000, JBIG2, GIF, PNG, BMP, etc.) in a PDF document.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () =>
            {
                WriteLine("--------------------------------");
                WriteLine("Starting Add Image Test...");
                WriteLine("--------------------------------\n");
                try
                {
                    PDFDoc doc = new PDFDoc();
                    SDFDoc sdfDoc = doc.GetSDFDoc();

                    ElementBuilder bld = new ElementBuilder();	// Used to build new Element objects
                    ElementWriter writer = new ElementWriter();	// Used to write Elements to the page	

                    Page page = doc.PageCreate();	// Start a new page 
                    writer.Begin(page);				// Begin writing to this page

                    // ----------------------------------------------------------
                    // Embed a JPEG image to the output document. 
                    Image img = await Image.CreateAsync(sdfDoc, Path.Combine(InputPath, "peppers.jpg"));

                    Element element = bld.CreateImage(img, new Matrix2D(200, 0, 0, 250, 50, 500));
                    writer.WritePlacedElement(element);


                    // ----------------------------------------------------------
                    // Add a TIFF image to the output file
                    img = await Image.CreateAsync(sdfDoc, Path.Combine(InputPath, "grayscale.tif"));
                    element = bld.CreateImage(img, new Matrix2D(img.GetImageWidth(), 0, 0, img.GetImageHeight(), 10, 50));
                    writer.WritePlacedElement(element);

                    writer.End();	// Finish writing to the page
                    doc.PagePushBack(page);


                    // ----------------------------------------------------------
                    // Embed a TIFF image using JBIG2 Encoding

                    // Create a new page 
                    page = doc.PageCreate(new pdftron.PDF.Rect(0, 0, 612, 794));
                    writer.Begin(page);	// Begin writing to the page

                    // Use JBIG2 Encoding
                    ObjSet objset = new ObjSet();
                    Obj jbig2_hint = objset.CreateName("JBIG2");

                    img = await Image.CreateAsync(sdfDoc, Path.Combine(InputPath, "multipage.tif"), jbig2_hint);
                    element = bld.CreateImage(img, new Matrix2D(612, 0, 0, 794, 0, 0));
                    writer.WritePlacedElement(element);

                    writer.End();  // Finish writing to the page
                    doc.PagePushBack(page);

                    // Embed the second TIFF frame. Use JBIG2 Encoding
                    page = doc.PageCreate(); // Create a new page 
                    writer.Begin(page);		 // Begin writing to this page


                    // ----------------------------------------------------------
                    // Add a JPEG2000 (JP2) image to the output file

                    // Create a new page 
                    page = doc.PageCreate();
                    writer.Begin(page);	// Begin writing to the page

                    // Embed the image.
                    img = await Image.CreateAsync(sdfDoc, Path.Combine(InputPath, "palm.jp2"));

                    // Position the image on the page.
                    element = bld.CreateImage(img, new Matrix2D(img.GetImageWidth(), 0, 0, img.GetImageHeight(), 96, 80));
                    writer.WritePlacedElement(element);

                    // Write 'JPEG2000 Sample' text string under the image.
                    writer.WriteElement(bld.CreateTextBegin(Font.Create(doc, FontStandardType1Font.e_times_roman), 32)); 
                    element = bld.CreateTextRun("JPEG2000 Sample");
                    element.SetTextMatrix(1, 0, 0, 1, 190, 30);
                    writer.WriteElement(element);
                    writer.WriteElement(bld.CreateTextEnd());

                    writer.End();	// Finish writing to the page
                    doc.PagePushBack(page);
                  
                    // Calling Dispose() results in increased performance and lower memory consumption.
                    bld.Dispose();
                    writer.Dispose();

                    String output_file_path = Path.Combine(OutputPath, "addimage.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    doc.Destroy();
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Annotation Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }
    }
}
