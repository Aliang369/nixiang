using UnityEngine;

public class ForgetShrinePointReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public CharacterClassDef characterClassDef;

	public override string GlobalItemTitle => GetRewardTitle(characterClassDef);

	public override string GlobalItemCellName => GetName(characterClassDef);

	public override Sprite GlobalItemCellIcon => GetIcon(characterClassDef);

	public override QualityLevel GlobalItemQuality => QualityLevel.Rare;

	public override string GetRewardBrief()
	{
		return GlobalItemTitle;
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return GetDescription(characterClassDef);
	}

	public static string GetName(CharacterClassDef characterClassDef)
	{
		string text = characterClassDef?.characterName;
		if (!string.IsNullOrEmpty(text))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_forget_wax_seal_name_with_class.FormatWith(text);
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_forget_wax_seal_name;
	}

	public static string GetRewardTitle(CharacterClassDef characterClassDef)
	{
		return GetName(characterClassDef);
	}

	public static string GetDescription(CharacterClassDef characterClassDef)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_forget_wax_seal_reward_description.FormatWith(characterClassDef?.characterName ?? "");
	}

	public static Sprite GetIcon(CharacterClassDef characterClassDef)
	{
		if (!(characterClassDef?.forgetShrinePointIcon != null))
		{
			return ScriptableObjectSingleton<GameDataManager>.instance.forgetShrinePointIcon;
		}
		return characterClassDef.forgetShrinePointIcon;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			forgetShinePoint = true,
			giveAppendInfo = new GlobalRewardGiveAppendInfo
			{
				appendCharacterClass = characterClassDef
			}
		};
	}
}
