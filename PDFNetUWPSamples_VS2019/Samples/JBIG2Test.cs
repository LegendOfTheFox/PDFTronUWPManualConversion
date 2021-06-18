//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.PDF;
using pdftron.SDF;

using pdftron.Common;
using PDFNetUniversalSamples.ViewModels;
using pdftron.Filters;
using Windows.Storage.Streams;

namespace PDFNetSamples
{
    /// <summary>
    /// This sample project illustrates how to recompress bi-tonal images in an 
    /// existing PDF document using JBIG2 compression. The sample is not intended 
    /// to be a generic PDF optimization tool.
    ///
    /// Also a sample page compressed using CCITT Fax compression is located under 
    /// 'PDFNet/Samples/TestFiles' folder.
    /// </summary>
    class JBIG2Test : Sample
    {
        private string FILE_NAME = "US061222892-a.pdf";

        public JBIG2Test() :
            base("JBIG2", "This sample recompress bi-tonal images in an existing PDF document using JBIG2 compression.")
        { }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () =>
           {
               WriteLine("--------------------------------");
               WriteLine("Starting JBIG2 Compression Test...");
               WriteLine("--------------------------------\n");

               PDFDoc pdf_doc = new PDFDoc(Path.Combine(InputPath, FILE_NAME));
               pdf_doc.InitSecurityHandler();

               SDFDoc cos_doc = pdf_doc.GetSDFDoc();

               int num_objs = cos_doc.XRefSize();

               // Loop through all cross reference table objects
               for (int i = 1; i < num_objs; ++i)
               {
                   Obj obj = cos_doc.GetObj(i);
                   if (obj != null && !obj.IsFree() && obj.IsStream())
                   {
                       // Process only images
                       DictIterator itr = obj.Find("Subtype");
                       if (!itr.HasNext() || itr.Value().GetName() != "Image")
                           continue;

                       pdftron.PDF.Image input_image = new pdftron.PDF.Image(obj);
                       pdftron.PDF.Image new_image = null;

                       // Process only gray-scale images
                       if (input_image.GetComponentNum() != 1)
                           continue;

                       int bpc = input_image.GetBitsPerComponent();
                       if (bpc != 1) // Recompress 1 BPC images
                           continue;

                       // Skip images that are already compressed using JBIG2
                       itr = obj.Find("Filter");
                       if (itr.HasNext() && itr.Value().IsName() &&
                           itr.Value().GetName() == "JBIG2Decode")
                           continue;

                       FilterReader reader = new FilterReader(obj.GetDecodedStream());

                       ObjSet hint_set = new ObjSet();

                       Obj hint = hint_set.CreateArray();
                       hint.PushBackName("JBIG2");
                       hint.PushBackName("Lossless");
                       hint.PushBackName("Threshold");
                       hint.PushBackNumber(0.4);
                       hint.PushBackName("SharePages");
                       hint.PushBackNumber(10000);

                       new_image = pdftron.PDF.Image.Create(
                           cos_doc,
                           reader,
                           input_image.GetImageWidth(),
                           input_image.GetImageHeight(),
                           1,
                           ColorSpace.CreateDeviceGray(),
                           hint);

                       Obj new_img_obj = new_image.GetSDFObj();

                       // Copy any important entries from the image dictionary
                       itr = obj.Find("ImageMask");
                       if (itr.HasNext()) new_img_obj.Put("ImageMask", itr.Value());

                       itr = obj.Find("Mask");
                       if (itr.HasNext()) new_img_obj.Put("Mask", itr.Value());

                       cos_doc.Swap(i, new_image.GetSDFObj().GetObjNum());
                   }
               }

               await pdf_doc.SaveAsync(Path.Combine(OutputPath, FILE_NAME), SDFDocSaveOptions.e_remove_unused).AsTask().ConfigureAwait(false);

               WriteLine("Saved " + Path.Combine(OutputPath, FILE_NAME));

               await AddFileToOutputList(Path.Combine(OutputPath, FILE_NAME));

               WriteLine("--------------------------------");
               WriteLine("Done JBIG2 Compression Test.");
               WriteLine("--------------------------------\n");

           })).AsAsyncAction();
        }
    }
}
