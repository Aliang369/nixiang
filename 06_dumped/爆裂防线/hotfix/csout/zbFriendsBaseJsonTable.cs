using Newtonsoft.Json.Linq;

public class zbFriendsBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/common/friendsBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
