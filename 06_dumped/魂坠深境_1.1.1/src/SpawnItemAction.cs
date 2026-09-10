using System;
using System.Collections;
using UnityEngine;

public class SpawnItemAction : ExecuteAction, ISpellCastContextAction
{
	private readonly Func<int> insertIndexer;

	private readonly object source;

	private readonly Item target;

	private readonly ItemContainer targetContainer;

	private readonly bool withAnim;

	private SpellCastInfo spellInfo;

	private readonly bool suppressAfterTriggerChapterEffectsUntilQueueClean;

	private readonly Action<Item> onItemSpawned;

	public SpellCastInfo SpellInfo
	{
		get
		{
			return spellInfo;
		}
		set
		{
			spellInfo = value;
		}
	}

	private Item SourceItem => source as Item;

	public SpawnItemAction(Item target, ItemContainer targetContainer, bool withAnim = false, object source = null, Func<int> insertIndexer = null, bool suppressAfterTriggerChapterEffectsUntilQueueClean = false, SpellCastInfo spellInfo = null, Action<Item> onItemSpawned = null)
	{
		this.target = target;
		this.targetContainer = targetContainer;
		this.withAnim = withAnim;
		this.source = source;
		this.spellInfo = spellInfo;
		this.insertIndexer = insertIndexer;
		this.suppressAfterTriggerChapterEffectsUntilQueueClean = suppressAfterTriggerChapterEffectsUntilQueueClean;
		this.onItemSpawned = onItemSpawned;
	}

	protected override void Update()
	{
		Item targetItem = target;
		Item spawnedItem = null;
		if (targetContainer == null)
		{
			return;
		}
		SpellCastInfo spellCastInfo = spellInfo;
		if (spellCastInfo != null && spellCastInfo.suppressChangeEnchantAndSpawnItemEffects)
		{
			return;
		}
		if (!targetContainer.CanAcceptItemWithinHardLimit(targetItem))
		{
			gameState.actionManager.ShowTableItemHardLimitHintOnce();
			return;
		}
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerSpawnItemResolve(target, targetContainer);
		if (resolveInfo.replaceItem != null)
		{
			targetItem = resolveInfo.replaceItem;
		}
		if (suppressAfterTriggerChapterEffectsUntilQueueClean)
		{
			gameState.actionManager.SuppressAfterTriggerChapterEffectsUntilQueueClean(targetItem);
		}
		PlayerItemDisplay preSpawnedPlayerDisplay = null;
		WrapperEnumerator onPreAddItem = gameState.StartCoroutine(OnPreAddItem);
		int num;
		if (!targetItem.battleJoined)
		{
			num = (targetContainer.IsBattleContainer() ? 1 : 0);
			if (num != 0)
			{
				targetItem.battleJoined = true;
			}
		}
		else
		{
			num = 0;
		}
		AddItemToContainerAction addItemToContainerAction = new AddItemToContainerAction(targetContainer, targetItem, source, AddItemEventInfo.AddType.Spawn, insertIndexer, onPreAddItem, OnAddItem, OnAddItemRejected);
		addItemToContainerAction.Ignore(ActionLock.Type.AddItem);
		gameState.actionManager.EnqueueAction(addItemToContainerAction);
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerSpawnItem(targetItem, source, targetContainer);
		});
		if (num != 0)
		{
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerJoinItemBattle(targetItem);
			});
		}
		EnqueueAction(delegate
		{
			if (spawnedItem != null)
			{
				onItemSpawned?.Invoke(spawnedItem);
			}
		});
		void OnAddItem(Item addedItem)
		{
			spawnedItem = addedItem;
			if (withAnim)
			{
				if (addedItem != null && addedItem.OwnerBft?.isPlayer == true)
				{
					ItemContainer owner = addedItem.owner;
					if (owner != null && owner.type == ItemContainer.ContainerType.Table)
					{
						gameState.UIManager.playerItemArea.OnSpawnItem(targetItem);
						return;
					}
				}
				if (addedItem != null && addedItem.OwnerBft?.isEnemy == true)
				{
					ItemContainer owner2 = addedItem.owner;
					if (owner2 != null && owner2.type == ItemContainer.ContainerType.Table)
					{
						gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnSpawnItem(addedItem, SourceItem);
						return;
					}
				}
				if ((addedItem != null && addedItem.owner?.type == ItemContainer.ContainerType.Deck) || (addedItem != null && addedItem.owner?.type == ItemContainer.ContainerType.DrawPile) || (addedItem != null && addedItem.owner?.type == ItemContainer.ContainerType.WastePile))
				{
					Vector3? vector = SourceItem?.GetPivot(gameState)?.ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
					Vector3? obj;
					if (!vector.HasValue)
					{
						ConvertablePivot pivot = targetItem.GetPivot(gameState);
						obj = ((pivot != null) ? new Vector3?(pivot.ToCanvasWorldPosition(gameState.UIManager.mainCanvas)) : targetContainer?.OwnerBft?.GetUIPivot(gameState)?.ToCanvasWorldPosition(gameState.UIManager.mainCanvas));
					}
					else
					{
						obj = vector;
					}
					Vector3? startShowPosition = obj;
					Vector3? vector2 = gameState.pivotManager.GetPivot(PivotManager.PivotType.EnemyPivot)?.ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
					Vector3? vector3 = addedItem?.GetPivot(gameState)?.ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
					if (startShowPosition.HasValue && vector2.HasValue && vector3.HasValue)
					{
						ItemInfoDisplayBase.AnimSetupInfo setupInfo = new ItemInfoDisplayBase.AnimSetupInfo
						{
							animType = ItemInfoDisplayBase.AnimSetupInfo.AnimType.Stay,
							startShowPosition = startShowPosition,
							stayPosition = vector2.Value,
							hidePosition = vector3.Value,
							flip = true
						};
						gameState.UIManager.battleItemInfoPanel.StartTween(addedItem, setupInfo);
					}
				}
			}
		}
		void OnAddItemRejected(Item rejectedItem)
		{
			if (!(preSpawnedPlayerDisplay == null))
			{
				gameState.UIManager.playerItemArea.CancelPreSpawnItem(preSpawnedPlayerDisplay);
				preSpawnedPlayerDisplay = null;
			}
		}
		IEnumerator OnPreAddItem()
		{
			if (withAnim)
			{
				ItemContainer itemContainer = targetContainer;
				if (itemContainer != null && itemContainer.owner?.isPlayer == true && targetContainer.type == ItemContainer.ContainerType.Table)
				{
					int num2 = insertIndexer?.Invoke() ?? 0;
					if (num2 == -1)
					{
						num2 = targetContainer.ItemsCount;
					}
					num2 = Mathf.Clamp(num2, 0, targetContainer.ItemsCount);
					yield return gameState.UIManager.playerItemArea.PreSpawnItem(targetItem, SourceItem, num2, delegate(PlayerItemDisplay display)
					{
						preSpawnedPlayerDisplay = display;
					});
				}
			}
		}
	}
}
