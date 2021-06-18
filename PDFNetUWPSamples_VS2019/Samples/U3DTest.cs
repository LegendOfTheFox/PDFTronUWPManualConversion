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
    public sealed class U3DTest : Sample
    {
        public U3DTest() :
            base("U3D", "This example illustrates how to embed U3D content (3 dimensional models) in PDF.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting U3D Test...");
                WriteLine("--------------------------------\n");
			    try  
			    {	 
				    PDFDoc doc = new PDFDoc();
				    pdftron.PDF.Page page = doc.PageCreate();
				    doc.PagePushBack(page);
				    Obj annots = doc.CreateIndirectArray();
				    page.GetSDFObj().Put("Annots", annots);

				    Create3DAnnotation(doc, annots);
                    String output_file_path = Path.Combine(OutputPath, "dice_u3d.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done U3D Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}

		void Create3DAnnotation(PDFDoc doc, Obj annots)
		{
			// ---------------------------------------------------------------------------------
			// Create a 3D annotation based on U3D content. PDF 1.6 introduces the capability 
			// for collections of three-dimensional objects, such as those used by CAD software, 
			// to be embedded in PDF files.
			Obj link_3D = doc.CreateIndirectDict();
			link_3D.PutName("Subtype", "3D");

			// Annotation location on the page
            pdftron.PDF.Rect bbox = new pdftron.PDF.Rect(25, 180, 585, 643);
			link_3D.PutRect("Rect", bbox.x1, bbox.y1, bbox.x2, bbox.y2);
			annots.PushBack(link_3D);

			// The 3DA entry is an activation dictionary (see Table 9.34 in the PDF Reference Manual) 
			// that determines how the state of the annotation and its associated artwork can change.
			Obj activation_dict_3D = link_3D.PutDict("3DA");

			// Set the annotation so that it is activated as soon as the page containing the 
			// annotation is opened. Other options are: PV (page view) and XA (explicit) activation.
			activation_dict_3D.PutName("A", "PO");

			// Embed U3D Streams (3D Model/Artwork).
            MappedFile u3d_file = new MappedFile(Path.Combine(InputPath, "dice.u3d"));
			FilterReader u3d_reader = new FilterReader(u3d_file);

			Obj u3d_data_dict = doc.CreateIndirectStream(u3d_reader);
            u3d_data_dict.PutName("Subtype", "U3D");
            link_3D.Put("3DD", u3d_data_dict);

			// Set the initial view of the 3D artwork that should be used when the annotation is activated.
			Obj view3D_dict = link_3D.PutDict("3DV");			
			view3D_dict.PutString("IN", "Unnamed");
			view3D_dict.PutString("XN", "Default");
			view3D_dict.PutName("MS", "M");
			view3D_dict.PutNumber("CO", 27.5);

			// A 12-element 3D transformation matrix that specifies a position and orientation 
			// of the camera in world coordinates.
			Obj tr3d = view3D_dict.PutArray("C2W");
			tr3d.PushBackNumber(1); tr3d.PushBackNumber(0); tr3d.PushBackNumber(0); 
			tr3d.PushBackNumber(0); tr3d.PushBackNumber(0); tr3d.PushBackNumber(-1);
			tr3d.PushBackNumber(0); tr3d.PushBackNumber(1); tr3d.PushBackNumber(0); 
			tr3d.PushBackNumber(0); tr3d.PushBackNumber(-27.5); tr3d.PushBackNumber(0);			

			// Create annotation appearance stream, a thumbnail which is used during printing or
			// in PDF processors that do not understand 3D data.
			Obj ap_dict = link_3D.PutDict("AP");
			ElementBuilder builder = new ElementBuilder();
			ElementWriter writer = new ElementWriter();
			writer.Begin(doc.GetSDFDoc());

			writer.WritePlacedElement(builder.CreateImage(
                pdftron.PDF.Image.Create(doc.GetSDFDoc(), Path.Combine(InputPath, "dice.jpg")), 
					   0, 0, bbox.Width(), bbox.Height()));

            Obj normal_ap_stream = writer.End();
			normal_ap_stream.PutName("Subtype", "Form");
			normal_ap_stream.PutRect("BBox", 0, 0, bbox.Width(), bbox.Height());
			ap_dict.Put("N", normal_ap_stream);
            
		}
	}
}
