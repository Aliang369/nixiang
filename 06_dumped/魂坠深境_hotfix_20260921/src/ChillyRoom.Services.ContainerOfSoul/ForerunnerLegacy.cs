using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ForerunnerLegacy
{
	[JsonProperty("guid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Guid { get; set; }

	[JsonProperty("playerId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long PlayerId { get; set; }

	[JsonProperty("legacyItem", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public CardRecord LegacyItem { get; set; }

	[JsonProperty("legacySkill", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public SkillRecord LegacySkill { get; set; }

	[JsonProperty("legacyMapID", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string LegacyMapID { get; set; }

	[JsonProperty("locationPivotIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int LocationPivotIndex { get; set; }

	[JsonProperty("stage", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Stage { get; set; }

	[JsonProperty("leaveTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime LeaveTime { get; set; }

	[JsonProperty("expireTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime ExpireTime { get; set; }

	[JsonProperty("shareEncounterCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ShareEncounterCount { get; set; }

	[JsonProperty("shareGetCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ShareGetCount { get; set; }

	[JsonProperty("shareWinCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ShareWinCount { get; set; }

	[JsonProperty("upvoteCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int UpvoteCount { get; set; }

	[JsonProperty("downvoteCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DownvoteCount { get; set; }

	[JsonProperty("note", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Note { get; set; }

	[JsonProperty("presetMessageId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string PresetMessageId { get; set; }

	[JsonProperty("signature", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Signature { get; set; }
}
