using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class UpdateStoryPointRequest
{
	[JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Id { get; set; }

	[JsonProperty("observeCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ObserveCount { get; set; }

	public UpdateStoryPointRequest(StoryPointManager.StoryPointState state)
	{
		Id = state.storyPoint.FullID;
		ObserveCount = state.observeCount;
	}
}
