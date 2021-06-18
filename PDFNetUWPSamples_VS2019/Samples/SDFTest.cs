//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.Filters;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class SDFTest : Sample
    {
        public SDFTest() :
            base("SDF", "The sample illustrates how to use basic Cos/SDF API to edit an existing document.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting SDF Test...");
                WriteLine("--------------------------------\n");
            
                string input_file_path = Path.Combine(InputPath, "fish.pdf");
                WriteLine("Opening input file " + input_file_path);

			    try
			    {
				    // Here we create a SDF/Cos document directly from PDF file. In case you have 
				    // PDFDoc you can always access SDF/Cos document using PDFDoc.GetSDFDoc() method.
				    SDFDoc doc = new SDFDoc(input_file_path);
				    doc.InitSecurityHandler();				

				    WriteLine("-------------------------------------------------");
                    WriteLine("Modifying info dictionary, adding custom properties, embedding a stream...");

				    Obj trailer = doc.GetTrailer();	// Get the trailer

				    // Now we will change PDF document information properties using SDF API

				    // Get the Info dictionary. 
				    DictIterator itr = trailer.Find("Info");	
				    Obj info;
				    if (itr.HasNext()) 
				    {
					    info = itr.Value();
					    // Modify 'Producer' entry.
					    info.PutString("Producer", "PDFTron PDFNet");

					    // Read title entry (if it is present)
					    itr = info.Find("Author"); 
					    if (itr.HasNext()) 
					    {
						    info.PutString("Author", itr.Value().GetAsPDFText() + "- Modified");
					    }
					    else 
					    {
						    info.PutString("Author", "Joe Doe");
					    }
				    }
				    else 
				    {
					    // Info dict is missing. 
					    info = trailer.PutDict("Info");
					    info.PutString("Producer", "PDFTron PDFNet");
					    info.PutString("Title", "My document");
				    }

				    // Create a custom inline dictionary within Info dictionary
				    Obj custom_dict = info.PutDict("My Direct Dict");

				    // Add some key/value pairs
				    custom_dict.PutNumber("My Number", 100);

				    Obj my_array = custom_dict.PutArray("My Array");

				    // Create a custom indirect array within Info dictionary
				    Obj custom_array = doc.CreateIndirectArray();	
				    info.Put("My Indirect Array", custom_array);
				
				    // Create indirect link to root
				    custom_array.PushBack(trailer.Get("Root").Value());

				    // Embed a custom stream (file my_stream.txt).
                    MappedFile embed_file = new MappedFile(Path.Combine(InputPath, "my_stream.txt"));
				    FilterReader mystm = new FilterReader(embed_file);
                    custom_array.PushBack(doc.CreateIndirectStream(mystm));

				    // Save the changes.
                    String output_file_path = Path.Combine(OutputPath, "sdftest_out.pdf");
                    await doc.SaveAsync(output_file_path, 0, "%PDF-1.4");
				    doc.Destroy();
				    WriteLine("Test completed.");
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
			    }
			    catch (Exception e) {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done SDF Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
