using Newtonsoft.Json.Linq;

public class zbMineBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/crossMine/mineBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
