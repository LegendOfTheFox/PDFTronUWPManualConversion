//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.PDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class RectTest : Sample
    {
        public RectTest() :
            base("Rect", "Shows how to change Page's MediaBox using Rect class.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Rect Test...");
                WriteLine("--------------------------------\n");
                
                string input_file_path = Path.Combine(InputPath, "tiger.pdf");
                WriteLine("Opening input file " + input_file_path);

			    try // Test  - Adjust the position of content within the page.
			    {
				    PDFDoc input_doc = new PDFDoc(input_file_path);
				    input_doc.InitSecurityHandler();

				    pdftron.PDF.Page pg = input_doc.GetPage(1);
				    pdftron.PDF.Rect media_box = pg.GetMediaBox();

				    media_box.x1 -= 200;	// translate the page 200 units (1 UInt = 1/72 inch)
				    media_box.x2 -= 200;

				    media_box.Update();

                    String output_file_path = Path.Combine(OutputPath, "tiger_shift.pdf");
                    await input_doc.SaveAsync(output_file_path, 0);
                    input_doc.Destroy();

                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Rect Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
