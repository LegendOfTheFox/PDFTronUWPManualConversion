//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.Filters;
using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class PDFPackageTest : Sample
    {
        public PDFPackageTest() :
            base("PDFPackage", "This sample illustrates how to create, extract, and manipulate PDF Portfolios (a.k.a. PDF Packages) using PDFNet SDK.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting PDFPackage Test...");
                WriteLine("--------------------------------\n");
			    try
			    {

				    PDFDoc doc = new PDFDoc();
				    AddPackage(doc, Path.Combine(InputPath, "numbered.pdf"), "My File 1");
				    AddPackage(doc, Path.Combine(InputPath, "newsletter.pdf"), "My Newsletter...");
				    AddPackage(doc, Path.Combine(InputPath, "peppers.jpg"), "An image");
				    AddCovePage(doc);
                    string output_file_path = Path.Combine(OutputPath, "package.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                    WriteLine(string.Format("PDFPackage created at: {0}{1}", output_file_path, Environment.NewLine));
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

				    doc.Destroy();
			    }
			    catch (Exception e)
			    {
				    WriteLine(e.Message);
			    }

			    // Extract parts from a PDF Package.
			    {
                    PDFDoc doc = new PDFDoc(Path.Combine(OutputPath, "package.pdf"));
				    doc.InitSecurityHandler();

				    pdftron.SDF.NameTree files = NameTree.Find(doc.GetSDFDoc(), "EmbeddedFiles");
				    if(files.IsValid()) 
				    {

                        WriteLine(string.Format("Extracting files from {0}:", Path.Combine(OutputPath, "package.pdf")));

					    // Traverse the list of embedded files.
					    NameTreeIterator i = files.GetIterator();
					    for (int counter = 0; i.HasNext(); i.Next(), ++counter) 
					    {
						    string entry_name = i.Key().GetAsPDFText();
                            WriteLine(string.Format("Part: {0}", entry_name));
						    FileSpec file_spec = new FileSpec(i.Value());
						    IFilter stm = file_spec.GetFileData();

						    if (stm!=null) 
						    {
                                string fname = Path.Combine(OutputPath, "extract_" + counter.ToString() + entry_name.Substring(entry_name.Length - 4));
                                stm.WriteToFile(fname, false);
                                WriteLine(string.Format("File {0} extracted from package", fname));
                                await AddFileToOutputList(fname).ConfigureAwait(false);
						    }
					    }
				    }
                }

            WriteLine("\n--------------------------------");
            WriteLine("Done Annotation Test.");
            WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}

		void AddPackage(PDFDoc doc, string file, string desc) 
		{
			NameTree files = NameTree.Create(doc.GetSDFDoc(), "EmbeddedFiles");
            FileSpec fs = FileSpec.Create(doc.GetSDFDoc(), file, true);
			byte[] file1_name = System.Text.Encoding.UTF8.GetBytes(file);
			files.Put(file1_name, fs.GetSDFObj());
			fs.GetSDFObj().PutText("Desc", desc);

			Obj collection = doc.GetRoot().FindObj("Collection");
			if (collection == null) collection = doc.GetRoot().PutDict("Collection");

			// You could here manipulate any entry in the Collection dictionary. 
			// For example, the following line sets the tile mode for initial view mode
			// Please refer to section '2.3.5 Collections' in PDF Reference for details.
			collection.PutName("View", "T");
		}

        void AddCovePage(PDFDoc doc) 
		{
			// Here we dynamically generate cover page (please see ElementBuilder 
			// sample for more extensive coverage of PDF creation API).
            pdftron.PDF.Page page = doc.PageCreate(new pdftron.PDF.Rect(0, 0, 200, 200));

			ElementBuilder b = new ElementBuilder();
			ElementWriter w = new ElementWriter();
			w.Begin(page);
			//Font font = Font.CreateTrueTypeFont(doc.GetSDFDoc(), new System.Drawing.Font("Comic Sans MS", 12), true, false);
            Font font = Font.Create(doc, pdftron.PDF.FontStandardType1Font.e_times_roman);
			w.WriteElement(b.CreateTextBegin(font, 12));
			Element e = b.CreateTextRun("My PDF Collection");
			e.SetTextMatrix(1, 0, 0, 1, 50, 96);
			e.GetGState().SetFillColorSpace(ColorSpace.CreateDeviceRGB());
			e.GetGState().SetFillColor(new ColorPt(1, 0, 0));
			w.WriteElement(e);
			w.WriteElement(b.CreateTextEnd());
			w.End();
			doc.PagePushBack(page);
			
			// Alternatively we could import a PDF page from a template PDF document
			// (for an example please see PDFPage sample project).
			// ...
		}
	}
}
