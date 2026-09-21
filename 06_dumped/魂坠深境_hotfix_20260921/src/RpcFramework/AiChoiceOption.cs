using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiChoiceOption
{
	public string? OptionId { get; set; }

	public string? Kind { get; set; }

	public string? Label { get; set; }

	public JObject? Data { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["optionId"] = OptionId,
			["kind"] = Kind,
			["label"] = Label,
			["data"] = Data
		};
	}
}
