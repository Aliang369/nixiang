public class CheckSameBft : CheckNode
{
	public GameEvent.EventBFTType a;

	public GameEvent.EventBFTType b;

	public override bool? Check(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(a);
		BattleFieldTransform eventBFT2 = eve.GetEventBFT(b);
		return eventBFT == eventBFT2 && eventBFT != null;
	}
}
