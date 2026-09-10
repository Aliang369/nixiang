using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public interface IRuntimeCommandBackend
{
	JObject GetStatus();

	JObject GetSceneStatus();

	JArray FindObjects(JObject request);

	IReadOnlyList<RuntimeUiElement> FindUiElements(JObject request);

	JObject ClickUi(JObject request);

	JObject InvokeRegistered(JObject request);

	bool IsConditionSatisfied(JObject request);
}
