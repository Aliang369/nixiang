using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiAuxiliaryAction
{
	public string? ActionId { get; set; }

	public string? Kind { get; set; }

	public string? Label { get; set; }

	public JObject? Data { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["actionId"] = ActionId,
			["kind"] = Kind,
			["label"] = Label,
			["data"] = Data
		};
	}
}
