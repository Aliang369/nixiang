using Newtonsoft.Json.Linq;

public class zbWorldBossBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/worldBoss/worldBossBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
