namespace WeldAdminPro.Core.Execution
{
	public enum BlockReason
	{
		None,
		NoStock,
		InsufficientStock,
		DependencyNotMet,
		NotScheduled,
		CapacityOverload,
		ManualHold,
		Unknown,
        WpsMismatch
    }
}