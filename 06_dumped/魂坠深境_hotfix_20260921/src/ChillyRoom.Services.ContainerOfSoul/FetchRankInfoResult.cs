using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class FetchRankInfoResult
{
	[JsonProperty("rankGameRecords", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<RankGameRecord> RankGameRecords { get; set; }

	[JsonProperty("playerBestGameRecord", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public RankGameRecord PlayerBestGameRecord { get; set; }

	public List<RankingManager.RankingData> GetRankingDatas(ApplicationStateInfo applicationState)
	{
		return RankGameRecords.Select((RankGameRecord r) => r.ToClient(new NetworkConvertContext(applicationState))).ToList();
	}

	public RankingManager.RankingData GetPlayerBestRankingData(ApplicationStateInfo applicationState)
	{
		return PlayerBestGameRecord?.ToClient(new NetworkConvertContext(applicationState));
	}
}
