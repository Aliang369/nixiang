using System.Collections.Generic;
using System.Linq;

public class ConvertItemPreserveProgressAction : ExecuteAction
{
	private readonly Item targetItem;

	private readonly ItemDefinition targetDefinition;

	public ConvertItemPreserveProgressAction(Item targetItem, ItemDefinition targetDefinition)
	{
		this.targetItem = targetItem;
		this.targetDefinition = targetDefinition;
	}

	protected override void Update()
	{
		if (targetItem != null && !(targetDefinition == null))
		{
			ItemDefinition def = targetItem.GetDef();
			Item sourceDeckItem = targetItem.sourceDeckItem;
			bool battleJoined = targetItem.battleJoined;
			int num = ResolveTargetUpgradeLevel(targetItem.upgradeLevel, targetDefinition);
			RemoveSourceInherentEnchants(def);
			List<Enchant> enchants = targetItem.Enchants.ToList();
			List<Rune> runes = targetItem.Runes.ToList();
			DetachPreservedAttachments(enchants, runes);
			Item template = BuildTargetTemplate();
			ApplyTemplateState(template, sourceDeckItem, battleJoined);
			ApplyTargetInherentEnchants(template);
			for (int i = 0; i < num; i++)
			{
				targetItem.Upgrade(gameState);
			}
			RestorePreservedAttachments(enchants, runes);
			RefreshConvertedItemDisplay();
		}
	}

	public static int ResolveTargetUpgradeLevel(int sourceUpgradeLevel, ItemDefinition definition)
	{
		if (sourceUpgradeLevel <= 0 || (object)definition == null || !definition.upgradeable)
		{
			return 0;
		}
		if (!definition.infiniteUpgrade)
		{
			return 1;
		}
		return sourceUpgradeLevel;
	}

	private Item BuildTargetTemplate()
	{
		return new Item(targetDefinition).Copy();
	}

	private void ApplyTemplateState(Item template, Item sourceDeckItem, bool battleJoined)
	{
		targetItem.SetDef(template.GetDef());
		targetItem.afterTurnEnd = template.afterTurnEnd;
		targetItem.blackboard = template.blackboard;
		targetItem.cooldown = template.cooldown;
		targetItem.counter = template.counter;
		targetItem.maxCounter = template.maxCounter;
		targetItem.resetInitCounter = template.resetInitCounter;
		targetItem.resetInitMaxCounter = template.resetInitMaxCounter;
		targetItem.pendingChapterResetMaxCounterDelta = template.pendingChapterResetMaxCounterDelta;
		targetItem.currentSelectSpellIndex = template.currentSelectSpellIndex;
		targetItem.deltaCD = template.deltaCD;
		targetItem.deltaCost = template.deltaCost;
		targetItem.runeSlot = template.runeSlot;
		targetItem.treasureValue = template.treasureValue;
		targetItem.upgradeLevel = template.upgradeLevel;
		targetItem.skin = template.skin;
		targetItem.skinBase = template.skinBase;
		targetItem.indexOverrideOwnerName = template.indexOverrideOwnerName;
		targetItem.tempValues = new Dictionary<string, object>(template.tempValues);
		targetItem.tagInstances = (from tag in template.tagInstances.OrEmptyIfNull()
			select tag.CopyToItem(targetItem)).ToList();
		targetItem.transformActive = template.transformActive;
		targetItem.ClearRuntimeTransformTemplate();
		targetItem.SetRemoveAfterCast(template.removeAfterCast);
		targetItem.sourceDeckItem = sourceDeckItem;
		targetItem.battleJoined = battleJoined;
	}

	private void RemoveSourceInherentEnchants(ItemDefinition sourceDefinition)
	{
		foreach (EnchantDef inherentDef in (sourceDefinition?.inherentEnchants).OrEmptyIfNull())
		{
			if (inherentDef == null)
			{
				continue;
			}
			Enchant enchant = targetItem.Enchants.FirstOrDefault((Enchant existing) => existing?.def == inherentDef);
			if (enchant != null)
			{
				if (inherentDef.stacking && (int)enchant.value > 1)
				{
					enchant.value = (int)enchant.value - 1;
				}
				else
				{
					targetItem.RemoveEnchant(enchant);
				}
			}
		}
	}

	private void DetachPreservedAttachments(List<Enchant> enchants, List<Rune> runes)
	{
		foreach (Enchant enchant in enchants)
		{
			targetItem.RemoveEnchant(enchant);
		}
		foreach (Rune rune in runes)
		{
			targetItem.RemoveRuneSilently(rune);
		}
	}

	private void ApplyTargetInherentEnchants(Item template)
	{
		foreach (Enchant item in template.Enchants.OrEmptyIfNull())
		{
			if (!(item?.def == null))
			{
				targetItem.AddEnchant(item.CopyToItem(targetItem));
			}
		}
	}

	private void RestorePreservedAttachments(List<Enchant> enchants, List<Rune> runes)
	{
		foreach (Enchant enchant in enchants)
		{
			if (enchant?.def == null)
			{
				continue;
			}
			ItemConstraint addConstraint = enchant.def.addConstraint;
			if (addConstraint == null || addConstraint.CheckItemAccept(targetItem) != false)
			{
				Enchant enchant2 = targetItem.Enchants.FirstOrDefault((Enchant attached) => attached?.def == enchant.def);
				if (enchant2 == null)
				{
					targetItem.AddEnchant(enchant);
					ReapplyEnchantEffects(enchant, enchant.value, applyAddEffect: true);
				}
				else if (enchant2.def.stacking)
				{
					enchant2.value = (int)enchant2.value + (int)enchant.value;
					enchant2.afterTurnEnd |= enchant.afterTurnEnd;
					MergeTempValues(enchant2.tempValues, enchant.tempValues);
					ReapplyEnchantEffects(enchant2, enchant.value, applyAddEffect: false);
				}
			}
		}
		foreach (Rune rune in runes)
		{
			if (!(rune?.def == null) && rune.def.CheckAddConstraint(targetItem) == true)
			{
				Rune rune2 = targetItem.Runes.FirstOrDefault((Rune attached) => attached?.def == rune.def);
				if (rune2 == null)
				{
					targetItem.AddRune(rune);
				}
				else if (rune2.def.stacking)
				{
					MergeTempValues(rune2.tempValues, rune.tempValues);
					targetItem.ChangeRuneValue(rune2, rune2.value + rune.value);
				}
			}
		}
	}

	private void ReapplyEnchantEffects(Enchant enchant, int delta, bool applyAddEffect)
	{
		if (applyAddEffect && enchant.def.addEffect != null)
		{
			GameEvent eve = CreateEnchantEvent(GameEvent.Type.AfterAddEnchant, enchant, 0);
			enchant.def.addEffect.Cast(eve);
		}
		if (enchant.def.changeEffect != null && delta != 0)
		{
			GameEvent eve2 = CreateEnchantEvent(GameEvent.Type.EnchantValueChange, enchant, delta);
			enchant.def.changeEffect.Cast(eve2);
		}
	}

	private GameEvent CreateEnchantEvent(GameEvent.Type type, Enchant enchant, int delta)
	{
		GameEvent gameEvent = new GameEvent(type, GameEvent.TriggerType.Real);
		gameEvent.gameState = gameState;
		gameEvent.TargetEnchant = enchant;
		gameEvent.deltaValue = delta;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, targetItem);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, targetItem);
		gameEvent.SetCaster(enchant);
		return gameEvent;
	}

	private static void MergeTempValues(Dictionary<string, object> target, Dictionary<string, object> source)
	{
		if (target == null || source == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> item in source)
		{
			target[item.Key] = item.Value;
		}
	}

	private void RefreshConvertedItemDisplay()
	{
		if (targetItem.owner == null)
		{
			return;
		}
		GameStateInfo gameStateInfo = gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true)
		{
			if (targetItem.owner == gameState.battleState.PlayerTableContainer)
			{
				gameState.UIManager.playerItemArea.OnConvertItem(targetItem);
			}
			else if (targetItem.owner == gameState.battleState.EnemyTableContainer)
			{
				gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnConvertItem(targetItem);
			}
			else if (targetItem.owner == gameState.battleState.PlayerCompanionContainer)
			{
				gameState.UIManager.playerBftUIDisplay.companionItemArea.OnConvertItem(targetItem);
			}
			else if (targetItem.owner == gameState.battleState.EnemyCompanionContainer)
			{
				gameState.UIManager.enemyBftUIDisplay.companionItemArea.OnConvertItem(targetItem);
			}
		}
		if (gameState?.battleState != null && !targetItem.battleJoined && targetItem.owner.IsBattleContainer())
		{
			targetItem.battleJoined = true;
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerJoinItemBattle(targetItem);
			});
		}
	}
}
