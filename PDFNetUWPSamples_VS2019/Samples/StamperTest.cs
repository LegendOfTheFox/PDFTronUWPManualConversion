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
    public sealed class StamperTest : Sample
    {
        public StamperTest() :
            base("Stamper", "The sample shows how to use 'pdftron.PDF.Stamper' utility class to stamp PDF pages with text, images, or with other PDF pages. ElementBuilder and ElementWriter should be used for more complex PDF stamping operations.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Stamper Test...");
                WriteLine("--------------------------------\n");

			    string input_filename = Path.Combine(InputPath, "newsletter.pdf");
			    //--------------------------------------------------------------------------------
			    // Example 1) Add text stamp to all pages, then remove text stamp from odd pages. 
			    try
                {
                    WriteLine("Opening input file " + input_filename);

                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_relative_scale, 0.5, 0.5);
                        s.SetAlignment(StamperHorizontalAlignment.e_horizontal_center, StamperVerticalAlignment.e_vertical_center);
                        s.SetFontColor(new ColorPt(1, 0, 0)); // set text color to red      
                        s.StampText(doc, "If you are reading this\nthis is an even page", new PageSet(1, doc.GetPageCount()));
                        //delete all text stamps in odd pages
                        Stamper.DeleteStamps(doc, new PageSet(1, doc.GetPageCount(), PageSetFilter.e_odd));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex1.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

			    //--------------------------------------------------------------------------------
			    // Example 2) Add Image stamp to first 2 pages. 
			    try
			    {
                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_relative_scale, .05, .05);
                        pdftron.PDF.Image img = pdftron.PDF.Image.Create(doc.GetSDFDoc(), Path.Combine(InputPath, "peppers.jpg"));
                        s.SetSize(StamperSizeType.e_relative_scale, 0.5, 0.5);
                        //set position of the image to the center, left of PDF pages
                        s.SetAlignment(StamperHorizontalAlignment.e_horizontal_left, StamperVerticalAlignment.e_vertical_center);
                        s.SetFontColor(new ColorPt(0, 0, 0, 0));
                        s.SetRotation(180);
                        s.SetAsBackground(false);
                        //only stamp first 2 pages
                        s.StampImage(doc, img, new PageSet(1, 2));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex2.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

			    //--------------------------------------------------------------------------------
			    // Example 3) Add Page stamp to all pages. 
			    try
			    {
                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        String fishInputPath = Path.Combine(InputPath, "fish.pdf");
                        WriteLine("\nOpening input file " + fishInputPath);
                        PDFDoc fish_doc = new PDFDoc(fishInputPath);
                        fish_doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_relative_scale, .5, .5);
                        pdftron.PDF.Page src_page = fish_doc.GetPage(1);
                        pdftron.PDF.Rect page_one_crop = src_page.GetCropBox();
                        // set size of the image to 10% of the original while keep the old aspect ratio
                        s.SetSize(StamperSizeType.e_absolute_size, page_one_crop.Width() * 0.1, -1);
                        s.SetOpacity(0.4);
                        s.SetRotation(-67);
                        //put the image at the bottom right hand corner
                        s.SetAlignment(StamperHorizontalAlignment.e_horizontal_right, StamperVerticalAlignment.e_vertical_bottom);
                        s.StampPage(doc, src_page, new PageSet(1, doc.GetPageCount()));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex3.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

			    //--------------------------------------------------------------------------------
			    // Example 4) Add Image stamp to first 20 odd pages. 
			    try
			    {
                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_absolute_size, 20, 20);
                        s.SetOpacity(1);
                        s.SetRotation(45);
                        s.SetAsBackground(true);
                        s.SetPosition(30, 40);
                        pdftron.PDF.Image img = pdftron.PDF.Image.Create(doc.GetSDFDoc(), Path.Combine(InputPath, "peppers.jpg"));
                        s.StampImage(doc, img, new PageSet(1, 20, PageSetFilter.e_odd));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex4.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

			    //--------------------------------------------------------------------------------
			    // Example 5) Add text stamp to first 20 even pages
			    try
			    {
                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_relative_scale, .05, .05);
                        s.SetPosition(0, 0);
                        s.SetOpacity(0.7);
                        s.SetRotation(90);
                        s.SetSize(StamperSizeType.e_font_size, 80, -1);
                        s.SetTextAlignment(StamperTextAlignment.e_align_center);
                        s.StampText(doc, "Goodbye\nMoon", new PageSet(1, 20, PageSetFilter.e_even));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex5.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

			    //--------------------------------------------------------------------------------
			    // Example 6) Add first page as stamp to all even pages
                try
                {
                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        PDFDoc fish_doc = new PDFDoc(Path.Combine(InputPath, "fish.pdf"));
                        fish_doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_relative_scale, .3, .3);
                        s.SetOpacity(1);
                        s.SetRotation(270);
                        s.SetAsBackground(true);
                        s.SetPosition(0.5, 0.5, true);
                        s.SetAlignment(StamperHorizontalAlignment.e_horizontal_left, StamperVerticalAlignment.e_vertical_bottom);
                        pdftron.PDF.Page page_one = fish_doc.GetPage(1);
                        s.StampPage(doc, page_one, new PageSet(1, doc.GetPageCount(), PageSetFilter.e_even));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex6.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
				    WriteLine(GetExceptionMessage(e));
                }

			    //--------------------------------------------------------------------------------
			    // Example 7) Add image stamp at top right corner in every pages
                try
                {
                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_relative_scale, .1, .1);
                        s.SetOpacity(0.8);
                        s.SetRotation(135);
                        s.SetAsBackground(false);
                        s.ShowsOnPrint(false);
                        s.SetAlignment(StamperHorizontalAlignment.e_horizontal_left, StamperVerticalAlignment.e_vertical_top);
                        s.SetPosition(10, 10);

                        pdftron.PDF.Image img = pdftron.PDF.Image.Create(doc.GetSDFDoc(), Path.Combine(InputPath, "peppers.jpg"));
                        s.StampImage(doc, img, new PageSet(1, doc.GetPageCount(), PageSetFilter.e_all));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex7.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
				    WriteLine(GetExceptionMessage(e));
                }

			    //--------------------------------------------------------------------------------
			    // Example 8) Add Text stamp to first 2 pages, and image stamp to first page.
			    //          Because text stamp is set as background, the image is top of the text
			    //          stamp. Text stamp on the first page is not visible.
                try
                {
                    using (PDFDoc doc = new PDFDoc(input_filename))
                    {
                        doc.InitSecurityHandler();

                        Stamper s = new Stamper(StamperSizeType.e_relative_scale, 0.07, -0.1);
                        s.SetAlignment(StamperHorizontalAlignment.e_horizontal_right, StamperVerticalAlignment.e_vertical_bottom);
                        s.SetAlignment(StamperHorizontalAlignment.e_horizontal_center, StamperVerticalAlignment.e_vertical_top);
                        s.SetFont(Font.Create(doc.GetSDFDoc(), FontStandardType1Font.e_courier, true));
                        s.SetFontColor(new ColorPt(1, 0, 0, 0)); //set color to red
                        s.SetTextAlignment(StamperTextAlignment.e_align_right);
                        s.SetAsBackground(true); //set text stamp as background
                        s.StampText(doc, "This is a title!", new PageSet(1, 2));

                        pdftron.PDF.Image img = pdftron.PDF.Image.Create(doc.GetSDFDoc(), Path.Combine(InputPath, "peppers.jpg"));
                        s.SetAsBackground(false); // set image stamp as foreground
                        s.StampImage(doc, img, new PageSet(1));

                        String output_file_path = Path.Combine(OutputPath, "newsletter.pdf.ex8.pdf");
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
				    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Stamper Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
