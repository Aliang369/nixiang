public class DeathblowAction : ExecuteAction
{
	public BattleFieldTransform bft;

	public Damage damage;

	public object source;

	public DeathblowAction(BattleFieldTransform bft, object source, Damage damage)
	{
		this.bft = bft;
		this.source = source;
		this.damage = damage;
	}

	protected override void Update()
	{
		int num = bft.extraHealthPoint;
		if (bft.characterAI != null && num > 0)
		{
			gameState.gameEventManager.TriggerBeforeBftDeathBlow(bft, source, damage);
			bft.extraHealthPoint = num - 1;
			if (!bft.characterAI.inPhase2)
			{
				bft.characterAI.inPhase2 = true;
				EnqueueAction(delegate
				{
					gameState.gameEventManager.TriggerEnemyEnterPhase2(bft);
				});
			}
			int num2 = bft.characterAI.secondPhaseMaxHealth;
			if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ExtraEnemyHealthForPhase2))
			{
				num2 = (int)(1.25f * (float)num2);
			}
			EnqueueAction(new SetBftMaxHealthAction(num2, null, bft));
			EnqueueAction(new SetBftHealthAction(num2, null, bft));
			if (bft.isEnemy && !gameState.gameEventManager.TriggerDeathBlowEnterCrystalFreezeResolve(bft).reject)
			{
				gameState.actionManager.EnqueueAction(new WaitUntilAction(() => !bft.attachDisplay.IsPushingForward()));
				gameState.actionManager.EnqueueAction(new EnterCrystalFreezeAction(bft));
			}
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAfterBftDeathBlow(bft, source, damage);
		});
	}
}
