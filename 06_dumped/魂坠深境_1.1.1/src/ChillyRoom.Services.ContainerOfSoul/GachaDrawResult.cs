using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class GachaDrawResult
{
	[JsonProperty("gachaResults", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<GachaDrawHistory> GachaResults { get; set; }

	[JsonProperty("newPoolState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public GachaPoolState NewPoolState { get; set; }

	[JsonProperty("gachaTotal")]
	public int GachaTotal { get; set; }
}
