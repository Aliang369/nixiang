public class TriggerPrayEffectAction : ExecuteAction
{
	public Pray pray;

	public TriggerPrayEffectAction(Pray pray)
	{
		this.pray = pray;
	}

	protected override void Update()
	{
		pray.TriggerEffect(gameState);
	}
}
