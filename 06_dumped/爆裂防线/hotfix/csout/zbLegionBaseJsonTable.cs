using Newtonsoft.Json.Linq;

public class zbLegionBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/legion/legionBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
