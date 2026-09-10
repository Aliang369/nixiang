public class ReverseContainerAction : GameAction
{
	public ItemContainer container;

	public ReverseContainerAction(ItemContainer container)
	{
		this.container = container;
	}

	protected override void Update()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		container.ReverseSlots();
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_shuffle);
		FinishAction();
	}
}
