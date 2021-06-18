//
// Copyright (c) 2001-2021 by PDFTron Systems Inc. All Rights Reserved.
//

// Uncomment this line if WinRTBouncyCastle is added as project reference.
// #define USE_BOUNCYCASTLE

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

using pdftron.PDF;
using pdftron.PDF.Annots;
using pdftron.SDF;

using Buffer = Windows.Storage.Streams.Buffer;
using Convert = System.Convert;
using PDFNetUniversalSamples.ViewModels;

namespace PDFNetSamples
{    
    public class DigitalSignaturesTest : Sample
    {
        public DigitalSignaturesTest() :
            base("DigitalSignatures", "Demonstrates the basic usage of high-level digital signature API to digitally sign and/or certify PDF documents.")
        {
        }

        public override IAsyncAction RunAsync()
        {
            return Task.Run(new System.Action(async () => {
                WriteLine("--------------------------------");
                WriteLine("Starting DigitalSignatures Test...");
                WriteLine("--------------------------------\n");
                
#if !USE_BOUNCYCASTLE
                WriteLine("No crypto library used. Please try adding WinRTBouncyCastle as project refrence and define USE_BOUNCYCASTLE symbol.");
                WriteLine("Download Bouncy Castle at: http://w8bouncycastle.codeplex.com/");
                WriteLine("Sample will use a fake signature to sign the document instead.");
#endif // USE_BOUNCYCASTLE
                try
                {
                    bool error = false;

                    //////////////////// TEST 0: 
                    /* Create an approval signature field that we can sign after certifying.
                    (Must be done before calling CertifyOnNextSave/SignOnNextSave/WithCustomHandler.) */
                    try
                    {
                        PDFDoc document = new PDFDoc(Path.Combine(InputPath, "tiger.pdf"));
                        DigitalSignatureField approvalSignatureField = document.CreateDigitalSignatureField("PDFTronApprovalSig");
                        SignatureWidget widgetAnnotApproval = SignatureWidget.Create(document, new pdftron.PDF.Rect(300, 300, 500, 200), approvalSignatureField);
                        Page page1 = document.GetPage(1);
                        page1.AnnotPushBack(widgetAnnotApproval);
                        await document.SaveAsync(Path.Combine(OutputPath, "tiger_withApprovalField_output.pdf"), SDFDocSaveOptions.e_remove_unused);
                        await AddFileToOutputList(Path.Combine(OutputPath, "tiger_withApprovalField_output.pdf")).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        WriteLine(GetExceptionMessage(e));
                        error = true;
                    }

                    //////////////////// TEST 1: certify a PDF.
                    try
                    {
                        await CertifyPDF(Path.Combine(InputPath, "tiger_withApprovalField.pdf"),
                            "PDFTronCertificationSig",
                            Path.Combine(InputPath, "pdftron.pfx"),
                            "password",
                            Path.Combine(InputPath, "pdftron.bmp"),
                            Path.Combine(OutputPath, "tiger_withApprovalField_certified_output.pdf"));

                        PrintSignaturesInfo(Path.Combine(OutputPath, "tiger_withApprovalField_certified_output.pdf"));
                        
                    }
                    catch (Exception e)
                    {
                        WriteLine(GetExceptionMessage(e));
                        error = true;
                    }

                    //////////////////// TEST 2: sign a PDF with a certification and an unsigned signature field in it.
                    try
                    {
                        await SignPDF(Path.Combine(InputPath, "tiger_withApprovalField_certified.pdf"),
                            "PDFTronApprovalSig",
                            Path.Combine(InputPath, "pdftron.pfx"),
                            "password",
                            Path.Combine(InputPath, "signature.jpg"),
                            Path.Combine(OutputPath, "tiger_withApprovalField_certified_approved_output.pdf"));

                        PrintSignaturesInfo(Path.Combine(OutputPath, "tiger_withApprovalField_certified_approved_output.pdf"));
                    }
                    catch (Exception e)
                    {
                        WriteLine(GetExceptionMessage(e));
                        error = true;
                    }

                    //////////////////// TEST 3: Clear a certification from a document that is certified and has two approval signatures.
                    try
                    {
                        await ClearSignature(Path.Combine(InputPath, "tiger_withApprovalField_certified_approved.pdf"),
                            "PDFTronCertificationSig",
                            Path.Combine(OutputPath, "tiger_withApprovalField_certified_approved_certcleared_output.pdf"));

                        PrintSignaturesInfo(Path.Combine(OutputPath, "tiger_withApprovalField_certified_approved_certcleared_output.pdf"));
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
                }
                catch (Exception e)
                {
                    WriteLine(GetExceptionMessage(e));
                }
                
                WriteLine("\n--------------------------------");
                WriteLine("Done DigitalSignatures Test.");
                WriteLine("--------------------------------\n");
            })).AsAsyncAction();
        }

        private async Task SignPDF(string inputDocumentPath,
            string inputApprovalFieldName,
            string inputPrivateKeyFilePath,
            string inputKeyFilePassword,
            string inputAppearanceImagePath,
            string inputOutPath)
        {
            WriteLine("================================================================================");
            WriteLine("Signing PDF document");

            // Open an existing PDF
            PDFDoc document = new PDFDoc(inputDocumentPath);

            // Sign the approval signatures.
            Field foundApprovalField = document.GetField(inputApprovalFieldName);
            DigitalSignatureField foundApprovalSignatureDigitalSignatureField = new DigitalSignatureField(foundApprovalField);
            Image image = Image.Create(document.GetSDFDoc(), inputAppearanceImagePath);
            SignatureWidget foundApprovalSignatureWidget = new SignatureWidget(foundApprovalField.GetSDFObj());
            foundApprovalSignatureWidget.CreateSignatureAppearance(image);

#if USE_BOUNCYCASTLE

                // Create a new instance of the SignatureHandler.
                MySignatureHandler signatureHandler = new MySignatureHandler();
                byte[] signatureData = signatureHandler.CreateSignature();
                signatureHandler.AppendData(signatureData);

                // Add the SignatureHandler instance to PDFDoc, making sure to keep track of it using the ID returned.
                var sigHandlerId = document.AddSignatureHandler(signatureHandler);
                foundApprovalSignatureDigitalSignatureField.SignOnNextSaveWithCustomHandler(sigHandlerId);
#else
            foundApprovalSignatureDigitalSignatureField.SignOnNextSave(inputPrivateKeyFilePath, inputKeyFilePassword);
#endif

            await document.SaveAsync(inputOutPath, SDFDocSaveOptions.e_incremental);
            WriteLine("Done. Result saved in " + inputOutPath);
            await AddFileToOutputList(inputOutPath).ConfigureAwait(false);
            WriteLine("================================================================================");
        }

        private async Task CertifyPDF(string inputDocumentPath,
            string inputCertificateFieldName,
            string inputPrivateKeyFilePath,
            string inputKeyFilePassword,
            string inputAppearanceImagePath,
            string inputOutPath)
        {
            WriteLine("================================================================================");
            WriteLine("Certifying PDF document");

            // Open an existing PDF
            PDFDoc document = new PDFDoc(inputDocumentPath);
            WriteLine("PDFDoc has " + (document.HasSignatures() ? "signatures" : "no signatures"));

            Page page1 = document.GetPage(1);

            // Create a random text field that we can lock using the field permissions feature.
            TextWidget annotation = TextWidget.Create(document, new pdftron.PDF.Rect(50, 550, 350, 600), "asdf_test_field");
            page1.AnnotPushBack(annotation);

            /* Create new signature form field in the PDFDoc. The name argument is optional;
            leaving it empty causes it to be auto-generated. However, you may need the name for later.
            Acrobat doesn't show digsigfield in side panel if it's without a widget. Using a
            Rect with 0 width and 0 height, or setting the NoPrint/Invisible flags makes it invisible. */
            DigitalSignatureField certificationSignatureField = document.CreateDigitalSignatureField(inputCertificateFieldName);
            SignatureWidget widgetAnnot = SignatureWidget.Create(document, new pdftron.PDF.Rect(0, 100, 200, 150), certificationSignatureField);
            page1.AnnotPushBack(widgetAnnot);

            // (OPTIONAL) Add an appearance.

            // Widget AP from image
            Image image = Image.Create(document.GetSDFDoc(), inputAppearanceImagePath);
            widgetAnnot.CreateSignatureAppearance(image);
            // End of optional appearance-adding code.

            // Add permissions. Lock the random text field.
            WriteLine("Adding document permissions.");
            certificationSignatureField.SetDocumentPermissions(DigitalSignatureFieldDocumentPermissions.e_annotating_formfilling_signing_allowed);
            WriteLine("Adding field permissions.");
            string[] fieldsToLock = new string[1];
            fieldsToLock[0] = "asdf_test_field";
            certificationSignatureField.SetFieldPermissions(DigitalSignatureFieldFieldPermissions.e_include, fieldsToLock);

#if USE_BOUNCYCASTLE

                // Create a new instance of the SignatureHandler.
                MySignatureHandler signatureHandler = new MySignatureHandler();
                byte[] signatureData = signatureHandler.CreateSignature();
                signatureHandler.AppendData(signatureData);

                // Add the SignatureHandler instance to PDFDoc, making sure to keep track of it using the ID returned.
                var sigHandlerId = document.AddSignatureHandler(signatureHandler);
                foundApprovalSignatureDigitalSignatureField.SignOnNextSaveWithCustomHandler(sigHandlerId);
#else
            certificationSignatureField.CertifyOnNextSave(inputPrivateKeyFilePath, inputKeyFilePassword);
#endif

            ///// (OPTIONAL) Add more information to the signature dictionary.
            certificationSignatureField.SetLocation("Vancouver, BC");
            certificationSignatureField.SetReason("Document certification.");
            certificationSignatureField.SetContactInfo("www.pdftron.com");
            ///// End of optional sig info code.

            // Save the PDFDoc. Once the method below is called, PDFNetC will also sign the document using the information provided.
            await document.SaveAsync(inputOutPath, 0);
            WriteLine("Done. Result saved in " + inputOutPath);
            await AddFileToOutputList(inputOutPath).ConfigureAwait(false);
            WriteLine("================================================================================");
        }

        private void PrintSignaturesInfo(string inputDocumentPath)
        {
            try
            {
                WriteLine("================================================================================");
                WriteLine("Reading and printing digital signature information");

                PDFDoc document = new PDFDoc(inputDocumentPath);

                if (!document.HasSignatures())
                {
                    WriteLine("Doc has no signatures.");
                    WriteLine("================================================================================");
                    return;
                }
                else
                {
                    WriteLine("Doc has signatures.");
                }


                for (FieldIterator fieldIterator = document.GetFieldIterator(); fieldIterator.HasNext(); fieldIterator.Next())
                {
                    if (fieldIterator.Current().IsLockedByDigitalSignature())
                    {
                        WriteLine("==========\nField locked by a digital signature");
                    }
                    else
                    {
                        WriteLine("==========\nField not locked by a digital signature");
                    }

                    WriteLine("Field name: " + fieldIterator.Current().GetName());
                    WriteLine("==========");
                }

                WriteLine("====================\nNow iterating over digital signatures only.\n====================");

                DigitalSignatureFieldIterator digitalSignatureFieldIterator = document.GetDigitalSignatureFieldIterator();
                for (; digitalSignatureFieldIterator.HasNext(); digitalSignatureFieldIterator.Next())
                {
                    WriteLine("==========");
                    WriteLine("Field name of digital signature: " + new Field(digitalSignatureFieldIterator.Current().GetSDFObj()).GetName());

                    DigitalSignatureField digitalSignatureField = digitalSignatureFieldIterator.Current();
                    if (!digitalSignatureField.HasCryptographicSignature())
                    {
                        WriteLine("Either digital signature field lacks a digital signature dictionary, " +
                            "or digital signature dictionary lacks a cryptographic Contents entry. " +
                            "Digital signature field is not presently considered signed.\n" +
                            "==========");
                        continue;
                    }

                    int certificateCount = Convert.ToInt32(digitalSignatureField.GetCertCount());
                    WriteLine("Cert count: " + certificateCount);
                    for (int i = 0; i < certificateCount; ++i)
                    {
                        byte[] certificateByteArray = digitalSignatureField.GetCert(Convert.ToUInt32(i));
                        WriteLine("Cert #" + i + " size: " + certificateByteArray.Length);
                    }

                    DigitalSignatureFieldSubFilterType subFilter = digitalSignatureField.GetSubFilter();

                    WriteLine("Subfilter type: " + Convert.ToInt32(subFilter));

                    if (subFilter != DigitalSignatureFieldSubFilterType.e_ETSI_RFC3161)
                    {
                        WriteLine("Signature's signer: " + digitalSignatureField.GetSignatureName());

                        Date signingTime = digitalSignatureField.GetSigningTime();
                        if (signingTime.IsValid())
                        {
                            WriteLine("Signing day: " + Convert.ToInt32(signingTime.day));
                        }

                        WriteLine("Location: " + digitalSignatureField.GetLocation());
                        WriteLine("Reason: " + digitalSignatureField.GetReason());
                        WriteLine("Contact info: " + digitalSignatureField.GetContactInfo());
                    }
                    else
                    {
                        WriteLine("SubFilter == e_ETSI_RFC3161 (DocTimeStamp; no signing info)\n");
                    }

                    WriteLine(((digitalSignatureField.HasVisibleAppearance()) ? "Visible" : "Not visible"));

                    DigitalSignatureFieldDocumentPermissions digitalSignatureDocumentPermissions = digitalSignatureField.GetDocumentPermissions();

                    var lockedFields = digitalSignatureField.GetLockedFields();
                    if (lockedFields != null)
                    {
                        foreach (string fieldName in lockedFields)
                        {
                            WriteLine("This digital signature locks a field named: " + fieldName);
                        }
                    }

                    switch (digitalSignatureDocumentPermissions)
                    {
                        case DigitalSignatureFieldDocumentPermissions.e_no_changes_allowed:
                            WriteLine("No changes to the document can be made without invalidating this digital signature.");
                            break;
                        case DigitalSignatureFieldDocumentPermissions.e_formfilling_signing_allowed:
                            WriteLine("Page template instantiation, form filling, and signing digital signatures are allowed without invalidating this digital signature.");
                            break;
                        case DigitalSignatureFieldDocumentPermissions.e_annotating_formfilling_signing_allowed:
                            WriteLine("Annotating, page template instantiation, form filling, and signing digital signatures are allowed without invalidating this digital signature.");
                            break;
                        case DigitalSignatureFieldDocumentPermissions.e_unrestricted:
                            WriteLine("Document not restricted by this digital signature.");
                            break;
                        default:
                            throw new Exception("Unrecognized digital signature document permission level.");
                    }
                    WriteLine("==========");
                }

                WriteLine("================================================================================");
            }
            catch (Exception e)
            {
                WriteLine(GetExceptionMessage(e));
            }
        }

        private async Task ClearSignature(string inputDocumentPath,
            string inputDigitalSignatureFieldName,
            string inputOutPath)
        {
            WriteLine("================================================================================");
            WriteLine("Clearing certification signature");

            PDFDoc document = new PDFDoc(inputDocumentPath);

            DigitalSignatureField digitalSignatureField = new DigitalSignatureField(document.GetField(inputDigitalSignatureFieldName));

            WriteLine("Clearing signature: " + inputDigitalSignatureFieldName);
            digitalSignatureField.ClearSignature();

            if (!digitalSignatureField.HasCryptographicSignature())
            {
                WriteLine("Cryptographic signature cleared properly.");
            }

            // Save incrementally so as to not invalidate other signatures from previous saves.
            await document.SaveAsync(inputOutPath, SDFDocSaveOptions.e_incremental);
            WriteLine("Done. Result saved in " + inputOutPath);
            await AddFileToOutputList(inputOutPath).ConfigureAwait(false);
            WriteLine("================================================================================");
        }
    }    

    class MySignatureHandler : ISignatureHandler
    {
        private List<byte> m_data = null;

        public MySignatureHandler()
        {
            m_data = new List<byte>();
        }

        public void AppendData(byte[] data)
        {
            m_data.AddRange(data);
        }

        public byte[] CreateSignature()
        {
#if USE_BOUNCYCASTLE
            var ms = new MemoryStream(Convert.FromBase64String(m_pkcs12));
            var p12Store = new Org.BouncyCastle.Pkcs.Pkcs12Store(ms, "password".ToCharArray());
            String mainAlias = String.Empty;
            foreach (var alias in p12Store.Aliases) {
                mainAlias = alias.ToString();
                break;
            }        
            var akp = p12Store.GetKey(mainAlias).Key;
            var x509Cert = p12Store.GetCertificate(mainAlias).Certificate;
            var certs = new ArrayList();
            certs.Add(x509Cert);
            var certStore = Org.BouncyCastle.X509.Store.X509StoreFactory.Create("CERTIFICATE/Collection", new Org.BouncyCastle.X509.Store.X509CollectionStoreParameters(certs));
            
            var sigGen = new Org.BouncyCastle.Cms.CmsSignedDataGenerator();
            sigGen.AddSigner(akp, x509Cert, Org.BouncyCastle.Asn1.Pkcs.PkcsObjectIdentifiers.Sha1WithRsaEncryption.Id);
            sigGen.AddCertificates(certStore);
            var data = new Org.BouncyCastle.Cms.CmsProcessableByteArray(m_data.ToArray());
            var signedData = sigGen.Generate(data, false);
            return (signedData.ContentInfo.GetDerEncoded());
#else // USE_BOUNCYCASTLE
            return (new byte[] { 0x00, 0x00, 0x00, 0x00 });
#endif // USE_BOUNCYCASTLE
        }

        public String GetName()
        {
            return "Adobe.PPKLite";
        }

        public bool Reset()
        {
            m_data.Clear();
            return (true);
        }
        
#if USE_BOUNCYCASTLE
        private readonly String m_pkcs12 = @"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";
#endif // USE_BOUNCYCASTLE
    }
}
