using System.Collections.Generic;
using System.Linq;

public class ConvertItemToDefinitionAction : ExecuteAction
{
	private readonly Item targetItem;

	private readonly ItemDefinition itemDefinition;

	public ConvertItemToDefinitionAction(Item targetItem, ItemDefinition itemDefinition)
	{
		this.targetItem = targetItem;
		this.itemDefinition = itemDefinition;
	}

	protected override void Update()
	{
		if (targetItem == null || itemDefinition == null)
		{
			return;
		}
		Item item = new Item(itemDefinition);
		targetItem.SetDef(item.GetDef());
		targetItem.counter = item.counter;
		targetItem.maxCounter = item.maxCounter;
		targetItem.resetInitCounter = item.resetInitCounter;
		targetItem.resetInitMaxCounter = item.resetInitMaxCounter;
		targetItem.blackboard = item.blackboard;
		targetItem.cooldown = item.cooldown;
		targetItem.deltaCD = item.deltaCD;
		targetItem.deltaCost = item.deltaCost;
		targetItem.currentSelectSpellIndex = item.currentSelectSpellIndex;
		targetItem.treasureValue = item.treasureValue;
		targetItem.sourceDeckItem = item.sourceDeckItem;
		targetItem.upgradeLevel = item.upgradeLevel;
		targetItem.skin = item.skin;
		targetItem.skinBase = item.skinBase;
		targetItem.transformActive = item.transformActive;
		targetItem.ClearRuntimeTransformTemplate();
		targetItem.SetRemoveAfterCast(value: false);
		targetItem.RemoveAllEnchants();
		foreach (Rune item2 in targetItem.Runes.ToList())
		{
			targetItem.RemoveRune(item2);
		}
		targetItem.tagInstances = new List<ItemTagInstance>();
		foreach (ItemTag tag in targetItem.def.tags)
		{
			targetItem.AddItemTag(tag);
		}
		if (targetItem.owner != null)
		{
			BattleStateInfo battleState = gameState.battleState;
			if (battleState != null && battleState.isInBattle)
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
			if (gameState.battleState != null && !targetItem.battleJoined && targetItem.owner.IsBattleContainer())
			{
				targetItem.battleJoined = true;
				EnqueueAction(delegate
				{
					gameState.gameEventManager.TriggerJoinItemBattle(targetItem);
				});
			}
		}
		EnqueueAction(new CleanInvalidEnchantAndRuneAction(targetItem));
	}
}
