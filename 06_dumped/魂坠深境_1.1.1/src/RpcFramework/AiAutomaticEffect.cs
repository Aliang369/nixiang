using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiAutomaticEffect
{
	public string? EffectId { get; set; }

	public string? Kind { get; set; }

	public string? Label { get; set; }

	public JObject? Data { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["effectId"] = EffectId,
			["kind"] = Kind,
			["label"] = Label,
			["data"] = Data
		};
	}
}
