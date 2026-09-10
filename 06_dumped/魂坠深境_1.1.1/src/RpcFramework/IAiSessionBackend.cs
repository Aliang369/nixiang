using Newtonsoft.Json.Linq;

namespace RpcFramework;

public interface IAiSessionBackend
{
	AiObservation CreateSession(JObject request);

	AiObservation Observe(string sessionId);

	AiActResult Act(string sessionId, string actionId, JObject request);

	AiObservation StepUntilDecision(string sessionId, JObject request);

	void CloseSession(string sessionId);

	JObject GetStatus();
}
