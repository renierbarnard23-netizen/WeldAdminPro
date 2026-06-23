public enum AlertLevel
{
	Info,
	Warning,
	Critical
}

public class SystemAlert
{
	public AlertLevel Level { get; set; }
	public string Message { get; set; } = "";
	public string Source { get; set; } = "";
}