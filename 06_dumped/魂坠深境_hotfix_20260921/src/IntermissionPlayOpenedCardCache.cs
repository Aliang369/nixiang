using System;
using ChillyRoom.Services.ContainerOfSoul;

[Serializable]
public sealed class IntermissionPlayOpenedCardCache
{
	[SaveField(null, null)]
	public int cardIndex;

	[SaveField(null, null)]
	public int slotId;

	[SaveField(null, null)]
	public IntermissionPlayRewardViewCache rewardView = new IntermissionPlayRewardViewCache();

	public static IntermissionPlayOpenedCardCache FromDto(IntermissionPlayOpenedCardDto dto)
	{
		return new IntermissionPlayOpenedCardCache
		{
			cardIndex = dto.CardIndex,
			slotId = dto.SlotId,
			rewardView = IntermissionPlayRewardViewCache.FromDto(dto.RewardView)
		};
	}

	public IntermissionPlayOpenedCardDto ToDto()
	{
		return new IntermissionPlayOpenedCardDto
		{
			CardIndex = cardIndex,
			SlotId = slotId,
			RewardView = rewardView.ToDto()
		};
	}
}
