using ChillyRoom.Services.ContainerOfSoul;

public sealed class IntermissionPlayDrawPresentationResult
{
	public int CardIndex { get; }

	public int SlotId { get; }

	public IntermissionPlayTicketType TicketType { get; }

	public GlobalRewardInfo Reward { get; }

	public IntermissionPlayDrawPresentationResult(int cardIndex, int slotId, IntermissionPlayTicketType ticketType, GlobalRewardInfo reward)
	{
		CardIndex = cardIndex;
		SlotId = slotId;
		TicketType = ticketType;
		Reward = reward;
	}
}
