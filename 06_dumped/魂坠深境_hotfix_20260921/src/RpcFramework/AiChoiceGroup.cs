using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiChoiceGroup
{
	public string? GroupId { get; set; }

	public string? Kind { get; set; }

	public string? Label { get; set; }

	public bool Required { get; set; }

	public int MinSelect { get; set; }

	public int MaxSelect { get; set; } = 1;

	public JObject? Data { get; set; }

	public List<AiChoiceOption> Options { get; } = new List<AiChoiceOption>();

	public JObject ToJObject()
	{
		return new JObject
		{
			["groupId"] = GroupId,
			["kind"] = Kind,
			["label"] = Label,
			["required"] = Required,
			["minSelect"] = MinSelect,
			["maxSelect"] = MaxSelect,
			["data"] = Data,
			["options"] = new JArray(Options.Select((AiChoiceOption o) => o.ToJObject()))
		};
	}
}
