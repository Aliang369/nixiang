using Newtonsoft.Json.Linq;

public class zbPeakBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/crossPeak/peakBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
