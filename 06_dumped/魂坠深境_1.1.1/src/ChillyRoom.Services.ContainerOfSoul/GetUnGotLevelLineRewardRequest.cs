using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetUnGotLevelLineRewardRequest
{
	[JsonProperty("levelLineId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string LevelLineId { get; set; }

	public GetUnGotLevelLineRewardRequest(LevelLine levelLine)
	{
		LevelLineId = levelLine.levelLineDef.FullID;
	}
}
