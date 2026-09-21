using System;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PurchaseRecord
{
	[JsonProperty("orderId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public long? OrderId { get; set; }

	[JsonProperty("quantity", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int Quantity { get; set; }

	[JsonProperty("purchaseTime", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime PurchaseTime { get; set; }

	[JsonProperty("orderStatus", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public OrderStatus OrderStatus { get; set; }
}
