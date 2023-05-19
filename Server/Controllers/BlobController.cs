using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class BlobController : ControllerBase {
        public async void BlobSampleCode() {
            await Console.Out.WriteLineAsync("Load Secret Key...");
            string keyVaultUrl = "https://azureblobsecret.vault.azure.net/";
            var secretClient = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());
            string secretName = "StorageAccountKey";
            KeyVaultSecret secret = secretClient.GetSecret(secretName);
            var storageAccountKey = secret.Value;
            await Console.Out.WriteLineAsync($"\tstorageAccountKey: {storageAccountKey}\n");

            string storageUri = "https://classhubfilestorage.blob.core.windows.net/";
            var blobServiceClient = new BlobServiceClient(
                new Uri(storageUri),
                new DefaultAzureCredential()
            );
            await Console.Out.WriteLineAsync($"StorageAccount Name: {blobServiceClient.AccountName}\n");

            // Create a unique name for the container
            // 고유한 컨테이너 이름 생성
            string containerName = "byungmeo" + Guid.NewGuid().ToString();

            // Create the container and return a container client object
            // 컨테이너를 생성함과 동시에 컨테이너 객체를 반환받음
            await Console.Out.WriteLineAsync("Creating Blob Container...");
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            await Console.Out.WriteLineAsync($"\tBlobContainer Name: {containerClient.Name}\n");

            // Get a reference to a blob
            BlobClient blobClient1 = containerClient.GetBlobClient("test1.txt");
            {
                await Console.Out.WriteLineAsync($"BlobClient Name: {blobClient1.Name}\n");

                // Upload data from the local file
                await Console.Out.WriteLineAsync("Uploading a blob to a Container...");
                var uploadResponse = await blobClient1.UploadAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello, World!")), overwrite: true);
                await Console.Out.WriteLineAsync($"\tResponse: {uploadResponse.ToString()}\n");
            }

            // Get a reference to a blob
            BlobClient blobClient2 = containerClient.GetBlobClient("test2.txt");
            {
                await Console.Out.WriteLineAsync($"BlobClient Name: {blobClient2.Name}\n");

                // Upload data from the local file
                await Console.Out.WriteLineAsync("Uploading a blob to a Container...");
                var uploadResponse = await blobClient2.UploadAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello, World!")), overwrite: true);
                await Console.Out.WriteLineAsync($"\tResponse: {uploadResponse.ToString()}\n");
            }

            await Console.Out.WriteLineAsync($"Generating SAS Uri...");
            // Create a SAS token that's valid for one hour.
            BlobSasBuilder sasBuilder = new BlobSasBuilder() {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient1.Name,
                Resource = "b",
                StartsOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(containerClient.AccountName, storageAccountKey)).ToString();

            // Now you can use the SAS token to create a download link
            UriBuilder sasUri = new UriBuilder(blobClient1.Uri) {
                Query = sasToken
            };
            string downloadLink = sasUri.ToString();
            await Console.Out.WriteLineAsync($"\tSAS Uri: {downloadLink}\n");

            // List all blobs in the container
            await Console.Out.WriteLineAsync("Listing blobs...");
            await foreach(BlobItem blobItem in containerClient.GetBlobsAsync()) {
                await Console.Out.WriteLineAsync($"\t{blobItem.Name}");
            }

            // Download the blob's contents and save it to a file
            string downloadFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/DOWNLOADED.txt";
            await Console.Out.WriteLineAsync($"\nDownloading blob to\n\t{downloadFilePath}");
            var response = await blobClient1.DownloadToAsync(downloadFilePath);
            await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");

            // Clean up
            await Console.Out.WriteLineAsync("Press any key to begin clean up");
            Console.ReadLine();

            await Console.Out.WriteLineAsync("Deleting blob...");
            response = await blobClient1.DeleteAsync();
            await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");

            // Container를 삭제하면 내부 blob도 함께 정리된다.
            await Console.Out.WriteLineAsync("Deleting blob container...");
            response = await containerClient.DeleteAsync();
            await Console.Out.WriteLineAsync($"\tResponse Status: {response.Status}\n");

            await Console.Out.WriteLineAsync("Done");
        }
    }
}
