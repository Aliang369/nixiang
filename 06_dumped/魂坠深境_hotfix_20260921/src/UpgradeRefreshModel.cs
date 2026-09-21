public abstract class UpgradeRefreshModel
{
	private int usedCount;

	public int UsedCount => usedCount;

	public abstract int MaxCount { get; }

	public abstract bool Enabled { get; }

	public virtual int CurrentPrice => 0;

	public virtual MatterDef Matter => null;

	public bool HasCost
	{
		get
		{
			if (Matter != null)
			{
				return CurrentPrice > 0;
			}
			return false;
		}
	}

	public bool Available
	{
		get
		{
			if (Enabled)
			{
				return usedCount < MaxCount;
			}
			return false;
		}
	}

	public virtual bool CanRefresh => Available;

	public void Refresh()
	{
		if (CanRefresh)
		{
			Pay();
			usedCount++;
			Regenerate();
		}
	}

	public void Reset()
	{
		usedCount = 0;
	}

	protected virtual void Pay()
	{
	}

	protected abstract void Regenerate();
}
