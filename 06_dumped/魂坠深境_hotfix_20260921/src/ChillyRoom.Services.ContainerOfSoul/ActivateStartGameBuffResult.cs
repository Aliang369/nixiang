using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class ActivateStartGameBuffResult
{
	[JsonProperty("buffs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<StartGameBuffKey> Buffs { get; set; }

	public List<global::StartGameBuff> GetStartGameBuffs(ApplicationStateInfo asi)
	{
		List<global::StartGameBuff> currentStartGameBuffs = asi.startGameBuffManager.currentStartGameBuffs;
		List<global::StartGameBuff> list = new List<global::StartGameBuff>();
		foreach (StartGameBuffKey buffKey in Buffs)
		{
			global::StartGameBuff startGameBuff = currentStartGameBuffs.FirstOrDefault((global::StartGameBuff b) => b.def.FullID == buffKey.StartGameBuffDefId && b.expireDayPeriod == buffKey.ExpireDayPeriod);
			if (startGameBuff != null)
			{
				list.Add(startGameBuff);
				currentStartGameBuffs.Remove(startGameBuff);
			}
		}
		return list;
	}
}
