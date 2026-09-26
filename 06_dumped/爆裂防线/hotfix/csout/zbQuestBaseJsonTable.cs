using Newtonsoft.Json.Linq;

public class zbQuestBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/mainQuest/questBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
