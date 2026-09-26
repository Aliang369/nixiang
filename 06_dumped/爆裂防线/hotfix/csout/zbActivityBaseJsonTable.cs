using Newtonsoft.Json.Linq;

public class zbActivityBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/activity/activityBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
