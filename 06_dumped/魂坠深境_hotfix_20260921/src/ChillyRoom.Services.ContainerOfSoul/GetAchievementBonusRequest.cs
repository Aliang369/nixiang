using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetAchievementBonusRequest
{
	[JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> Id { get; set; }

	public GetAchievementBonusRequest(params AchievementDef[] achievements)
	{
		Id = achievements.Select((AchievementDef d) => d.FullID).ToList();
	}
}
