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
    public sealed class ImpositionTest : Sample
    {
        public ImpositionTest() :
            base("Imposition", "The sample illustrates how multiple pages can be combined/imposed using PDFNet. Page imposition can be used to arrange/order pages prior to printing or to assemble a 'master' page from several 'source' pages. Using PDFNet API it is possible to write applications that can re-order the pages such that they will display in the correct order when the hard copy pages are compiled and folded correctly.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Imposition Test...");
                WriteLine("--------------------------------\n");
			    try	
			    {
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);
                    PDFDoc in_doc = new PDFDoc(input_file_path);
				    in_doc.InitSecurityHandler();

				    // Create a list of pages to import from one PDF document to another.
                    IList<pdftron.PDF.Page> import_list = new List<pdftron.PDF.Page>();
				    for (PageIterator itr = in_doc.GetPageIterator(); itr.HasNext(); itr.Next()) 
					    import_list.Add(itr.Current());

				    PDFDoc new_doc = new PDFDoc(); //  Create a new document
                    IList<pdftron.PDF.Page> imported_pages = new_doc.ImportPages(import_list);

				    // Paper dimension for A3 format in points. Because one inch has 
				    // 72 points, 11.69 inch 72 = 841.69 points
                    pdftron.PDF.Rect media_box = new pdftron.PDF.Rect(0, 0, 1190.88, 841.69); 
				    double mid_point = media_box.Width()/2;

				    ElementBuilder builder = new ElementBuilder();
				    ElementWriter  writer  = new ElementWriter();

				    for (int i=0; i<imported_pages.Count; ++i)
				    {
					    // Create a blank new A3 page and place on it two pages from the input document.
                        pdftron.PDF.Page new_page = new_doc.PageCreate(media_box);
					    writer.Begin(new_page);

					    // Place the first page
                        pdftron.PDF.Page src_page = (pdftron.PDF.Page)imported_pages[i];
					    Element element = builder.CreateForm(src_page);

					    double sc_x = mid_point / src_page.GetPageWidth();
					    double sc_y = media_box.Height() / src_page.GetPageHeight();
					    double scale = Math.Min(sc_x, sc_y);
					    element.GetGState().SetTransform(scale, 0, 0, scale, 0, 0);
					    writer.WritePlacedElement(element);

					    // Place the second page
					    ++i; 
					    if (i<imported_pages.Count)	
					    {
                            src_page = (pdftron.PDF.Page)imported_pages[i];
						    element = builder.CreateForm(src_page);
						    sc_x = mid_point / src_page.GetPageWidth();
						    sc_y = media_box.Height() / src_page.GetPageHeight();
						    scale = Math.Min(sc_x, sc_y);
						    element.GetGState().SetTransform(scale, 0, 0, scale, mid_point, 0);
						    writer.WritePlacedElement(element);
					    }

                        writer.End();
					    new_doc.PagePushBack(new_page);
				    }

                    string output_file_path = Path.Combine(OutputPath, "newsletter_booklet.pdf");
                    await new_doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                    new_doc.Destroy();
                    in_doc.Destroy();
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
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
