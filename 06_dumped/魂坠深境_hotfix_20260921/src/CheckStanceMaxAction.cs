public class CheckStanceMaxAction : GameAction
{
	public Stance stance;

	public object source;

	public CheckStanceMaxAction(Stance stance, object source = null)
	{
		this.stance = stance;
		this.source = source;
	}

	protected override void Update()
	{
		if (source == null || stance == null || stance.def == null || stance.value <= 0)
		{
			FinishAction();
		}
		else if (stance.value >= gameState.stanceManager.stanceMaxValue)
		{
			Stance before = stance;
			Stance deltaStance = new Stance(stance.def, gameState.stanceManager.stanceMaxValue, gameState.playerState.bft);
			stance.value -= gameState.stanceManager.stanceMaxValue;
			if (stance.value == 0)
			{
				stance = null;
			}
			gameState.gameEventManager.TriggerBeforeChangeStanceValue(before, stance, deltaStance, source, Stance.ChangeType.Full.ToString());
			if (gameState.stanceManager.NeedRepeatTrigger)
			{
				gameState.gameEventManager.TriggerBeforeChangeStanceValue(before, stance, deltaStance, source, Stance.ChangeType.Full.ToString());
			}
			gameState.stanceManager.currentStance = stance;
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerAfterChangeStanceValue(before, stance, deltaStance, source, Stance.ChangeType.Full.ToString());
			});
			if (gameState.stanceManager.NeedRepeatTrigger)
			{
				gameState.gameEventManager.TriggerAfterChangeStanceValue(before, stance, deltaStance, source, Stance.ChangeType.Full.ToString());
			}
			EnqueueAction(new CheckStanceMaxAction(gameState.stanceManager.currentStance, source));
		}
		else
		{
			FinishAction();
		}
	}
}
