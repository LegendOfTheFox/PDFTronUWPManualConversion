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
    public sealed class PDFDocMemoryTest : Sample
    {
        public PDFDocMemoryTest() :
            base("PDFDocMemory", "The sample illustrates how to read/write a PDF document from/to memory buffer. This is useful for applications that work with dynamic PDFdocuments that don't need to be saved/read from a disk.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting PDFDocMemory Test...");
                WriteLine("--------------------------------\n");
			    try  
			    {
				    // Read a PDF document from a IRandomAccessStream or pass-in a memory buffer...
                    PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "tiger.pdf"));
				    doc.InitSecurityHandler();

				    int num_pages = doc.GetPageCount();

				    ElementWriter writer = new ElementWriter();
				    ElementReader reader = new ElementReader();
				    Element element;

				    // Perform some document editing ...
				    // Here we simply copy all elements from one page to another.
				    for(int i = 1; i <= num_pages; ++i)
				    {
                        pdftron.PDF.Page pg = doc.GetPage(2 * i - 1);

					    reader.Begin(pg);
                        pdftron.PDF.Page new_page = doc.PageCreate(pg.GetMediaBox());
					    doc.PageInsert(doc.GetPageIterator(2*i), new_page);

					    writer.Begin(new_page);
					    while ((element = reader.Next()) != null) 	// Read page contents
					    {
						    writer.WriteElement(element);
					    }

                        writer.End();
					    reader.End();
				    }

                    string output_file_path = Path.Combine(OutputPath, "doc_memory_edit.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_remove_unused);
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // Read some data from the file stored in memory
				    reader.Begin(doc.GetPage(1));
				    while ((element = reader.Next()) != null) {
					    if (element.GetType() == ElementType.e_path)
						    WriteLine("Path, ");
				    }
				    reader.End();
				    doc.Destroy();
			    }
			    catch (Exception e) 
                {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done PDFDocMemory Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
