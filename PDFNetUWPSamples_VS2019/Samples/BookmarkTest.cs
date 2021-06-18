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
    //-----------------------------------------------------------------------------------------
    // The sample code illustrates how to read, write, and edit existing outline items 
    // and create new bookmarks using both the high-level and the SDF/Cos API.
    //-----------------------------------------------------------------------------------------
    public sealed class BookmarkTest : Sample
    {
        public BookmarkTest() :
            base("Bookmark", "The sample code illustrates how to read and edit existing outline items and create new bookmarks using the high-level API.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Bookmark Test...");
                WriteLine("--------------------------------\n");
                try {
                    String input_file_path = Path.Combine(InputPath, "numbered.pdf");
                    WriteLine("Opening input file " + input_file_path);
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();

                    // Lets first create the root bookmark items. 
                    Bookmark red = Bookmark.Create(doc, "Red");
                    Bookmark green = Bookmark.Create(doc, "Green");
                    Bookmark blue = Bookmark.Create(doc, "Blue");

                    doc.AddRootBookmark(red);
                    doc.AddRootBookmark(green);
                    doc.AddRootBookmark(blue);

                    // You can also add new root bookmarks using Bookmark.AddNext("...")
                    blue.AddNext("foo");
                    blue.AddNext("bar");

                    // We can now associate new bookmarks with page destinations:

                    // The following example creates an 'explicit' destination (see 
                    // section '8.2.1 Destinations' in PDF Reference for more details)
                    Destination red_dest = Destination.CreateFit(doc.GetPage(1));
                    red.SetAction(pdftron.PDF.Action.CreateGoto(red_dest));

                    // Create an explicit destination to the first green page in the document
                    green.SetAction(pdftron.PDF.Action.CreateGoto(
                        Destination.CreateFit(doc.GetPage(10))));

                    // The following example creates a 'named' destination (see 
                    // section '8.2.1 Destinations' in PDF Reference for more details)
                    // Named destinations have certain advantages over explicit destinations.
                    String key = "blue1";
                    pdftron.PDF.Action blue_action = pdftron.PDF.Action.CreateGoto(key, Destination.CreateFit(doc.GetPage(19)));

                    blue.SetAction(blue_action);

                    // We can now add children Bookmarks
                    Bookmark sub_red1 = red.AddChild("Red - Page 1");
                    sub_red1.SetAction(pdftron.PDF.Action.CreateGoto(Destination.CreateFit(doc.GetPage(1))));
                    Bookmark sub_red2 = red.AddChild("Red - Page 2");
                    sub_red2.SetAction(pdftron.PDF.Action.CreateGoto(Destination.CreateFit(doc.GetPage(2))));
                    Bookmark sub_red3 = red.AddChild("Red - Page 3");
                    sub_red3.SetAction(pdftron.PDF.Action.CreateGoto(Destination.CreateFit(doc.GetPage(3))));
                    Bookmark sub_red4 = sub_red3.AddChild("Red - Page 4");
                    sub_red4.SetAction(pdftron.PDF.Action.CreateGoto(Destination.CreateFit(doc.GetPage(4))));
                    Bookmark sub_red5 = sub_red3.AddChild("Red - Page 5");
                    sub_red5.SetAction(pdftron.PDF.Action.CreateGoto(Destination.CreateFit(doc.GetPage(5))));
                    Bookmark sub_red6 = sub_red3.AddChild("Red - Page 6");
                    sub_red6.SetAction(pdftron.PDF.Action.CreateGoto(Destination.CreateFit(doc.GetPage(6))));

                    // Example of how to find and delete a bookmark by title text.
                    Bookmark foo = doc.GetFirstBookmark().Find("foo");
                    if (foo.IsValid()) {
                        foo.Delete();
                    }

                    Bookmark bar = doc.GetFirstBookmark().Find("bar");
                    if (bar.IsValid()) {
                        bar.Delete();
                    }

                    // Adding color to Bookmarks. Color and other formatting can help readers 
                    // get around more easily in large PDF documents.
                    red.SetColor(1, 0, 0);
                    green.SetColor(0, 1, 0);
                    green.SetFlags(2);			// set bold font
                    blue.SetColor(0, 0, 1);
                    blue.SetFlags(3);			// set bold and italic

                    String output_file_path = Path.Combine(OutputPath, "bookmark.pdf");
                    await doc.SaveAsync(output_file_path, 0);
                    doc.Destroy();
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                }
                catch (Exception e) {
                    WriteLine(GetExceptionMessage(e));
                }

                // The following example illustrates how to traverse the outline tree using 
                // Bookmark navigation methods: Bookmark.GetNext(), Bookmark.GetPrev(), 
                // Bookmark.GetFirstChild () and Bookmark.GetLastChild ().
                try {
                    String input_file_path = Path.Combine(OutputPath, "bookmark.pdf");
                    // Open the document that was saved in the previous code sample
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();

                    Bookmark root = doc.GetFirstBookmark();
                    PrintOutlineTree(root);

                    doc.Destroy();
                }
                catch (Exception e) {
                    WriteLine(GetExceptionMessage(e));
                }

                // The following example illustrates how to create a Bookmark to a page 
                // in a remote document. A remote go-to action is similar to an ordinary 
                // go-to action, but jumps to a destination in another PDF file instead 
                // of the current file. See Section 8.5.3 'Remote Go-To Actions' in PDF 
                // Reference Manual for details.
                try {
                    String input_file_path = Path.Combine(OutputPath, "bookmark.pdf");
                    PDFDoc doc = new PDFDoc(input_file_path);
                    doc.InitSecurityHandler();

                    // Create file specification (the file referred to by the remote bookmark)
                    Obj file_spec = doc.CreateIndirectDict();
                    file_spec.PutName("Type", "Filespec");
                    file_spec.PutString("F", "bookmark.pdf");

                    FileSpec spec = new FileSpec(file_spec);
                    pdftron.PDF.Action goto_remote = pdftron.PDF.Action.CreateGotoRemote(spec, 5, true);

                    Bookmark remoteBookmark1 = Bookmark.Create(doc, "REMOTE BOOKMARK 1");
                    remoteBookmark1.SetAction(goto_remote);
                    doc.AddRootBookmark(remoteBookmark1);

                    // Create another remote bookmark, but this time using the low-level SDF/Cos API.
                    Bookmark remoteBookmark2 = Bookmark.Create(doc, "REMOTE BOOKMARK 2");
                    doc.AddRootBookmark(remoteBookmark2);
                    Obj gotoR = remoteBookmark2.GetSDFObj().PutDict("A");
                    {	// Create the 'Action' dictionary.
                        gotoR.PutName("S", "GoToR"); // Set action type
                        gotoR.PutBool("NewWindow", true);

                        // Set the file specification
                        gotoR.Put("F", file_spec);

                        // Set the destination.
                        Obj dest = gotoR.PutArray("D");
                        dest.PushBackNumber(9);  // jump to the first page. Note that Acrobat indexes pages from 0.
                        dest.PushBackName("Fit"); // Fit the page
                    }

                    String output_file_path = Path.Combine(OutputPath, "bookmark_remote.pdf");
                    await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                    doc.Destroy();
                    WriteLine("Done. Result saved in " + output_file_path);
                    await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                }
                catch (Exception e) {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Bookmark Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        void PrintIndent(Bookmark item)
        {
            int indent = item.GetIndent() - 1;
            String indentStr = String.Empty;
            for (int i = 0; i < indent; ++i)
                indentStr += ("  ");

            Write(indentStr);
        }

        // Prints out the outline tree to the standard output
        void PrintOutlineTree(Bookmark item)
        {
            for (; item.IsValid(); item = item.GetNext())
            {
                PrintIndent(item);
                WriteLine(String.Format("{0:s}{1:s} ACTION -> ", (item.IsOpen() ? "- " : "+ "), item.GetTitle()));

                // Print Action
                pdftron.PDF.Action action = item.GetAction();
                if (action.IsValid())
                {
                    if (action.GetType() == pdftron.PDF.ActionType.e_GoTo)
                    {
                        Destination dest = action.GetDest();
                        if (dest.IsValid())
                        {
                            pdftron.PDF.Page page = dest.GetPage();
                            WriteLine(String.Format("Goto Page #{0:d}", page.GetIndex()));
                        }
                    }
                    else
                    {
                        WriteLine("Not a 'GoTo' action");
                    }
                }
                else
                {
                    WriteLine("NULL");
                }

                if (item.HasChildren())	 // Recursively print children sub-trees
                {
                    PrintOutlineTree(item.GetFirstChild());
                }
            }
        }
    }
}
