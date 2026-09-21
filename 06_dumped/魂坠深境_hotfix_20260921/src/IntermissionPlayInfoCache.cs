using System;
using System.Collections.Generic;
using ChillyRoom.Services.ContainerOfSoul;

[Serializable]
public sealed class IntermissionPlayInfoCache
{
	[SaveField(null, null)]
	public bool isOpen;

	[SaveField(null, null)]
	public string closeReason;

	[SaveField(null, null)]
	public DateTimeOffset serverNowUtc;

	[SaveField(null, null)]
	public DateTimeOffset activityEndTimeUtc;

	[SaveField(null, null)]
	public int currentDayIndex;

	[SaveField(null, null)]
	public DateTimeOffset nextRefreshTimeUtc;

	[SaveField(null, null)]
	public int paidGiftProgress;

	[SaveField(null, null)]
	public bool dailyTaskCompleted;

	[SaveField(null, null)]
	public int dailyTaskCompletedCount;

	[SaveField(null, null)]
	public bool dailyTicketGranted;

	[SaveField(null, null)]
	public int dailyTicketGrantedCount;

	[SaveField(null, null)]
	public int dailyTicketCount;

	[SaveField(null, null)]
	public int carryTicketCount;

	[SaveField(null, null)]
	public int otherworldTicketICount;

	[SaveField(null, null)]
	public int otherworldGiftCount;

	[SaveField(null, null)]
	public Dictionary<string, string> title = new Dictionary<string, string>();

	[SaveField(null, null)]
	public Dictionary<string, string> description = new Dictionary<string, string>();

	[SaveField(null, null)]
	public Dictionary<string, string> helpText = new Dictionary<string, string>();

	[SaveField(null, null)]
	public List<IntermissionPlayRewardSlotCache> rewardSlots = new List<IntermissionPlayRewardSlotCache>();

	[SaveField(null, null)]
	public List<IntermissionPlayOpenedCardCache> openedCards = new List<IntermissionPlayOpenedCardCache>();

	public static IntermissionPlayInfoCache FromResponse(GetIntermissionPlayInfoResponse response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		return new IntermissionPlayInfoCache
		{
			isOpen = response.IsOpen,
			closeReason = response.CloseReason,
			serverNowUtc = response.ServerNowUtc,
			activityEndTimeUtc = response.ActivityEndTimeUtc,
			currentDayIndex = response.CurrentDayIndex,
			nextRefreshTimeUtc = response.NextRefreshTimeUtc,
			paidGiftProgress = response.PaidGiftProgress,
			dailyTaskCompleted = response.DailyTaskCompleted,
			dailyTaskCompletedCount = response.DailyTaskCompletedCount,
			dailyTicketGranted = response.DailyTicketGranted,
			dailyTicketGrantedCount = response.DailyTicketGrantedCount,
			dailyTicketCount = response.DailyTicketCount,
			carryTicketCount = response.CarryTicketCount,
			otherworldTicketICount = response.OtherworldTicketICount,
			otherworldGiftCount = response.OtherworldGiftCount,
			title = new Dictionary<string, string>(response.Title),
			description = new Dictionary<string, string>(response.Description),
			helpText = new Dictionary<string, string>(response.HelpText),
			rewardSlots = response.RewardSlots.ConvertAll<IntermissionPlayRewardSlotCache>(IntermissionPlayRewardSlotCache.FromDto),
			openedCards = response.OpenedCards.ConvertAll<IntermissionPlayOpenedCardCache>(IntermissionPlayOpenedCardCache.FromDto)
		};
	}

	public GetIntermissionPlayInfoResponse ToResponse(string activityId)
	{
		return new GetIntermissionPlayInfoResponse
		{
			ActivityId = activityId,
			IsOpen = isOpen,
			CloseReason = closeReason,
			ServerNowUtc = serverNowUtc,
			ActivityEndTimeUtc = activityEndTimeUtc,
			CurrentDayIndex = currentDayIndex,
			NextRefreshTimeUtc = nextRefreshTimeUtc,
			PaidGiftProgress = paidGiftProgress,
			DailyTaskCompleted = dailyTaskCompleted,
			DailyTaskCompletedCount = ((dailyTaskCompletedCount > 0) ? dailyTaskCompletedCount : (dailyTaskCompleted ? 1 : 0)),
			DailyTicketGranted = dailyTicketGranted,
			DailyTicketGrantedCount = ((dailyTicketGrantedCount > 0) ? dailyTicketGrantedCount : (dailyTicketGranted ? 1 : 0)),
			DailyTicketCount = dailyTicketCount,
			CarryTicketCount = carryTicketCount,
			OtherworldTicketICount = otherworldTicketICount,
			OtherworldGiftCount = otherworldGiftCount,
			Title = (title ?? new Dictionary<string, string>()),
			Description = (description ?? new Dictionary<string, string>()),
			HelpText = (helpText ?? new Dictionary<string, string>()),
			RewardSlots = (rewardSlots?.ConvertAll((IntermissionPlayRewardSlotCache cache) => cache.ToDto()) ?? new List<IntermissionPlayRewardSlotDto>()),
			OpenedCards = (openedCards?.ConvertAll((IntermissionPlayOpenedCardCache cache) => cache.ToDto()) ?? new List<IntermissionPlayOpenedCardDto>())
		};
	}
}
