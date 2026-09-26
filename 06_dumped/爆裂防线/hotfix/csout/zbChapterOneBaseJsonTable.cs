using Newtonsoft.Json.Linq;

public class zbChapterOneBaseJsonTable
{
	private JObject jsonObject;

	public void Init()
	{
		jsonObject = zbTableLoader.Instance.LoadConfig<JObject>("config/journey/chapterOne/chapterOneBaseJson.json");
	}

	public JObject GetRecord()
	{
		return jsonObject;
	}
}
