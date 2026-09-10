public class ChangeExtraHealthPoint : Effect
{
	public int delta;

	public GameEvent.EventBFTType bftType;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(bftType);
		if (eventBFT == null)
		{
			return NodeCastResult.Failure;
		}
		eventBFT.extraHealthPoint = (int)eventBFT.extraHealthPoint + delta;
		return NodeCastResult.Success;
	}
}
