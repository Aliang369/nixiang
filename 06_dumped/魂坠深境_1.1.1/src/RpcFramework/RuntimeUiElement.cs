using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class RuntimeUiElement
{
	public string? Id { get; set; }

	public string? Path { get; set; }

	public string? Name { get; set; }

	public string? Text { get; set; }

	public bool Interactable { get; set; }

	public bool Active { get; set; }

	public bool Visible { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["id"] = Id,
			["path"] = Path,
			["name"] = Name,
			["text"] = Text,
			["interactable"] = Interactable,
			["active"] = Active,
			["visible"] = Visible
		};
	}
}
