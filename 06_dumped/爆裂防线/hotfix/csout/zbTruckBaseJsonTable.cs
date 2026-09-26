using Newtonsoft.Json.Linq;

public class zbTruckBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/truck/truckBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
