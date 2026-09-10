using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class IdolState
{
	[JsonProperty("idolId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string IdolId { get; set; }

	[JsonProperty("currentLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int CurrentLevel { get; set; }

	[JsonProperty("currentDifficultyLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int CurrentDifficultyLevel { get; set; }

	[JsonProperty("mainlineDifficulty", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? MainlineDifficulty { get; set; }

	[JsonProperty("seasonExperience", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int SeasonExperience { get; set; }

	[JsonProperty("seasonSoul", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int SeasonSoul { get; set; }
}
