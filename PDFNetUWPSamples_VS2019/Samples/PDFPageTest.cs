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
using System.Collections.Generic;

namespace PDFNetSamples
{
    public sealed class PDFPageTest : Sample
    {
        public PDFPageTest() :
            base("PDFPage", "The sample illustrates how to copy pages from one document to another, how to delete, and re-arrange pages and how to use ImportPages() method for very efficient copy and merge operations.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting PDFPage Test...");
                WriteLine("--------------------------------\n");
			    // Sample 1 - Split a PDF document into multiple pages
			    try
			    {
                    WriteLine("_______________________________________________");
                    WriteLine("Sample 1 - Split a PDF document into multiple pages...");
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                   
                    WriteLine("Opening input file " + input_file_path);

                    using (PDFDoc in_doc = new PDFDoc(input_file_path))
                    {
                        int page_num = in_doc.GetPageCount();
                        for (int i = 1; i <= page_num; ++i)
                        {
                            using (PDFDoc new_doc = new PDFDoc())
                            {
                                new_doc.InsertPages(0, in_doc, i, i, PDFDocInsertFlag.e_none);
                                String output_file_path = Path.Combine(OutputPath, "newsletter_split_page_" + i + ".pdf");
                                await new_doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_remove_unused);
                                WriteLine("Done. Results saved in " + output_file_path);
                                await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                            }
                        }
                    }
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

			    // Sample 2 - Merge several PDF documents into one
			    try
			    {
                    WriteLine("_______________________________________________");
                    WriteLine("Sample 2 - Merge several PDF documents into one...");

                    using (PDFDoc new_doc = new PDFDoc())
                    {
                        new_doc.InitSecurityHandler();
                        int page_num = 15;
                        for (int i = 1; i <= page_num; ++i)
                        {
                            String fpath = Path.Combine(OutputPath, "newsletter_split_page_" + i + ".pdf");
                            WriteLine("Opening " + fpath);
                            using (PDFDoc in_doc = new PDFDoc(fpath))
                            {
                                new_doc.InsertPages(i, in_doc, 1, in_doc.GetPageCount(), PDFDocInsertFlag.e_none);
                            }
                        }
                        String output_file_path = Path.Combine(OutputPath, "newsletter_merge_pages.pdf");
                        await new_doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_remove_unused);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

			    // Sample 3 - Delete every second page
			    try	
			    {
                    WriteLine("_______________________________________________");
                    WriteLine("Sample 3 - Delete every second page...");
                    
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);

                    using (PDFDoc in_doc = new PDFDoc(input_file_path))
                    {
                        in_doc.InitSecurityHandler();

                        int page_num = in_doc.GetPageCount();
                        PageIterator itr;
                        while (page_num >= 1)
                        {
                            itr = in_doc.GetPageIterator(page_num);
                            in_doc.PageRemove(itr);
                            page_num -= 2;
                        }
                        
                        String output_file_path = Path.Combine(OutputPath, "newsletter_page_remove.pdf");
                        await in_doc.SaveAsync(output_file_path, 0);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch(Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

			    // Sample 4 - Inserts a page from one document at different
			    // locations within another document                       
			    try   
			    {	 
				    WriteLine("_______________________________________________");
				    WriteLine("Sample 4 - Insert a page at different locations...");

                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);

				    PDFDoc in1_doc = new PDFDoc(input_file_path);
				    in1_doc.InitSecurityHandler();

                    string input_file_path_2 = Path.Combine(InputPath, "fish.pdf");
                    WriteLine("Opening input file " + input_file_path_2);

				    PDFDoc in2_doc = new PDFDoc(input_file_path_2);
				    in2_doc.InitSecurityHandler();

                    pdftron.PDF.Page src_page = in2_doc.GetPage(1);
				    int page_num = in1_doc.GetPageCount();
				    for (int i=1; i<page_num; i+=3) 
				    {
					    PageIterator it = in1_doc.GetPageIterator(i);
					    in1_doc.PageInsert(it, src_page);
				    }
;
                    String output_file_path = Path.Combine(OutputPath, "newsletter_page_insert.pdf");
                    await in1_doc.SaveAsync(output_file_path, 0);
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    in1_doc.Destroy();
                    in2_doc.Destroy();
			    }
			    catch(Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

			    // Sample 5 - Replicate pages within a single document
			    try	
			    {
				    WriteLine("_______________________________________________");
                    WriteLine("Sample 5 - Replicate pages within a single document...");
                    
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);

                    using (PDFDoc doc = new PDFDoc(input_file_path))
                    {
                        doc.InitSecurityHandler();

                        // Replicate the cover page three times (copy page #1 and place it before the 
                        // seventh page in the document page sequence)
                        pdftron.PDF.Page cover = doc.GetPage(1);
                        doc.PageInsert(doc.GetPageIterator(7), cover);
                        doc.PageInsert(doc.GetPageIterator(7), cover);
                        doc.PageInsert(doc.GetPageIterator(7), cover);

                        // Replicate the cover page two more times by placing it before and after
                        // existing pages.
                        doc.PagePushFront(cover);
                        doc.PagePushBack(cover);

                        String output_file_path = Path.Combine(OutputPath, "newsletter_page_clone.pdf");
                        await doc.SaveAsync(output_file_path, 0);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch(Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

			    // Sample 6 - Use ImportPages() in order to copy multiple pages at once    
			    // in order to preserve shared resources between pages (e.g. images, fonts,
			    // colorspaces, etc.)
			    try	
			    {    
				    WriteLine("_______________________________________________");
				    WriteLine("Sample 6 - Preserving shared resources using ImportPages...");
                
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);

                    using (PDFDoc in_doc = new PDFDoc(input_file_path))
                    {
                        in_doc.InitSecurityHandler();
                        using (PDFDoc new_doc = new PDFDoc())
                        {
                            IList<pdftron.PDF.Page> copy_pages = new List<pdftron.PDF.Page>();
                            for (PageIterator itr = in_doc.GetPageIterator(); itr.HasNext(); itr.Next())
                            {
                                copy_pages.Add(itr.Current());
                            }

                            IList<pdftron.PDF.Page> imported_pages = new_doc.ImportPages(copy_pages);
						    for (int i=0; i!=imported_pages.Count; ++i) {
                                new_doc.PagePushFront((pdftron.PDF.Page)imported_pages[i]); // Order pages in reverse order. 
							    // Use PagePushBack() if you would like to preserve the same order.
						    }
                            
                            String output_file_path = Path.Combine(OutputPath, "newsletter_import_pages.pdf");
                            await new_doc.SaveAsync(output_file_path, 0);
				            WriteLine("Done. Result saved in " + output_file_path);
                            WriteLine("\nNote that the output file size is less than half the size");
                            WriteLine("of the file produced using individual page copy operations");
                            WriteLine("between two documents");
                            await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                        }
                    }
			    }
			    catch(Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done PDFPage Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
