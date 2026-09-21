using System.Linq;

public class ExileItemAction : ExecuteAction
{
	public enum ExileType
	{
		Combat,
		Permanent
	}

	private const string FireSeedEnchantGuid = "97fb47d81f913754aa2ac35e19f09a57";

	private const string AttackTagGuid = "4c915235a6aef3c4fb082e1a99cd01de";

	private readonly HitNFXConfig hitEffect;

	private readonly object source;

	private readonly Item targetItem;

	private readonly ExileType exileType;

	public ExileItemAction(Item targetItem, object source, HitNFXConfig hitEffect = null, ExileType exileType = ExileType.Combat)
	{
		this.targetItem = targetItem;
		this.source = source;
		this.hitEffect = hitEffect;
		this.exileType = exileType;
		Ignore(ActionLock.Type.RemoveItem);
	}

	protected override void Update()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (targetItem == null || targetItem.BlocksExternalExile(source, exileType) || gameState.gameEventManager.TriggerExileResolveInfo(targetItem, source, exileType, GameEvent.TriggerType.Real).reject)
		{
			return;
		}
		ItemContainer originContainer = targetItem.owner;
		if (exileType == ExileType.Combat && TryResolveFireSeed(originContainer))
		{
			return;
		}
		object obj;
		if (exileType == ExileType.Combat)
		{
			ItemContainer itemContainer = originContainer;
			if (itemContainer != null && itemContainer.IsBattleContainer())
			{
				obj = targetItem.OwnerBft?.exilePileItemContainer;
				goto IL_00ba;
			}
		}
		obj = null;
		goto IL_00ba;
		IL_00ba:
		ItemContainer targetExilePile = (ItemContainer)obj;
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerBeforeExileItem(targetItem, source, originContainer);
		});
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_destroy);
		if (targetItem.owner != null && targetItem.owner.type != ItemContainer.ContainerType.Deck)
		{
			hitEffect?.Play(gameState, targetItem);
			EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_exile, targetItem));
		}
		EnqueueAction(delegate
		{
			BattleFieldTransform ownerBft = targetItem.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				ItemContainer owner = targetItem.owner;
				if (owner != null && owner.type == ItemContainer.ContainerType.Table)
				{
					gameState.UIManager.playerItemArea.OnItemExile(targetItem);
				}
			}
			BattleFieldTransform ownerBft2 = targetItem.OwnerBft;
			if (ownerBft2 != null && ownerBft2.isEnemy)
			{
				ItemContainer owner2 = targetItem.owner;
				if (owner2 != null && owner2.type == ItemContainer.ContainerType.Table)
				{
					gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnExileItem(targetItem);
				}
			}
		});
		EnqueueAction(new RemoveItemAction(targetItem, source));
		if (targetExilePile != null)
		{
			EnqueueAction(delegate
			{
				targetExilePile.AddItemSilently(targetItem);
			});
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerExileItem(targetItem, source, originContainer);
		});
	}

	private bool TryResolveFireSeed(ItemContainer originContainer)
	{
		if (targetItem != null)
		{
			ItemContainer itemContainer = originContainer;
			if (itemContainer != null && itemContainer.IsBattleContainer())
			{
				Enchant enchant = targetItem.Enchants?.FirstOrDefault((Enchant enchant2) => enchant2?.def?.id == "97fb47d81f913754aa2ac35e19f09a57" && (int)enchant2.value > 0);
				if (enchant == null)
				{
					return false;
				}
				EnqueueAction(delegate
				{
					gameState.gameEventManager.TriggerBeforeExileItem(targetItem, source, originContainer);
				});
				EnqueueAction(delegate
				{
					gameState.gameEventManager.TriggerExileItem(targetItem, source, originContainer);
				});
				EnqueueAction(new ChangeEnchantAction(targetItem, targetItem, enchant.def, -1));
				if (IsAttackItem(targetItem))
				{
					EnqueueAction(new TriggerCastItemAction(targetItem, targetItem, TriggerTargetSpell.TargetingType.Default, autoTargetForPlayer: true, useSourceAsTriggerAnchor: true, suppressRemoveAfterCast: true, triggerFromProtectedExile: true));
				}
				return true;
			}
		}
		return false;
	}

	private static bool IsAttackItem(Item item)
	{
		if (item == null || item.def?.tags?.Any((ItemTag tag) => tag?.id == "4c915235a6aef3c4fb082e1a99cd01de") != true)
		{
			if (item == null)
			{
				return false;
			}
			return item.tagInstances?.Any((ItemTagInstance tagInstance) => tagInstance?.tag?.id == "4c915235a6aef3c4fb082e1a99cd01de") == true;
		}
		return true;
	}
}
