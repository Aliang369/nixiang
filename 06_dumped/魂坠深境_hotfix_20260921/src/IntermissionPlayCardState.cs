using ChillyRoom.Services.ContainerOfSoul;

public sealed class IntermissionPlayCardState
{
	public int CardIndex { get; }

	public IntermissionPlayOpenedCardDto OpenedCard { get; }

	public bool CanClick { get; }

	public IntermissionPlayCardState(int cardIndex, IntermissionPlayOpenedCardDto openedCard, bool canClick)
	{
		CardIndex = cardIndex;
		OpenedCard = openedCard;
		CanClick = canClick;
	}
}
