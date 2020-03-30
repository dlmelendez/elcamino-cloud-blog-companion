// Old
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Blob.Protocol;

//-------------------

// New v12
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureBlobSdkBlog
{
    public class Example1
    {
        public async Task BlobPostCodeAsync()
        {
            //Service and Container Basics
            // Old
            // Create CloudBlobClient from the connection string.
            CloudBlobClient blobClient = CloudStorageAccount.Parse("StorageConnectionString")
                                        .CreateCloudBlobClient();

            // Get and create the container for the blobs
            CloudBlobContainer containerOld = blobClient.GetContainerReference("BlobContainerName");
            await containerOld.CreateIfNotExistsAsync();

            //-------------------

            // New v12
            // Create BlobServiceClient from the connection string.
            BlobServiceClient blobServiceClient = new BlobServiceClient("StorageConnectionString");

            // Get and create the container for the blobs
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("BlobContainerName");
            await container.CreateIfNotExistsAsync();

            //Common Blob Operations
            //CloudBlockBlob vs. BlobClient
            //Old CloudBlockBlob

            CloudBlockBlob blobOld = containerOld.GetBlockBlobReference("BlobName");

            //-------------------

            //New v12 BlobClient

            BlobClient blob = container.GetBlobClient("BlobName");

            //

            //Uploading Json Text
            //Old

            string jsonEntityContent = "{ }";

            blobOld = containerOld.GetBlockBlobReference("BlobName");
            blobOld.Properties.ContentType = "application/json";

            await blobOld.UploadTextAsync(jsonEntityContent);
            await blobOld.SetPropertiesAsync();

            //-------------------

            //New v12

            jsonEntityContent = "{ }";

            blob = container.GetBlobClient("BlobName");
            await blob.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(jsonEntityContent)),
                new BlobHttpHeaders()
                {
                    ContentType = "application/json"
                });

            //
        }

        //Download Json Text With Exception Handling
        //Old

        // CloudBlockBlob blobJson = container.GetBlockBlobReference("BlobName");
        // Entity jsonEntity = await GetEntityBlobAsync<Entity>(blobJson);

        public async Task<Entity> GetEntityBlobAsync<Entity>(CloudBlockBlob blobJson)
         where Entity : class, new()
        {
            try
            {
                using (Stream s = await blobJson.OpenReadAsync())
                {
                    using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            return serializer.Deserialize<Entity>(reader);
                        }
                    }
                }
            }
            catch (StorageException storageEx)
                when (storageEx.RequestInformation.ErrorCode
                        == BlobErrorCodeStrings.BlobNotFound)
            {
                return null;
            }
        }

        //-------------------

        //New v12

        //BlobClient blobJson = container.GetBlobClient("BlobName");
        //Entity jsonEntity = await GetEntityBlobAsync<Entity>(blobJson);

        public async Task<Entity> GetEntityBlobAsync<Entity>(BlobClient blobJson)
         where Entity : class, new()
        {
            try
            {
                Response<BlobDownloadInfo> download = await blobJson.DownloadAsync();
                using (Stream s = download.Value.Content)
                {
                    using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            return serializer.Deserialize<Entity>(reader);
                        }
                    }
                }
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                return null;
            }
        }

        //


        //Get All Blobs in a Container
        //Old

        public IEnumerable<CloudBlockBlob> GetAllBlobs(CloudBlobContainer container)
        {
            BlobContinuationToken token = new BlobContinuationToken();
            while (token != null)
            {
                var blobSegment = container.ListBlobsSegmented(string.Empty, true,
                    BlobListingDetails.None, 100,
                    token,
                    new BlobRequestOptions(),
                    new Microsoft.Azure.Storage.OperationContext());

                foreach (var blobItem in blobSegment.Results)
                {
                    CloudBlockBlob blockBlob = blobItem as CloudBlockBlob;
                    if (blockBlob != null)
                    {
                        yield return blockBlob;
                    }
                }
                token = blobSegment.ContinuationToken;
            }
        }

        //-------------------

        //New v12

        public IEnumerable<BlobClient> GetAllBlobs(BlobContainerClient container)
        {
            string token = null;
            do
            {
                Pageable<BlobItem> pageable =
                            container.GetBlobs(BlobTraits.None, BlobStates.None, string.Empty);
                IEnumerable<Page<BlobItem>> pages = pageable.AsPages(token, 100);
                foreach (Page<BlobItem> page in pages)
                {
                    token = page.ContinuationToken;
                    foreach (BlobItem blob in page.Values)
                    {
                        yield return container.GetBlobClient(blob.Name);
                    }
                }
            } while (!string.IsNullOrEmpty(token));
        }

        // -- or --

        public async IAsyncEnumerable<BlobClient> GetAllBlobsAsync(BlobContainerClient container)
        {
            string token = null;
            do
            {
                AsyncPageable<BlobItem> pageable =
                            container.GetBlobsAsync(BlobTraits.None, BlobStates.None, string.Empty);
                IAsyncEnumerable<Page<BlobItem>> pages = pageable.AsPages(token, 100);
                await foreach (Page<BlobItem> page in pages)
                {
                    token = page.ContinuationToken;
                    foreach (BlobItem blob in page.Values)
                    {
                        yield return container.GetBlobClient(blob.Name);
                    }
                }
            } while (!string.IsNullOrEmpty(token));
        }

        //

    }
}

