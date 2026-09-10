public class DequeueSpellRequestAction : ExecuteAction
{
	private readonly PlayerControlInfo controlInfo;

	public DequeueSpellRequestAction(PlayerControlInfo controlInfo)
	{
		this.controlInfo = controlInfo;
	}

	protected override void Update()
	{
		if (controlInfo.requests.Count != 0)
		{
			if (controlInfo.RealSpellCastInfo.castState != SpellCastInfo.State.Preparing)
			{
				gameState.actionManager.EnqueueActionToEnd(new WaitFrameAction(1));
				gameState.actionManager.EnqueueActionToEnd(new DequeueSpellRequestAction(controlInfo));
			}
			else
			{
				ExecuteRequest(controlInfo.requests.Dequeue());
				gameState.actionManager.EnqueueActionToEnd(new DequeueSpellRequestAction(controlInfo));
			}
		}
	}

	private void ExecuteRequest(PlayerControlInfo.SpellRequest request)
	{
		if (request.withAnim)
		{
			gameState.UIManager.playerItemArea.OnTriggerAbilityAnim(request.spellItem, Ability.AbilityAnimPriority.Strong, Ability.AbilityAnimType.None);
		}
		if (request.targetItem == null && request.spellItem.spellNeedsTarget)
		{
			PlayerControlInfo.EnqueueSpellSelectItemAsTargetAction(request.spellItem, delegate(Item t)
			{
				request.targetItem = t;
			}, gameState);
		}
		gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			controlInfo.displaySpellInfo.spellItem = request.spellItem;
			controlInfo.displaySpellInfo.targetItem = request.targetItem;
			controlInfo.displaySpellInfo.AddCost(GameEvent.TriggerType.InBattleDisplay);
			controlInfo.TryCastDisplaySpell();
		}));
		if (request.interval != 0f)
		{
			EnqueueAction(new WaitAction(request.interval));
		}
		if (request.waitForAnimComplete)
		{
			EnqueueAction(new WaitUntilAction(() => gameState.UIManager.playerItemArea.IsItemAnimationComplete(request.spellItem)));
		}
	}
}
