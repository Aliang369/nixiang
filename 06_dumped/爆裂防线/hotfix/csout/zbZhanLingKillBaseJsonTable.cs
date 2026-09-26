using Newtonsoft.Json.Linq;

public class zbZhanLingKillBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/operate/zhanLingKill/zhanLingKillBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
