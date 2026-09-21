using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ChillyRoom.Services.ContainerOfSoul;

[Preserve]
public sealed class IntermissionPlayRewardSlotDto
{
	[JsonProperty("slotId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int SlotId { get; set; }

	[JsonProperty("displayOrder", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int DisplayOrder { get; set; }

	[JsonProperty("rewardLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public int RewardLevel { get; set; }

	[JsonProperty("isDrawn", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
	public bool IsDrawn { get; set; }

	[JsonProperty("rewardView", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
	public IntermissionPlayRewardViewDto RewardView { get; set; } = new IntermissionPlayRewardViewDto();
}
