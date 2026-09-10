using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class PleaseReviewPopupConfigReply
{
	[JsonProperty("pleaseReviewPopupEnabled", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public bool PleaseReviewPopupEnabled { get; set; }

	[JsonProperty("pleaseReviewPopupType", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int PleaseReviewPopupType { get; set; } = -1;
}
