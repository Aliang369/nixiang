using Newtonsoft.Json.Linq;

public class zbElementTrialBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/elementTrial/elementTrialBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
