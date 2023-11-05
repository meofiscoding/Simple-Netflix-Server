using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace CrawlData.Helper
{
    public static class GCSHelper
    {
        public static string UploadFile(string bucketName, string filePath, string objectName)
        {
            var storage = StorageClient.Create();
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
                    throw new Exception($"Error when get stream from url {filePath}", ex);
                }
            }
            else
            {
                // read stream from local file
                fileStream = File.OpenRead(filePath);
            }
            storage.UploadObject(bucketName, objectName, null, fileStream);
            Console.WriteLine($"Uploaded {objectName}.");
            return MakePublic(bucketName, objectName);
        }

        public static string UploadFile(string bucketName, Stream content, string objectName)
        {
            var storage = StorageClient.Create();
            storage.UploadObject(bucketName, objectName, null, content);
            Console.WriteLine($"Uploaded {objectName}.");
            return MakePublic(bucketName, objectName);
        }


        public static string MakePublic(string bucketName = "your-unique-bucket-name", string objectName = "your-object-name")
        {
            var storage = StorageClient.Create();
            var storageObject = storage.GetObject(bucketName, objectName);
            storageObject.Acl ??= new List<ObjectAccessControl>();
            storage.UpdateObject(storageObject, new UpdateObjectOptions { PredefinedAcl = PredefinedObjectAcl.PublicRead });
            Console.WriteLine(objectName + " is now public and can be fetched from " + storageObject.SelfLink);

            return storageObject.SelfLink;
        }
    }
}
