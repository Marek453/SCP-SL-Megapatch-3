namespace LlockhamIndustries.ExtensionMethods
{
	public static class IntExtensionMethods
	{
		public static int IterateUp(this int i, int IEnumaratorCount)
		{
			if (i < IEnumaratorCount - 1)
			{
				return i + 1;
			}
			return 0;
		}

		public static int IterateDown(this int i, int IEnumaratorCount)
		{
			if (i > 0)
			{
				return i - 1;
			}
			return IEnumaratorCount - 1;
		}
	}
}
