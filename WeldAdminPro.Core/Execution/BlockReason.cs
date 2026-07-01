namespace WeldAdminPro.Core.Execution
{
	public enum BlockReason
	{
		None,
		NoStock,
        MaterialShortage,
        InsufficientStock,
		DependencyNotMet,
		NotScheduled,
		CapacityOverload,
		ManualHold,
		Unknown
	}
}