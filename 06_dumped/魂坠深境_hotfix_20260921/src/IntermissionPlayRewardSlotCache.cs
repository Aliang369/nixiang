using System;
using ChillyRoom.Services.ContainerOfSoul;

[Serializable]
public sealed class IntermissionPlayRewardSlotCache
{
	[SaveField(null, null)]
	public int slotId;

	[SaveField(null, null)]
	public int displayOrder;

	[SaveField(null, null)]
	public int rewardLevel;

	[SaveField(null, null)]
	public bool isDrawn;

	[SaveField(null, null)]
	public IntermissionPlayRewardViewCache rewardView = new IntermissionPlayRewardViewCache();

	public static IntermissionPlayRewardSlotCache FromDto(IntermissionPlayRewardSlotDto dto)
	{
		return new IntermissionPlayRewardSlotCache
		{
			slotId = dto.SlotId,
			displayOrder = dto.DisplayOrder,
			rewardLevel = dto.RewardLevel,
			isDrawn = dto.IsDrawn,
			rewardView = IntermissionPlayRewardViewCache.FromDto(dto.RewardView)
		};
	}

	public IntermissionPlayRewardSlotDto ToDto()
	{
		return new IntermissionPlayRewardSlotDto
		{
			SlotId = slotId,
			DisplayOrder = displayOrder,
			RewardLevel = rewardLevel,
			IsDrawn = isDrawn,
			RewardView = rewardView.ToDto()
		};
	}
}
