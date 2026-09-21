public class SwapYinYangPower : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.gameState?.trigramsManager == null)
		{
			return NodeCastResult.Failure;
		}
		TrigramsManager trigramsManager = eve.gameState.trigramsManager;
		if ((int)trigramsManager.yangPower > (int)trigramsManager.yinPower)
		{
			eve.gameState.actionManager.EnqueueAction(new TrigramsManager.ConvertYinYangPowerAction((int)trigramsManager.yangPower - (int)trigramsManager.yinPower, TrigramsManager.YinYangType.Yang, eve.Caster));
		}
		else if ((int)trigramsManager.yangPower < (int)trigramsManager.yinPower)
		{
			eve.gameState.actionManager.EnqueueAction(new TrigramsManager.ConvertYinYangPowerAction((int)trigramsManager.yinPower - (int)trigramsManager.yangPower, TrigramsManager.YinYangType.Yin, eve.Caster));
		}
		Effect.EnqueueAction(eve, new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
		return NodeCastResult.Success;
	}
}
