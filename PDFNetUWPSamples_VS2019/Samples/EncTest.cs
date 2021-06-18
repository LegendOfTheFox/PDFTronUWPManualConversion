//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Foundation;

using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class EncTestCS : Sample
    {
        public EncTestCS() :
            base("Encryption Test", "Illustrates how to add password protection to a document and how to open it again.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () =>
            {
                WriteLine("--------------------------------");
                WriteLine("Starting Encryption Test...");
                WriteLine("--------------------------------\n");

                // Example 1: Securing a document with password protection and adjusting permissions 
                // on the document.
                try
                {
                    // Open the test file
                    WriteLine("-------------------------------------------------");
                    WriteLine("Securing an existing document...");

                    String input_file_path = Path.Combine(InputPath, "fish.pdf");
                    PDFDoc doc = new PDFDoc(input_file_path);

                    if (!doc.InitSecurityHandler())
                    {
                        WriteLine("Document authentication error...");
                        return;
                    }


                    SecurityHandler new_handler = new SecurityHandler();

                    string my_password = "test";
                    new_handler.ChangeUserPassword(my_password);

                    new_handler.SetPermission(SecurityHandlerPermission.e_print, true);
                    new_handler.SetPermission(SecurityHandlerPermission.e_extract_content, false);

                    doc.SetSecurityHandler(new_handler);

                    WriteLine(string.Format("\nTo open document use password: {0}\n", my_password));

                    String output_file_path = Path.Combine(OutputPath, "secured.pdf");
                    await doc.SaveAsync(output_file_path, 0);

                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }


                // Example 2: Reading password protected document without user feedback.
                try
                {
                    WriteLine("-------------------------------------------------");
                    WriteLine("Open the password protected document from the first example...");

                    String input_file_path = Path.Combine(OutputPath, "secured.pdf");
                    PDFDoc doc = new PDFDoc(input_file_path);   // Open the encrypted document that we saved in the first example. 

                    if (!doc.InitStdSecurityHandler("test"))
                    {
                        WriteLine("Document authentication error...");
                        WriteLine("The password is not valid.");
                        return;
                    }
                    else
                    {
                        WriteLine("The password is correct! Document can now be used for reading and editing");

                        // Remove the password security and save the changes to a new file.
                        doc.SetSecurityHandler(null);

                        String output_file_path = Path.Combine(OutputPath, "secured_nomore1.pdf");
                        await doc.SaveAsync(output_file_path, 0);

                        WriteLine("Done. Result saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                        
                        WriteLine("Done. Result saved in secured_nomore1.pdf");

                        
                    }
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("To see how to open a document with user feedback, see the PDFViewCtrlDemo and the Open and HandlePassword functions.");
                WriteLine("--------------------------------\n");

                WriteLine("\n--------------------------------");
                WriteLine("Done Encryption Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }
    }
}
