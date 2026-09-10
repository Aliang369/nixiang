public class PersistentHitNFX3DConfig : NFXConfig
{
	public float duration;

	public override VfxType GetValidVfxType()
	{
		return VfxType.Hit3D;
	}
}
