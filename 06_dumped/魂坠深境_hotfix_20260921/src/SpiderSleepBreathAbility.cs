using System.Collections.Generic;

public class SpiderSleepBreathAbility : Ability, ISpiderPerTargetPerformanceAbility, IEnemyIntentDisplayAbility
{
	public int damage = 1;

	public int damageTimes = 3;

	public int curseCount = 2;

	public ItemDefinition chronicDeath;

	public bool addSpawnItemEnchant;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (SpiderContractEnemyAbilityUtility.IsSelfSpellCast(eve) && damage > 0)
		{
			return damageTimes > 0;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		for (int i = 0; i < damageTimes; i++)
		{
			eve.gameState.actionManager.EnqueueAction(new DamageAction(new DamageCommand
			{
				baseDamage = damage,
				source = eve.CasterItem,
				targetBFT = eve.gameState.playerState.bft,
				overflow = true,
				spellCastInfo = eve.spellInfo
			}));
		}
		if (chronicDeath == null)
		{
			return;
		}
		for (int j = 0; j < curseCount; j++)
		{
			Item item = new Item(chronicDeath);
			if (addSpawnItemEnchant)
			{
				SpiderContractEnemyAbilityUtility.AddSpawnItemEnchant(item);
			}
			SpiderContractEnemyAbilityUtility.EnqueueAction2(eve);
			eve.gameState.actionManager.EnqueueAction(new SpawnItemAction(item, eve.gameState.playerState.TableContainer, withAnim: true, eve.CasterItem, () => 0));
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(curseCount)
		};
	}
}
