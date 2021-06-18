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
    public class PDFDocLockTest : Sample
    {
        public PDFDocLockTest() :
            base("PDFDocLock", "This sample demonstrates concurrent reading and writing with multiple threads using PDFNet's document locking API.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting PDFDocLock Test...");
                WriteLine("--------------------------------\n");

                String input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                try
                {
                    PDFDoc doc = new PDFDoc(input_file_path);
                    Task readTask1 = new Task(async () =>
                    {
                        try
                        {
                            String threadId = Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "readTask1";
                            doc.LockRead();
                            WriteLine("Thread " + threadId + " exporting page 1 to PNG.");
                            PDFDraw draw = new PDFDraw();
                            Page page1 = doc.GetPage(1);
                            String output_file_path = Path.Combine(OutputPath, "newsletter_lock_page1.png");
                            draw.Export(page1, output_file_path, "PNG");
                            
                            doc.UnlockRead();

                            WriteLine("Thread " + threadId + " saved image to " + output_file_path);
                            await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            WriteLine(GetExceptionMessage(e));
                        }
                    });

                    Task readTask2 = new Task(async () =>
                    {
                        try
                        {
                            String threadId = Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "readTask2";
                            doc.LockRead();
                            WriteLine("Thread " + threadId + " exporting page 2 to BMP.");
                            PDFDraw draw = new PDFDraw();
                            Page page2 = doc.GetPage(2);
                            String output_file_path = Path.Combine(OutputPath, "newsletter_lock_page2.bmp");
                            draw.Export(page2, output_file_path, "BMP");

                            doc.UnlockRead();

                            WriteLine("Thread " + threadId + " saved image to " + output_file_path);
                            await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            WriteLine(GetExceptionMessage(e));
                        }
                    });

                    Task writeTask = new Task(async () => {
                        try
                        {
                            String threadId = Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString() : "writeTask";
                            doc.Lock();
                            WriteLine("Thread " + threadId + " modifying PDF document.");
                            var root = doc.GetRoot();
                            root.PutString("Modified", DateTime.Now.ToString());
                            String output_file_path = Path.Combine(OutputPath, "newsletter_lockmodified.pdf");
                            await doc.SaveAsync(output_file_path, 0);

                            doc.Unlock();

                            WriteLine("Thread " + threadId + " saved document to " + output_file_path);
                            await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            WriteLine(GetExceptionMessage(e));
                        }
                    });

                    WriteLine("Starting tasks.");
                    readTask1.Start();
                    readTask2.Start();
                    await Task.Delay(100);
                    writeTask.Start();
                    writeTask.Wait();
                    readTask2.Wait();
                    readTask1.Wait();
                    WriteLine("All tasks finished.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done PDFDocLock Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }
    }
}
