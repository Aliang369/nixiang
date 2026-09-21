using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RpcFramework;

public static class RpcJson
{
	public static string Format(JToken token)
	{
		return token.ToString(Formatting.Indented);
	}
}
