using Microsoft.EntityFrameworkCore;
using Pronia.Context;

namespace Pronia.Helpers;

public static class ExtensionMethods
{
    public static bool CheckSize(this IFormFile file, int mb)
    {
        return file.Length < mb*1024*1024;
    }

    public static bool CheckType(this IFormFile file, string type = "image")
    {
        return file.ContentType.Contains(type);
    }

    public static async Task<string> SaveFileAsync(this IFormFile file, string folderPath)
    {
        string uniqueName = Guid.NewGuid().ToString() + file.FileName;
        string path = Path.Combine(folderPath, uniqueName);
        using FileStream stream = new FileStream(path, FileMode.Create); 
        await file.CopyToAsync(stream);
        return uniqueName;
    }

    public static void DeleteFile(string path)
    {
        if(File.Exists(path))
            File.Delete(path);
    }
}