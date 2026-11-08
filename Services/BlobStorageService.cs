using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"]
            ?? configuration.GetConnectionString("AzureBlobStorage")
            ?? configuration["AZURE_STORAGE_CONNECTION_STRING"]
            ?? "UseDevelopmentStorage=true";

        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = configuration["AzureBlobStorage:ContainerName"]
            ?? configuration["AzureBlob:ContainerName"]
            ?? "raceday-images";
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder = "")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Please select an image file.", nameof(file));

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Choose an image smaller than 5 MB.", nameof(file));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            throw new ArgumentException("Only JPG, PNG, and WebP images are supported.", nameof(file));

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        if (!string.IsNullOrWhiteSpace(folder))
            fileName = $"{folder.Trim('/')}/{fileName}";

        var blobClient = containerClient.GetBlobClient(fileName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType
        });

        return blobClient.Uri.ToString();
    }
}
