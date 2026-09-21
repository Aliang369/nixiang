using System;
using System.Text;

public class ChangeCounterAction : GameAction
{
	private int delta;

	private readonly object source;

	private readonly Item targetItem;

	private readonly bool withPopupText;

	private readonly bool consumeDefenceWhenDepleted;

	private readonly Action<int, int, int, int> onCompleted;

	public ChangeCounterAction(int delta, Item targetItem, object source, bool withPopupText = false, bool consumeDefenceWhenDepleted = false, Action<int, int, int, int> onCompleted = null)
	{
		this.delta = delta;
		this.targetItem = targetItem;
		this.source = source;
		this.withPopupText = withPopupText;
		this.consumeDefenceWhenDepleted = consumeDefenceWhenDepleted;
		this.onCompleted = onCompleted;
	}

	protected override void Update()
	{
		if (targetItem == null)
		{
			onCompleted?.Invoke(0, 0, 0, 0);
			FinishAction();
			return;
		}
		int initCounter = targetItem.counter;
		int currentDefendCounter = targetItem.GetCurrentDefendCounter();
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerCounterChangeResolveInfo(targetItem, source, delta, GameEvent.TriggerType.Real);
		if (resolveInfo.reject)
		{
			onCompleted?.Invoke(initCounter, 0, initCounter, targetItem.maxCounter);
			FinishAction();
			return;
		}
		int num = targetItem.counter;
		int maxCounter = targetItem.maxCounter;
		if (targetItem.UsesDefenceCounterLimit)
		{
			targetItem.counter = ResolveCounterValue(num, delta, resolveInfo.deltaPercent, resolveInfo.deltaValue, maxCounter);
		}
		else
		{
			delta = (int)((100f + (float)resolveInfo.deltaPercent) / 100f * (float)delta);
			delta += resolveInfo.deltaValue;
			int num2 = num + delta;
			if (maxCounter != 0 && num2 > maxCounter)
			{
				num2 = maxCounter;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			targetItem.counter = num2;
		}
		int finalCounter = targetItem.counter;
		int finalDelta = finalCounter - initCounter;
		if (withPopupText)
		{
			PopupTextAction popupTextAction = new PopupTextAction(targetItem, finalDelta, ScriptableObjectSingleton<VFXDataPreset>.instance.changeCounterPopupTextPrefab, CounterTextConverter, positiveSign: true);
			popupTextAction.manualSetColor = targetItem.GetCounterTextColor();
			EnqueueAction(popupTextAction);
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerCounterChange(targetItem, source, finalDelta, targetItem.counter);
		});
		if (initCounter != maxCounter && finalCounter == maxCounter)
		{
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerCounterReachMaxCounter(targetItem, source);
			});
		}
		if (consumeDefenceWhenDepleted && currentDefendCounter > 0 && targetItem.GetCurrentDefendCounter() <= 0 && targetItem.ShouldBreakWhenDefendDepleted())
		{
			EnqueueAction(new ConsumeItemAction(targetItem, ConsumeItemAction.ConsumeType.Damage));
		}
		EnqueueAction(delegate
		{
			onCompleted?.Invoke(initCounter, finalDelta, finalCounter, maxCounter);
		});
		FinishAction();
	}

	public static int ResolveCounterValue(int currentCounter, int requestedDelta, int deltaPercent, int deltaValue, int maxCounter)
	{
		int num = CombatValueLimit.TruncateToInt((100f + (float)deltaPercent) / 100f * (float)requestedDelta);
		int num2 = CombatValueLimit.Clamp((long)CombatValueLimit.Clamp(currentCounter) + (long)num + deltaValue);
		if (maxCounter != 0)
		{
			num2 = Math.Min(num2, CombatValueLimit.Clamp(maxCounter));
		}
		return num2;
	}

	private string CounterTextConverter(string numText)
	{
		StringBuilder stringBuilder = new StringBuilder(numText.Length);
		for (int i = 0; i < numText.Length; i++)
		{
			char c = numText[i];
			if (char.IsNumber(c))
			{
				stringBuilder.Append(TextDisplayHelper.ToNumSpriteString(int.Parse(c.ToString())));
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
