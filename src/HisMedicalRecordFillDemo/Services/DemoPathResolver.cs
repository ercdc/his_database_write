namespace HisMedicalRecordFillDemo.Services;

public static class DemoPathResolver
{
    public static string GetRoot(IHostEnvironment environment)
    {
        foreach (var startPath in new[] { environment.ContentRootPath, AppContext.BaseDirectory })
        {
            for (var directory = new DirectoryInfo(startPath); directory is not null; directory = directory.Parent)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Resources")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "Fixtures")))
                    return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("未找到同时包含 Resources 与 Fixtures 的 Demo 根目录。");
    }
}
