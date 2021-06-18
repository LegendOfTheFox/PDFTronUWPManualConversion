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
    public sealed class PDFRedactTest : Sample
    {
        public PDFRedactTest() :
            base("PDFRedact", "PDF Redactor is a separately licensable Add-on that offers options to remove (not just covering or obscuring) content within a region of PDF. ")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting PDFRedact Test...");
                WriteLine("--------------------------------\n");

                try
                {
                    IList<RedactorRedaction> rarr = new List<RedactorRedaction>();
                    rarr.Add(new RedactorRedaction(1, new pdftron.PDF.Rect(0, 0, 600, 600), true, "Top Secret"));
                    rarr.Add(new RedactorRedaction(2, new pdftron.PDF.Rect(30, 30, 550, 550), true, "Top Secret"));
                    rarr.Add(new RedactorRedaction(2, new pdftron.PDF.Rect(100, 100, 200, 200), false, "bar"));
                    rarr.Add(new RedactorRedaction(2, new pdftron.PDF.Rect(300, 300, 400, 400), false, ""));
                    rarr.Add(new RedactorRedaction(2, new pdftron.PDF.Rect(500, 500, 600, 600), false, ""));
                    rarr.Add(new RedactorRedaction(3, new pdftron.PDF.Rect(0, 0, 700, 20), false, ""));
                    string output_file_path = Path.Combine(OutputPath, "redacted.pdf");
                    await RedactAsync(Path.Combine(InputPath, "newsletter.pdf"), output_file_path, rarr);
                }
                catch (Exception e)
                {
                    WriteLine(e.Message);
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done PDFRedact Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        async Task RedactAsync(string input, string output, IList<RedactorRedaction> rarr)
        {
            using (PDFDoc doc = new PDFDoc(input))
            {
                doc.InitSecurityHandler();

                RedactorAppearance app = new RedactorAppearance();
                //app.Font = new System.Drawing.Font("Arial", 12);
                app.PositiveOverlayColor = Windows.UI.Colors.Red;
                app.NegativeOverlayColor = Windows.UI.Colors.WhiteSmoke;

                string output_file_path = Path.Combine(OutputPath, "redacted.pdf");
                Redactor.Redact(doc, rarr, app);
                await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                WriteLine("Result of redaction saved in " + output_file_path);
                await AddFileToOutputList(output_file_path).ConfigureAwait(false);
            }
        }
	}
}
