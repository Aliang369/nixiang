using ChillyRoom.Services.ContainerOfSoul;

public sealed class IntermissionPlayReclaimPresentationResult
{
	public IntermissionPlayTicketType TicketType { get; }

	public int ReclaimedCount { get; }

	public int GiftCount { get; }

	public GlobalRewardInfo Reward { get; }

	public IntermissionPlayReclaimPresentationResult(IntermissionPlayTicketType ticketType, int reclaimedCount, int giftCount, GlobalRewardInfo reward)
	{
		TicketType = ticketType;
		ReclaimedCount = reclaimedCount;
		GiftCount = giftCount;
		Reward = reward;
	}
}
