// Services/AzureService.cs
using System.Text;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using myapp_back.azure.interfaces;
using myapp_back.config;

namespace myapp_back.azure.services
{
    public class AzureService : IAzureService
    {
        private readonly AzureOptions _azureOptions;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureService(IOptions<AzureOptions> azureOptions, BlobServiceClient blobServiceClient)
        {
            _azureOptions = azureOptions.Value;
            _blobServiceClient = blobServiceClient;

        }

        public async Task<string> GenerateUploadSasUriAsync(string filename)
        {
            try
            {
                string containerName = _azureOptions.ContainerName;

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(filename);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = filename,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Create | BlobSasPermissions.Write);
                var sasUri = blobClient.GenerateSasUri(sasBuilder);


                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error generating SAS URI: " + ex.Message);
                throw;
            }
        }

        public async Task MergeChunksAsync(string fileId, int totalChunks, string outputFileName)
        {
           
            var sourceContainerClient = _blobServiceClient.GetBlobContainerClient(_azureOptions.ContainerName);
            await sourceContainerClient.CreateIfNotExistsAsync();

           
            var uploadsContainerClient = _blobServiceClient.GetBlobContainerClient("uploads");
            await uploadsContainerClient.CreateIfNotExistsAsync();

            var outputBlobClient = uploadsContainerClient.GetBlockBlobClient(outputFileName);
            var blockIds = new List<string>();

            for (int i = 0; i < totalChunks; i++)
            {
                var chunkBlobName = $"{fileId}_part{i}";
                var chunkBlobClient = sourceContainerClient.GetBlobClient(chunkBlobName);

                if (await chunkBlobClient.ExistsAsync())
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"block-{i:D6}"));
                    using var chunkStream = await chunkBlobClient.OpenReadAsync();

                    await outputBlobClient.StageBlockAsync(
                        base64BlockId: blockId,
                        content: chunkStream);

                    blockIds.Add(blockId);
                }
                else
                {
                    throw new Exception($"Chunk {chunkBlobName} not found in blob storage.");
                }
            }

            await outputBlobClient.CommitBlockListAsync(blockIds);

            await outputBlobClient.SetHttpHeadersAsync(new BlobHttpHeaders
            {
                ContentType = "video/mp4"
            });

            Console.WriteLine($"Successfully merged chunks into uploads/{outputFileName}");
        }

    }
}