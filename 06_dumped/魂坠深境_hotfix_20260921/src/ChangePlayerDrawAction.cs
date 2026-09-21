public class ChangePlayerDrawAction : ExecuteAction
{
	public int delta;

	public ChangePlayerDrawAction(int delta)
	{
		this.delta = delta;
	}

	protected override void Update()
	{
		BattleFieldTransform bft = gameState.playerState.bft;
		bft.turnDrawItemCount = (int)bft.turnDrawItemCount + delta;
	}
}
