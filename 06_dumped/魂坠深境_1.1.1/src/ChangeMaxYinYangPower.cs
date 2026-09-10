using System.Collections.Generic;
using UnityEngine;

public class ChangeMaxYinYangPower : Effect
{
	[SerializeField]
	private TrigramsManager.DurationType durationType;

	[SerializeField]
	private ValueGenerator yang;

	[SerializeField]
	private ValueGenerator yin;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.gameState?.trigramsManager == null)
		{
			return NodeCastResult.Failure;
		}
		if (durationType != TrigramsManager.DurationType.Temp && durationType != TrigramsManager.DurationType.Constant && durationType != TrigramsManager.DurationType.Turn)
		{
			TraceLog.Error($"侠客阴阳DurationType错误{durationType} - {eve.CasterItem?.ItemName}");
			return NodeCastResult.Failure;
		}
		int valueOrDefault = (yin?.GenerateValue(eve)).GetValueOrDefault();
		int valueOrDefault2 = (yang?.GenerateValue(eve)).GetValueOrDefault();
		Effect.EnqueueAction(eve, new TrigramsManager.ChangYinYangPowerAction(valueOrDefault, valueOrDefault2, durationType, eve.Caster));
		Effect.EnqueueAction(eve, new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { yin, yang };
	}
}
