using Newtonsoft.Json.Linq;

public class zbVipBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/vip/vipBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
