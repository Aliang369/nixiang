using System;
using System.Collections.Generic;
using ChillyRoom.Services.ContainerOfSoul;

public sealed class IntermissionPlayModel
{
	public GetIntermissionPlayInfoResponse Info { get; private set; }

	public IntermissionPlayDrawResponse LastDrawResult { get; private set; }

	public string ActivityId => Info?.ActivityId;

	public bool IsOpen => Info?.IsOpen ?? false;

	public string CloseReason => Info?.CloseReason;

	public DateTimeOffset ServerNowUtc => Info?.ServerNowUtc ?? default(DateTimeOffset);

	public DateTimeOffset ActivityEndTimeUtc => Info?.ActivityEndTimeUtc ?? default(DateTimeOffset);

	public int CurrentDayIndex => Info?.CurrentDayIndex ?? 0;

	public DateTimeOffset NextRefreshTimeUtc => Info?.NextRefreshTimeUtc ?? default(DateTimeOffset);

	public int PaidGiftProgress => Info?.PaidGiftProgress ?? 0;

	public bool DailyTaskCompleted => Info?.DailyTaskCompleted ?? false;

	public int DailyTaskCompletedCount => Info?.DailyTaskCompletedCount ?? 0;

	public bool DailyTicketGranted => Info?.DailyTicketGranted ?? false;

	public int DailyTicketGrantedCount => Info?.DailyTicketGrantedCount ?? 0;

	public int DailyTicketCount => Info?.DailyTicketCount ?? 0;

	public int CarryTicketCount => Info?.CarryTicketCount ?? 0;

	public int OtherworldTicketICount => Info?.OtherworldTicketICount ?? 0;

	public int OtherworldTicketIICount => CarryTicketCount;

	public int OtherworldGiftCount => Info?.OtherworldGiftCount ?? 0;

	public IReadOnlyDictionary<string, string> Description => Info?.Description;

	public IReadOnlyDictionary<string, string> HelpText => Info?.HelpText;

	public IReadOnlyList<IntermissionPlayRewardSlotDto> RewardSlots => Info?.RewardSlots;

	public IReadOnlyList<IntermissionPlayOpenedCardDto> OpenedCards => Info?.OpenedCards;

	public void ApplyInfo(GetIntermissionPlayInfoResponse response)
	{
		Info = response ?? throw new ArgumentNullException("response");
		LastDrawResult = null;
	}

	public void ApplyDraw(IntermissionPlayDrawResponse response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (Info == null)
		{
			throw new InvalidOperationException("Intermission play info must be loaded before applying a draw result.");
		}
		Info.DailyTicketCount = response.DailyTicketCount;
		Info.CarryTicketCount = response.CarryTicketCount;
		Info.NextRefreshTimeUtc = response.NextRefreshTimeUtc;
		Info.OpenedCards = response.OpenedCards;
		foreach (IntermissionPlayRewardSlotDto rewardSlot in Info.RewardSlots)
		{
			rewardSlot.IsDrawn = response.DrawnSlotIds.Contains(rewardSlot.SlotId);
		}
		LastDrawResult = response;
	}

	public void ApplyReclaim(IntermissionPlayReclaimTicketsResponse response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (Info == null)
		{
			throw new InvalidOperationException("Intermission play info must be loaded before applying a reclaim result.");
		}
		Info.OtherworldTicketICount = response.OtherworldTicketICount;
		Info.CarryTicketCount = response.OtherworldTicketIICount;
		Info.OtherworldGiftCount = response.OtherworldGiftCount;
	}

	public void ApplyDailyTaskClaim(IntermissionPlayClaimDailyTaskResponse response)
	{
		if (response == null)
		{
			throw new ArgumentNullException("response");
		}
		if (Info == null)
		{
			throw new InvalidOperationException("Intermission play info must be loaded before applying a task claim result.");
		}
		Info.DailyTaskCompleted = response.DailyTaskCompleted;
		Info.DailyTaskCompletedCount = response.DailyTaskCompletedCount;
		Info.DailyTicketGranted = response.DailyTicketGranted;
		Info.DailyTicketGrantedCount = response.DailyTicketGrantedCount;
		Info.DailyTicketCount = response.DailyTicketCount;
	}

	public IntermissionPlayDrawResponse ConsumeLastDrawResult()
	{
		IntermissionPlayDrawResponse lastDrawResult = LastDrawResult;
		LastDrawResult = null;
		return lastDrawResult;
	}
}
