public class IsGuide : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.gameState?.applicationState?.guideManager.RunningIntroModule != null;
	}
}
