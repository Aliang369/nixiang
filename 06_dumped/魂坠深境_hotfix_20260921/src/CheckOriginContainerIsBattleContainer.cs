public class CheckOriginContainerIsBattleContainer : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.OriginContainer?.IsBattleContainer() ?? false;
	}
}
