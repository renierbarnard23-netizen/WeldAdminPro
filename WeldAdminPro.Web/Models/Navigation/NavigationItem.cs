namespace WeldAdminPro.Web.Models.Navigation;

public class NavigationItem
{
    public string Text { get; set; } = "";

    public string Url { get; set; } = "";

    public string Icon { get; set; } = "";

    public string? Permission { get; set; }

    public List<NavigationItem> Children { get; set; } = new();
}