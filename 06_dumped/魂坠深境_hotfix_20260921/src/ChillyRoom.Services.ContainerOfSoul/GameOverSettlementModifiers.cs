using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GameOverSettlementModifiers
{
	[JsonProperty("expMultiplier", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ExpMultiplier { get; set; }

	[JsonProperty("classMatterMultiplier", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int ClassMatterMultiplier { get; set; }

	[JsonProperty("normalEnemyGemBonusLikeElite", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool NormalEnemyGemBonusLikeElite { get; set; }

	[JsonProperty("sourceInfluenceIds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<string> SourceInfluenceIds { get; set; }
}
