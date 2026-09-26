using Newtonsoft.Json.Linq;

public class zbPvpBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/pvp/pvpBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
