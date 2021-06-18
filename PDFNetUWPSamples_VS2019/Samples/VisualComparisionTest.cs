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
    public sealed class VisualComparisionTest : Sample
    {
        public VisualComparisionTest() :
            base("VisualComparision", "PDFNet can generate a visual comparison between two different documents. The comparison result itself is a resolution independent PDF -- well suited for adding high-quality diff support into existing app workflows.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Visual Comparision Test...");
                WriteLine("--------------------------------\n");

                bool error = await DiffingTestAsync();

                if (error)
                {
                    WriteLine("Visual Comparision failed");
                }
                else
                {
                    WriteLine("Visual Comparision succeeded");
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Visual Comparision Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        private async Task<bool> DiffingTestAsync()
        {
            bool error = false;

            string firstDocumentPath = Path.Combine(InputPath, "diff_doc_1.pdf");
            string secondDocumentPath = Path.Combine(InputPath, "diff_doc_2.pdf");
            string outputDocumentPath = Path.Combine(OutputPath, "VisualComparision.pdf");

            if ((!File.Exists(firstDocumentPath)) || (!File.Exists(secondDocumentPath)))
            {
                WriteLine("Atleast one input file does not exist. Aborting.");
                error = true;
                return error;
            }

            try
            {
				using (PDFDoc firstDocument = new PDFDoc(firstDocumentPath))
				using (PDFDoc secondDocument = new PDFDoc(secondDocumentPath))
				{
					Page firstDocumentPage = firstDocument.GetPage(1);
					Page secondDocumentPage = secondDocument.GetPage(1);

					DiffOptions diffOptions = new DiffOptions();
					diffOptions.SetColorA(new ColorPt(1, 0, 0));
					diffOptions.SetColorB(new ColorPt(0, 0, 0));
					diffOptions.SetBlendMode(GStateBlendMode.e_bl_normal);

					PDFDoc outputDocument = new PDFDoc();
					outputDocument.AppendVisualDiff(firstDocumentPage, secondDocumentPage, diffOptions);
					await outputDocument.SaveAsync(outputDocumentPath, SDFDocSaveOptions.e_linearized);
                    await AddFileToOutputList(outputDocumentPath).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                WriteLine(GetExceptionMessage(e));
                error = true;
            }

            return error;
        }
    }
}
