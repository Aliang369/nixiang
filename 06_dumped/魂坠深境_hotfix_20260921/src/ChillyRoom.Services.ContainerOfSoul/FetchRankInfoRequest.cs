using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class FetchRankInfoRequest
{
	[JsonProperty("contractId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ContractId { get; set; }

	[JsonProperty("page", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Page { get; set; }

	public FetchRankInfoRequest(ContractDef contract, int page)
	{
		ContractId = contract.FullID;
		Page = page;
	}
}
