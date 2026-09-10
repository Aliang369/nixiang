using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderMissileNeedleSwarmAbility : Ability
{
	[NonSerialized]
	private HashSet<BattleFieldTransform> damagedEnemies;

	public ItemDefinition wound;

	public int woundCount = 2;

	public float woundDamageMultiplier = 0.3f;

	public ItemDefinition venomSprayEffectSource;

	public ItemDefinition spikeRainEffectSource;

	private HashSet<BattleFieldTransform> DamagedEnemies => damagedEnemies ?? (damagedEnemies = new HashSet<BattleFieldTransform>());

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterDamage;
		yield return GameEvent.Type.EnemyTurnStart;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.CasterOwner;
		if (!base.CheckTriggerGameEventAble(eve) || battleFieldTransform == null || !battleFieldTransform.isEnemy || SpiderContractEnemyAbilityUtility.GetSpiderRule(eve.gameState) == null)
		{
			return false;
		}
		if (eve.type == GameEvent.Type.AfterDamage)
		{
			Damage damage = eve.damage;
			if (damage != null && damage.isHealthDamage && eve.damage.targetBFT == battleFieldTransform)
			{
				return eve.damage.subCounter > 0;
			}
			return false;
		}
		if (eve.type == GameEvent.Type.EnemyTurnStart && wound != null)
		{
			return woundCount > 0;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		BattleFieldTransform casterOwner = eve.CasterOwner;
		if (eve.type == GameEvent.Type.AfterDamage)
		{
			DamagedEnemies.Add(casterOwner);
			return;
		}
		bool num = DamagedEnemies.Remove(casterOwner);
		TriggerEffect(eve, null);
		PlaySpellEffects(eve, venomSprayEffectSource);
		PlaySpellEffects(eve, spikeRainEffectSource);
		int num2 = Mathf.FloorToInt(Mathf.Pow(3f + (float)SpiderContractEnemyAbilityUtility.GetNestActivity(eve.gameState), 1.15f));
		int num3 = (num ? woundCount : (woundCount + 2));
		for (int i = 0; i < num3; i++)
		{
			Item target = new Item(wound)
			{
				counter = num2,
				resetInitCounter = num2
			};
			eve.gameState.actionManager.EnqueueAction(new SpawnItemAction(target, eve.gameState.playerState.DrawPileContainer, withAnim: true, eve.Caster));
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int num = Mathf.FloorToInt(Mathf.Pow(3f + (float)SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState), 1.15f));
		Item item = ((wound == null) ? null : new Item(wound)
		{
			counter = num,
			resetInitCounter = num
		});
		if (item != null)
		{
			item.tempValues["SpiderPhase2Activity"] = SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState);
		}
		return new NodeDisplayInfo
		{
			refItems = new NodeDisplayInfo.Element<List<Item>>((item == null) ? new List<Item>() : new List<Item> { item }),
			times = new NodeDisplayInfo.Element<int?>(woundCount),
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityLevelSuffix(SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState)))
		};
	}

	private static void PlaySpellEffects(GameEvent eve, ItemDefinition source)
	{
		ItemSpell obj = source?.itemSpells?.FirstOrDefault();
		obj?.spellEffectConfig?.Play(eve);
		obj?.spellEffect3DConfig?.Play(eve);
	}
}
