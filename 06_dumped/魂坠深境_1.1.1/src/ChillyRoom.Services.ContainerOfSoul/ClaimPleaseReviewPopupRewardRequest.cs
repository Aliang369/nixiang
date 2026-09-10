using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class ClaimPleaseReviewPopupRewardRequest
{
	[JsonProperty("popupIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int PopupIndex { get; set; }
}
