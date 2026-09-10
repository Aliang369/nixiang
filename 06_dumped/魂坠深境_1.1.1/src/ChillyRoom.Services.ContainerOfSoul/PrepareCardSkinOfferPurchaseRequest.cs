using System.CodeDom.Compiler;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class PrepareCardSkinOfferPurchaseRequest
{
	[JsonProperty("purchaseId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string PurchaseId { get; set; }

	[JsonProperty("slotIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int SlotIndex { get; set; }

	[JsonProperty("expectedCardSkinId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public string ExpectedCardSkinId { get; set; }

	[JsonProperty("dailyPeriod", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? DailyPeriod { get; set; }

	[JsonProperty("refreshRevision", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int? RefreshRevision { get; set; }
}
