using Newtonsoft.Json.Linq;

public class zbArenaBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/arena/arenaBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
