namespace WeldAdminPro.Web.Models.Navigation;

public class NavigationSection
{
    public string Text { get; set; } = "";

    public string Icon { get; set; } = "";

    public string? Permission { get; set; }

    public List<NavigationItem> Items { get; set; } = new();
}