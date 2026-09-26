using Newtonsoft.Json.Linq;

public class zbResourceBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/common/resourceBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
