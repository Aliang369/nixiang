using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class ClientPrepareCardSkinOfferPurchaseRequest
{
	[JsonProperty("purchaseId")]
	public string PurchaseId { get; set; }

	[JsonProperty("slotIndex")]
	public int SlotIndex { get; set; }

	[JsonProperty("expectedCardSkinId")]
	public string ExpectedCardSkinId { get; set; }

	[JsonProperty("dailyPeriod")]
	public int? DailyPeriod { get; set; }

	[JsonProperty("refreshRevision")]
	public int? RefreshRevision { get; set; }
}
