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
    public sealed class TextExtractTest : Sample
    {
        public TextExtractTest() :
            base("TextExtract", "The sample illustrates the basic text extraction capabilities of PDFNet.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(() => {
                WriteLine("--------------------------------");
                WriteLine("Starting TextExtract Test...");
                WriteLine("--------------------------------\n");
                bool example1_basic = true;
                bool example2_xml = true;
                bool example3_wordlist = true;
                bool example4_advanced = false;
                bool example5_low_level = false;

                // Sample code showing how to use high-level text extraction APIs.

                try
                {
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();
                    pdftron.PDF.Page page = doc.GetPage(1);
                    if (page == null)
                    {
                        WriteLine("Page not found.");
                        return;
                    }

                    TextExtractor txt = new TextExtractor();
                    txt.Begin(page);  // Read the page.
                    // Other options you may want to consider...
                    // txt.Begin(page, null, TextExtractor.ProcessingFlags.e_no_dup_remove);
                    // txt.Begin(page, null, TextExtractor.ProcessingFlags.e_remove_hidden_text);
                    // ...

                    string outputResult = string.Empty;

                    // Example 1. Get all text on the page in a single string.
                    // Words will be separated with space or new line characters.
                    if (example1_basic)
                    {
                        // Get the word count.
                        outputResult += string.Format("Word Count: {0}\n", txt.GetWordCount());
                        outputResult += string.Format("\n- GetAsText --------------------------\n{0}\n", txt.GetAsText());
                        outputResult += "-----------------------------------------------------------\n";
                    }

                    // Example 2. Get XML logical structure for the page.
                    if (example2_xml)
                    {
                        String text = txt.GetAsXML(TextExtractorXMLOutputFlags.e_words_as_elements | TextExtractorXMLOutputFlags.e_output_bbox | TextExtractorXMLOutputFlags.e_output_style_info);

                        outputResult += string.Format("\n\n- GetAsXML  --------------------------\n{0}\n", text);
                        outputResult += "-----------------------------------------------------------\n";
                    }

                    // Example 3. Extract words one by one.
                    if (example3_wordlist)
                    {
                        TextExtractorWord word;
                        for (TextExtractorLine line = txt.GetFirstLine(); line.IsValid(); line = line.GetNextLine())
                        {
                            string wordlist = "";
                            for (word = line.GetFirstWord(); word.IsValid(); word = word.GetNextWord())
                            {
                                wordlist += " " + word.GetString();
                            }
                            outputResult += wordlist + "\n";
                        }
                        outputResult += "-----------------------------------------------------------\n";
                    }

                    // Print result of Example 1, 2 and 3
                    WriteLine(outputResult);

                    // Example 3. A more advanced text extraction example. 
                    // The output is XML structure containing paragraphs, lines, words, 
                    // as well as style and positioning information.
                    if (example4_advanced)
                    {
                        pdftron.PDF.Rect bbox;
                        int cur_flow_id = -1, cur_para_id = -1;

                        TextExtractorLine line;
                        TextExtractorWord word;
                        TextExtractorStyle s, line_style;

                        // For each line on the page...
                        for (line = txt.GetFirstLine(); line.IsValid(); line = line.GetNextLine())
                        {
                            if (line.GetNumWords() == 0)
                            {
                                continue;
                            }

                            if (cur_flow_id != line.GetFlowID())
                            {
                                if (cur_flow_id != -1)
                                {
                                    if (cur_para_id != -1)
                                    {
                                        cur_para_id = -1;
                                        WriteLine("</Para>");
                                    }
                                    WriteLine("</Flow>");
                                }
                                cur_flow_id = line.GetFlowID();
                                WriteLine(string.Format("<Flow id=\"{0}\">", cur_flow_id));
                            }

                            if (cur_para_id != line.GetParagraphID())
                            {
                                if (cur_para_id != -1)
                                    WriteLine("</Para>");
                                cur_para_id = line.GetParagraphID();
                                WriteLine(string.Format("<Para id=\"{0}\">", cur_para_id));
                            }

                            bbox = line.GetBBox();
                            line_style = line.GetStyle();
                            WriteLine(string.Format("<Line box=\"{0}, {1}, {2}, {3}\"", bbox.x1, bbox.y1, bbox.x2, bbox.y2));
                            PrintStyle(line_style);
                            

                            // For each word in the line...
                            for (word = line.GetFirstWord(); word.IsValid(); word = word.GetNextWord())
                            {
                                // Output the bounding box for the word.
                                bbox = word.GetBBox();
                                Write(string.Format("<Word box=\"{0}, {1}, {2}, {3}\"", bbox.x1, bbox.y1, bbox.x2, bbox.y2));

                                int sz = word.GetStringLen();
                                if (sz == 0) continue;

                                // If the word style is different from the parent style, output the new style.
                                s = word.GetStyle();
                                if (s != line_style)
                                {
                                    PrintStyle(s);
                                }

                                WriteLine(string.Format(">\n{0}", word.GetString()));
                                WriteLine("</Word>");
                            }
                            WriteLine("</Line>");
                        }

                        if (cur_flow_id != -1)
                        {
                            if (cur_para_id != -1)
                            {
                                cur_para_id = -1;
                                WriteLine("</Para>");
                            }
                            WriteLine("</Flow>");
                        }
                    }

                    doc.Destroy();
                    WriteLine("Done.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                // Sample code showing how to use low-level text extraction APIs.
                if (example5_low_level)
                {
                    try
                    {
                        LowLevelTextExtractUtils util = new LowLevelTextExtractUtils();
                        PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "newsletter.pdf"));
                        doc.InitSecurityHandler();

                        // Example 1. Extract all text content from the document
                        ElementReader reader = new ElementReader();
                        PageIterator itr = doc.GetPageIterator();
                        //for (; itr.HasNext(); itr.Next()) //  Read every page
                        {
                            reader.Begin(itr.Current());
                            //LowLevelTextExtractUtils.DumpAllText(reader);
                            WriteLine(util.DumpAllText(reader, this));
                            reader.End();
                        }

                        // Example 2. Extract text based on the selection rectangle.
                        WriteLine("----------------------------------------------------");
                        WriteLine("Extract text based on the selection rectangle.");
                        WriteLine("----------------------------------------------------");

                        pdftron.PDF.Page first_page = doc.GetPage(1);
                        string field1 = util.ReadTextFromRect(first_page, new pdftron.PDF.Rect(27, 392, 563, 534), reader);
                        string field2 = util.ReadTextFromRect(first_page, new pdftron.PDF.Rect(28, 551, 106, 623), reader);
                        string field3 = util.ReadTextFromRect(first_page, new pdftron.PDF.Rect(208, 550, 387, 621), reader);

                        WriteLine(string.Format("Field 1: {0}", field1));
                        WriteLine(string.Format("Field 2: {0}", field2));
                        WriteLine(string.Format("Field 3: {0}", field3));
                        // ... 


                        doc.Destroy();
                        WriteLine("Done.");
                    }
                    catch (Exception e)
                    {
                        WriteLine(GetExceptionMessage(e));
                    }
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done TextExtract Test.");
                WriteLine("--------------------------------\n");
                Flush();
            })).AsAsyncAction();
        }

        void PrintStyle(TextExtractorStyle s)
        {
            WriteLine(string.Format(" style=\"font-family: {0}; font-size: {1}; {2}\"", s.GetFontName(), s.GetFontSize(), (s.IsSerif() ? " sans-serif; " : " ")));
        }
    }

    sealed class LowLevelTextExtractUtils
    {
        // A utility method used to dump all text content in the 
        // console window.

        public String DumpAllText(ElementReader reader, Sample sample)
        {
            String result = "";
            Element element;
            //int i = 0;
            while ((element = reader.Next()) != null)
            {
                switch (element.GetType())
                {
                    case ElementType.e_text_begin:
                        result += ("--> Text Block Begin");
                        break;
                    case ElementType.e_text_end:
                        result += ("--> Text Block End");
                        break;
                    case ElementType.e_text:
                        {
                            pdftron.PDF.Rect bbox = new pdftron.PDF.Rect();
                            element.GetBBox(bbox);
                            result += (string.Format("\n--> BBox: {0}, {1}, {2}, {3}", bbox.x1, bbox.y1, bbox.x2, bbox.y2));

                            String txt = element.GetTextString();
                            sample.WriteLine(txt);
                            break;
                        }
                    case ElementType.e_text_new_line:
                        {
                            result += ("--> New Line");
                            break;
                        }
                    case ElementType.e_form: // Process form XObjects
                        {
                            reader.FormBegin();
                            DumpAllText(reader, sample);
                            reader.End();
                            break;
                        }
                }
            }
            return result;
        }

        string _srch_str;

        // A helper method for ReadTextFromRect
        void RectTextSearch(ElementReader reader, pdftron.PDF.Rect pos)
        {
            Element element;
            while ((element = reader.Next()) != null)
            {
                switch (element.GetType())
                {
                    case ElementType.e_text:
                        {
                            pdftron.PDF.Rect bbox = new pdftron.PDF.Rect();
                            element.GetBBox(bbox);
                            if (bbox.IntersectRect(bbox, pos))
                            {
                                _srch_str += element.GetTextString();
                                _srch_str += "\n"; // add a new line?
                            }
                            break;
                        }
                    case ElementType.e_text_new_line:
                        {
                            break;
                        }
                    case ElementType.e_form: // Process form XObjects
                        {
                            reader.FormBegin();
                            RectTextSearch(reader, pos);
                            reader.End();
                            break;
                        }
                }
            }
        }

        // A utility method used to extract all text content from
        // a given selection rectangle. The rectangle coordinates are
        // expressed in PDF user/page coordinate system.
        public string ReadTextFromRect(pdftron.PDF.Page page, pdftron.PDF.Rect pos, ElementReader reader)
        {
            _srch_str = "";
            reader.Begin(page);
            RectTextSearch(reader, pos);
            reader.End();
            return _srch_str;
        }
    }
}
