using Newtonsoft.Json.Linq;

public class zbPassportBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/passport/passportBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
