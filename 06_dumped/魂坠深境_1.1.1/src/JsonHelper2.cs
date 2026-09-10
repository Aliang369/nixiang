using Newtonsoft.Json.Linq;

public static class JsonHelper2
{
	public static bool IsNull(this JToken jToken)
	{
		if (jToken == null)
		{
			return true;
		}
		return false;
	}
}
