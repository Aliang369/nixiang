using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PlayerReportRequest
{
	[JsonProperty("sceneType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string SceneType { get; set; }

	[JsonProperty("reportType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ReportType { get; set; }

	[JsonProperty("targetPlayerId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long? TargetPlayerId { get; set; }

	[JsonProperty("description", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Description { get; set; }

	[JsonProperty("additionalInfo", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string AdditionalInfo { get; set; }
}
