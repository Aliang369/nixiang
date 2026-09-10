using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class DailyChallengeScoreMetrics
{
	[JsonProperty("durationSeconds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? DurationSeconds { get; set; }

	[JsonProperty("deckCount", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? DeckCount { get; set; }

	[JsonProperty("blessingCount", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? BlessingCount { get; set; }

	[JsonProperty("historicalRealHealthDamage", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? HistoricalRealHealthDamage { get; set; }

	[JsonProperty("finalGold", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? FinalGold { get; set; }

	[JsonProperty("finalSoulCrystal", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? FinalSoulCrystal { get; set; }

	[JsonProperty("finalSanity", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? FinalSanity { get; set; }
}
