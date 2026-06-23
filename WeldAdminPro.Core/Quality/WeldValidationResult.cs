namespace WeldAdminPro.Core.Quality
{
    public class WeldValidationResult
    {
        public bool IsValid { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}