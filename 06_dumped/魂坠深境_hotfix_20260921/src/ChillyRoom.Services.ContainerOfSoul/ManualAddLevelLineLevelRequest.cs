using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ManualAddLevelLineLevelRequest
{
	[JsonProperty("levelLineId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string LevelLineId { get; set; }

	[JsonProperty("addLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int AddLevel { get; set; }

	public ManualAddLevelLineLevelRequest(LevelLine levelLine, int addLevel)
	{
		LevelLineId = levelLine.levelLineDef.FullID;
		AddLevel = addLevel;
	}
}
