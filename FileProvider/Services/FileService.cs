
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Contexts;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FileProvider.Services;

public class FileService(ILogger<FileService> logger, DataContext dataContext, BlobServiceClient client)
{
    private ILogger<FileService> _logger = logger;
    private readonly DataContext _dataContext = dataContext;
    private readonly BlobServiceClient _client = client;
    private BlobContainerClient? _container;


    public async Task SetBlobContainerAsync(string containerName)
    {
        _container = _client.GetBlobContainerClient(containerName);
        await _container.CreateIfNotExistsAsync(PublicAccessType.BlobContainer); //vad bra det går
    }

    public string SetFileName(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        return fileName ;
    }

    public async Task<string> UpLoadFileAsync (IFormFile file, FileEntity fileEntity)
    {
        BlobHttpHeaders headers = new()
        {
            ContentType = file.ContentType
        };

        var blobClient = _container?.GetBlobClient(fileEntity.FileName);

        using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(stream, headers);

        return blobClient.Uri.ToString();
    }

    public async Task SaveToDatabaseAsync(FileEntity fileEntity)
    {
        _dataContext.Files.Add(fileEntity);
        await _dataContext.SaveChangesAsync();
    }

}
