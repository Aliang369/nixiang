using Newtonsoft.Json.Linq;

public class zbLadderBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/crossLadder/ladderBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
