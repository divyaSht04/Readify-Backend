using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;

    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveFile(IFormFile file, string directory)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        // Create directory if it doesn't exist
        var uploadPath = Path.Combine(_environment.ContentRootPath, "images", directory);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // Generate a unique filename
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        // Save the file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return the relative path to be stored in the database
        return $"{directory}/{fileName}";
    }

    public void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var fullPath = Path.Combine(_environment.ContentRootPath, "images", filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
} 