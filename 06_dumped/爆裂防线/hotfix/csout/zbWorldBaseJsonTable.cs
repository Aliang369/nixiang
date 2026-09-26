using Newtonsoft.Json.Linq;

public class zbWorldBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/world/worldBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
