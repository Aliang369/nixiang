using Newtonsoft.Json;

namespace ChillyRoom.Services.ContainerOfSoul;

public sealed class MarkLimitedTimeBundlePopupShownRequest
{
	[JsonProperty("purchaseId")]
	public string PurchaseId { get; set; }
}
