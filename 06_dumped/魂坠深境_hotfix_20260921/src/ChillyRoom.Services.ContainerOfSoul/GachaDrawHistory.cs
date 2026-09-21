using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GachaDrawHistory
{
	[JsonProperty("contentType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GachaContentType ContentType { get; set; }

	[JsonProperty("contentId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ContentId { get; set; }

	[JsonProperty("contentQualityLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public QualityLevel ContentQualityLevel { get; set; }

	[JsonProperty("gachaReward", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GlobalRewardInfo GachaReward { get; set; }

	[JsonProperty("isWish", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool IsWish { get; set; }

	[JsonProperty("drawDateTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime DrawDateTime { get; set; }

	[JsonProperty("gachaPoolId")]
	public string GachaPoolId { get; set; } = string.Empty;
}
