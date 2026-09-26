using Newtonsoft.Json.Linq;

public class zbWallSubjectBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/operate/wallSubject/wallSubjectBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
