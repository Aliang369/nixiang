using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PurchaseItemRequest
{
	[JsonProperty("purchaseId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string PurchaseId { get; set; }

	[JsonProperty("times", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Times { get; set; }

	public PurchaseItemRequest(PurchaseInfo purchase, int times)
	{
		PurchaseId = purchase.purchaseId;
		Times = times;
	}
}
