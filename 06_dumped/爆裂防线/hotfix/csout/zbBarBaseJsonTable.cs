using Newtonsoft.Json.Linq;

public class zbBarBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/bar/barBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
