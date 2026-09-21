using Newtonsoft.Json.Linq;

namespace RpcFramework;

public sealed class AiActResult
{
	public bool Accepted { get; set; }

	public AiObservation? Observation { get; set; }

	public string? Message { get; set; }

	public JObject ToJObject()
	{
		return new JObject
		{
			["accepted"] = Accepted,
			["message"] = Message,
			["observation"] = ((Observation == null) ? null : Observation.ToJObject())
		};
	}
}
