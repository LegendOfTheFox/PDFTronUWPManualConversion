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
using pdftron.Common;

namespace PDFNetSamples
{
    public sealed class UndoRedoTest : Sample
    {
        public UndoRedoTest() :
            base("UndoRedo", "The following sample illustrates how to use the UndoRedo API.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting UndoRedo Test...");
                WriteLine("--------------------------------\n");

                bool error = await UndoRedoTestAsync();

                if (error)
                {
                    WriteLine("UndoRedo failed");
                }
                else
                {
                    WriteLine("UndoRedo succeeded");
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done UndoRedo Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        private async Task<bool> UndoRedoTestAsync()
        {
            bool error = false;

            // Relative path to the folder containing test files.
            string inputPath = Path.Combine(InputPath, "newsletter.pdf");
            string inputImagePath = Path.Combine(InputPath, "peppers.jpg");
            string outputPath = Path.Combine(OutputPath, "addimage.pdf");
            string outputUndoPath = Path.Combine(OutputPath, "addimage_undone.pdf");
            string outputRedoPath = Path.Combine(OutputPath, "addimage_redone.pdf");

            try
            {
                // Open the PDF document.
                PDFDoc document = new PDFDoc(inputPath);
                ElementBuilder elementBuilder = new ElementBuilder(); // Used to build new Element objects	
                ElementWriter elementWriter = new ElementWriter();	// Used to write Elements to the page                	

                UndoManager undoManager = document.GetUndoManager();

                // Take a snapshot to which we can undo after making changes.
                ResultSnapshot snapshot0 = undoManager.TakeSnapshot();
                DocSnapshot snapshot0State = snapshot0.CurrentState();

                Page page = document.PageCreate();   // Start a new page

                elementWriter.Begin(page);		// Begin writing to this page

                // ----------------------------------------------------------
                // Add JPEG image to the file
                Image image = Image.Create(document.GetSDFDoc(), inputImagePath);
                Element element = elementBuilder.CreateImage(image, new Matrix2D(200, 0, 0, 250, 50, 500));
                elementWriter.WritePlacedElement(element);
                elementWriter.End();	// Finish writing to the page
                document.PagePushFront(page);

                // Take a snapshot after making changes, so that we can redo later (after undoing first).
                ResultSnapshot snapshot1 = undoManager.TakeSnapshot();
                if (snapshot1.PreviousState().Equals(snapshot0State))
                {
                    WriteLine("snapshot1 previous state equals snapshot0State; previous state is correct");
                }

                DocSnapshot snapshot1State = snapshot1.CurrentState();

                await document.SaveAsync(outputPath, SDFDocSaveOptions.e_incremental);
                await AddFileToOutputList(outputPath).ConfigureAwait(false);

                if (undoManager.CanUndo())
                {
                    ResultSnapshot undoSnapshot = undoManager.Undo();

                    await document.SaveAsync(outputUndoPath, SDFDocSaveOptions.e_incremental);
                    await AddFileToOutputList(outputUndoPath).ConfigureAwait(false);

                    DocSnapshot undoSnapshotState = undoSnapshot.CurrentState();
                    if (undoSnapshotState.Equals(snapshot0State))
                    {
                        WriteLine("undoSnapshotState equals snapshot0State; undo was successful");
                    }

                    if (undoManager.CanRedo())
                    {
                        ResultSnapshot redoSnapshot = undoManager.Redo();

                        await document.SaveAsync(outputRedoPath, SDFDocSaveOptions.e_incremental);
                        await AddFileToOutputList(outputRedoPath).ConfigureAwait(false);

                        if (redoSnapshot.PreviousState().Equals(undoSnapshotState))
                        {
                            WriteLine("redoSnapshot previous state equals undoSnapshotState; previous state is correct");
                        }

                        DocSnapshot redoSnapshotState = redoSnapshot.CurrentState();
                        if (redoSnapshotState.Equals(snapshot1State))
                        {
                            WriteLine("snapshot1 and redoSnapshot are equal; redo was successful");
                        }
                    }
                    else
                    {
                        WriteLine("Problem encountered - cannot redo.");
                    }
                }
                else
                {
                    WriteLine("Problem encountered - cannot undo.");
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
