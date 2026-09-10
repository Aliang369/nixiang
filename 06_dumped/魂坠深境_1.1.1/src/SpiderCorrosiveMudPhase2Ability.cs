using System.Collections.Generic;
using UnityEngine;

public class SpiderCorrosiveMudPhase2Ability : Ability, IEnemyIntentDisplayAbility
{
	public EnchantDef poisonEnchant;

	public int poisonStacks = 2;

	public float healthBase = 1f;

	public float activityDivisor = 1.45f;

	public float activityExponent = 1.25f;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterDamage;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.CasterOwner;
		if (base.CheckTriggerGameEventAble(eve) && battleFieldTransform != null && battleFieldTransform.isEnemy)
		{
			CharacterAI characterAI = battleFieldTransform.characterAI;
			if (characterAI != null && characterAI.inPhase2 && battleFieldTransform.characterAI.SecondPhaseAbilityEnable && eve.damage?.targetBFT == battleFieldTransform)
			{
				return eve.damage.subCounter > 0;
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			Item item = (eve.damage.source as Item) ?? ((eve.damage.source as IAttachment)?.AttachTarget as Item) ?? eve.damage.sourceSpellInfo?.spellItem;
			if (item != null && poisonEnchant != null && poisonStacks > 0)
			{
				SpiderContractEnemyAbilityUtility.EnqueueAction2(eve);
				eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(item, eve.CasterItem, poisonEnchant, poisonStacks));
			}
			eve.gameState.actionManager.EnqueueAction(new HealAction(eve.CasterItem, eve.CasterOwner, GetHealAmount(eve.gameState)));
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int nestActivity = SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState);
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		nodeDisplayInfo.value = new NodeDisplayInfo.Element<int?>(GetHealAmount(eve?.gameState));
		nodeDisplayInfo.str = new NodeDisplayInfo.Element<string>(TextDataPreset.ValueFormat.FormatWith(nestActivity));
		return nodeDisplayInfo;
	}

	private int GetHealAmount(GameStateInfo gameState)
	{
		int nestActivity = SpiderContractEnemyAbilityUtility.GetNestActivity(gameState);
		return Mathf.FloorToInt(healthBase + Mathf.Pow((float)nestActivity / activityDivisor, activityExponent));
	}
}
