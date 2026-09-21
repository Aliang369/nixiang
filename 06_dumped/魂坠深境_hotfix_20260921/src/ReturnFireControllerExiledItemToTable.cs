using System.Collections.Generic;

public class ReturnFireControllerExiledItemToTable : FireControllerExileEffect
{
	public bool chooseWhenUpgraded = true;

	public bool chooseWhenSkinBranch = true;

	public bool exileAfterCast = true;

	public bool exileWhenSkinBranch;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform bft = FireControllerExileEffect.GetCasterBft(eve);
		ItemContainer table = bft?.tableContainer;
		if (table == null)
		{
			return NodeCastResult.Success;
		}
		if (ShouldExileCaster(eve))
		{
			Effect.EnqueueAction(eve, new ExileItemAction(eve.CasterItem, eve.CasterItem));
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				ReturnExiledItem(eve, bft, table);
			}));
			return NodeCastResult.Success;
		}
		ReturnExiledItem(eve, bft, table);
		return NodeCastResult.Success;
	}

	private void ReturnExiledItem(GameEvent eve, BattleFieldTransform bft, ItemContainer table)
	{
		List<Item> list = bft?.exilePileItemContainer?.GetAllItems();
		if (list == null || list.Count == 0)
		{
			return;
		}
		if (chooseWhenUpgraded)
		{
			Item casterItem = eve.CasterItem;
			if (casterItem != null && casterItem.upgraded)
			{
				goto IL_0070;
			}
		}
		if (!chooseWhenSkinBranch || !FireControllerExileEffect.IsSkinBranch(eve))
		{
			int num = eve.gameState.randomManager.Next() % list.Count;
			if (num < 0)
			{
				num += list.Count;
			}
			Effect.EnqueueAction(eve, new TransferItemAction(list[num], table, eve.CasterItem, TransferItemAction.TransferAnimType.Transfer, () => 0));
			return;
		}
		goto IL_0070;
		IL_0070:
		List<Item> selectedItems = new List<Item>();
		SelectItemInfo selectItemInfo = SelectItemInfo.SimpleSelectSingleItem(list, delegate
		{
			if (selectedItems != null && selectedItems.Count != 0 && selectedItems[0] != null)
			{
				Effect.EnqueueAction(eve, new TransferItemAction(selectedItems[0], table, eve.CasterItem, TransferItemAction.TransferAnimType.Transfer, () => 0));
			}
		}, null, selectedItems, null, () => ScriptableObjectSingleton<TextDataPreset>.instance.term_select_one_exiled_card_to_draw_pile_top);
		selectItemInfo.selectorBft = bft;
		Effect.EnqueueAction(eve, new SelectItemAction(selectItemInfo, PlayerSelectItemsEffect.SelectPanelType.PileSelect));
	}

	private bool ShouldExileCaster(GameEvent eve)
	{
		if (!exileAfterCast || eve.CasterItem == null)
		{
			return false;
		}
		if (FireControllerExileEffect.IsSkinBranch(eve) && !exileWhenSkinBranch)
		{
			return false;
		}
		return true;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int num;
		if (chooseWhenUpgraded)
		{
			Item casterItem = eve.CasterItem;
			if (casterItem != null && casterItem.upgraded)
			{
				num = 1;
				goto IL_0030;
			}
		}
		num = ((chooseWhenSkinBranch && FireControllerExileEffect.IsSkinBranch(eve)) ? 1 : 0);
		goto IL_0030;
		IL_0030:
		bool value = (byte)num != 0;
		return new NodeDisplayInfo
		{
			outputBool = new NodeDisplayInfo.Element<bool?>(value)
		};
	}
}
