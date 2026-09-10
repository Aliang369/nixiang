using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GlobalRewardGiveAppendInfo
{
	[JsonProperty("repeated", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool Repeated { get; set; }

	[JsonProperty("repeatTokenCompensate", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RepeatTokenCompensate { get; set; }

	[JsonProperty("soul", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Soul { get; set; }

	[JsonProperty("characterLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int CharacterLevel { get; set; }

	[JsonProperty("isNewGive", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool IsNewGive { get; set; }

	[JsonProperty("convertObsessionToUnlockCharacter", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool ConvertObsessionToUnlockCharacter { get; set; }

	[JsonProperty("convertObsessionToDebris", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool ConvertObsessionToDebris { get; set; }

	[JsonProperty("cardSkinOwnershipAdded", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool? CardSkinOwnershipAdded { get; set; }

	[JsonProperty("cardSkinLevelIncreased", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool? CardSkinLevelIncreased { get; set; }
}
