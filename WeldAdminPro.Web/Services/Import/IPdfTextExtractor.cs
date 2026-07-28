namespace WeldAdminPro.Web.Services.Import;

public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(Stream stream);
}