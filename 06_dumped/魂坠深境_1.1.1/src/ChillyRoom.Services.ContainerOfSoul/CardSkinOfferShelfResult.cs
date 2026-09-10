using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v13.0.0.0))")]
[Preserve]
public class CardSkinOfferShelfResult
{
	[JsonProperty("dailyPeriod", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int DailyPeriod { get; set; }

	[JsonProperty("refreshRevision", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public int RefreshRevision { get; set; }

	[JsonProperty("nextDailyRefreshAtUtc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public DateTime NextDailyRefreshAtUtc { get; set; }

	[JsonProperty("refreshCost", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public MatterInfo RefreshCost { get; set; }

	[JsonProperty("canRefresh", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool CanRefresh { get; set; }

	[JsonProperty("hasAnyOffer", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public bool HasAnyOffer { get; set; }

	[JsonProperty("offers", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	[Preserve]
	public List<CardSkinOfferSlotResult> Offers { get; set; }
}
