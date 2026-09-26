using Newtonsoft.Json.Linq;

public class zbSeasonBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/season/seasonBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
