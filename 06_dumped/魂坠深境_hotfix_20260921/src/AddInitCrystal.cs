public class AddInitCrystal : GameConfigEffect
{
	public int crystal;

	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.initCrystal += crystal;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(crystal)
		};
	}
}
