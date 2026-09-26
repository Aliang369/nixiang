using Newtonsoft.Json.Linq;

public class zbIdleBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/idle/idleBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
