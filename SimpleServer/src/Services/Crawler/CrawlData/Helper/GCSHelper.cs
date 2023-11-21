using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Serilog;

namespace CrawlData.Helper
{
    public static class GCSHelper
    {
        public static string UploadFile(string filePath, string objectName)
        {
            // set google credential
            GoogleCredential credential = GoogleCredential.FromFile(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"));
            var storage = StorageClient.Create(credential);
            Stream fileStream;
            // check if file path is url
            if (filePath.StartsWith("http"))
            {
                // read stream from url
                var HttpClient = new HttpClient();
                try
                {
                    fileStream = HttpClient.GetStreamAsync(filePath).Result;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error when get stream from url {filePath}: {ex.Message}");
                    // retry to get stream from url
                    fileStream = HttpClient.GetStreamAsync(filePath).Result;
                }
            }
            else
            {
                // read stream from local file
                fileStream = File.OpenRead(filePath);
            }
            storage.UploadObject(Consts.BUCKET_NAME, objectName, null, fileStream);
            Console.WriteLine($"Uploaded {objectName}.");
            return MakePublic(objectName);
        }

        public static string UploadFile(Stream content, string objectName)
        {
            var storage = StorageClient.Create();
            storage.UploadObject(Consts.BUCKET_NAME, objectName, null, content);
            Console.WriteLine($"Uploaded {objectName}.");
            return MakePublic(objectName);
        }


        public static string MakePublic(string objectName = "your-object-name")
        {
            var storage = StorageClient.Create();
            var storageObject = storage.GetObject(Consts.BUCKET_NAME, objectName);
            storageObject.Acl ??= new List<ObjectAccessControl>();
            storage.UpdateObject(storageObject, new UpdateObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead });
            Console.WriteLine(objectName + " is now public and can be fetched from " + storageObject.SelfLink);

            return storageObject.SelfLink;
        }

        public static bool IsFileExist(string objectName = "your-object-name")
        {
            var storage = StorageClient.Create();
            try
            {
                storage.GetObject(Consts.BUCKET_NAME, objectName);
                return true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
