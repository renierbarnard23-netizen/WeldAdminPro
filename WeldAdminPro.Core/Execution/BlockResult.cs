namespace WeldAdminPro.Core.Execution
{
	public class BlockResult
	{
		public BlockReason Reason { get; set; }
		public string Message { get; set; } = "";

		public bool IsBlocked => Reason != BlockReason.None;

		public static BlockResult None()
		{
			return new BlockResult
			{
				Reason = BlockReason.None,
				Message = string.Empty
			};
		}

		public static BlockResult Create(BlockReason reason, string message)
		{
			return new BlockResult
			{
				Reason = reason,
				Message = message
			};
		}
	}
}