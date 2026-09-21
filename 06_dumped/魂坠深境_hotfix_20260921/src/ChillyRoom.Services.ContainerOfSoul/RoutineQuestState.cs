using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class RoutineQuestState
{
	[JsonProperty("guid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string Guid { get; set; }

	[JsonProperty("questDefID", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string QuestDefID { get; set; }

	[JsonProperty("questTargetDefID", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string QuestTargetDefID { get; set; }

	[JsonProperty("progressPerLayer", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ProgressPerLayer { get; set; }

	[JsonProperty("layerFinished", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int LayerFinished { get; set; }
}
