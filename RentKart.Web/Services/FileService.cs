using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using RentKart.Core.Interfaces;

namespace RentKart.Web.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream is empty or null.");

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", folderName);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var destStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destStream);
        }

        return $"/uploads/{folderName}/{uniqueFileName}";
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;
        
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var physicalPath = Path.Combine(webRoot, filePath.TrimStart('/'));
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }
}
