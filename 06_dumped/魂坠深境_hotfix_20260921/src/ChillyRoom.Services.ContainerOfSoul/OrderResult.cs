using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class OrderResult
{
	[JsonProperty("orderId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long OrderId { get; set; }

	[JsonProperty("purchaseId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string PurchaseId { get; set; }

	[JsonProperty("result", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public PurchaseItemResult Result { get; set; }
}
