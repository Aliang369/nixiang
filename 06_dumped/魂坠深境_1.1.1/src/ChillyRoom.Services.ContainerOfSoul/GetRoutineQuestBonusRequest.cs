using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GetRoutineQuestBonusRequest
{
	[JsonProperty("questGuid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> QuestGuid { get; set; }

	public GetRoutineQuestBonusRequest(params RoutineQuest[] quests)
	{
		QuestGuid = quests.Select((RoutineQuest q) => q.guid).ToList();
	}
}
