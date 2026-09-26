using Newtonsoft.Json.Linq;

public class zbBadwordsJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/common/badwordsJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
