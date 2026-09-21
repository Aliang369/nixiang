using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnlockCTSOGlobalReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public List<CTScriptableObject> unlocks;

	public override bool isHidden => true;

	public override Sprite GlobalItemCellIcon
	{
		get
		{
			if (unlocks.Count >= 1 && unlocks[0] != null && unlocks[0] is IAssetIcon assetIcon)
			{
				return assetIcon.IconAsset;
			}
			return ScriptableObjectSingleton<GameDataManager>.instance.itemPlaceHolderIcon;
		}
	}

	public override Item GlobalItemRefItem
	{
		get
		{
			if (unlocks == null || unlocks.Count < 1)
			{
				return null;
			}
			if (!(unlocks[0] is ItemDefinition def))
			{
				return null;
			}
			return new Item(def);
		}
	}

	public override Rune GlobalItemRefRune
	{
		get
		{
			if (unlocks == null || unlocks.Count < 1)
			{
				return null;
			}
			if (!(unlocks[0] is RuneDef def))
			{
				return null;
			}
			return new Rune(def);
		}
	}

	public override Influence globalItemRefInfluence
	{
		get
		{
			if (unlocks == null || unlocks.Count < 1)
			{
				return null;
			}
			if (!(unlocks[0] is InfluenceDef def))
			{
				return null;
			}
			return new Influence(def);
		}
	}

	public static string GetUnlockCTSOBrief(List<CTScriptableObject> unlocks)
	{
		if (unlocks.TrueForAll((CTScriptableObject u) => u is ItemDefinition))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_reward_unlock_New_Item_arg0;
		}
		if (unlocks.TrueForAll((CTScriptableObject u) => u is InfluenceDef))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_reward_unlock_New_Influence_arg0;
		}
		if (unlocks.TrueForAll((CTScriptableObject u) => u is WorldEvent))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_new_unlock_New_WorldEvent_arg0;
		}
		if (unlocks.TrueForAll((CTScriptableObject u) => u is RuneDef))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_new_unlock_New_Rune_arg0;
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_reward_unlock_New_CTSO_arg0;
	}

	public static string GetUnlockCTSODetail(List<CTScriptableObject> unlocks)
	{
		if (unlocks.TrueForAll((CTScriptableObject u) => u is ItemDefinition))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_reward_unlock_New_Item_arg1.FormatWith(unlocks.Select((CTScriptableObject c) => (c as ItemDefinition).itemName).Aggregate((string a, string b) => a + " " + b));
		}
		if (unlocks.TrueForAll((CTScriptableObject u) => u is InfluenceDef))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_reward_unlock_New_Influence_arg1.FormatWith(unlocks.Select((CTScriptableObject c) => (c as InfluenceDef).InfluenceName).Aggregate((string a, string b) => a + " " + b));
		}
		if (unlocks.TrueForAll((CTScriptableObject u) => u is WorldEvent))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_new_unlock_New_WorldEvent_arg1.FormatWith(unlocks.Select((CTScriptableObject c) => (c as WorldEvent).EventName).Aggregate((string a, string b) => a + " " + b));
		}
		if (unlocks.TrueForAll((CTScriptableObject u) => u is RuneDef))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_new_unlock_New_Rune_arg1.FormatWith(unlocks.Select((CTScriptableObject c) => (c as RuneDef).Name).Aggregate((string a, string b) => a + " " + b));
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_reward_unlock_New_CTSO_arg1.FormatWith(unlocks.Select((CTScriptableObject c) => c.name).Aggregate((string a, string b) => a + " " + b));
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			unlockCTSOs = unlocks
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(string.Join(".", unlocks.ConvertAll((CTScriptableObject u) => u?.name ?? "")));
	}
}
