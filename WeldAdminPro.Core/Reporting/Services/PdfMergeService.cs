using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class PdfMergeService
    {
        public void Merge(
            string outputFile,
            List<string> inputFiles)
        {
            using var outputDocument =
                new PdfDocument();

            foreach (var file in inputFiles)
            {
                if (!File.Exists(file))
                    continue;

                try
                {
                    using var inputDocument =
                        PdfReader.Open(
                            file,
                            PdfDocumentOpenMode.Import);

                    for (int i = 0;
                        i < inputDocument.PageCount;
                        i++)
                    {
                        outputDocument.AddPage(
                            inputDocument.Pages[i]);
                    }
                }
                catch
                {
                    // Ignore invalid PDFs
                }
            }

            outputDocument.Save(outputFile);
        }
    }
}
