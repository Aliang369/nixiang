using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ExpChangeRecord
{
	[JsonProperty("changeType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public ExpChangeType ChangeType { get; set; }

	[JsonProperty("baseExpBonus", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int BaseExpBonus { get; set; }

	[JsonProperty("fixAddExpBonus", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int FixAddExpBonus { get; set; }

	[JsonProperty("basePercentExpBonus", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int BasePercentExpBonus { get; set; }

	[JsonProperty("sumPercentExpBonus", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int SumPercentExpBonus { get; set; }

	[JsonProperty("progress", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Progress { get; set; }

	[JsonProperty("difficultyLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DifficultyLevel DifficultyLevel { get; set; }

	[JsonProperty("achievementId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string AchievementId { get; set; }

	[JsonProperty("dailyMinTimes", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DailyMinTimes { get; set; }

	[JsonProperty("dailyMaxTimes", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DailyMaxTimes { get; set; }
}
