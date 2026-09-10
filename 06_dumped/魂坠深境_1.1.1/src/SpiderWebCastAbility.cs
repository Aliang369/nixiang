using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class SpiderWebCastAbility : Ability, ISpiderPerTargetPerformanceAbility, IEnemyIntentDisplayAbility
{
	public EnchantDef webbedEnchant;

	public int targetCount = 3;

	public int webbedStacks = 1;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve) && SpiderContractEnemyAbilityUtility.IsSelfSpellCast(eve) && webbedEnchant != null)
		{
			return targetCount > 0;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		List<Item> allItems = eve.gameState.playerState.TableContainer.GetAllItems();
		int num = Mathf.Min(targetCount, allItems.Count);
		if (num <= 0)
		{
			return;
		}
		foreach (Item item in allItems.SelectRandomCountUnique(num, eve.gameState.randomManager.NextDouble))
		{
			SpiderContractEnemyAbilityUtility.EnqueueAction2(eve);
			eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(item, eve.CasterItem, webbedEnchant, webbedStacks));
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(targetCount)
		};
	}
}
