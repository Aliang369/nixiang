using System.Collections.Generic;
using System.Linq;

public static class TAFormatter
{
	public static List<string> FormatItemList(List<Item> items)
	{
		try
		{
			return items.ConvertAll((Item it) => it.def.FullID);
		}
		catch
		{
			return new List<string>();
		}
	}

	public static List<Dictionary<string, object>> FormatItemDetailList(List<Item> items)
	{
		return items.Select(FormatItemDetail).ToList();
	}

	public static Dictionary<string, object> FormatItemDetail(Item item)
	{
		return new Dictionary<string, object>().Append("id", item.def.FullID).Append("upgrade_level", item.upgradeLevel).Append("counter", item.counter)
			.Append("runes", FormatRuneList(item.Runes.ToList()))
			.Append("skin", item.skin?.FullID);
	}

	public static Dictionary<string, object> FormatQuestDetail(RoutineQuest quest)
	{
		try
		{
			return new Dictionary<string, object>().Append("id", quest.def.FullID).Append("progress", quest.progress).Append("finished", quest.AllLayerFinished)
				.Append("target_id", quest.character?.FullID ?? quest.skill?.FullID);
		}
		catch
		{
			return new Dictionary<string, object>();
		}
	}

	public static List<Dictionary<string, object>> FormatGlobalRewardDetailList(List<GlobalRewardInfo> rewards)
	{
		return rewards.Select(FormatGlobalRewardDetail).ToList();
	}

	public static Dictionary<string, object> FormatGlobalRewardDetail(GlobalRewardInfo rewardInfo)
	{
		return new Dictionary<string, object>().Append("type", rewardInfo.RewardType.ToString()).Append("source_type", rewardInfo.rewardSourceType.ToString()).Append("source_id", rewardInfo.rewardSourceId)
			.Append("target_id", rewardInfo.RewardTargetId)
			.Append("count", ((IGlobalItemDisplaySetup)rewardInfo).GlobalItemCount)
			.Append("repeat", rewardInfo.giveAppendInfo?.repeated ?? false);
	}

	public static List<Dictionary<string, object>> FormatLevelLineStateList(List<LevelLine> lines)
	{
		return lines.Select(FormatLevelLineState).ToList();
	}

	public static Dictionary<string, object> FormatLevelLineState(LevelLine line)
	{
		return new Dictionary<string, object>().Append("id", line.levelLineDef.FullID).Append("level", line.currentLevelIndex).Append("current_exp", line.currentExp)
			.Append("is_overflow", line.IsMaxLevel);
	}

	public static List<Dictionary<string, object>> FormatRoutineQuestList(List<RoutineQuest> quests)
	{
		return quests.Select(FormatQuestDetail).ToList();
	}

	public static List<Dictionary<string, object>> FormatSkillDetailList(IEnumerable<GameSkill> skills)
	{
		return skills.Select(FormatSkillDetail).ToList();
	}

	public static List<Dictionary<string, object>> FormatSkillDetailList(IEnumerable<ProfileSkill> skills)
	{
		return skills.Select(FormatSkillDetail).ToList();
	}

	public static Dictionary<string, object> FormatSkillDetail(GameSkill skill)
	{
		if (skill?.skillDef == null || skill.profileSkill == null)
		{
			return new Dictionary<string, object>();
		}
		return new Dictionary<string, object>().Append("id", skill.skillDef.FullID).Append("level", skill.QualityLevel.QualityToLevel()).Append("durability", skill.durability)
			.Append("archetype", skill.archeTypeIndex.ToString())
			.Append("gems", FormatSkillGemList(skill.profileSkill.skillGemsMounted))
			.Append("slot_count", skill.profileSkill.HexShapeSlotCount);
	}

	public static Dictionary<string, object> FormatSkillDetail(ProfileSkill skill)
	{
		return FormatSkillDetail(new GameSkill(skill));
	}

	public static List<string> FormatSkillGemList(IEnumerable<SkillGem> skillGems)
	{
		if (skillGems == null)
		{
			return new List<string>();
		}
		return skillGems.Select(FormatSkillGem).ToList();
	}

	public static string FormatSkillGem(SkillGem skillGem)
	{
		return skillGem.def.FullID;
	}

	public static List<Dictionary<string, object>> FormatDamageDetailList(List<Damage> damages)
	{
		return damages.Select(FormatDamageDetail).ToList();
	}

	public static Dictionary<string, object> FormatDamageDetail(Damage damage)
	{
		string value = ((damage.targetBFT == null) ? "未知" : (damage.targetBFT.isPlayer ? "玩家" : "敌人"));
		string value2 = "未知";
		string value3 = "";
		if (damage.source is Item item)
		{
			value2 = "卡牌";
			value3 = item.def.FullID;
		}
		else if (damage.source is Enchant enchant)
		{
			value2 = "附魔";
			value3 = enchant.def.FullID;
		}
		else if (damage.source is Rune rune)
		{
			value2 = "印记";
			value3 = rune.def.FullID;
		}
		else if (damage.source is Influence influence)
		{
			value2 = "祝福";
			value3 = influence.def.FullID;
		}
		return new Dictionary<string, object>().Append("subcounter", damage.subCounter).Append("target_type", value).Append("source_type", value2)
			.Append("source_id", value3);
	}

	public static List<string> FormatLineLevelList(IEnumerable<LevelLineDef.LineLevel> lineLevels, LevelLine line)
	{
		return lineLevels.Select((LevelLineDef.LineLevel l) => FormatLineLevel(l, line)).ToList();
	}

	public static string FormatLineLevel(LevelLineDef.LineLevel level, LevelLine line)
	{
		if (level == null)
		{
			return "";
		}
		return line.levelLineDef.levels.IndexOf(level).ToString();
	}

	public static List<string> FormatRuneList(List<Rune> runes)
	{
		return runes.ConvertAll((Rune r) => r.def.FullID);
	}

	public static List<string> FormatSkillList(List<ProfileSkill> skills)
	{
		return skills.ConvertAll((ProfileSkill r) => r.def.FullID);
	}

	public static List<string> FormatSkillList(List<GameSkill> skills)
	{
		return skills.ConvertAll((GameSkill r) => r.skillDef.FullID);
	}

	public static List<string> FormatSkillWordList(List<SkillWord> words)
	{
		return words.ConvertAll((SkillWord r) => r.def.FullID);
	}

	public static List<string> FormatInfluenceList(List<Influence> infs)
	{
		return infs.ConvertAll((Influence r) => r.def.FullID);
	}

	public static List<string> FormatItemSkinList(List<ItemSkinDef> skins)
	{
		return skins.ConvertAll((ItemSkinDef s) => s?.FullID ?? "");
	}

	public static List<Dictionary<string, object>> FormatMatterInfoList(List<MatterInfo> matterInfos)
	{
		return matterInfos.ConvertAll<Dictionary<string, object>>(FormatMatterInfo);
	}

	public static Dictionary<string, object> FormatMatterInfo(MatterInfo matterInfo)
	{
		return new Dictionary<string, object>().Append("id", matterInfo.matterDef.FullID).Append("count", matterInfo.count);
	}

	public static List<Dictionary<string, object>> FormatLevelLineList(List<LevelLine> levellines)
	{
		return levellines.ConvertAll<Dictionary<string, object>>(FormatLevelLine);
	}

	public static Dictionary<string, object> FormatLevelLine(LevelLine levelline)
	{
		return new Dictionary<string, object>().Append("id", levelline.levelLineDef.FullID).Append("level", levelline.currentLevelIndex).Append("current_exp", levelline.currentExp)
			.Append("is_overflow", levelline.IsMaxLevel);
	}

	public static Dictionary<string, object> FormatDefender(PVPDefender defender)
	{
		return new Dictionary<string, object>().Append("guid", defender.guid).Append("defender_name", defender.defenderName).Append("avatar_id", defender.enemyAvatarRef.CTAssetId)
			.Append("comp_list", FormatDeckPresetCompList(defender.deckPreset.comps));
	}

	public static List<string> FormatDeckPresetCompList(List<DeckPresetComp> comps)
	{
		return comps.ConvertAll<string>(FormatDeckPresetComp);
	}

	public static string FormatDeckPresetComp(DeckPresetComp comp)
	{
		if (comp.def == null)
		{
			return "";
		}
		return comp.def.TargetAssetID;
	}
}
