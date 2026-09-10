using UnityEngine;

public class NFXConfig
{
	[SerializeField]
	public NFXSetup setup;

	public virtual VfxType GetValidVfxType()
	{
		return VfxType.None;
	}
}
