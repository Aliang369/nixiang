using Newtonsoft.Json.Linq;

public class zbHomeBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/homelandV2/homeBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
