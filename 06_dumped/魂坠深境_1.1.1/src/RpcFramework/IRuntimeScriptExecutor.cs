using Newtonsoft.Json.Linq;

namespace RpcFramework;

public interface IRuntimeScriptExecutor
{
	JObject ExecuteBody(RuntimeScriptExecuteRequest request);
}
