using Newtonsoft.Json.Linq;

public class zbRestaurantBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/operate/restaurant/restaurantBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
