using Newtonsoft.Json.Linq;

public class zbSnowVeilBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/operate/snowVeil/snowVeilBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
