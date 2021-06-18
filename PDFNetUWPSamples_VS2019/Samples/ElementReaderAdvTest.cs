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
    public sealed class ElementReaderAdvTest : Sample
    {
        public ElementReaderAdvTest() :
            base("ElementReaderAdv", "The sample shows how to use some of more advanced PDFNet features. The sample code illustrates how to extract text, paths, and images. The sample also shows how to do color conversion, image normalization, and how to process changes in the graphics state.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(() => {
                WriteLine("--------------------------------");
                WriteLine("Starting ElementReaderAdv Test...");
                WriteLine("--------------------------------\n");
                try
                {
                    WriteLine("Extract page element information from all pages in the document.");
                    string input_file_path = Path.Combine(InputPath, "newsletter.pdf");
                    WriteLine("Opening input file " + input_file_path);
                    PDFDoc doc = new PDFDoc(input_file_path);

                    doc.InitSecurityHandler();

                    int pgnum = doc.GetPageCount();
                    PageIterator itr;

                    ElementReader page_reader = new ElementReader();

                    itr = doc.GetPageIterator(); //Read first page
                    WriteLine(String.Format("Page {0:d} ----------------------------------------", itr.GetPageNumber()));

                    pdftron.PDF.Rect crop_box = itr.Current().GetCropBox();
                    crop_box.Normalize();

                    WriteLine(String.Format(" Page Rectangle: x={0:f} y={1:f} x2={2:f} y2={3:f}", crop_box.x1, crop_box.y1, crop_box.x2, crop_box.y2));
                    WriteLine(String.Format(" Page Size: width={0:f} height={1:f}", crop_box.Width(), crop_box.Height()));

                    page_reader.Begin(itr.Current());
                    ProcessElements(page_reader);
                    page_reader.End();

                    doc.Destroy();
                    WriteLine("\nDone.");
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done ElementReaderAdv Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        String m_buf;

        String ProcessPath(ElementReader reader, Element path)
        {
            String result = "";
            if (path.IsClippingPath())
            {
                result += ("This is a clipping path.\n");
            }

            PathData pathData = path.GetPathData();
            pathData.get_pts();
            double[] data = pathData.get_pts();// points;
            int data_sz = data.Length;
            //int data_sz = path.

            byte[] opr = pathData.get_ops();//operators;
            int opr_sz = opr.Length;

            int opr_itr = 0, opr_end = opr_sz;
            int data_itr = 0, data_end = data_sz;
            double x1, y1, x2, y2, x3, y3;

            // Use path.GetCTM() if you are interested in CTM (current transformation matrix).

            result += (" Path Data Points := \"\n");
            for (; opr_itr < opr_end; ++opr_itr)
            {
                switch ((PathDataPathSegmentType)((int)opr[opr_itr]))
                {
                    case PathDataPathSegmentType.e_moveto:
                        x1 = data[data_itr]; ++data_itr;
                        y1 = data[data_itr]; ++data_itr;
                        m_buf = String.Format("M{0:g5} {1:g5}", x1, y1);
                        result += (m_buf + "\n");
                        break;
                    case PathDataPathSegmentType.e_lineto:
                        x1 = data[data_itr]; ++data_itr;
                        y1 = data[data_itr]; ++data_itr;
                        m_buf = String.Format(" L{0:g5} {1:g5}", x1, y1);
                        result += (m_buf + "\n");
                        break;
                    case PathDataPathSegmentType.e_cubicto:
                        x1 = data[data_itr]; ++data_itr;
                        y1 = data[data_itr]; ++data_itr;
                        x2 = data[data_itr]; ++data_itr;
                        y2 = data[data_itr]; ++data_itr;
                        x3 = data[data_itr]; ++data_itr;
                        y3 = data[data_itr]; ++data_itr;
                        m_buf = String.Format(" C{0:g5} {1:g5} {2:g5} {3:g5} {4:g5} {5:g5}",
                            new object[] { x1, y1, x2, y2, x3, y3 });
                        result += (m_buf + "\n");
                        break;
                    case PathDataPathSegmentType.e_rect:
                        {
                            x1 = data[data_itr]; ++data_itr;
                            y1 = data[data_itr]; ++data_itr;
                            double w = data[data_itr]; ++data_itr;
                            double h = data[data_itr]; ++data_itr;
                            x2 = x1 + w;
                            y2 = y1;
                            x3 = x2;
                            y3 = y1 + h;
                            double x4 = x1;
                            double y4 = y3;
                            m_buf = String.Format("M{0:g5} {1:g5} L{2:g5} {3:g5} L{4:g5} {5:g5} L{6:g5} {7:g5} Z",
                                new object[] { x1, y1, x2, y2, x3, y3, x4, x3 });
                            result += (m_buf);
                            break;
                        }
                    case PathDataPathSegmentType.e_closepath:
                        result += ("\n Close Path\n");
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }
            }

            result += ("\" ");

            GState gs = path.GetGState();

            // Set Path State 0 (stroke, fill, fill-rule) -----------------------------------
            if (path.IsStroked())
            {
                result += ("Stroke path\n");

                if (gs.GetStrokeColorSpace().GetType() == ColorSpaceType.e_pattern)
                {
                    result += ("Path has associated pattern\n");
                }
                else
                {
                    // Get stroke color (you can use PDFNet color conversion facilities)
                    // ColorPt rgb = new ColorPt();
                    // gs.GetStrokeColorSpace().Convert2RGB(gs.GetStrokeColor(), rgb);
                }
            }
            else
            {
                // Do not stroke path
            }

            if (path.IsFilled())
            {
                result += ("Fill path\n");

                if (gs.GetFillColorSpace().GetType() == ColorSpaceType.e_pattern)
                {
                    result += ("Path has associated pattern\n");
                }
                else
                {
                    // ColorPt rgb = new ColorPt();
                    // gs.GetFillColorSpace().Convert2RGB(gs.GetFillColor(), rgb);
                }
            }
            else
            {
                // Do not fill path
            }

            // Process any changes in graphics state  ---------------------------------

            GSChangesIterator gs_itr = reader.GetChangesIterator();
            for (; gs_itr.HasNext(); gs_itr.Next())
            {
                switch (gs_itr.Current())
                {
                    case GStateGStateAttribute.e_transform:
                        // Get transform matrix for this element. Unlike path.GetCTM() 
                        // that return full transformation matrix gs.GetTransform() return 
                        // only the transformation matrix that was installed for this element.
                        //
                        // gs.GetTransform();
                        break;
                    case GStateGStateAttribute.e_line_width:
                        // gs.GetLineWidth();
                        break;
                    case GStateGStateAttribute.e_line_cap:
                        // gs.GetLineCap();
                        break;
                    case GStateGStateAttribute.e_line_join:
                        // gs.GetLineJoin();
                        break;
                    case GStateGStateAttribute.e_flatness:
                        break;
                    case GStateGStateAttribute.e_miter_limit:
                        // gs.GetMiterLimit();
                        break;
                    case GStateGStateAttribute.e_dash_pattern:
                        {
                            // double[] dashes;
                            // gs.GetDashes(dashes);
                            // gs.GetPhase()
                            break;
                        }
                    case GStateGStateAttribute.e_fill_color:
                        {
                            if (gs.GetFillColorSpace().GetType() == ColorSpaceType.e_pattern &&
                                 gs.GetFillPattern().GetType() != PatternColorType.e_shading)
                            {
                                //process the pattern data
                                reader.PatternBegin(true);
                                ProcessElements(reader);
                                reader.End();
                            }
                            break;
                        }
                }
            }
            reader.ClearChangeList();
            return result;
        }

        String ProcessText(ElementReader page_reader)
        {
            String result = "";
            // Begin text element
            result += ("Begin Text Block:\n");

            Element element;
            while ((element = page_reader.Next()) != null)
            {
                switch (element.GetType())
                {
                    case ElementType.e_text_end:
                        // Finish the text block
                        result += ("End Text Block.\n");
                        return result;

                    case ElementType.e_text:
                        {
                            GState gs = element.GetGState();

                            ColorSpace cs_fill = gs.GetFillColorSpace();
                            ColorPt fill = gs.GetFillColor();

                            ColorPt outc = new ColorPt();
                            cs_fill. Convert2RGB(fill, outc);


                            ColorSpace cs_stroke = gs.GetStrokeColorSpace();
                            ColorPt stroke = gs.GetStrokeColor();

                            Font font = gs.GetFont();

                            result += ("Font Name: ");
                            result += (font.GetName() + "\n");
                            // font.IsFixedWidth();
                            // font.IsSerif();
                            // font.IsSymbolic();
                            // font.IsItalic();
                            // ... 

                            // double word_spacing = gs.GetWordSpacing();
                            // double char_spacing = gs.GetCharSpacing();

                            // Use element.GetCTM() if you are interested in the CTM 
                            // (current transformation matrix).
                            if (font.GetType() == FontType.e_Type3)
                            {
                                //type 3 font, process its data
                                for (CharIterator itr = element.GetCharIterator(); itr.HasNext(); itr.Next())
                                {
                                    page_reader.Type3FontBegin(itr.Current());
                                    ProcessElements(page_reader);
                                    page_reader.End();
                                }
                            }

                            else
                            {
                                Matrix2D ctm = element.GetCTM();

                                Matrix2D text_mtx = element.GetTextMatrix();

                                Matrix2D mtx = Matrix2D.Mult(ctm, text_mtx);
                                double font_sz_scale_factor = System.Math.Sqrt(mtx.m_b * mtx.m_b + mtx.m_d * mtx.m_d);
                                double font_size = gs.GetFontSize();
                                result += (String.Format(" Font Size: {0:f}\n", font_sz_scale_factor * font_size));

                                ColorPt font_color = gs.GetFillColor();
                                ColorSpace cs = gs.GetFillColorSpace();

                                ColorPt rgb = new ColorPt();
                                cs.Convert2RGB(font_color, rgb);
                                //Color font_color_rgb = Color.FromArgb(255, (byte)(rgb.get_c(0)*255),
                                //	(byte)(rgb.get_c(1)*255), (byte)(rgb.get_c(2)*255));

                                result += (String.Format("Font Color(RGB): red={0:d} green={1:d} blue={2:d}\n",
                                    (byte)(rgb.Get(0) * 255),
                                    (byte)(rgb.Get(1) * 255),
                                    (byte)(rgb.Get(2) * 255)));

                                pdftron.Common.DoubleRef x, y;
                                int char_code;

                                for (CharIterator itr = element.GetCharIterator(); itr.HasNext(); itr.Next())
                                {
                                    result += ("Character code: ");
                                    char_code = itr.Current().char_code;
                                    result += (String.Format("{0}\n", (char)char_code));

                                    x = new pdftron.Common.DoubleRef(itr.Current().x);		// character positioning information
                                    y = new pdftron.Common.DoubleRef(itr.Current().y);

                                    // To get the exact character positioning information you need to 
                                    // concatenate current text matrix with CTM and then multiply 
                                    // relative positioning coordinates with the resulting matrix.
                                    //
                                    mtx = Matrix2D.Mult(ctm, text_mtx);
                                    mtx.Mult(x, y);

                                    result += (String.Format(" Position: x={0:f} y={1:f}\n", x.Value, y.Value));
                                }
                            }
                            break;
                        }
                }
            }
            return result;
        }

        int image_counter = 0;

        String ProcessImage(Element image)
        {
            String result = "";
            bool image_mask = image.IsImageMask();
            bool interpolate = image.IsImageInterpolate();
            int width = image.GetImageWidth();
            int height = image.GetImageHeight();
            int out_data_sz = width * height * 3;

            result += (String.Format("\nImage: width=\"{0:d}\" height=\"{1:d}\"\n", width, height));

            // Matrix2D mtx = image.GetCTM(); // image matrix (page positioning info)

            ++image_counter;
            /*
             System.Drawing.Bitmap bmp = image.GetBitmap();
             bmp.Save(output_path + "reader_img_extract_" + image_counter.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
             */

            // Alternatively you can use GetImageData to read the raw (decoded) image data
            // image.GetBitsPerComponent();	
            // image.GetImageData();	// get raw image data
            // another approach is to use Image2RGB filter that converts every image to 
            // RGB format. This could save you time since you don't need to deal with color 
            // conversions, image up-sampling, decoding etc.
            // ----------------
            //   Image2RGB img_conv = new Image2RGB(image);	// Extract and convert image to RGB 8-bpc format
            //   FilterReader reader = new FilterReader(img_conv);			//   
            //   byte[] image_data_out = new byte[out_data_sz];  // A buffer used to keep image data.
            //   reader.Read(image_data_out);  // image_data_out contains RGB image data.
            // ----------------
            // Note that you don't need to read a whole image at a time. Alternatively
            // you can read a chuck at a time by repeatedly calling reader.Read(buf, buf_sz) 
            // until the function returns 0.
            return result;
        }

        String ProcessElements(ElementReader reader)
        {
            String resultMsg = "";
            Element element;
            while ((element = reader.Next()) != null)  // Read page contents
            {
                switch (element.GetType())
                {
                    case ElementType.e_path:          // Process path data...
                        {
                            resultMsg += ProcessPath(reader, element) + "\n";
                            break;
                        }
                    case ElementType.e_text_begin:    // Process text strings...
                        {
                            resultMsg += ProcessText(reader) + "\n";
                            break;
                        }
                    case ElementType.e_form:          // Process form XObjects
                        {
                            reader.FormBegin();
                            resultMsg += ProcessElements(reader) + "\n";
                            reader.End();
                            break;
                        }
                    case ElementType.e_image:         // Process Images
                        {
                            resultMsg += ProcessImage(element) + "\n";
                            break;
                        }
                }
            }
            // Print result msg
            Write(resultMsg);

            return "";
        }
    }
}
