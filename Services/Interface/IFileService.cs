using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public interface IFileService
{
    Task<string?> SaveFile(IFormFile file, string directory);
    void DeleteFile(string filePath);
} 