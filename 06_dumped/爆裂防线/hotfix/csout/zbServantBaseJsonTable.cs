using Newtonsoft.Json.Linq;

public class zbServantBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/servant/servantBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
