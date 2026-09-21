using System.Collections.Generic;
using System.Linq;

public class ConvertItemAction : ExecuteAction
{
	public Item convertTemplate;

	public Item targetItem;

	public ConvertItemAction(Item targetItem, Item convertTemplate)
	{
		this.targetItem = targetItem;
		this.convertTemplate = convertTemplate;
	}

	protected override void Update()
	{
		if (targetItem == null || convertTemplate == null)
		{
			return;
		}
		List<Rune> list = targetItem.Runes.ToList();
		foreach (Rune item in list)
		{
			targetItem.RemoveRune(item);
		}
		targetItem.SetDef(convertTemplate.GetDef());
		targetItem.skin = convertTemplate.skin;
		targetItem.skinBase = convertTemplate.skinBase;
		targetItem.counter = convertTemplate.counter;
		targetItem.maxCounter = convertTemplate.maxCounter;
		targetItem.resetInitCounter = convertTemplate.resetInitCounter;
		targetItem.resetInitMaxCounter = convertTemplate.resetInitMaxCounter;
		targetItem.blackboard = convertTemplate.blackboard;
		targetItem.deltaCost = convertTemplate.deltaCost;
		targetItem.treasureValue = convertTemplate.treasureValue;
		targetItem.sourceDeckItem = convertTemplate.sourceDeckItem;
		targetItem.battleJoined = convertTemplate.battleJoined;
		targetItem.upgradeLevel = convertTemplate.upgradeLevel;
		targetItem.tagInstances = convertTemplate.tagInstances;
		targetItem.tagInstances = new List<ItemTagInstance>();
		foreach (ItemTag tag in targetItem.def.tags)
		{
			targetItem.AddItemTag(tag);
		}
		foreach (Rune item2 in list)
		{
			if (item2.def.CheckAddConstraint(targetItem) == true)
			{
				targetItem.AddRune(item2);
			}
		}
		if (targetItem.owner != null)
		{
			if (gameState.battleState.isInBattle)
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
			if (!targetItem.battleJoined && targetItem.owner.IsBattleContainer())
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
