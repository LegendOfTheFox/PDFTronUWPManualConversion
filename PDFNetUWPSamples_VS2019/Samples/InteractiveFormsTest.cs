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
using System.Collections.Generic;
using pdftron.PDF.Annots;

namespace PDFNetSamples
{
    public sealed class InteractiveFormsTest : Sample
    {
        public InteractiveFormsTest() :
            base("InteractiveForms", "The sample illustrates some basic PDFNet capabilities related to interactive forms (also known as AcroForms).")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting InteractiveForms Test...");
                WriteLine("--------------------------------\n");

                // The vector used to store the name and count of all fields.
                // This is used later on to clone the fields
                Dictionary<string, int> fieldNames = new Dictionary<string, int>();
                bool error = false;

                //----------------------------------------------------------------------------------
                // Example 1: Programatically create new Form Fields and Widget Annotations.
                //----------------------------------------------------------------------------------
                try
                {
                    PDFDoc document = new PDFDoc();

                    // Create a blank new page and add some form fields.
                    Page blankPage = document.PageCreate();

                    // Text Widget Creation 
                    // Create an empty text widget with black text.
                    TextWidget text1 = TextWidget.Create(document, new pdftron.PDF.Rect(110, 700, 380, 730));
                    text1.SetText("Basic Text Field");
                    text1.RefreshAppearance();
                    blankPage.AnnotPushBack(text1);
                    // Create a vertical text widget with blue text and a yellow background.
                    TextWidget text2 = TextWidget.Create(document, new pdftron.PDF.Rect(50, 400, 90, 730));
                    text2.SetRotation(90);
                    // Set the text content.
                    text2.SetText("    ****Lucky Stars!****");
                    // Set the font type, text color, font size, border color and background color.
                    text2.SetFont(Font.Create(document, FontStandardType1Font.e_helvetica_oblique));
                    text2.SetFontSize(28);
                    text2.SetTextColor(new ColorPt(0, 0, 1), 3);
                    text2.SetBorderColor(new ColorPt(0, 0, 0), 3);
                    text2.SetBackgroundColor(new ColorPt(1, 1, 0), 3);
                    text2.RefreshAppearance();
                    // Add the annotation to the page.
                    blankPage.AnnotPushBack(text2);
                    // Create two new text widget with Field names employee.name.first and employee.name.last
                    // This logic shows how these widgets can be created using either a field name string or
                    // a Field object
                    TextWidget text3 = TextWidget.Create(document, new pdftron.PDF.Rect(110, 660, 380, 690), "employee.name.first");
                    text3.SetText("Levi");
                    text3.SetFont(Font.Create(document, FontStandardType1Font.e_times_bold));
                    text3.RefreshAppearance();
                    blankPage.AnnotPushBack(text3);
                    Field employeeLastName = document.FieldCreate("employee.name.last", FieldType.e_text, "Ackerman");
                    TextWidget text4 = TextWidget.Create(document, new pdftron.PDF.Rect(110, 620, 380, 650), employeeLastName);
                    text4.SetFont(Font.Create(document, FontStandardType1Font.e_times_bold));
                    text4.RefreshAppearance();
                    blankPage.AnnotPushBack(text4);

                    // Signature Widget Creation (unsigned)
                    SignatureWidget signature1 = SignatureWidget.Create(document, new pdftron.PDF.Rect(110, 560, 260, 610));
                    signature1.RefreshAppearance();
                    blankPage.AnnotPushBack(signature1);

                    // CheckBox Widget Creation
                    // Create a check box widget that is not checked.
                    CheckBoxWidget check1 = CheckBoxWidget.Create(document, new pdftron.PDF.Rect(140, 490, 170, 520));
                    check1.RefreshAppearance();
                    blankPage.AnnotPushBack(check1);
                    // Create a check box widget that is checked.
                    CheckBoxWidget check2 = CheckBoxWidget.Create(document, new pdftron.PDF.Rect(190, 490, 250, 540), "employee.name.check1");
                    check2.SetBackgroundColor(new ColorPt(1, 1, 1), 3);
                    check2.SetBorderColor(new ColorPt(0, 0, 0), 3);
                    // Check the widget (by default it is unchecked).
                    check2.SetChecked(true);
                    check2.RefreshAppearance();
                    blankPage.AnnotPushBack(check2);

                    // PushButton Widget Creation
                    PushButtonWidget pushbutton1 = PushButtonWidget.Create(document, new pdftron.PDF.Rect(380, 490, 520, 540));
                    pushbutton1.SetTextColor(new ColorPt(1, 1, 1), 3);
                    pushbutton1.SetFontSize(36);
                    pushbutton1.SetBackgroundColor(new ColorPt(0, 0, 0), 3);
                    // Add a caption for the pushbutton.
                    pushbutton1.SetStaticCaptionText("PushButton");
                    pushbutton1.RefreshAppearance();
                    blankPage.AnnotPushBack(pushbutton1);

                    // ComboBox Widget Creation
                    ComboBoxWidget combo1 = ComboBoxWidget.Create(document, new pdftron.PDF.Rect(280, 560, 580, 610));
                    // Add options to the combobox widget.
                    combo1.AddOption("Combo Box No.1");
                    combo1.AddOption("Combo Box No.2");
                    combo1.AddOption("Combo Box No.3");
                    // Make one of the options in the combo box selected by default.
                    combo1.SetSelectedOption("Combo Box No.2");
                    combo1.SetTextColor(new ColorPt(1, 0, 0), 3);
                    combo1.SetFontSize(28);
                    combo1.RefreshAppearance();
                    blankPage.AnnotPushBack(combo1);

                    // ListBox Widget Creation
                    ListBoxWidget list1 = ListBoxWidget.Create(document, new pdftron.PDF.Rect(400, 620, 580, 730));
                    // Add one option to the listbox widget.
                    list1.AddOption("List Box No.1");
                    // Add multiple options to the listbox widget in a batch.
                    string[] listOptions = new string[2] { "List Box No.2", "List Box No.3" };
                    list1.AddOptions(listOptions);
                    // Select some of the options in list box as default options
                    list1.SetSelectedOptions(listOptions);
                    // Enable list box to have multi-select when editing. 
                    list1.GetField().SetFlag(FieldFlag.e_multiselect, true);
                    list1.SetFont(Font.Create(document, FontStandardType1Font.e_times_italic));
                    list1.SetTextColor(new ColorPt(1, 0, 0), 3);
                    list1.SetFontSize(28);
                    list1.SetBackgroundColor(new ColorPt(1, 1, 1), 3);
                    list1.RefreshAppearance();
                    blankPage.AnnotPushBack(list1);

                    // RadioButton Widget Creation
                    // Create a radio button group and add three radio buttons in it. 
                    RadioButtonGroup radioGroup = RadioButtonGroup.Create(document, "RadioGroup");
                    RadioButtonWidget radioButton1 = radioGroup.Add(new pdftron.PDF.Rect(140, 410, 190, 460));
                    radioButton1.SetBackgroundColor(new ColorPt(1, 1, 0), 3);
                    radioButton1.RefreshAppearance();
                    RadioButtonWidget radiobutton2 = radioGroup.Add(new pdftron.PDF.Rect(310, 410, 360, 460));
                    radiobutton2.SetBackgroundColor(new ColorPt(0, 1, 0), 3);
                    radiobutton2.RefreshAppearance();
                    RadioButtonWidget radiobutton3 = radioGroup.Add(new pdftron.PDF.Rect(480, 410, 530, 460));
                    // Enable the third radio button. By default the first one is selected
                    radiobutton3.EnableButton();
                    radiobutton3.SetBackgroundColor(new ColorPt(0, 1, 1), 3);
                    radiobutton3.RefreshAppearance();
                    radioGroup.AddGroupButtonsToPage(blankPage);

                    // Custom push button annotation creation
                    PushButtonWidget customPushButton1 = PushButtonWidget.Create(document, new pdftron.PDF.Rect(260, 320, 360, 360));
                    // Set the annotation appearance.
                    customPushButton1.SetAppearance(CreateCustomButtonAppearance(document, false), AnnotAnnotationState.e_normal);
                    // Create 'SubmitForm' action. The action will be linked to the button.
                    FileSpec url = FileSpec.CreateURL(document.GetSDFDoc(), "http://www.pdftron.com");
                    pdftron.PDF.Action buttonAction = pdftron.PDF.Action.CreateSubmitForm(url);
                    // Associate the above action with 'Down' event in annotations action dictionary.
                    Obj annotAction = customPushButton1.GetSDFObj().PutDict("AA");
                    annotAction.Put("D", buttonAction.GetSDFObj());
                    blankPage.AnnotPushBack(customPushButton1);

                    // Add the page as the last page in the document.
                    document.PagePushBack(blankPage);

                    // If you are not satisfied with the look of default auto-generated appearance 
                    // streams you can delete "AP" entry from the Widget annotation and set 
                    // "NeedAppearances" flag in AcroForm dictionary:
                    //    doc.GetAcroForm().PutBool("NeedAppearances", true);
                    // This will force the viewer application to auto-generate new appearance streams 
                    // every time the document is opened.
                    //
                    // Alternatively you can generate custom annotation appearance using ElementWriter 
                    // and then set the "AP" entry in the widget dictionary to the new appearance
                    // stream.
                    //
                    // Yet another option is to pre-populate field entries with dummy text. When 
                    // you edit the field values using PDFNet the new field appearances will match 
                    // the old ones.
                    document.RefreshFieldAppearances();

                    await document.SaveAsync(Path.Combine(OutputPath, "forms_test1.pdf"), 0);
                    await AddFileToOutputList(Path.Combine(OutputPath, "forms_test1.pdf")).ConfigureAwait(false);

                    WriteLine("Done.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                    error = true;
                }

                //----------------------------------------------------------------------------------
                // Example 2: 
                // Fill-in forms / Modify values of existing fields.
                // Traverse all form fields in the document (and print out their names). 
                // Search for specific fields in the document.
                //----------------------------------------------------------------------------------
                try
                {
                    PDFDoc document = new PDFDoc(Path.Combine(OutputPath, "forms_test1.pdf"));
                    document.InitSecurityHandler();

                    FieldIterator iterator;
                    Field field;
                    for (iterator = document.GetFieldIterator(); iterator.HasNext(); iterator.Next())
                    {
                        field = iterator.Current();
                        string currentFieldName = field.GetName();
                        // Add one to the count for this field name for later processing
                        fieldNames[currentFieldName] = (fieldNames.ContainsKey(currentFieldName) ? fieldNames[currentFieldName] + 1 : 1);

                        WriteLine("Field name: " + field.GetName());
                        WriteLine("Field partial name: " + field.GetPartialName());
                        string stringValue = field.GetValueAsString();

                        Write("Field type: ");
                        FieldType type = field.GetType();
                        switch (type)
                        {
                            case FieldType.e_button:
                                WriteLine("Button");
                                break;
                            case FieldType.e_radio:
                                WriteLine("Radio button: Value = " + stringValue);
                                break;
                            case FieldType.e_check:
                                field.SetValue(true);
                                WriteLine("Check box: Value = " + stringValue);
                                break;
                            case FieldType.e_text:
                                {
                                    WriteLine("Text");

                                    // Edit all variable text in the document
                                    string oldValue = "none";
                                    if (field.GetValue() != null)
                                    {
                                        oldValue = field.GetValue().GetAsPDFText();
                                    }

                                    field.SetValue("This is a new value. The old one was: " + oldValue);
                                }
                                break;
                            case FieldType.e_choice:
                                WriteLine("Choice");
                                break;
                            case FieldType.e_signature:
                                WriteLine("Signature");
                                break;
                        }

                        WriteLine("------------------------------");
                    }

                    // Search for a specific field
                    field = document.GetField("employee.name.first");
                    if (field != null)
                    {
                        WriteLine("Field search for {0} was successful " + field.GetName());
                    }
                    else
                    {
                        WriteLine("Field search failed.");
                    }

                    // Regenerate field appearances.
                    document.RefreshFieldAppearances();
                    await document.SaveAsync(Path.Combine(OutputPath, "forms_test_edit.pdf"), 0);
                    await AddFileToOutputList(Path.Combine(OutputPath, "forms_test_edit.pdf")).ConfigureAwait(false);
                    WriteLine("Done.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                    error = true;
                }

                //----------------------------------------------------------------------------------
                // Sample: Form templating
                // Replicate pages and form data within a document. Then rename field names to make 
                // them unique.
                //----------------------------------------------------------------------------------
                try
                {
                    // Sample: Copying the page with forms within the same document
                    PDFDoc document = new PDFDoc(Path.Combine(OutputPath, "forms_test1.pdf"));

                    document.InitSecurityHandler();

                    Page sourcePage = document.GetPage(1);
                    document.PagePushBack(sourcePage);  // Append several copies of the second page
                    document.PagePushBack(sourcePage);  // Note that forms are successfully copied
                    document.PagePushBack(sourcePage);
                    document.PagePushBack(sourcePage);

                    // Now we rename fields in order to make every field unique.
                    // You can use this technique for dynamic template filling where you have a 'master'
                    // form page that should be replicated, but with unique field names on every page. 
                    foreach (KeyValuePair<string, int> currentField in fieldNames)
                    {
                        RenameAllFields(document, currentField.Key, currentField.Value);
                    }

                    await document.SaveAsync(Path.Combine(OutputPath, "forms_test1_cloned.pdf"), 0);
                    await AddFileToOutputList(Path.Combine(OutputPath, "forms_test1_cloned.pdf")).ConfigureAwait(false);
                    WriteLine("Done.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                    error = true;
                }

                //----------------------------------------------------------------------------------
                // Sample: 
                // Flatten all form fields in a document.
                // Note that this sample is intended to show that it is possible to flatten
                // individual fields. PDFNet provides a utility function PDFDoc.FlattenAnnotations()
                // that will automatically flatten all fields.
                //----------------------------------------------------------------------------------
                try
                {
                    PDFDoc document = new PDFDoc(Path.Combine(OutputPath, "forms_test1.pdf"));

                    document.InitSecurityHandler();

                    bool auto = true;
                    if (auto)
                    {
                        document.FlattenAnnotations();
                    }
                    else  // Manual flattening 
                    {
                        // Traverse all pages
                        PageIterator pageIterator = document.GetPageIterator();
                        for (; pageIterator.HasNext(); pageIterator.Next())
                        {
                            Page page = pageIterator.Current();
                            for (int i = page.GetNumAnnots() - 1; i >= 0; --i)
                            {
                                IAnnot annot = page.GetAnnot(i);
                                if (annot.GetAnnotType() == AnnotType.e_Widget)
                                {
                                    annot.Flatten(page);
                                }
                            }
                        }
                    }

                    await document.SaveAsync(Path.Combine(OutputPath, "forms_test1_flattened.pdf"), 0);
                    await AddFileToOutputList(Path.Combine(OutputPath, "forms_test1_flattened.pdf")).ConfigureAwait(false);
                    WriteLine("Done.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                    error = true;
                }

                //////////////////// End of tests. ////////////////////
                if (error)
                {
                    WriteLine("Tests FAILED!!!\n==========");
                }
                else
                {
                    WriteLine("Tests successful.\n==========");
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done InteractiveForms Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}

        // fieldNumber has to be greater than 0.
        static void RenameAllFields(PDFDoc document, string name, int fieldNumber = 1)
        {
            Field field = document.GetField(name);
            for (int counter = 1; field != null; ++counter)
            {
                string fieldNewName = name;
                int updateCount = System.Convert.ToInt32(Math.Ceiling(counter / System.Convert.ToDouble(fieldNumber)));
                field.Rename(name + "-" + updateCount.ToString());
                field = document.GetField(name);
            }
        }

        static Obj CreateCustomButtonAppearance(PDFDoc document, bool buttonDown)
        {
            // Create a button appearance stream ------------------------------------
            using (ElementBuilder builder = new ElementBuilder())
            using (ElementWriter writer = new ElementWriter())
            {
                writer.Begin(document.GetSDFDoc());

                // Draw background
                Element element = builder.CreateRect(0, 0, 101, 37);
                element.SetPathFill(true);
                element.SetPathStroke(false);
                element.GetGState().SetFillColorSpace(ColorSpace.CreateDeviceGray());
                element.GetGState().SetFillColor(new ColorPt(0.75, 0.0, 0.0));
                writer.WriteElement(element);

                // Draw 'Submit' text
                writer.WriteElement(builder.CreateTextBegin());

                element = builder.CreateTextRun("Submit", Font.Create(document, FontStandardType1Font.e_helvetica_bold), 12);
                element.GetGState().SetFillColor(new ColorPt(0, 0, 0));

                if (buttonDown)
                {
                    element.SetTextMatrix(1, 0, 0, 1, 33, 10);
                }
                else
                {
                    element.SetTextMatrix(1, 0, 0, 1, 30, 13);
                }

                writer.WriteElement(element);
                writer.WriteElement(builder.CreateTextEnd());

                Obj buttonAppearanceStream = writer.End();

                // Set the bounding box
                buttonAppearanceStream.PutRect("BBox", 0, 0, 101, 37);
                buttonAppearanceStream.PutName("Subtype", "Form");
                return buttonAppearanceStream;
            }
        }
    }
}
