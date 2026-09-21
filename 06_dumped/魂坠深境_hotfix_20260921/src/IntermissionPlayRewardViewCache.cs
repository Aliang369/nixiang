using System;
using ChillyRoom.Services.ContainerOfSoul;

[Serializable]
public sealed class IntermissionPlayRewardViewCache
{
	[SaveField(null, null)]
	public string type;

	[SaveField(null, null)]
	public string id;

	[SaveField(null, null)]
	public int count;

	public static IntermissionPlayRewardViewCache FromDto(IntermissionPlayRewardViewDto dto)
	{
		return new IntermissionPlayRewardViewCache
		{
			type = dto.Type,
			id = dto.Id,
			count = dto.Count
		};
	}

	public IntermissionPlayRewardViewDto ToDto()
	{
		return new IntermissionPlayRewardViewDto
		{
			Type = type,
			Id = id,
			Count = count
		};
	}
}
