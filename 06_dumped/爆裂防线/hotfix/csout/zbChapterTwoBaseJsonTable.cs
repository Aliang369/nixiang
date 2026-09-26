using Newtonsoft.Json.Linq;

public class zbChapterTwoBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/journey/chapterTwo/chapterTwoBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
