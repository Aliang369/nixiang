using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class zbPvpHandler
{
	public int modSelectNum;

	public int modResetDiamond;

	public int modRefreshDiamond;

	public float monsterDamageLayer;

	public void Init()
	{
		JObject record = zbDataConfig.Instance.pvpBaseJsonTable.GetRecord();
		modSelectNum = Extensions.Value<int>((IEnumerable<JToken>)record["modSelectNum"]);
		modResetDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["modResetDiamond"]);
		modRefreshDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["modRefreshDiamond"]);
		monsterDamageLayer = Extensions.Value<float>((IEnumerable<JToken>)record["monsterDamageLayer"]);
	}
}
