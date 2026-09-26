using Newtonsoft.Json.Linq;

public class zbLegionWarBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/legionWar/legionWarBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
