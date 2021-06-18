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
    public sealed class ElementEditTest : Sample
    {
        public ElementEditTest() :
            base("ElementEdit", "The sample code shows how to edit the page display list and how to modify graphics state attributes on existing Elements. In particular the sample program strips all images from the page and changes text color to blue.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting ElementEdit Test...");
                WriteLine("--------------------------------\n");

			    try
                {
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);
                    PDFDoc doc = new PDFDoc(input_file_path);
				    doc.InitSecurityHandler();

				    int num_pages = doc.GetPageCount();

                    PageIterator itr = doc.GetPageIterator();

				    ElementWriter writer = new ElementWriter();
				    ElementReader reader = new ElementReader();

                    while (itr.HasNext())
                    {
                        Page page = itr.Current();
				        reader.Begin(page);
					    writer.Begin(page, ElementWriterWriteMode.e_replacement, false);
					    ProcessElements(reader, writer);
					    writer.End();
					    reader.End();

                        itr.Next();
				    }
                    
                    String output_file_path = Path.Combine(OutputPath, "newsletter_edited.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_remove_unused);
				    doc.Destroy();
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

                WriteLine("\n--------------------------------");
                WriteLine("Done ElementEdit Test.");
                WriteLine("--------------------------------\n");
		    })).AsAsyncAction();
        }

		void ProcessElements(ElementReader reader, ElementWriter writer)
		{
			Element element;
			while ((element = reader.Next()) != null) 	// Read page contents
			{
				switch (element.GetType())
				{
					case ElementType.e_image:
					case ElementType.e_inline_image:
						// remove all images by skipping them
						continue;
                    case ElementType.e_path:				// Process path data...
                        {
                            // Set all paths to red color.
                            GState gs = element.GetGState();
                            gs.SetFillColorSpace(ColorSpace.CreateDeviceRGB());
                            gs.SetFillColor(new ColorPt(1, 0, 0));
                            writer.WriteElement(element);
                            break;
                        }
					case ElementType.e_text: 				// Process text strings...
						{
							// Set all text to blue color.
							GState gs = element.GetGState();
							gs.SetFillColorSpace(ColorSpace.CreateDeviceRGB());
							gs.SetFillColor(new ColorPt(0, 0, 1));
							writer.WriteElement(element);
							break;
						}
					case ElementType.e_form:				// Recursively process form XObjects
						{
							writer.WriteElement(element);

							reader.FormBegin();
							ElementWriter new_writer = new ElementWriter();
							new_writer.Begin(element.GetXObject(), true);
							ProcessElements(reader, new_writer);
							new_writer.End();
							reader.End();

							break;
						}
					default:
						{
							writer.WriteElement(element);
							break;
						}		
				}
			}
		}
	}
}
