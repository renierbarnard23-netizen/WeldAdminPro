namespace WeldAdminPro.Web.Models.Navigation;

public class NavigationNode
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Text { get; set; } = "";

    public string Url { get; set; } = "";

    public string Icon { get; set; } = "";

    public string? Permission { get; set; }

    public bool Expanded { get; set; } = true;

    public List<NavigationNode> Children { get; set; } = new();
}