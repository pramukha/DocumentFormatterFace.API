using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using DocFormatterFace.API.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocFormatterFace.API.Services
{
    public class IdScannerService : IIdScannerService
    {
        private readonly string endpoint;
        private readonly string apiKey;
        private readonly FormRecognizerClient formRecognizerClient;
        IFaceClient faceClient;
        const string RECOGNITION_MODEL4 = RecognitionModel.Recognition04;

        private readonly string _endpoint;
        private readonly string _subscriptionKey;


        public IdScannerService(IConfiguration configuration)
        {
            this.apiKey = configuration["AzureIdScannerAPI:SUBSCRIPTION_KEY"];
            this.endpoint = configuration["AzureIdScannerAPI:ENDPOINT"];
            var credential = new AzureKeyCredential(apiKey);
            this.formRecognizerClient = new FormRecognizerClient(new Uri(endpoint), credential);

            _endpoint = configuration["AzureFaceAPI:ENDPOINT"];
            _subscriptionKey = configuration["AzureFaceAPI:SUBSCRIPTION_KEY"];

            faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_subscriptionKey)) { Endpoint = _endpoint };
        }

        public async Task<IdentityDoc> ScanIdentityDoc(string imageString)
        {
            var bytes = Convert.FromBase64String(imageString);
            IdentityDoc doc = new IdentityDoc();
            RecognizeIdentityDocumentsOperation operation = await this.formRecognizerClient.StartRecognizeIdentityDocumentsAsync(new MemoryStream(bytes));
            Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
            RecognizedFormCollection identityDocuments = operationResponse.Value;

            
            RecognizedForm identityDocument = identityDocuments.Single();

            if (identityDocument.Fields.TryGetValue("MachineReadableZone", out FormField MachineReadableZone))
            {
                
                if (MachineReadableZone.Value.ValueType == FieldValueType.Dictionary)
                {
                    var address = MachineReadableZone.Value.AsDictionary();

                    doc = ConvertDictionaryTo<IdentityDoc>(address);
                    doc.MRZ = MachineReadableZone.ValueData.Text;
                    doc.Confidence = MachineReadableZone.Confidence;
                }
            }

            return doc;
        }

        private static T ConvertDictionaryTo<T>(IReadOnlyDictionary<string, FormField> dictionary) where T : new()
        {
            Type type = typeof(T);
            T ret = new T();

            foreach (var keyValue in dictionary)
            {
                type.GetProperty(keyValue.Key).SetValue(ret, getformFieldValue(keyValue.Value), null);
            }

            return ret;
        }

        private static string getformFieldValue(FormField filed)
        {

            if (filed.Value.ValueType == FieldValueType.String)
            {
                return filed.Value.AsString();
            }

            if (filed.Value.ValueType == FieldValueType.CountryRegion)
            {
                return filed.Value.AsCountryRegion().ToString();
            }

            if (filed.Value.ValueType == FieldValueType.Date)
            {
                DateTime dateValue = filed.Value.AsDate();
                return dateValue.ToShortDateString();
            }


            return filed.Value.AsString();
        }

 
        public async Task<ScanResponse> Verify(ScanRequestAzure req)
        {
            //Console.WriteLine("========VERIFY========");
            //Console.WriteLine();

            //List<string> targetImageFileNames = new List<string> { "pp3.png",
            //                "pp3testPhotos.png",
            //                "test1-Photos.png", "test1PP-Photos.png"
            //};
            //string sourceImageFileName1 = "pp3testPhotos.png";
            //string sourceImageFileName2 = "test1-Photos.png";


            //List<Guid> targetFaceIds = new List<Guid>();
            //foreach (var imageFileName in targetImageFileNames)
            //{
            //    // Detect faces from target image url.
            //    List<DetectedFace> detectedFaces = await DetectFaceRecognize(client, $"{url}{imageFileName} ", recognitionModel03);
            //    targetFaceIds.Add(detectedFaces[0].FaceId.Value);
            //    Console.WriteLine($"{detectedFaces.Count} faces detected from image `{imageFileName}`.");
            //}

            // Detect faces from source image file 1.
            List<DetectedFace> detectedFaces1 = await DetectFaceRecognize(faceClient, req.FaceImage, RECOGNITION_MODEL4);
            // Console.WriteLine($"{detectedFaces1.Count} faces detected from image `{sourceImageFileName1}`.");
            Guid sourceFaceId1 = detectedFaces1[0].FaceId.Value;

            // Detect faces from source image file 2.
            List<DetectedFace> detectedFaces2 = await DetectFaceRecognize(faceClient, req.PPImage, RECOGNITION_MODEL4);
            //Console.WriteLine($"{detectedFaces2.Count} faces detected from image `{sourceImageFileName2}`.");
            Guid sourceFaceId2 = detectedFaces2[0].FaceId.Value;

            // Verification example for faces of the same person.
            VerifyResult verifyResult1 = await faceClient.Face.VerifyFaceToFaceAsync(sourceFaceId1, sourceFaceId2);

            return new ScanResponse { Confidence = verifyResult1.Confidence, IsIdentical = verifyResult1.IsIdentical };
            //Console.WriteLine(
            //    verifyResult1.IsIdentical
            //        ? $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of the same (Positive) person, similarity confidence: {verifyResult1.Confidence}."
            //        : $"Faces from {sourceImageFileName1} & {targetImageFileNames[0]} are of different (Negative) persons, similarity confidence: {verifyResult1.Confidence}.");

            //// Verification example for faces of different persons.
            //VerifyResult verifyResult2 = await client.Face.VerifyFaceToFaceAsync(sourceFaceId2, targetFaceIds[0]);
            //Console.WriteLine(
            //    verifyResult2.IsIdentical
            //        ? $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of the same (Negative) person, similarity confidence: {verifyResult2.Confidence}."
            //        : $"Faces from {sourceImageFileName2} & {targetImageFileNames[0]} are of different (Positive) persons, similarity confidence: {verifyResult2.Confidence}.");

            //Console.WriteLine();
        }

        private static async Task<List<DetectedFace>> DetectFaceRecognize(IFaceClient faceClient, string imageString, string recognition_model)
        {
            // Detect faces from image URL. Since only recognizing, use the recognition model 1.
            // We use detection model 3 because we are not retrieving attributes.
            var bytes = Convert.FromBase64String(imageString);
            //System.IO.Stream stream = new StreamContent(new MemoryStream(bytes));

            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(new MemoryStream(bytes), true, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03);
            //Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{Path.GetFileName(url)}`");
            return detectedFaces.ToList();
        }
    }
}
