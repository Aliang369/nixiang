public class TriggerBonusAction : ExecuteAction
{
	private readonly BattleBonus bonus;

	private readonly bool EndEncounterAfterBonus;

	private readonly bool isBattle;

	public TriggerBonusAction(BattleBonus bonus, bool EndEncounterAfterBonus = true, bool isBattle = false)
	{
		this.bonus = bonus;
		this.EndEncounterAfterBonus = EndEncounterAfterBonus;
		this.isBattle = isBattle;
	}

	protected override void Update()
	{
		if (bonus == null)
		{
			return;
		}
		if (GameAutomationHooks.TryHandleBonus(gameState, bonus, EndEncounterAfterBonus, isBattle))
		{
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerTriggerBonus(bonus);
			});
			return;
		}
		if (isBattle)
		{
			gameState.UIManager.ShowUI(gameState.UIManager.pickSelectPanel);
			gameState.UIManager.pickSelectPanel.Setup(bonus);
			gameState.UIManager.pickSelectPanel.endEncounterAfterBonus = EndEncounterAfterBonus;
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerTriggerBonus(bonus);
		});
	}
}
