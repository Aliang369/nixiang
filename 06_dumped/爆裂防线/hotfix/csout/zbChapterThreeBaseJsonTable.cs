using Newtonsoft.Json.Linq;

public class zbChapterThreeBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/journey/chapterThree/chapterThreeBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
