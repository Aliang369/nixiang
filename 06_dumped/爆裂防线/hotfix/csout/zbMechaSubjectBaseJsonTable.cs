using Newtonsoft.Json.Linq;

public class zbMechaSubjectBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/operate/mechaSubject/mechaSubjectBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
