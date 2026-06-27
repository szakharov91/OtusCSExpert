using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusCSExpert.LoadTesting.Utils;

public static class VisualStudioProvider
{
    /// <summary>
    /// Пытается найти директорию, в которой находится файл решения (*.sln).
    /// </summary>
    /// <returns>DirectoryInfo с путём к папке, где найден .sln файл, или null, если файл не найден.</returns>
    public static DirectoryInfo TryGetSolutionDirectoryInfo()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Поднимаемся по дереву папок, пока не найдём файл *.sln
        while (directory != null && !directory.GetFiles("*.slnx").Any())
        {
            directory = directory.Parent;
        }

        return directory;
    }

    public static string GetPathToPrerequisites(string solutionDir)
    {
        return Path.Combine(solutionDir, "prerequisites");
    }
}
