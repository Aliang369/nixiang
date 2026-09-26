using Newtonsoft.Json.Linq;

public class zbGameBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/common/gameBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
