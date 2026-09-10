using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ActivateStartGameBuffRequest
{
	[JsonProperty("activateBuffs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<StartGameBuffKey> ActivateBuffs { get; set; }

	public ActivateStartGameBuffRequest(List<global::StartGameBuff> buffs)
	{
		ActivateBuffs = buffs.Select((global::StartGameBuff b) => new StartGameBuffKey
		{
			StartGameBuffDefId = b.def.FullID,
			ExpireDayPeriod = b.expireDayPeriod
		}).ToList();
	}
}
