//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class PDFDrawTest : Sample
    {
        public PDFDrawTest() :
            base("PDFDraw", "This sample illustrates how to use the built-in rasterizer in order to render PDF images on the fly and how to save resulting images in PNG and JPEG format.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting PDFDraw Test...");
                WriteLine("--------------------------------\n");
                await this.Run().ConfigureAwait(false);
                WriteLine("\n--------------------------------");
                WriteLine("Done PDFDraw Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        async Task Run()
        {
            /*
            try
            {
                // Optional: Set ICC color profiles to fine tune color conversion 
                // for PDF 'device' color spaces. You can use your own ICC profiles. 
                // Standard Adobe color profiles can be download from Adobes site: 
                // http://www.adobe.com/support/downloads/iccprofiles/iccprofiles_win.html
                //
                // Simply drop all *.icc files in PDFNet resource folder or you specify 
                // the full pathname.
                //---
                // PDFNet.SetResourcesPath("../../../../../resources");
                // PDFNet.SetColorManagement();
                // PDFNet.SetDefaultDeviceCMYKProfile("USWebCoatedSWOP.icc"); // will search in PDFNet resource folder.
                // PDFNet.SetDefaultDeviceRGBProfile("AdobeRGB1998.icc"); 

                // Optional: Set predefined font mappings to override default font 
                // substitution for documents with missing fonts. For example:
                //---
                // PDFNet.AddFontSubst("StoneSans-Semibold", "C:/WINDOWS/Fonts/comic.ttf");
                // PDFNet.AddFontSubst("StoneSans", "comic.ttf");  // search for 'comic.ttf' in PDFNet resource folder.
                // PDFNet.AddFontSubst(PDFNet.CharacterOrdering.e_Identity, "C:/WINDOWS/Fonts/arialuni.ttf");
                // PDFNet.AddFontSubst(PDFNet.CharacterOrdering.e_Japan1, "C:/Program Files/Adobe/Acrobat 7.0/Resource/CIDFont/KozMinProVI-Regular.otf");
                // PDFNet.AddFontSubst(PDFNet.CharacterOrdering.e_Japan2, "c:/myfonts/KozMinProVI-Regular.otf");
                //
                // If fonts are in PDFNet resource folder, it is not necessary to specify 
                // the full path name. For example,
                //---
                // PDFNet.AddFontSubst(PDFNet.CharacterOrdering.e_Korea1, "AdobeMyungjoStd-Medium.otf");
                // PDFNet.AddFontSubst(PDFNet.CharacterOrdering.e_CNS1, "AdobeSongStd-Light.otf");
                // PDFNet.AddFontSubst(PDFNet.CharacterOrdering.e_GB1, "AdobeMingStd-Light.otf");
            }
            catch
            {
                WriteLine("The specified color profile was not found.");
            }
            */

            using (PDFDraw draw = new PDFDraw())
            {
                //--------------------------------------------------------------------------------
                // Example 1) Convert the first PDF page to PNG at 92 DPI. 
                // A three step tutorial to convert PDF page to an image.
                try
                {
                    // A) Open the PDF document.
                    using (PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "tiger.pdf")))
                    {
                        // Initialize the security handler, in case the PDF is encrypted.
                        doc.InitSecurityHandler();

                        // B) The output resolution is set to 92 DPI.
                        draw.SetDPI(92);

                        // C) Rasterize the first page in the document and save the result as PNG.
                        pdftron.PDF.Page pg = doc.GetPage(1);
                        String output_file_path = Path.Combine(OutputPath, "tiger_92dpi.png");
                        draw.Export(pg, output_file_path);
                        WriteLine(String.Format("Example 1: Result saved in {0}", output_file_path));
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                        // Export the same page as TIFF
                        output_file_path = Path.Combine(OutputPath, "tiger_92dpi.tif");
                        draw.Export(pg, output_file_path, "TIFF");
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                //--------------------------------------------------------------------------------
                // Example 2) Convert the all pages in a given document to JPEG at 72 DPI.
                ObjSet hint_set = new ObjSet(); // A collection of rendering 'hits'.
                WriteLine("Example 2:");

                try
                {
                    using (PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "newsletter.pdf")))
                    {
                        // Initialize the security handler, in case the PDF is encrypted.
                        doc.InitSecurityHandler();

                        draw.SetDPI(72); // Set the output resolution is to 72 DPI.

                        // Use optional encoder parameter to specify JPEG quality.
                        Obj encoder_param = hint_set.CreateDict();
                        encoder_param.PutNumber("Quality", 80);

                        // Traverse all pages in the document.
                        for (PageIterator itr = doc.GetPageIterator(); itr.HasNext(); itr.Next())
                        {
                            string output_file_path = string.Format(@"{0}\newsletter{1:d}.jpg", OutputPath, itr.GetPageNumber());
                            WriteLine(String.Format("\nResult saved in {0}", output_file_path));
                            draw.Export(itr.Current(), output_file_path, "JPEG", encoder_param);
                            await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                try  // Examples 3-5
                {
                    // Common code for remaining samples.

                    using (PDFDoc tiger_doc = new PDFDoc(Path.Combine(InputPath, "tiger.pdf")))
                    {
                        // Initialize the security handler, in case the PDF is encrypted.
                        tiger_doc.InitSecurityHandler();
                        pdftron.PDF.Page page = tiger_doc.GetPage(1);

                        //--------------------------------------------------------------------------------
                        // Example 3) Convert the first page to WriteableBitmap. Also, rotate the 
                        // page 90 degrees and save the result as TIFF.
                        draw.SetDPI(100); // Set the output resolution is to 100 DPI.
                        draw.SetRotate(pdftron.PDF.PageRotate.e_90);  // Rotate all pages 90 degrees clockwise.

                        StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
                        StorageFile outputFile = await storageFolder.CreateFileAsync("tiger_100dpi_rot90.tif", CreationCollisionOption.ReplaceExisting);

                        var bmpInfo = await draw.GetRawBitmapAsync(page).AsTask().ConfigureAwait(false);
                        byte[] pixels = bmpInfo.Buffer;
                        int height = bmpInfo.Height;
                        int width = bmpInfo.Width;

                        int offset;

                        for (int row = 0; row < height; row++)
                        {
                            for (int col = 0; col < width; col++)
                            {
                                offset = (row * width * 4) + (col * 4);
                                byte B = pixels[offset];
                                byte G = pixels[offset + 1];
                                byte R = pixels[offset + 2];
                                byte A = pixels[offset + 3];

                                // convert to RGBA format for BitmapEncoder
                                pixels[offset] = R; // Red
                                pixels[offset + 1] = G; // Green
                                pixels[offset + 2] = B; // Blue
                                pixels[offset + 3] = A; // Alpha
                            }
                        }
                        IRandomAccessStream writeStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite);
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.TiffEncoderId, writeStream);
                        encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied, (uint)width, (uint) height, 96, 96, pixels);
                        await encoder.FlushAsync().AsTask().ConfigureAwait(false);
                        await writeStream.GetOutputStreamAt(0).FlushAsync().AsTask().ConfigureAwait(false);

                        //bmp.Save(output_path + "tiger_100dpi_rot90.tif", System.Drawing.Imaging.ImageFormat.Tiff);
                        //Message += String.Format("\nExample 3: Result saved in {0}", output_path + "tiger_100dpi_rot90.tif");
                        draw.SetRotate(pdftron.PDF.PageRotate.e_0);  // Disable image rotation for remaining samples.

                        //--------------------------------------------------------------------------------
                        // Example 4) Convert PDF page to a fixed image size. Also illustrates some 
                        // other features in PDFDraw class such as rotation, image stretching, exporting 
                        // to grayscale, or monochrome.

                        // Initialize render 'gray_hint' parameter, that is used to control the 
                        // rendering process. In this case we tell the rasterizer to export the image as 
                        // 1 Bit Per Component (BPC) image.
                        Obj mono_hint = hint_set.CreateDict();
                        mono_hint.PutNumber("BPC", 1);

                        // SetImageSize can be used instead of SetDPI() to adjust page  scaling 
                        // dynamically so that given image fits into a buffer of given dimensions.
                        String output_file_path = Path.Combine(OutputPath, "tiger_1000x1000.png");
                        draw.SetImageSize(1000, 1000);		// Set the output image to be 1000 wide and 1000 pixels tall
                        draw.Export(page, output_file_path, "PNG", mono_hint);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                        WriteLine(String.Format("\nExample 4: Result saved in {0}", output_file_path));

                        draw.SetImageSize(200, 400);	    // Set the output image to be 200 wide and 300 pixels tall
                        draw.SetRotate(pdftron.PDF.PageRotate.e_180);  // Rotate all pages 90 degrees clockwise.

                        // 'gray_hint' tells the rasterizer to export the image as grayscale.
                        Obj gray_hint = hint_set.CreateDict();
                        gray_hint.PutName("ColorSpace", "Gray");

                        output_file_path = Path.Combine(OutputPath, "tiger_200x400_rot180.png");
                        draw.Export(page, output_file_path, "PNG", gray_hint);
                        WriteLine(String.Format("\nExample 4: Result saved in {0}", output_file_path));
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                        draw.SetImageSize(400, 200, false, true);  // The third parameter sets 'preserve-aspect-ratio' to false.
                        draw.SetRotate(pdftron.PDF.PageRotate.e_0);    // Disable image rotation.
                        output_file_path = Path.Combine(OutputPath, "tiger_400x200_stretch.jpg");
                        draw.Export(page, output_file_path, "JPEG");
                        WriteLine(String.Format("\nExample 4: Result saved in {0}", output_file_path));
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                        //--------------------------------------------------------------------------------
                        // Example 5) Zoom into a specific region of the page and rasterize the 
                        // area at 200 DPI and as a thumbnail (i.e. a 50x50 pixel image).
                        page.SetCropBox(new pdftron.PDF.Rect(216, 522, 330, 600));	// Set the page crop box.

                        // Select the crop region to be used for drawing.
                        draw.SetPageBox(pdftron.PDF.PageBox.e_crop);
                        draw.SetDPI(900);  // Set the output image resolution to 900 DPI.
                        output_file_path = Path.Combine(OutputPath, "tiger_zoom_900dpi.png");
                        draw.Export(page, output_file_path, "PNG");
                        WriteLine(String.Format("\nExample 5: Result saved in {0}", output_file_path));
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                        draw.SetImageSize(50, 50);	   // Set the thumbnail to be 50x50 pixel image.
                        output_file_path = Path.Combine(OutputPath, "tiger_zoom_50x50.png");
                        draw.Export(page, output_file_path, "PNG");
                        WriteLine(String.Format("\nExample 6: Result saved in {0}", output_file_path));
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                Obj cmyk_hint = hint_set.CreateDict();
                cmyk_hint.PutName("ColorSpace", "CMYK");

                //--------------------------------------------------------------------------------
                // Example 7) Convert the first PDF page to CMYK TIFF at 92 DPI. 
                // A three step tutorial to convert PDF page to an image.
                try
                {
                    // A) Open the PDF document.
                    using (PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "tiger.pdf")))
                    {
                        // Initialize the security handler, in case the PDF is encrypted.
                        doc.InitSecurityHandler();

                        // B) The output resolution is set to 92 DPI.
                        draw.SetDPI(92);

                        // C) Rasterize the first page in the document and save the result as TIFF.
                        String output_file_path = Path.Combine(OutputPath, "out1.tif");
                        pdftron.PDF.Page pg = doc.GetPage(1);
                        draw.Export(pg, output_file_path, "TIFF", cmyk_hint);
                        WriteLine(String.Format("\nExample 7: Result saved in {0}", output_file_path));
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }
                // using PDFDraw
            }
        }
    }
}
