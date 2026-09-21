using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class TalkToFortressNPCRequest
{
	[JsonProperty("fortressNPCId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string FortressNPCId { get; set; }

	[JsonProperty("progressId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ProgressId { get; set; }

	public TalkToFortressNPCRequest(FortressNPC npc, ApplicationStateInfo asi)
	{
		FortressNPCId = npc.FullId;
		ProgressId = npc.GetCurrentDisplayProgressNode(asi).activateStoryPoint.FullID;
	}
}
