using System.Collections.Generic;
using UnityEngine;

public class YinYangStateConvertItem : Ability
{
	[SerializeField]
	private ItemDefinition yin;

	[SerializeField]
	private ItemDefinition yang;

	[SerializeField]
	private ItemDefinition defaultItem;

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.type != GameEvent.Type.ConvertYinYangPower && eve.type != GameEvent.Type.AddItem)
		{
			return false;
		}
		if (eve.CasterItem?.GetDef() == GetItemDef(eve))
		{
			return false;
		}
		return true;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (IsPlayerAnimAble(eve))
		{
			if (eve.CasterEnchant != null)
			{
				TriggerAnim(eve.gameState, eve.CasterEnchant, eve.CasterEnchant.attachItem);
			}
			else if (eve.CasterRune != null)
			{
				TriggerAnim(eve.gameState, eve.CasterRune, eve.CasterRune.attachItem);
			}
			else if (eve.CasterInfluence != null)
			{
				TriggerAnim(eve.gameState, eve.CasterInfluence);
			}
			else if (eve.CasterItem != null)
			{
				TriggerAnim(eve.gameState, eve.CasterItem);
			}
		}
		spellEffectConfig?.Play(eve);
		spellEffect3DConfig?.Play(eve);
		Item item = eve.CasterItem.Copy();
		item.SetDef(GetItemDef(eve));
		EnqueueAction(eve, new ConvertItemAction(eve.CasterItem, item));
		if (IsPlayerAnimAble(eve) && eve.CasterItem != null)
		{
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				TriggerAnimEnd(eve.gameState, eve.CasterItem);
			}));
		}
	}

	public ItemDefinition GetItemDef(GameEvent eve)
	{
		if (eve.gameState.trigramsManager.GetCurrentState().HasFlag(TrigramsManager.YinYangStateType.Yin))
		{
			return yin;
		}
		if (eve.gameState.trigramsManager.GetCurrentState().HasFlag(TrigramsManager.YinYangStateType.Yang))
		{
			return yang;
		}
		return defaultItem;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.ConvertYinYangPower;
		yield return GameEvent.Type.AddItem;
	}
}
