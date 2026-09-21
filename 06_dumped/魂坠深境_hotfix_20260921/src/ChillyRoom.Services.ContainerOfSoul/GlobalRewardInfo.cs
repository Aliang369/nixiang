using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GlobalRewardInfo
{
	[JsonProperty("matterInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public MatterInfo MatterInfo { get; set; }

	[JsonProperty("skill", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ProfileSkill Skill { get; set; }

	[JsonProperty("skillGem", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public SkillGem SkillGem { get; set; }

	[JsonProperty("unlockCardSkin", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string UnlockCardSkin { get; set; }

	[JsonProperty("getStartGameBuff", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public StartGameBuff GetStartGameBuff { get; set; }

	[JsonProperty("unlockCTSOs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> UnlockCTSOs { get; set; }

	[JsonProperty("unlockSeasonPassId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string UnlockSeasonPassId { get; set; }

	[JsonProperty("unlockHighSeasonPassId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string UnlockHighSeasonPassId { get; set; }

	[JsonProperty("setGlobalVariableFlag", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string SetGlobalVariableFlag { get; set; }

	[JsonProperty("unlockPermantVIP", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool UnlockPermantVIP { get; set; }

	[JsonProperty("unlockMonthlyCard", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool UnlockMonthlyCard { get; set; }

	[JsonProperty("UnlockPVPAvatar", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string UnlockPVPAvatar { get; set; }

	[JsonProperty("nestedType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public NestedRewardType NestedType { get; set; }

	[JsonProperty("nestedRewards", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GlobalRewardInfo> NestedRewards { get; set; }

	[JsonProperty("sourceType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public RewardSourceType SourceType { get; set; }

	[JsonProperty("sourceId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string SourceId { get; set; }

	[JsonProperty("baseTokenCompensate", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int BaseTokenCompensate { get; set; }

	[JsonProperty("giveAppendInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GlobalRewardGiveAppendInfo GiveAppendInfo { get; set; }

	[JsonProperty("changeLevelLineExpResult", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ChangeExpResult ChangeLevelLineExpResult { get; set; }

	[JsonProperty("forgetShinePoint", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool ForgetShinePoint { get; set; }

	[JsonProperty("sumTokenCompensate", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int SumTokenCompensate { get; set; }
}
