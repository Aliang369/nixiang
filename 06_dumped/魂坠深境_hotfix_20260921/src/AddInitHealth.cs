public class AddInitHealth : GameConfigEffect
{
	public int health;

	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.initHealth += health;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(health)
		};
	}
}
