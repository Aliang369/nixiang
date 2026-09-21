using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayOpenedCardDto
{
	[JsonProperty("cardIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int CardIndex { get; set; }

	[JsonProperty("slotId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int SlotId { get; set; }

	[JsonProperty("rewardView", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public IntermissionPlayRewardViewDto RewardView { get; set; } = new IntermissionPlayRewardViewDto();
}
