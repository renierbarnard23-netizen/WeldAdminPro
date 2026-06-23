using System;
using System.IO;

namespace WeldAdminPro.Core.Helpers
{
    public static class DocumentPathHelper
    {
        public static string GetProjectDocumentFolder(Guid projectId)
        {
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WeldAdminPro",
                "Projects",
                projectId.ToString(),
                "Documents");

            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            return baseDir;
        }

        public static string CopyToProjectFolder(Guid projectId, string sourceFile)
        {
            var folder = GetProjectDocumentFolder(projectId);

            var fileName = Path.GetFileName(sourceFile);
            var destination = Path.Combine(folder, fileName);

            File.Copy(sourceFile, destination, overwrite: true);

            return destination;
        }
    }
}