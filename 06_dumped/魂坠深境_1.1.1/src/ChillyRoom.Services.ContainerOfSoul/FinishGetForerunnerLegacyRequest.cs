using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class FinishGetForerunnerLegacyRequest
{
	[JsonProperty("forerunnerLegacyGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ForerunnerLegacyGuid { get; set; }

	[JsonProperty("feedbackType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GetForerunnerLegacyFeedbackType FeedbackType { get; set; }

	[JsonProperty("commentNote", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string CommentNote { get; set; }
}
