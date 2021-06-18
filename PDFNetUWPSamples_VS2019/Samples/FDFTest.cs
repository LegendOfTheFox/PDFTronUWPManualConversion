//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.FDF;
using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class FDFTest : Sample
    {
        public FDFTest() :
            base("FDF", "PDFNet includes a full support for FDF (Forms Data Format) and capability to merge/extract forms data (FDF) with/from PDF. The sample illustrates basic FDF merge/extract functionality available in PDFNet.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting FDF Test...");
                WriteLine("--------------------------------\n");
			    // Example 1)
			    // Iterate over all form fields in the document. Display all field names.
			    try  
			    {
                    String input_file_path = Path.Combine(InputPath, "form1.pdf");
                    PDFDoc doc = new PDFDoc(input_file_path);
				    doc.InitSecurityHandler();
				
				    FieldIterator itr;
				    for(itr=doc.GetFieldIterator(); itr.HasNext(); itr.Next())
				    {
                        WriteLine(String.Format("Field name: {0:s}", itr.Current().GetName()));
					    WriteLine(String.Format("Field partial name: {0:s}", itr.Current().GetPartialName()));

					    Write("Field type: ");
					    FieldType type = itr.Current().GetType();
					    switch(type)
					    {
						    case FieldType.e_button: 
							    WriteLine("Button"); break;
						    case FieldType.e_text: 
							    WriteLine("Text"); break;
						    case FieldType.e_choice: 
							    WriteLine("Choice"); break;
						    case FieldType.e_signature: 
							    WriteLine("Signature"); break;
					    }

					    WriteLine("\n------------------------------");
				    }

				    doc.Destroy();
				    WriteLine("Done.");
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

                // Example 2) Import XFDF into FDF, then merge data from FDF into PDF
			    try  
			    {
                    // XFDF to FDF
                    // form fields
                    WriteLine("Import form field data from XFDF to FDF.");

                    String input_file_path = Path.Combine(InputPath, "form1_data.xfdf");
                    String output_file_path = Path.Combine(OutputPath, "form1_data.fdf");
                    FDFDoc fdf_doc1 = await FDFDoc.CreateFromXFDFAsync(input_file_path);
                    await fdf_doc1.SaveAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // annotations
                    WriteLine("Import annotations from XFDF to FDF.");

                    input_file_path = Path.Combine(InputPath, "form1_annots.xfdf");
                    output_file_path = Path.Combine(OutputPath, "form1_annots.fdf");
                    FDFDoc fdf_doc2 = await FDFDoc.CreateFromXFDFAsync(input_file_path);
                    await fdf_doc2.SaveAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // FDF to PDF
                    // form fields
                    WriteLine("Merge form field data from FDF.");
                    input_file_path = Path.Combine(InputPath, "form1.pdf");
                    PDFDoc doc = new PDFDoc(input_file_path);
				    doc.InitSecurityHandler();
                    doc.FDFMerge(fdf_doc1);

                    // Refreshing missing appearances is not required here, but is recommended to make them 
                    // visible in PDF viewers with incomplete annotation viewing support. (such as Chrome)
                    doc.RefreshAnnotAppearances();

                    output_file_path = Path.Combine(OutputPath, "form1_filled.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    //annotations
                    WriteLine("Merge annotations from FDF.");
                    doc.FDFMerge(fdf_doc2);

                    // Refreshing missing appearances is not required here, but is recommended to make them 
                    // visible in PDF viewers with incomplete annotation viewing support. (such as Chrome)
                    doc.RefreshAnnotAppearances();

                    output_file_path = Path.Combine(OutputPath, "form1_filled_with_annots.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
				    doc.Destroy();
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

				    WriteLine("Done.");
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

                // Example 3) Extract data from PDF to FDF, then export FDF as XFDF
			    try  
			    {
                    // PDF to FDF
                    String output_file_path = Path.Combine(OutputPath, "form1_filled_with_annots.pdf");
                    PDFDoc in_doc = new PDFDoc(output_file_path);
                    in_doc.InitSecurityHandler();

                    // form fields only
				    WriteLine("Extract form fields data to FDF.");

                    FDFDoc doc_fields = in_doc.FDFExtract(PDFDocExtractFlag.e_forms_only);
                    doc_fields.SetPdfFileName("../form1_filled_with_annots.pdf");
                    output_file_path = Path.Combine(OutputPath, "form1_filled_data.fdf");
                    await doc_fields.SaveAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // annotations only
				    WriteLine("Extract annotations to FDF.");

                    FDFDoc doc_annots = in_doc.FDFExtract(PDFDocExtractFlag.e_annots_only);
                    doc_annots.SetPdfFileName("../form1_filled_with_annots.pdf");
                    output_file_path = Path.Combine(OutputPath, "form1_filled_annot.fdf");
                    await doc_annots.SaveAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // both form fields and annotations
                    WriteLine("Extract both form fields and annotations to FDF.");

                    FDFDoc doc_both = in_doc.FDFExtract(PDFDocExtractFlag.e_both);
                    doc_both.SetPdfFileName("../form1_filled_with_annots.pdf");
                    output_file_path = Path.Combine(OutputPath, "form1_filled_both.fdf");
                    await doc_both.SaveAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // FDF to XFDF
                    // form fields
                    WriteLine("Export form field data from FDF to XFDF.");
                    
                    output_file_path = Path.Combine(OutputPath, "form1_filled_data.xfdf");
                    await doc_fields.SaveAsXFDFAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // annotations
                    WriteLine("Export annotations from FDF to XFDF.");
                    
                    output_file_path = Path.Combine(OutputPath, "form1_filled_annot.xfdf");
                    await doc_annots.SaveAsXFDFAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    // both form fields and annotations
                    WriteLine("Export both form fields and annotations from FDF to XFDF.");

                    output_file_path = Path.Combine(OutputPath, "form1_filled_both.xfdf");
                    await doc_both.SaveAsXFDFAsync(output_file_path);
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                    in_doc.Destroy();
                    WriteLine("Done.");
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

			    // Example 4) Read FDF files directly
			    try  
			    {
                    String output_file_path = Path.Combine(OutputPath, "form1_filled_data.fdf");
				    FDFDoc doc = new FDFDoc(output_file_path);
				    FDFFieldIterator itr = doc.GetFieldIterator();
				    for(; itr.HasNext(); itr.Next()) 
				    {
                        WriteLine(String.Format("Field name: {0:s}", itr.Current().GetName()));
                        WriteLine(String.Format("Field partial name: {0:s}", itr.Current().GetPartialName()));
                        WriteLine("------------------------------");
				    }
                    
                    WriteLine("Done.");
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

			    // Example 5) Direct generation of FDF.
			    try  
			    {
				    FDFDoc doc = new FDFDoc();

				    // Create new fields (i.e. key/value pairs).
				    doc.FieldCreate("Company", (int)FieldType.e_text, "PDFTron Systems");
				    doc.FieldCreate("First Name", (int)FieldType.e_text, "John");
				    doc.FieldCreate("Last Name", (int)FieldType.e_text, "Doe");
				    // ...		

				    // doc.SetPdfFileName("mydoc.pdf");
                    String output_file_path = Path.Combine(OutputPath, "fdf_sample_output.fdf");
				    await doc.SaveAsync(output_file_path);
                    WriteLine("Done. Results saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path);
			    }
			    catch (Exception e)
			    {
                    WriteLine(GetExceptionMessage(e));
			    }

                WriteLine("\n--------------------------------");
                WriteLine("Done FDF Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
