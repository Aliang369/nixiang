using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ForceUpdatePolicyResult
{
	[JsonProperty("targetVersion", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string TargetVersion { get; set; }

	[JsonProperty("effectiveAtUtc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime EffectiveAtUtc { get; set; }

	[JsonProperty("availableAtUtc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime AvailableAtUtc { get; set; }
}
