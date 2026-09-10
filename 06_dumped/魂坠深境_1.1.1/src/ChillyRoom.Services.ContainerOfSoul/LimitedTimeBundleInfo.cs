using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class LimitedTimeBundleInfo
{
	[JsonProperty("purchaseId")]
	[Preserve]
	public string PurchaseId { get; set; } = string.Empty;

	[JsonProperty("bundleStartTimeUtc")]
	[Preserve]
	public DateTime? BundleStartTimeUtc { get; set; }

	[JsonProperty("endTimeUtc")]
	[Preserve]
	public DateTime? EndTimeUtc { get; set; }

	[JsonProperty("popupShown")]
	[Preserve]
	public bool PopupShown { get; set; }
}
