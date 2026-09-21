using System.Linq;

public class BreakItemAction : GameAction
{
	public Damage damage;

	private int? originalDefendCounter;

	public object source;

	public Item target;

	public BreakItemAction(Item target, object source, Damage damage, int? originalDefendCounter = null)
	{
		this.target = target;
		this.source = source;
		this.damage = damage;
		this.originalDefendCounter = originalDefendCounter;
	}

	protected override void Update()
	{
		int? num = originalDefendCounter;
		if (!num.HasValue)
		{
			originalDefendCounter = target?.GetCurrentDefendCounter();
		}
		if (!gameState.gameEventManager.TriggerBreakItemResolve(target, source, damage, GameEvent.TriggerType.Real, originalDefendCounter).reject)
		{
			gameState.gameEventManager.TriggerBeforeBreakItem(target, source, damage, originalDefendCounter);
			Damage obj = damage;
			if (obj != null && obj.breakChoicePending)
			{
				FinishAction();
				return;
			}
			if (target.Companion && target.DamageAble)
			{
				ExileItemAction action = new ExileItemAction(target, source);
				EnqueueAction(action);
			}
			else
			{
				ItemContainer owner = target.owner;
				if (owner != null && owner.owner?.isPlayer == true && target.owner.items.Count((Item it) => it.DamageAble) == 1)
				{
					EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.lastDefendBreakShake));
				}
				EnqueueAction(new ConsumeItemAction(target, ConsumeItemAction.ConsumeType.Damage));
			}
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerAfterBreakItem(target, source, damage, originalDefendCounter);
			});
		}
		FinishAction();
	}
}
