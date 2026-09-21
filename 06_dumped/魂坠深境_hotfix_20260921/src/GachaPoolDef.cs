using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class GachaPoolDef : CTScriptableObject
{
	public enum GachaPoolType
	{
		Stable,
		SkillStable,
		Limited
	}

	public class GachaDrawBlock
	{
		public GachaContentType contentType;

		public QualityLevel qualityLevel;

		public int count;

		public int weight;

		public string ContentDisplayString => ScriptableObjectSingleton<TextDataPreset>.instance.GetQualityLevelText(qualityLevel, withColor: true) + " " + contentType.ToString() + $"X{count}";
	}

	public enum GachaContentType
	{
		None = 0,
		Obsession = 2,
		ObsessionDebris = 3,
		Skill = 4
	}

	[CTSOAssetName]
	public string poolName;

	[LocalizeText("卡池名称")]
	public string poolDisplayName;

	public Sprite gachaPoolIcon;

	[SerializeField]
	public GachaSet gachaSet;

	[TextArea(3, 100)]
	[LocalizeText("卡池基础规则描述")]
	public string baseRuleInfo;

	public int displayPriority;

	public MatterDef gachaUIPreviewMatter;

	public List<MatterDef> gachaUIPreviewMatters;

	public GachaPoolType poolType;

	public string pityGroupId;

	public UnityDateTime? startDate;

	public UnityDateTime? endDate;

	public MatterInfo singleDrawCost;

	public List<GachaDrawBlock> gachaDrawBlocks;

	public Dictionary<CTScriptableObject, int> baseGachaWeightContents;

	public bool enableGachaCountLimitRule;

	public int gachaCountLimit;

	public ResetTimeType gachaCountLimitResetType;

	public bool enableFreeDraw;

	public int freeDrawCount;

	public ResetTimeType freeDrawResetTimeType;

	public bool enableUPRule;

	public Dictionary<CTScriptableObject, int> UPGachaWeightContents;

	public int UPPercent;

	public bool enableHardPityRule;

	public int hardPityCount;

	public QualityLevel hardPityQualityLevel;

	public bool enableSoftPityRule;

	public QualityLevel softPityQualityLevel;

	public int softPityEffectCount;

	public int softPityMaxCount;

	public bool enableIntroSoftPityRule;

	public int introSoftPityCount;

	public bool enableGachaWishRule;

	public bool enableGrandPrizeRule;

	public int grandPrizeCount;

	public PlayerCharacterDef grandPrizeCharacter;

	public bool enableSkillGuideDrawRule;

	public List<SkillDef> gachaGuideDrawList;

	public bool generateSkillCompensateToken = true;

	public override string CTAssetIDPrepend => "GACHAPOOL";

	private string GachaBlockPreviewText
	{
		get
		{
			string text = "";
			if (gachaDrawBlocks == null || gachaDrawBlocks.Count == 0)
			{
				return "无抽取块";
			}
			float num = gachaDrawBlocks.Sum((GachaDrawBlock b) => b.weight);
			HashSet<CTScriptableObject> hashSet = new HashSet<CTScriptableObject>();
			foreach (GachaDrawBlock gachaDrawBlock in gachaDrawBlocks)
			{
				text = text + gachaDrawBlock.ContentDisplayString + " " + ((float)gachaDrawBlock.weight / num).ToString("P") + Environment.NewLine;
				List<string> list = new List<string>();
				if (baseGachaWeightContents != null)
				{
					foreach (CTScriptableObject key in baseGachaWeightContents.Keys)
					{
						if (key == null)
						{
							continue;
						}
						bool flag = false;
						if (key is PlayerCharacterDef)
						{
							if (gachaDrawBlock.contentType == GachaContentType.Obsession || gachaDrawBlock.contentType == GachaContentType.ObsessionDebris)
							{
								flag = true;
							}
						}
						else if (key is SkillDef skillDef && gachaDrawBlock.contentType == GachaContentType.Skill && skillDef.BaseQualityLevel == gachaDrawBlock.qualityLevel)
						{
							flag = true;
						}
						if (flag)
						{
							string text2 = key.name;
							if (key is SkillDef skillDef2)
							{
								text2 = text2 + "(" + skillDef2.attachCharacterClass?.characterName + ")";
							}
							list.Add(text2);
							hashSet.Add(key);
						}
					}
				}
				text = ((list.Count <= 0) ? (text + "  包含: (无匹配内容)" + Environment.NewLine) : (text + "  包含: " + string.Join(", ", list) + Environment.NewLine));
				text += Environment.NewLine;
			}
			if (baseGachaWeightContents != null)
			{
				List<string> list2 = new List<string>();
				foreach (CTScriptableObject key2 in baseGachaWeightContents.Keys)
				{
					if (!(key2 == null) && !hashSet.Contains(key2))
					{
						list2.Add(key2.name);
					}
				}
				if (list2.Count > 0)
				{
					text = text + "【未匹配任何抽取块的内容】" + Environment.NewLine;
					text = text + "  " + string.Join(", ", list2) + Environment.NewLine;
				}
			}
			return text;
		}
	}

	public GachaHistoryInfo GenerateWishObsession(PlayerCharacterDef character)
	{
		GachaHistoryInfo gachaHistoryInfo = GenerateCharacterObsession(character);
		gachaHistoryInfo.wish = true;
		return gachaHistoryInfo;
	}

	public GachaHistoryInfo GenerateGrandPrize()
	{
		return GenerateCharacterObsession(grandPrizeCharacter);
	}

	public GachaHistoryInfo GenerateGuideDraw(SkillDef guideSkill)
	{
		return GenerateSkill(guideSkill, GachaContentType.Skill);
	}

	public GachaHistoryInfo GenerateBaseBlockDraw(ApplicationStateInfo applicationStateInfo, GachaDrawBlock block)
	{
		if (block.contentType == GachaContentType.Obsession || block.contentType == GachaContentType.ObsessionDebris)
		{
			PlayerCharacterDef character = baseGachaWeightContents.Where((KeyValuePair<CTScriptableObject, int> kv) => kv.Key is PlayerCharacterDef).ToList().SelectRandomWeight((KeyValuePair<CTScriptableObject, int> kv) => kv.Value)
				.Key as PlayerCharacterDef;
			if (block.contentType == GachaContentType.Obsession)
			{
				return GenerateCharacterObsession(character);
			}
			if (block.contentType == GachaContentType.ObsessionDebris)
			{
				return GenerateCharacterObsessionDebris(character, block.count);
			}
		}
		if (block.contentType == GachaContentType.Skill)
		{
			SkillDef skill = baseGachaWeightContents.Where((KeyValuePair<CTScriptableObject, int> kv) => kv.Key is SkillDef skillDef && skillDef.BaseQualityLevel == block.qualityLevel).ToList().SelectRandomWeight((KeyValuePair<CTScriptableObject, int> kv) => kv.Value)
				.Key as SkillDef;
			return GenerateSkill(skill, GachaContentType.Skill);
		}
		return null;
	}

	public GachaHistoryInfo GenerateCharacterObsession(PlayerCharacterDef character)
	{
		QualityLevel qualityLevel = QualityLevel.Epic;
		GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo
		{
			matter = new MatterInfo(character.obsession, 1)
		};
		globalRewardInfo.baseTokenCompensate = ScriptableObjectSingleton<FortressDataManager>.instance.gachaBaseCompensateTokenDic[qualityLevel];
		return new GachaHistoryInfo
		{
			gachaPoolDef = this,
			contentType = GachaContentType.Obsession,
			content = character,
			qualityLevel = qualityLevel,
			reward = globalRewardInfo,
			wish = false,
			date = DateTime.UtcNow
		};
	}

	public GachaHistoryInfo GenerateCharacterObsessionDebris(PlayerCharacterDef character, int count)
	{
		QualityLevel qualityLevel = QualityLevel.Uncommon;
		GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo
		{
			matter = new MatterInfo(character.obsessionDebris, count)
		};
		globalRewardInfo.baseTokenCompensate = ScriptableObjectSingleton<FortressDataManager>.instance.gachaBaseCompensateTokenDic[qualityLevel];
		return new GachaHistoryInfo
		{
			gachaPoolDef = this,
			contentType = GachaContentType.ObsessionDebris,
			content = character,
			qualityLevel = qualityLevel,
			reward = globalRewardInfo,
			wish = false,
			date = DateTime.UtcNow
		};
	}

	public GachaHistoryInfo GenerateSkill(SkillDef skill, GachaContentType type)
	{
		GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo
		{
			skill = new ProfileSkill(skill)
		};
		if (generateSkillCompensateToken)
		{
			globalRewardInfo.baseTokenCompensate = ScriptableObjectSingleton<FortressDataManager>.instance.gachaBaseCompensateTokenDic[skill.BaseQualityArchType.qualityLevel];
		}
		return new GachaHistoryInfo
		{
			gachaPoolDef = this,
			contentType = type,
			content = skill,
			qualityLevel = skill.BaseQualityArchType.qualityLevel,
			reward = globalRewardInfo,
			wish = false,
			date = DateTime.UtcNow
		};
	}

	public static QualityLevel GetGachaContentQualityLevel(CTScriptableObject target, GachaContentType type)
	{
		if (target is PlayerCharacterDef)
		{
			switch (type)
			{
			case GachaContentType.Obsession:
				return QualityLevel.Epic;
			case GachaContentType.ObsessionDebris:
				return QualityLevel.Uncommon;
			}
		}
		else if (target is SkillDef skillDef)
		{
			return skillDef.BaseQualityLevel;
		}
		return QualityLevel.Common;
	}
}
