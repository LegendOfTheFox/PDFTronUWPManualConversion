//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

using pdftron.PDF;
using pdftron.SDF;

using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{
    public sealed class OptimizerTest : Sample
    {
        public OptimizerTest() :
            base("Optimizer", "The sample shows how to use 'pdftron.PDF.Optimizer' to reduce PDF file size by reducing the file size, removing redundant information, and compressing data streams using the latest in image compression technology. 'pdftron.PDF.Optimizer' is an optional Add-On to PDFNet Core SDK.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting Optimizer Test...");
                WriteLine("--------------------------------\n");
                //--------------------------------------------------------------------------------
			    // Example 1) Simple optimization of a pdf with default settings. 
			    //
			    try
			    {
                    using (PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "newsletter.pdf")))
                    {
                        doc.InitSecurityHandler();
                        Optimizer.Optimize(doc);
                        String output_file_path = Path.Combine(OutputPath, "newsletter_opt1.pdf");
                        WriteLine("Saving " + output_file_path);
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }
				
			    //--------------------------------------------------------------------------------
			    // Example 2) Reduce image quality and use jpeg compression for
			    // non monochrome images.
			    try
			    {
                    using (PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "newsletter.pdf")))
                    {
                        doc.InitSecurityHandler();

                        OptimizerImageSettings image_settings = new OptimizerImageSettings();

                        // low quality jpeg compression
                        image_settings.SetCompressionMode(OptimizerImageSettingsCompressionMode.e_jpeg);
                        image_settings.SetQuality(1);

                        // Set the output dpi to be standard screen resolution
                        image_settings.SetImageDPI(144, 96);

                        // this option will recompress images not compressed with
                        // jpeg compression and use the result if the new image
                        // is smaller.
                        image_settings.ForceRecompression(true);

                        // this option is not commonly used since it can 
                        // potentially lead to larger files.  It should be enabled
                        // only if the output compression specified should be applied
                        // to every image of a given type regardless of the output image size
                        //image_settings.ForceChanges(true);

                        // use the same settings for both color and grayscale images
                        OptimizerOptimizerSettings opt_settings = new OptimizerOptimizerSettings();
                        opt_settings.SetColorImageSettings(image_settings);
                        opt_settings.SetGrayscaleImageSettings(image_settings);


                        Optimizer.Optimize(doc, opt_settings);

                        String output_file_path = Path.Combine(OutputPath, "newsletter_opt2.pdf");
                        WriteLine("Saving " + output_file_path);
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
			    }

			    //--------------------------------------------------------------------------------
			    // Example 3) Use monochrome image settings and default settings
			    // for color and grayscale images. 
			    try
			    {
                    using (PDFDoc doc = new PDFDoc(Path.Combine(InputPath, "newsletter.pdf")))
                    {
                        doc.InitSecurityHandler();

                        OptimizerMonoImageSettings mono_image_settings = new OptimizerMonoImageSettings();

                        mono_image_settings.SetCompressionMode(OptimizerMonoImageSettingsCompressionMode.e_jbig2);
                        mono_image_settings.ForceRecompression(true);

                        OptimizerOptimizerSettings opt_settings = new OptimizerOptimizerSettings();
                        opt_settings.SetMonoImageSettings(mono_image_settings);

                        Optimizer.Optimize(doc, opt_settings);

                        String output_file_path = Path.Combine(OutputPath, "newsletter_opt3.pdf");
                        WriteLine("Saving " + output_file_path);
                        await doc.SaveAsync(output_file_path, SDFDocSaveOptions.e_linearized);
                        WriteLine("Done. Results saved in " + output_file_path);
                        await AddFileToOutputList(output_file_path).ConfigureAwait(false);
                    }
			    }
			    catch (Exception e)
			    {
				    WriteLine(GetExceptionMessage(e));
                }

                WriteLine("\n--------------------------------");
                WriteLine("Done Optimizer Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
		}
	}
}
