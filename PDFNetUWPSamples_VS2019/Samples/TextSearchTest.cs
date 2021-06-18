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
    public sealed class TextSearchTest : Sample
    {
        public TextSearchTest() :
            base("TextSearch", "This sample shows how to use pdftron.PDF.TextSearch to search text on PDF pages using regular expressions. TextSearch utility class builds on functionality available in TextExtractor to simplify most common search operations.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting TextSearch Test...");
                WriteLine("--------------------------------\n");
                try
                {
                    string input_file_path = Path.Combine(InputPath, "credit card numbers.pdf");
                    WriteLine("Opening input file " + input_file_path);
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();

                    pdftron.Common.Int32Ref page_num = new pdftron.Common.Int32Ref(0);
                    //String result_str = "", ambient_string = "";
                    pdftron.Common.StringRef result_str = new pdftron.Common.StringRef();
                    pdftron.Common.StringRef ambient_string = new pdftron.Common.StringRef();
                    Highlights hlts = new Highlights();

                    TextSearch txt_search = new TextSearch();
                    int mode = (int)(TextSearchSearchMode.e_whole_word) | (int)(TextSearchSearchMode.e_page_stop) | (int)(TextSearchSearchMode.e_highlight);
                    //String pattern = "joHn sMiTh";
                    String pattern = "John Smith";

                    //call Begin() method to initialize the text search.
                    txt_search.Begin(doc, pattern, mode, -1, -1);

                    int step = 0;

                    //call Run() method iteratively to find all matching instances.
                    while (true)
                    {
                        TextSearchResultCode code = txt_search.Run(page_num, result_str, ambient_string, hlts);

                        if (code == TextSearchResultCode.e_found)
                        {
                            if (step == 0)
                            {	//step 0: found "John Smith"
                                //note that, here, 'ambient_string' and 'hlts' are not written to, 
                                //as 'e_ambient_string' and 'e_highlight' are not set.
                                WriteLine(result_str.Value + "'s credit card number is: ");

                                //now switch to using regular expressions to find John's credit card number
                                mode = txt_search.GetMode();
                                mode |= (int)(TextSearchSearchMode.e_reg_expression | TextSearchSearchMode.e_highlight);
                                txt_search.SetMode(mode);
                                pattern = "\\d{4}-\\d{4}-\\d{4}-\\d{4}"; //or "(\\d{4}-){3}\\d{4}"
                                txt_search.SetPattern(pattern);

                                ++step;
                            }
                            else if (step == 1)
                            {
                                //step 1: found John's credit card number
                                //result_str.ConvertToAscii(char_buf, 32, true);
                                //cout << "  " << char_buf << endl;
                                WriteLine(result_str.Value);

                                //note that, here, 'hlts' is written to, as 'e_highlight' has been set.
                                //output the highlight info of the credit card number
                                hlts.Begin(doc);
                                while (hlts.HasNext())
                                {
                                    WriteLine("The current highlight is from page: " + hlts.GetCurrentPageNumber());
                                    hlts.Next();
                                }

                                //see if there is an AMEX card number
                                pattern = "\\d{4}-\\d{6}-\\d{5}";
                                txt_search.SetPattern(pattern);

                                ++step;
                            }
                            else if (step == 2)
                            {
                                //found an AMEX card number
                                WriteLine("There is an AMEX card number:\n  " + result_str.Value);

                                //change mode to find the owner of the credit card; supposedly, the owner's
                                //name proceeds the number
                                mode = txt_search.GetMode();
                                mode |= (int)(TextSearchSearchMode.e_search_up);
                                txt_search.SetMode(mode);
                                pattern = "[A-z]+ [A-z]+";
                                txt_search.SetPattern(pattern);

                                ++step;
                            }
                            else if (step == 3)
                            {
                                //found the owner's name of the AMEX card
                                WriteLine("Is the owner's name:\n  " + result_str.Value + "?");

                                //add a link annotation based on the location of the found instance
                                hlts.Begin(doc);
                                while (hlts.HasNext())
                                {
                                    pdftron.PDF.Page cur_page = doc.GetPage(hlts.GetCurrentPageNumber());
                                    double[] quads = hlts.GetCurrentQuads();
                                    int quad_count = quads.Length / 8;
                                    for (int i = 0; i < quad_count; ++i)
                                    {
                                        //assume each quad is an axis-aligned rectangle
                                        int offset = 8 * i;
                                        double x1 = Math.Min(Math.Min(Math.Min(quads[offset + 0], quads[offset + 2]), quads[offset + 4]), quads[offset + 6]);
                                        double x2 = Math.Max(Math.Max(Math.Max(quads[offset + 0], quads[offset + 2]), quads[offset + 4]), quads[offset + 6]);
                                        double y1 = Math.Min(Math.Min(Math.Min(quads[offset + 1], quads[offset + 3]), quads[offset + 5]), quads[offset + 7]);
                                        double y2 = Math.Max(Math.Max(Math.Max(quads[offset + 1], quads[offset + 3]), quads[offset + 5]), quads[offset + 7]);

                                        pdftron.PDF.Annots.Link hyper_link = pdftron.PDF.Annots.Link.Create(doc.GetSDFDoc(), new pdftron.PDF.Rect(x1, y1, x2, y2), pdftron.PDF.Action.CreateURI(doc.GetSDFDoc(), "http://www.pdftron.com"));
                                        hyper_link.RefreshAppearance();
                                        cur_page.AnnotPushBack(hyper_link);
                                    }
                                    hlts.Next();
                                }
                                string output_file_path = Path.Combine(OutputPath, "credit card numbers_linked.pdf");
                                await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                                WriteLine("Done. Results saved in " + output_file_path);
                                await AddFileToOutputList(output_file_path).ConfigureAwait(false);

                                break;
                            }
                        }
                        else if (code == TextSearchResultCode.e_page)
                        {
                            //you can update your UI here, if needed
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done TextSearch Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }
    }
}
