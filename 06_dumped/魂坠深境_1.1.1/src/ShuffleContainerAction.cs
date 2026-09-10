public class ShuffleContainerAction : ExecuteAction
{
	public ItemContainer container;

	public ShuffleContainerAction(ItemContainer container)
	{
		this.container = container;
	}

	protected override void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		container.ShuffleSlots();
		container.gameState.gameEventManager.TriggerAfterShuffleContainer(container);
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_shuffle);
	}
}
