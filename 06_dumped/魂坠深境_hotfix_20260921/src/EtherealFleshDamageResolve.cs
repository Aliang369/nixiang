using UnityEngine;

public class EtherealFleshDamageResolve : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.TargetBft;
		if (battleFieldTransform == null || eve.resolveInfo == null || eve.deltaValue >= 0)
		{
			return NodeCastResult.Failure;
		}
		int num = battleFieldTransform.tempHealth;
		int num2 = battleFieldTransform.maxHealth;
		if (num <= 0 || num2 <= 0)
		{
			return NodeCastResult.Failure;
		}
		int num3 = -eve.deltaValue;
		float num4 = Mathf.Clamp01((float)num / (float)num2);
		int num5 = Mathf.Max(1, Mathf.CeilToInt((float)num3 * (1f - num4)));
		eve.resolveInfo.deltaValue += num3 - num5;
		eve.resolveInfo.realHealthOnly = true;
		return NodeCastResult.Success;
	}
}
