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
    public sealed class PatternTest : Sample
    {
        public PatternTest() :
            base("Pattern", "This example illustrates how to create various PDF patterns and shadings.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Pattern Test...");
                WriteLine("--------------------------------\n");
			    try	
			    {
				    PDFDoc doc = new PDFDoc();
				    ElementWriter writer = new ElementWriter();	
				    ElementBuilder eb = new ElementBuilder();

				    // The following sample illustrates how to create and use tiling patterns
				    pdftron.PDF.Page page = doc.PageCreate();
				    writer.Begin(page);

				    Element element = eb.CreateTextBegin(Font.Create(doc.GetSDFDoc(), FontStandardType1Font.e_times_bold), 1);
				    writer.WriteElement(element);  // Begin the text block

				    element = eb.CreateTextRun("G");
				    element.SetTextMatrix(720, 0, 0, 720, 20, 240);
				    GState gs = element.GetGState();
				    gs.SetTextRenderMode(GStateTextRenderingMode.e_fill_stroke_text);
				    gs.SetLineWidth(4);

				    // Set the fill color space to the Pattern color space. 
				    gs.SetFillColorSpace(ColorSpace.CreatePattern());
                    gs.SetFillColor(CreateTilingPattern(doc));

				    writer.WriteElement(element);
				    writer.WriteElement(eb.CreateTextEnd()); // Finish the text block

                    writer.End();	// Save the page
				    doc.PagePushBack(page);
				    //-----------------------------------------------

				    /// The following sample illustrates how to create and use image tiling pattern
				    page = doc.PageCreate();
				    writer.Begin(page);
				
				    eb.Reset();
				    element = eb.CreateRect(0, 0, 612, 794);

				    // Set the fill color space to the Pattern color space. 
				    gs = element.GetGState();
				    gs.SetFillColorSpace(ColorSpace.CreatePattern());
                    gs.SetFillColor(CreateImageTilingPattern(doc));
				    element.SetPathFill(true);		

				    writer.WriteElement(element);

                    writer.End();	// Save the page
				    doc.PagePushBack(page);
				    //-----------------------------------------------

				    /// The following sample illustrates how to create and use PDF shadings
				    page = doc.PageCreate();
				    writer.Begin(page);

				    eb.Reset();
				    element = eb.CreateRect(0, 0, 612, 794);

				    // Set the fill color space to the Pattern color space. 
				    gs = element.GetGState();
				    gs.SetFillColorSpace(ColorSpace.CreatePattern());
				    gs.SetFillColor(CreateAxialShading(doc));
				    element.SetPathFill(true);		

				    writer.WriteElement(element);

                    writer.End();	// save the page
				    doc.PagePushBack(page);
				    //-----------------------------------------------

                    String output_file_path = Path.Combine(OutputPath, "patterns.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_remove_unused);
				    doc.Destroy();
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Pattern Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}

        PatternColor CreateTilingPattern(PDFDoc doc) 
		{
			ElementWriter writer = new ElementWriter();	
			ElementBuilder eb = new ElementBuilder();

			// Create a new pattern content stream - a heart. ------------
			writer.Begin(doc.GetSDFDoc());
			eb.PathBegin();
			eb.MoveTo(0, 0);
			eb.CurveTo(500, 500, 125, 625, 0, 500);
			eb.CurveTo(-125, 625, -500, 500, 0, 0);
			Element heart = eb.PathEnd();
			heart.SetPathFill(true); 
		
			// Set heart color to red.
			heart.GetGState().SetFillColorSpace(ColorSpace.CreateDeviceRGB()); 
			heart.GetGState().SetFillColor(new ColorPt(1, 0, 0)); 
			writer.WriteElement(heart);

            Obj pattern_dict = writer.End();

			// Initialize pattern dictionary. For details on what each parameter represents please 
			// refer to Table 4.22 (Section '4.6.2 Tiling Patterns') in PDF Reference Manual.
			pattern_dict.PutName("Type", "Pattern");
			pattern_dict.PutNumber("PatternType", 1);

			// TilingType - Constant spacing.
			pattern_dict.PutNumber("TilingType", 1); 

			// This is a Type1 pattern - A colored tiling pattern.
			pattern_dict.PutNumber("PaintType", 1);

			// Set bounding box
			pattern_dict.PutRect("BBox", -253, 0, 253, 545);

			// Set the pattern matrix
			pattern_dict.PutMatrix("Matrix", new Matrix2D(0.04, 0, 0, 0.04, 0, 0));

			// Set the desired horizontal and vertical spacing between pattern cells, 
			// measured in the pattern coordinate system.
			pattern_dict.PutNumber("XStep", 1000);
			pattern_dict.PutNumber("YStep", 1000);
		
			return new PatternColor(pattern_dict); // finished creating the Pattern resource
		}

        PatternColor CreateImageTilingPattern(PDFDoc doc) 
		{
			ElementWriter writer = new ElementWriter();
			ElementBuilder eb = new ElementBuilder();

			// Create a new pattern content stream - a single bitmap object ----------
            writer.Begin(doc.GetSDFDoc());
            pdftron.PDF.Image img = pdftron.PDF.Image.Create(doc.GetSDFDoc(), Path.Combine(InputPath, "butterfly.png"));
			Element img_element = eb.CreateImage(img, 0, 0, img.GetImageWidth(), img.GetImageHeight());
			writer.WritePlacedElement(img_element);
            Obj pattern_dict = writer.End();

			// Initialize pattern dictionary. For details on what each parameter represents please 
			// refer to Table 4.22 (Section '4.6.2 Tiling Patterns') in PDF Reference Manual.
			pattern_dict.PutName("Type", "Pattern");
			pattern_dict.PutNumber("PatternType", 1);

			// TilingType - Constant spacing.
			pattern_dict.PutNumber("TilingType", 1); 

			// This is a Type1 pattern - A colored tiling pattern.
			pattern_dict.PutNumber("PaintType", 1);

			// Set bounding box
			pattern_dict.PutRect("BBox", -253, 0, 253, 545);

			// Set the pattern matrix
			pattern_dict.PutMatrix("Matrix", new Matrix2D(0.3, 0, 0, 0.3, 0, 0));

			// Set the desired horizontal and vertical spacing between pattern cells, 
			// measured in the pattern coordinate system.
			pattern_dict.PutNumber("XStep", 300);
			pattern_dict.PutNumber("YStep", 300);
				
			return new PatternColor(pattern_dict); // finished creating the Pattern resource
		}

        PatternColor CreateAxialShading(PDFDoc doc) 
		{
			// Create a new Shading object ------------
			Obj pattern_dict = doc.CreateIndirectDict();

			// Initialize pattern dictionary. For details on what each parameter represents 
			// please refer to Tables 4.30 and 4.26 in PDF Reference Manual
			pattern_dict.PutName("Type", "Pattern");
			pattern_dict.PutNumber("PatternType", 2); // 2 stands for shading
					
			Obj shadingDict = pattern_dict.PutDict("Shading");
			shadingDict.PutNumber("ShadingType", 2);
			shadingDict.PutName("ColorSpace", "DeviceCMYK");
					
			// Set the coordinates of the axial shading to the output
			shadingDict.PutRect("Coords", 0, 0, 612, 794);

			// Set the Functions for the axial shading
			Obj funct = shadingDict.PutDict("Function");
			Obj C0 = funct.PutArray("C0");
			C0.PushBackNumber(1);
			C0.PushBackNumber(0);
			C0.PushBackNumber(0);
			C0.PushBackNumber(0);

			Obj C1 = funct.PutArray("C1");
			C1.PushBackNumber(0);
			C1.PushBackNumber(1);
			C1.PushBackNumber(0);
			C1.PushBackNumber(0);
					
			Obj domain = funct.PutArray("Domain");
			domain.PushBackNumber(0);
			domain.PushBackNumber(1);
			
			funct.PutNumber("FunctionType", 2);
			funct.PutNumber("N", 1);

			return new PatternColor(pattern_dict);
		}
	}
}
