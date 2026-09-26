using Newtonsoft.Json.Linq;

public class zbFusionBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/operate/fusion/fusionBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
