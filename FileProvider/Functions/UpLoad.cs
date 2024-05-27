using FileProvider.Services;
using Infrastructure.Contexts;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FileProvider.Functions
{
    public class UpLoad(ILogger<UpLoad> logger, FileService fileService)
    {
       private readonly ILogger<UpLoad> _logger = logger;
       private readonly FileService _fileService = fileService;

        [Function("UpLoad")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                if (req.Form.Files["file"] is IFormFile file)
                {
                    var containerName = !string.IsNullOrEmpty(req.Query["containerName"]) ? req.Query["containerName"].ToString() : "files";

                    var fileEntity = new FileEntity
                    {
                        FileName = _fileService.SetFileName(file),
                        ContentType = file.ContentType,
                        ContainerName = containerName
                        
                };

                    await _fileService.SetBlobContainerAsync(fileEntity.ContainerName);
                   var filePath = await _fileService.UpLoadFileAsync(file, fileEntity);
                   fileEntity.FilePath = filePath;

                    await _fileService.SaveToDatabaseAsync(fileEntity);
                    return new OkObjectResult(fileEntity);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return new BadRequestResult();
        }
    }
}
