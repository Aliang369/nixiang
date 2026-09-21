public class AntiCheatSaveState : ISaveCallback
{
	[SaveField(null, null)]
	public int tamperCount;

	[SaveField(null, null)]
	public int mismatchReadCount;

	public void BeforeSave()
	{
		tamperCount = ProtectedValueGuard.TamperCount;
		mismatchReadCount = ProtectedValueGuard.MismatchReadCount;
	}

	public void AfterLoad()
	{
		ProtectedValueGuard.Restore(tamperCount, mismatchReadCount);
	}
}
