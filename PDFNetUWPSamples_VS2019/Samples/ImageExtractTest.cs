//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.Common;
using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class ImageExtractTest : Sample
    {
        public ImageExtractTest() :
            base("ImageExtract", "This sample illustrates couple of approaches to PDF image extraction.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () =>
            {
                WriteLine("--------------------------------");
                WriteLine("Starting ImageExtract Test...");
                WriteLine("--------------------------------\n");
                // Example 1: 
                // Extract images by traversing the display list for 
                // every page. With this approach it is possible to obtain 
                // image positioning information and DPI.
                try
                {
                    String input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();

                    ElementReader reader = new ElementReader();
                    PageIterator itr;
                    for (itr = doc.GetPageIterator(); itr.HasNext(); itr.Next())
                    {
                        reader.Begin(itr.Current());
                        await ImageExtract(reader).ConfigureAwait(false);
                        reader.End();
                    }
                    doc.Destroy();
                    WriteLine("Done.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }
                
                WriteLine("----------------------------------------------------------------");

                // Example 2: 
                // Extract images by scanning the low-level document.
                try
                {
                    String input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();
                    image_counter = 0;

                    SDFDoc cos_doc = doc.GetSDFDoc();
                    int num_objs = cos_doc.XRefSize();
                    for (int i = 1; i < num_objs; ++i)
                    {
                        Obj obj = cos_doc.GetObj(i);
                        if (obj != null && !obj.IsFree() && obj.IsStream())
                        {
                            // Process only images
                            DictIterator itr = obj.Find("Subtype");
                            if (!itr.HasNext() || itr.Value().GetName() != "Image")
                                continue;

                            itr = obj.Find("Type");
                            if (!itr.HasNext() || itr.Value().GetName() != "XObject")
                                continue;

                            pdftron.PDF.Image image = new pdftron.PDF.Image(obj);
                        
                            WriteLine(string.Format("--> Image: {0}", ++image_counter));
                            WriteLine(string.Format("    Width: {0}", image.GetImageWidth()));
                            WriteLine(string.Format("    Height: {0}", image.GetImageHeight()));
                            WriteLine(string.Format("    BPC: {0}", image.GetBitsPerComponent()));

                            string fname = Path.Combine(OutputPath, "image_extract2_" + image_counter.ToString() + ".png");
                            image.ExportAsPng(fname);  // or Export() to automatically select format
                            WriteLine("Image exported to " + fname);
                            await AddFileToOutputList(fname).ConfigureAwait(false);

                            // Convert PDF bitmap to GDI+ Bitmap...
                            //Bitmap bmp = image.GetBitmap();
                            //bmp.Save(fname, ImageFormat.Png);
                            //

                            // Instead of converting PDF images to a Bitmap, you can also extract 
                            // uncompressed/compressed image data directly using element.GetImageData() 
                            // as illustrated in ElementReaderAdv sample project.
                        }
                    }

                    doc.Destroy();
                    WriteLine("Done.");
                }
                catch (Exception e)
                {
                    WriteLine("\n" + e.ToString());
                }
                
                WriteLine("\n--------------------------------");
                WriteLine("Done ImageExtract Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        ///-----------------------------------------------------------------------------------
        /// This sample illustrates one approach to PDF image extraction 
        /// using PDFNet.
        /// 
        /// Note: Besides direct image export, you can also convert PDF images 
        /// to GDI+ Bitmap, or extract uncompressed/compressed image data directly 
        /// using element.GetImageData() (e.g. as illustrated in ElementReaderAdv 
        /// sample project).
        ///-----------------------------------------------------------------------------------

        int image_counter = 0;

        async Task<string> ImageExtract(ElementReader reader)
        {
            String result = "";
            Element element;
            while ((element = reader.Next()) != null)
            {
                switch (element.GetType())
                {
                    case ElementType.e_image:
                    case ElementType.e_inline_image:
                        {
                            result += (string.Format("--> Image: {0}\n", ++image_counter));
                            result += (string.Format("    Width: {0}\n", element.GetImageWidth()));
                            result += (string.Format("    Height: {0}\n", element.GetImageHeight()));
                            result += (string.Format("    BPC: {0}\n", element.GetBitsPerComponent()));

                            Matrix2D ctm = element.GetCTM();
                            //double x2 = 1, y2 = 1;
                            //ctm.Mult(ref x2, ref y2);
                            /*
                            pdftron.Common.Double x2 = new pdftron.Common.Double(1);
                            pdftron.Common.Double y2 = new pdftron.Common.Double(1);
                            ctm.Mult(x2, y2);
                            
                            Message += string.Format("\n    Coords: x1={0}, y1={1}, x2={2}, y2={3}", ctm.m_h, ctm.m_v, x2, y2);
                            */
                            if (element.GetType() == ElementType.e_image)
                            {
                                string fname = Path.Combine(OutputPath, "image_extract1_" + image_counter.ToString() + ".tif");
                                pdftron.PDF.Image image = new pdftron.PDF.Image(element.GetXObject());
                                image.ExportAsTiff(fname);  // or Export() to automatically select format
                                WriteLine("Image exported to " + fname);
                                await AddFileToOutputList(fname).ConfigureAwait(false);

                                // Convert PDF bitmap to GDI+ Bitmap...
                                //Bitmap bmp = element.GetBitmap();
                                //bmp.Save(fname, ImageFormat.Png);
                                //

                                // Instead of converting PDF images to a Bitmap, you can also extract 
                                // uncompressed/compressed image data directly using element.GetImageData() 
                                // as illustrated in ElementReaderAdv sample project.
                            }
                            break;
                        }
                    case ElementType.e_form: // Process form XObjects
                        {
                            reader.FormBegin();
                            result += await ImageExtract(reader);
                            reader.End();
                            break;
                        }
                }
            }
            return result;
        }
    }
}
