using Newtonsoft.Json.Linq;

public class zbEndlessMineBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/operate/endlessMine/endlessMineBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
