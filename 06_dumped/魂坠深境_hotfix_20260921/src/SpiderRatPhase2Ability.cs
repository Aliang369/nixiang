using System.Collections.Generic;
using UnityEngine;

public class SpiderRatPhase2Ability : Ability, IEnemyIntentDisplayAbility
{
	public float activityBase = 2f;

	public float activityMultiplier = 0.75f;

	public float activityExponent = 1.15f;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.GenerateDamageDamageValueResolve;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.CasterOwner;
		if (base.CheckTriggerGameEventAble(eve) && battleFieldTransform != null && battleFieldTransform.isEnemy)
		{
			CharacterAI characterAI = battleFieldTransform.characterAI;
			if (characterAI != null && characterAI.inPhase2 && battleFieldTransform.characterAI.SecondPhaseAbilityEnable && eve.damageResolveType == Damage.ResolveType.DamageValue)
			{
				return eve.SourceItem?.OwnerBft == battleFieldTransform;
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			eve.resolveInfo.sourceDelta += GetDamageIncrease(eve.CasterOwner, eve.gameState);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		BattleFieldTransform enemy = eve?.CasterOwner;
		int nestActivity = SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState);
		float damagePercent = GetDamagePercent(nestActivity);
		int damageIncrease = GetDamageIncrease(enemy, eve?.gameState);
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		nodeDisplayInfo.value = new NodeDisplayInfo.Element<int?>(damageIncrease);
		nodeDisplayInfo.str = new NodeDisplayInfo.Element<string>($"{damagePercent:0.##}%（{TextDataPreset.ValueFormat.FormatWith(damageIncrease)}）的伤害提升（活性等级{TextDataPreset.ValueFormat.FormatWith(nestActivity)}）");
		return nodeDisplayInfo;
	}

	private int GetDamageIncrease(BattleFieldTransform enemy, GameStateInfo gameState)
	{
		int nestActivity = SpiderContractEnemyAbilityUtility.GetNestActivity(gameState);
		return Mathf.FloorToInt((float)Mathf.Max(0, enemy?.health ?? ((ProtectedInt)0)) * GetDamagePercent(nestActivity) / 100f);
	}

	private float GetDamagePercent(int activity)
	{
		return activityBase + Mathf.Pow(activityMultiplier * (float)activity, activityExponent);
	}
}
