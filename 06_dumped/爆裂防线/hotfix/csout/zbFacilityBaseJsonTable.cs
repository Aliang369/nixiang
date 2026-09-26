using Newtonsoft.Json.Linq;

public class zbFacilityBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/facility/facilityBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
