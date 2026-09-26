using Newtonsoft.Json.Linq;

public class zbEndlessHordeBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/endlessHorde/endlessHordeBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
