public class AddInitCoin : GameConfigEffect
{
	public int coin;

	public override void Prepare(CharacterGameInitConfig characterGameConfig)
	{
		characterGameConfig.initCoin += coin;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(coin)
		};
	}
}
